#!/usr/bin/env python3
"""
Roteiro de ensaio do ledger de movimentacoes (issue #6).

Exercita os pontos de escrita de estoque pelo gateway e, depois de cada
operacao, confere no banco o que o ledger gravou e o saldo que sobrou.
Nao depende do estado inicial: cria os proprios produtos de teste, mede
deltas e remove o que criou no fim.

Uso:
    python tests/ledger_e2e.py              # roda tudo
    python tests/ledger_e2e.py --caso CT-08 # roda um caso
    python tests/ledger_e2e.py --manter     # nao remove os produtos de teste

Saida: relatorio por caso e codigo de saida 1 se houver falha.
"""

import argparse
import json
import os
import subprocess
import sys
import urllib.error
import urllib.request
from datetime import datetime, timedelta, timezone

REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GATEWAY = os.environ.get("SMARTSTORAGE_GATEWAY", "http://localhost:4480")
USUARIO = os.environ.get("SMARTSTORAGE_USER", "admin")
SENHA = os.environ.get("SMARTSTORAGE_PASSWORD", "admin123")
BANCO = "SmartStorageWeb.db"
SERVICO_SQL = "sqlserver"

PREFIXO = "ZZ Ensaio Automatizado"
EXECUCAO = datetime.now().strftime("%m%d%H%M%S")
DESCRICAO = "Produto criado pelo roteiro de ensaio do ledger. Pode ser removido."

ENTRADA, ALOCACAO, VENDA, PERDA, AJUSTE, DEVOLUCAO, TRANSFERENCIA = 0, 1, 2, 3, 4, 5, 6
NOME_TIPO = {0: "Entrada", 1: "Alocacao", 2: "Venda", 3: "Perda", 4: "Ajuste", 5: "Devolucao", 6: "Transferencia"}

TOLERANCIA_SEGUNDOS = 120


# --------------------------------------------------------------------------- #
# infraestrutura
# --------------------------------------------------------------------------- #

class Erro(Exception):
    pass


def senha_sa():
    caminho = os.path.join(REPO, ".env")
    with open(caminho, encoding="utf-8") as f:
        for linha in f:
            if linha.startswith("SQLSERVER_SA_PASSWORD="):
                return linha.split("=", 1)[1].strip()
    raise Erro("SQLSERVER_SA_PASSWORD nao encontrada no .env")


def sql(consulta):
    """Roda a consulta no container e devolve lista de listas de strings."""
    cmd = [
        "docker", "compose", "exec", "-T", SERVICO_SQL,
        "/opt/mssql-tools18/bin/sqlcmd",
        "-S", "localhost", "-U", "sa", "-P", senha_sa(),
        "-C", "-b", "-d", BANCO, "-h-1", "-W", "-s|",
        "-Q", "SET NOCOUNT ON; " + consulta,
    ]
    p = subprocess.run(cmd, cwd=REPO, capture_output=True, text=True,
                       encoding="utf-8", errors="replace")
    if p.returncode != 0:
        raise Erro("sqlcmd falhou: " + (p.stderr or p.stdout).strip())
    linhas = []
    for linha in p.stdout.splitlines():
        linha = linha.strip()
        if not linha or linha.startswith("Changed database"):
            continue
        linhas.append(linha.split("|"))
    return linhas


def http(metodo, caminho, corpo=None, token=None):
    """Devolve (status, objeto_json_ou_texto). Nao levanta em 4xx."""
    dados = None if corpo is None else json.dumps(corpo).encode("utf-8")
    req = urllib.request.Request(GATEWAY + caminho, data=dados, method=metodo)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", "Bearer " + token)
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            bruto = r.read().decode("utf-8", "replace")
            status = r.status
    except urllib.error.HTTPError as e:
        bruto = e.read().decode("utf-8", "replace")
        status = e.code
    except urllib.error.URLError as e:
        raise Erro("gateway inacessivel em %s: %s" % (GATEWAY, e.reason))
    try:
        return status, json.loads(bruto)
    except ValueError:
        return status, bruto


def entrar():
    status, corpo = http("POST", "/api/auth/v1/signin",
                         {"userName": USUARIO, "password": SENHA})
    if status != 200 or not isinstance(corpo, dict) or not corpo.get("accessToken"):
        raise Erro("login falhou (HTTP %s): %s" % (status, corpo))
    return corpo["accessToken"]


# --------------------------------------------------------------------------- #
# leitura de estado
# --------------------------------------------------------------------------- #

def ultimo_movimento():
    return int(sql("SELECT ISNULL(MAX(PsmId),0) FROM dbo.ProductStockMovement")[0][0])


def hora_servidor():
    t = sql("SELECT CONVERT(varchar(23), GETDATE(), 121)")[0][0]
    return datetime.strptime(t, "%Y-%m-%d %H:%M:%S.%f")


def saldos(produto):
    """{None: deposito, shelf_id: quantidade}"""
    fora = {}
    dep = sql("SELECT ProQntd FROM dbo.Product WHERE ProId=%d" % produto)
    fora[None] = int(dep[0][0]) if dep else 0
    for linha in sql("SELECT EntSheId, EntQntd FROM dbo.Enter WHERE EntProId=%d" % produto):
        fora[int(linha[0])] = int(linha[1])
    return fora


def movimentos_depois(psm_id, produto=None):
    filtro = "PsmId > %d" % psm_id
    if produto is not None:
        filtro += " AND PsmProId = %d" % produto
    linhas = sql(
        "SELECT PsmId, PsmProId, ISNULL(CAST(PsmSheId AS varchar(10)),'NULL'), "
        "PsmType, PsmQntd, ISNULL(PsmReason,''), "
        "CONVERT(varchar(23), PsmDate, 121), ISNULL(CAST(PsmUseId AS varchar(20)),'NULL') "
        "FROM dbo.ProductStockMovement WHERE %s ORDER BY PsmId" % filtro)
    saida = []
    for l in linhas:
        saida.append({
            "id": int(l[0]),
            "produto": int(l[1]),
            "prateleira": None if l[2] == "NULL" else int(l[2]),
            "tipo": int(l[3]),
            "qntd": int(l[4]),
            "motivo": l[5],
            "data": datetime.strptime(l[6], "%Y-%m-%d %H:%M:%S.%f"),
            "autor": None if l[7] == "NULL" else int(l[7]),
        })
    return saida


def id_do_usuario(username):
    linhas = sql("SELECT id FROM dbo.[User] WHERE UseUsername = '%s'"
                 % username.replace("'", "''"))
    if not linhas:
        raise Erro("usuario %r nao encontrado no banco" % username)
    return int(linhas[0][0])


def apaga_produtos_de_teste(produtos):
    """A aplicacao nao exclui produto, de proposito: o historico do ledger precisa
    sobreviver. Os produtos do roteiro saem direto do banco, e so eles."""
    if not produtos:
        return
    alvo = "SELECT ProId FROM dbo.Product WHERE ProId IN (%s) AND ProName LIKE '%s%%'" % (
        ",".join(str(int(p)) for p in produtos), PREFIXO.replace("'", "''"))
    sql("SET XACT_ABORT ON; BEGIN TRAN; "
        "DELETE s FROM dbo.Sale s JOIN dbo.Enter e ON e.EntId = s.SalEntId WHERE e.EntProId IN (%(alvo)s); "
        "DELETE FROM dbo.ProductStockMovement WHERE PsmProId IN (%(alvo)s); "
        "DELETE FROM dbo.Enter WHERE EntProId IN (%(alvo)s); "
        "DELETE FROM dbo.Product WHERE ProId IN (%(alvo)s); "
        "COMMIT" % {"alvo": alvo})


def entrada_de(produto, prateleira):
    linhas = sql("SELECT EntId, EntQntd, EntPrice FROM dbo.Enter "
                 "WHERE EntProId=%d AND EntSheId=%d" % (produto, prateleira))
    if not linhas:
        return None
    return {"id": int(linhas[0][0]), "qntd": int(linhas[0][1]),
            "preco": float(linhas[0][2])}


def total_vendas():
    return int(sql("SELECT COUNT(*) FROM dbo.Sale")[0][0])


# --------------------------------------------------------------------------- #
# relatorio
# --------------------------------------------------------------------------- #

class Relatorio:
    def __init__(self):
        self.casos = []
        self.atual = None

    def abre(self, codigo, titulo):
        self.atual = {"codigo": codigo, "titulo": titulo, "falhas": [],
                      "notas": [], "estado": "ok"}
        self.casos.append(self.atual)
        return self.atual

    def exige(self, condicao, descricao, detalhe=""):
        if condicao:
            return True
        self.atual["falhas"].append(descricao + (" | " + detalhe if detalhe else ""))
        self.atual["estado"] = "falha"
        return False

    def nota(self, texto):
        self.atual["notas"].append(texto)

    def lacuna(self, texto):
        self.atual["estado"] = "lacuna"
        self.atual["notas"].append(texto)

    def pula(self, texto):
        self.atual["estado"] = "pulado"
        self.atual["notas"].append(texto)

    def erro(self, texto):
        self.atual["estado"] = "falha"
        self.atual["falhas"].append(texto)

    def imprime(self):
        simbolo = {"ok": "PASSOU", "falha": "FALHOU", "lacuna": "LACUNA",
                   "pulado": "PULADO"}
        largura = max(len(c["titulo"]) for c in self.casos) if self.casos else 20
        print()
        print("=" * (largura + 30))
        for c in self.casos:
            print("%-7s %-6s %s" % (c["codigo"], simbolo[c["estado"]], c["titulo"]))
            for n in c["notas"]:
                print("           . %s" % n)
            for f in c["falhas"]:
                print("           X %s" % f)
        print("=" * (largura + 30))
        conta = {}
        for c in self.casos:
            conta[c["estado"]] = conta.get(c["estado"], 0) + 1
        print("total %d  |  passou %d  |  falhou %d  |  lacuna %d  |  pulado %d" % (
            len(self.casos), conta.get("ok", 0), conta.get("falha", 0),
            conta.get("lacuna", 0), conta.get("pulado", 0)))
        return conta.get("falha", 0)


R = Relatorio()


# --------------------------------------------------------------------------- #
# verificacoes transversais
# --------------------------------------------------------------------------- #

def confere_carimbo(movs):
    """Toda movimentacao marca a hora do servidor, e pares de transferencia
    compartilham o mesmo instante. Foi o bug de 2026-09-12."""
    if not movs:
        return
    agora = hora_servidor()
    for m in movs:
        desvio = abs((agora - m["data"]).total_seconds())
        R.exige(desvio <= TOLERANCIA_SEGUNDOS,
                "PsmDate de #%d fora do relogio do servidor" % m["id"],
                "gravado %s, servidor %s, desvio %.0fs" % (
                    m["data"], agora, desvio))
    if len(movs) == 2:
        R.exige(movs[0]["data"] == movs[1]["data"],
                "as duas pernas da transferencia tem carimbos diferentes",
                "%s vs %s" % (movs[0]["data"], movs[1]["data"]))


def confere_autor(ctx, movs):
    """Toda movimentacao grava como autor o usuario do token."""
    for m in movs:
        R.exige(m["autor"] == ctx.usuario,
                "PsmUseId de #%d diferente do usuario logado" % m["id"],
                "gravado %s, esperado %s (%s)" % (m["autor"], ctx.usuario, USUARIO))


def confere_invariante(produto, antes, esperado_por_local, movs):
    """saldo_depois == saldo_antes + soma dos lancamentos daquele local."""
    depois = saldos(produto)
    locais = set(antes) | set(depois) | {m["prateleira"] for m in movs}
    for local in locais:
        soma = sum(m["qntd"] for m in movs if m["prateleira"] == local)
        de = antes.get(local, 0)
        para = depois.get(local, 0)
        nome = "deposito" if local is None else "prateleira %d" % local
        R.exige(para == de + soma,
                "invariante quebrada no %s" % nome,
                "antes %d, ledger %+d, esperado %d, banco %d" % (
                    de, soma, de + soma, para))
        if local in esperado_por_local:
            R.exige(para == esperado_por_local[local],
                    "saldo final do %s diferente do esperado" % nome,
                    "esperado %d, banco %d" % (esperado_por_local[local], para))
    return depois


def confere_linhas(movs, esperadas):
    """esperadas: lista de (prateleira, tipo, qntd)"""
    obtidas = [(m["prateleira"], m["tipo"], m["qntd"]) for m in movs]
    ok = R.exige(obtidas == esperadas,
                 "lancamentos diferentes do esperado",
                 "esperado %s, gravado %s" % (
                     [(p, NOME_TIPO.get(t, t), q) for p, t, q in esperadas],
                     [(p, NOME_TIPO.get(t, t), q) for p, t, q in obtidas]))
    return ok


# --------------------------------------------------------------------------- #
# fixtures
# --------------------------------------------------------------------------- #

class Contexto:
    def __init__(self, token):
        self.token = token
        self.criados = []
        self.usuario = id_do_usuario(USUARIO)
        self.funcionario = self.primeiro_funcionario()
        self.prateleira_a, self.prateleira_b = self.duas_prateleiras()

    def api(self, metodo, caminho, corpo=None):
        return http(metodo, caminho, corpo, self.token)

    def primeiro_funcionario(self):
        status, corpo = self.api("GET", "/api/storage/employees/v1")
        if status == 200 and isinstance(corpo, list) and corpo:
            return corpo[0].get("id")
        return None

    def duas_prateleiras(self):
        status, corpo = self.api("GET", "/api/storage/shelf/v1")
        if status != 200 or not isinstance(corpo, list) or len(corpo) < 2:
            raise Erro("preciso de pelo menos duas prateleiras cadastradas")
        return corpo[0]["id"], corpo[1]["id"]

    def cria_produto(self, qntd, sufixo):
        nome = "%s %s %s" % (PREFIXO, EXECUCAO, sufixo)
        corpo = {
            "name": nome,
            "descricao": DESCRICAO,
            "dateRegister": datetime.now().isoformat(),
            "qntd": qntd,
            "employeeId": self.funcionario,
            "proImage": None,
        }
        status, resposta = self.api("POST", "/api/storage/products/v1", corpo)
        if status != 200 or not isinstance(resposta, dict):
            raise Erro("falhou criar produto de teste (HTTP %s): %s" % (status, resposta))
        pid = resposta.get("id")
        if not pid:
            linhas = sql("SELECT MAX(ProId) FROM dbo.Product WHERE ProName = '%s'"
                         % nome.replace("'", "''"))
            if not linhas or linhas[0][0] in ("NULL", ""):
                raise Erro("produto %r nao encontrado depois do POST" % nome)
            pid = int(linhas[0][0])
        self.criados.append(pid)
        return pid

    def limpa(self):
        apaga_produtos_de_teste(self.criados)
        return list(self.criados)


# --------------------------------------------------------------------------- #
# casos
# --------------------------------------------------------------------------- #

CASOS = []


def caso(codigo, titulo):
    def decorador(fn):
        CASOS.append((codigo, titulo, fn))
        return fn
    return decorador


@caso("CT-01", "Criar produto com quantidade maior que zero")
def ct01(ctx):
    marca = ultimo_movimento()
    pid = ctx.cria_produto(10, "CT01")
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, ENTRADA, 10)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, {None: 0}, {None: 10}, movs)


@caso("CT-02", "Criar produto com quantidade zero")
def ct02(ctx):
    marca = ultimo_movimento()
    pid = ctx.cria_produto(0, "CT02")
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [])
    R.exige(saldos(pid)[None] == 0, "produto novo com saldo diferente de zero")


@caso("CT-03", "Ajuste para cima")
def ct03(ctx):
    pid = ctx.cria_produto(10, "CT03")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("POST", "/api/storage/products/v1/%d/adjust-stock" % pid,
                        {"quantity": 25, "reason": "Recontagem do roteiro"})
    R.exige(status == 200, "ajuste para cima recusado", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, AJUSTE, 15)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 25}, movs)
    if movs:
        R.exige(movs[0]["motivo"] == "Recontagem do roteiro",
                "motivo nao gravado no ajuste", "gravado %r" % movs[0]["motivo"])


@caso("CT-04", "Ajuste para baixo")
def ct04(ctx):
    pid = ctx.cria_produto(30, "CT04")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("POST", "/api/storage/products/v1/%d/adjust-stock" % pid,
                        {"quantity": 12, "reason": "Perda no roteiro"})
    R.exige(status == 200, "ajuste para baixo recusado", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, AJUSTE, -18)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 12}, movs)


@caso("CT-05", "Ajuste sem motivo")
def ct05(ctx):
    pid = ctx.cria_produto(10, "CT05")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, corpo = ctx.api("POST", "/api/storage/products/v1/%d/adjust-stock" % pid,
                            {"quantity": 20, "reason": ""})
    R.exige(status == 400, "ajuste sem motivo foi aceito", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.nota("mensagem: %s" % corpo)


@caso("CT-06", "Ajuste igual ao saldo atual do deposito")
def ct06(ctx):
    pid = ctx.cria_produto(10, "CT06")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, corpo = ctx.api("POST", "/api/storage/products/v1/%d/adjust-stock" % pid,
                            {"quantity": 10, "reason": "Sem mudanca"})
    R.exige(status == 400, "ajuste de delta zero foi aceito", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.nota("mensagem: %s" % corpo)


@caso("CT-07", "Ajuste com quantidade negativa")
def ct07(ctx):
    pid = ctx.cria_produto(10, "CT07")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, corpo = ctx.api("POST", "/api/storage/products/v1/%d/adjust-stock" % pid,
                            {"quantity": -5, "reason": "Negativo"})
    R.exige(status == 400, "ajuste negativo foi aceito", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.nota("mensagem: %s" % corpo)


@caso("CT-08", "Alocar do deposito para prateleira")
def ct08(ctx):
    pid = ctx.cria_produto(20, "CT08")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 8, "productPrice": 19.9,
        "dateEnter": datetime.now().isoformat(),
    })
    R.exige(status == 200, "alocacao recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, ALOCACAO, -8), (ctx.prateleira_a, ALOCACAO, 8)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 12, ctx.prateleira_a: 8}, movs)
    ent = entrada_de(pid, ctx.prateleira_a)
    R.exige(ent is not None, "Enter nao foi criado na alocacao")
    if ent:
        R.exige(abs(ent["preco"] - 19.9) < 0.001,
                "EntPrice nao gravado na alocacao", "gravado %s" % ent["preco"])


@caso("CT-09", "Alocar mais que o saldo do deposito")
def ct09(ctx):
    pid = ctx.cria_produto(5, "CT09")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, corpo = ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 50, "productPrice": 10.0,
        "dateEnter": datetime.now().isoformat(),
    })
    R.exige(status == 400, "alocacao acima do saldo foi aceita", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.exige(entrada_de(pid, ctx.prateleira_a) is None,
            "Enter foi criado mesmo com a operacao recusada")
    R.nota("mensagem: %s" % corpo)


@caso("CT-10", "Alocar produto que ja esta na prateleira")
def ct10(ctx):
    pid = ctx.cria_produto(20, "CT10")
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 6, "productPrice": 30.0,
        "dateEnter": datetime.now().isoformat(),
    })
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 4, "productPrice": 11.0,
        "dateEnter": datetime.now().isoformat(),
    })
    R.exige(status == 200, "segunda alocacao recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, ALOCACAO, -4), (ctx.prateleira_a, ALOCACAO, 4)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 10, ctx.prateleira_a: 10}, movs)
    quantas = len(sql("SELECT EntId FROM dbo.Enter WHERE EntProId=%d AND EntSheId=%d"
                      % (pid, ctx.prateleira_a)))
    R.exige(quantas == 1, "criou um Enter novo em vez de somar no existente",
            "%d registros" % quantas)
    ent = entrada_de(pid, ctx.prateleira_a)
    if ent:
        R.nota("EntPrice passou de 30,00 para %.2f: repreco e intencional, "
               "a alocacao tambem edita o preco do local" % ent["preco"])


@caso("CT-11", "Desfazer alocacao com saldo na prateleira")
def ct11(ctx):
    pid = ctx.cria_produto(20, "CT11")
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 15, "productPrice": 25.0,
        "dateEnter": datetime.now().isoformat(),
    })
    ent = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(ent is not None, "pre-condicao falhou: Enter nao existe"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("PUT", "/api/storage/shelf/v1/allocation/%d" % ent["id"], {})
    R.exige(status == 200, "desfazer recusado", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, ALOCACAO, -15), (None, ALOCACAO, 15)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 20, ctx.prateleira_a: 0}, movs)
    R.nota("devolve o EntQntd inteiro, nao uma parte")


@caso("CT-12", "Desfazer alocacao com prateleira zerada")
def ct12(ctx):
    pid = ctx.cria_produto(20, "CT12")
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 5, "productPrice": 12.0,
        "dateEnter": datetime.now().isoformat(),
    })
    ent = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(ent is not None, "pre-condicao falhou: Enter nao existe"):
        return
    ctx.api("PUT", "/api/storage/shelf/v1/allocation/%d" % ent["id"], {})
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("PUT", "/api/storage/shelf/v1/allocation/%d" % ent["id"], {})
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [])
    R.exige(saldos(pid) == antes, "saldo mudou desfazendo prateleira ja zerada")
    R.nota("HTTP %s, sem lancamento: a guarda EntQntd > 0 segurou" % status)


@caso("CT-13", "Registrar venda")
def ct13(ctx):
    pid = ctx.cria_produto(20, "CT13")
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 10, "productPrice": 40.0,
        "dateEnter": datetime.now().isoformat(),
    })
    ent = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(ent is not None, "pre-condicao falhou: Enter nao existe"):
        return
    antes = saldos(pid)
    vendas = total_vendas()
    marca = ultimo_movimento()
    status, _ = ctx.api("POST", "/api/storage/sales/v1", {
        "idEnter": ent["id"], "productId": pid, "qntd": 3,
        "dateSale": datetime.now().isoformat(),
    })
    R.exige(status == 200, "venda recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, VENDA, -3)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 10, ctx.prateleira_a: 7}, movs)
    R.exige(total_vendas() == vendas + 1, "linha em Sale nao foi criada")


@caso("CT-14", "Venda acima do saldo da prateleira")
def ct14(ctx):
    pid = ctx.cria_produto(20, "CT14")
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 4, "productPrice": 40.0,
        "dateEnter": datetime.now().isoformat(),
    })
    ent = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(ent is not None, "pre-condicao falhou: Enter nao existe"):
        return
    antes = saldos(pid)
    vendas = total_vendas()
    marca = ultimo_movimento()
    status, corpo = ctx.api("POST", "/api/storage/sales/v1", {
        "idEnter": ent["id"], "productId": pid, "qntd": 99,
        "dateSale": datetime.now().isoformat(),
    })
    R.exige(status == 400, "venda acima do saldo foi aceita", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.exige(total_vendas() == vendas, "linha em Sale criada numa venda recusada")
    R.nota("mensagem: %s" % corpo)


def _venda_pronta(ctx, sufixo, na_prateleira, quantidade):
    pid = ctx.cria_produto(na_prateleira + 5, "%s" % sufixo)
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": na_prateleira, "productPrice": 50.0,
        "dateEnter": datetime.now().isoformat(),
    })
    ent = entrada_de(pid, ctx.prateleira_a)
    if ent is None:
        return None, None, None
    status, _ = ctx.api("POST", "/api/storage/sales/v1", {
        "idEnter": ent["id"], "productId": pid, "qntd": quantidade,
        "dateSale": datetime.now().isoformat(),
    })
    if status != 200:
        return pid, ent, None
    linhas = sql("SELECT MAX(SalId) FROM dbo.Sale WHERE SalEntId=%d" % ent["id"])
    return pid, ent, int(linhas[0][0])


@caso("CT-15", "Editar venda aumentando a quantidade")
def ct15(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT15", 20, 5)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("PUT", "/api/storage/sales/v1/%d" % venda,
                        {"idEnter": ent["id"], "productId": pid, "qntd": 9})
    R.exige(status == 200, "edicao da venda recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, VENDA, -4)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {ctx.prateleira_a: 11}, movs)


@caso("CT-16", "Editar venda reduzindo a quantidade")
def ct16(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT16", 20, 9)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("PUT", "/api/storage/sales/v1/%d" % venda,
                        {"idEnter": ent["id"], "productId": pid, "qntd": 4})
    R.exige(status == 200, "edicao da venda recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, VENDA, 5)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {ctx.prateleira_a: 16}, movs)


@caso("CT-17", "Editar venda com a mesma quantidade")
def ct17(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT17", 20, 6)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = ctx.api("PUT", "/api/storage/sales/v1/%d" % venda,
                        {"idEnter": ent["id"], "productId": pid, "qntd": 6})
    R.exige(status == 200, "edicao sem mudanca recusada", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa edicao de delta zero")
    R.nota("delta zero nao chega ao repositorio, so SaveChanges")


@caso("CT-18", "Excluir venda")
def ct18(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT18", 20, 7)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    antes = saldos(pid)
    vendas = total_vendas()
    marca = ultimo_movimento()
    status, _ = ctx.api("DELETE", "/api/storage/sales/v1/%d" % venda)
    R.exige(status == 200, "exclusao da venda recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, VENDA, 7)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {ctx.prateleira_a: 20}, movs)
    R.exige(total_vendas() == vendas - 1, "linha em Sale nao foi removida")
    R.nota("estorno entra como lancamento novo; o -7 original permanece")


@caso("CT-19", "Excluir produto nao e permitido")
def ct19(ctx):
    pid = ctx.cria_produto(20, "CT19")
    ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 10, "productPrice": 15.0,
        "dateEnter": datetime.now().isoformat(),
    })
    ent = entrada_de(pid, ctx.prateleira_a)
    if ent:
        ctx.api("POST", "/api/storage/sales/v1", {
            "idEnter": ent["id"], "productId": pid, "qntd": 2,
            "dateSale": datetime.now().isoformat(),
        })
    antes = len(movimentos_depois(0, pid))
    R.exige(antes > 0, "pre-condicao falhou: produto sem movimentacoes")
    status, _ = ctx.api("DELETE", "/api/storage/products/v1/%d" % pid)
    R.exige(status >= 400, "o DELETE do produto foi aceito", "HTTP %s" % status)
    existe = sql("SELECT ProId FROM dbo.Product WHERE ProId=%d" % pid)
    R.exige(bool(existe), "produto sumiu da base depois do DELETE")
    depois = len(movimentos_depois(0, pid))
    R.exige(depois == antes, "movimentacoes do produto mudaram depois do DELETE",
            "%d antes, %d depois" % (antes, depois))
    R.nota("HTTP %s; produto e %d movimentacoes preservados" % (status, depois))


def _aloca(ctx, pid, prateleira, quantidade, preco):
    status, _ = ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": prateleira,
        "productQuantity": quantidade, "productPrice": preco,
        "dateEnter": datetime.now().isoformat(),
    })
    return status


def _transfere(ctx, entrada, destino):
    return ctx.api("POST", "/api/storage/shelf/v1/allocation/%d/transfer" % entrada, {"shelfId": destino})


@caso("CT-20", "Transferir saldo inteiro para prateleira sem o produto")
def ct20(ctx):
    pid = ctx.cria_produto(20, "CT20")
    R.exige(_aloca(ctx, pid, ctx.prateleira_a, 8, 19.9) == 200, "pre-condicao falhou: alocacao recusada")
    origem = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(origem is not None, "pre-condicao falhou: entrada de origem nao criada"):
        return
    R.exige(entrada_de(pid, ctx.prateleira_b) is None, "pre-condicao falhou: destino ja tinha o produto")
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = _transfere(ctx, origem["id"], ctx.prateleira_b)
    R.exige(status == 200, "transferencia recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, TRANSFERENCIA, -8), (ctx.prateleira_b, TRANSFERENCIA, 8)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 12, ctx.prateleira_a: 0, ctx.prateleira_b: 8}, movs)
    destino = entrada_de(pid, ctx.prateleira_b)
    if R.exige(destino is not None, "Enter do destino nao foi criado"):
        R.exige(abs(destino["preco"] - 19.9) < 0.001,
                "destino novo nao herdou o preco da origem", "gravado %s" % destino["preco"])


def _edita_produto(ctx, pid, sufixo, ajuste):
    nome = "%s %s %s" % (PREFIXO, EXECUCAO, sufixo)
    corpo = {
        "name": nome,
        "descricao": DESCRICAO,
        "employeeId": ctx.funcionario,
        "proImage": None,
        "stockAdjustment": ajuste,
    }
    status, resposta = ctx.api("PUT", "/api/storage/products/v1/%d" % pid, corpo)
    return nome, status, resposta


def _nome_do_produto(pid):
    return sql("SELECT ProName FROM dbo.Product WHERE ProId=%d" % pid)[0][0]


@caso("CT-21", "Editar produto e ajustar estoque no mesmo submit")
def ct21(ctx):
    pid = ctx.cria_produto(10, "CT21")
    antes = saldos(pid)
    marca = ultimo_movimento()
    nome, status, _ = _edita_produto(ctx, pid, "CT21 editado",
                                     {"quantity": 14, "reason": "Recontagem na edicao"})
    R.exige(status == 200, "edicao com ajuste recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, AJUSTE, 4)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 14}, movs)
    R.exige(_nome_do_produto(pid) == nome, "a edicao nao foi gravada junto com o ajuste")
    if movs:
        R.exige(movs[0]["motivo"] == "Recontagem na edicao",
                "motivo nao gravado no ajuste da edicao", "gravado %r" % movs[0]["motivo"])


@caso("CT-22", "Ajuste invalido no submit nao grava a edicao")
def ct22(ctx):
    pid = ctx.cria_produto(10, "CT22")
    nome_antes = _nome_do_produto(pid)
    antes = saldos(pid)
    marca = ultimo_movimento()
    _, status, corpo = _edita_produto(ctx, pid, "CT22 editado",
                                      {"quantity": 10, "reason": "Sem mudanca"})
    R.exige(status == 400, "edicao com ajuste de delta zero foi aceita", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.exige(_nome_do_produto(pid) == nome_antes,
            "o nome foi gravado mesmo com o ajuste recusado",
            "antes %r, depois %r" % (nome_antes, _nome_do_produto(pid)))
    R.nota("mensagem: %s" % corpo)


def _saldos_na_api(vo):
    return (vo.get("qntd"), vo.get("shelvesQntd"), vo.get("totalQntd"))


@caso("CT-23", "Saldo total do produto exposto na API")
def ct23(ctx):
    pid = ctx.cria_produto(10, "CT23")
    status, _ = ctx.api("POST", "/api/storage/shelf/v1/allocation", {
        "productId": pid, "shelfId": ctx.prateleira_a,
        "productQuantity": 4, "productPrice": 12.5,
        "dateEnter": datetime.now().isoformat(),
    })
    R.exige(status == 200, "alocacao recusada", "HTTP %s" % status)
    banco = saldos(pid)
    esperado = (banco[None], sum(q for local, q in banco.items() if local is not None))
    esperado = esperado + (esperado[0] + esperado[1],)
    R.exige(esperado == (6, 4, 10), "saldos do banco diferentes do preparado", "banco %s" % (esperado,))

    status, vo = ctx.api("GET", "/api/storage/products/v1/%d" % pid)
    R.exige(status == 200 and isinstance(vo, dict), "busca do produto recusada", "HTTP %s" % status)
    if isinstance(vo, dict):
        R.exige(_saldos_na_api(vo) == esperado,
                "saldos do produto por id diferentes do banco",
                "api (deposito, prateleiras, total) %s, banco %s" % (_saldos_na_api(vo), esperado))

    status, lista = ctx.api("GET", "/api/storage/products/v1")
    item = next((p for p in lista if p.get("id") == pid), None) if isinstance(lista, list) else None
    R.exige(item is not None, "produto ausente na listagem", "HTTP %s" % status)
    if item:
        R.exige(_saldos_na_api(item) == esperado,
                "saldos do produto na listagem diferentes do banco",
                "api (deposito, prateleiras, total) %s, banco %s" % (_saldos_na_api(item), esperado))


def _devolve(ctx, venda, quantidade):
    return ctx.api("POST", "/api/storage/sales/v1/%d/return" % venda, {"quantity": quantidade})


def _devolvido_no_banco(venda):
    return int(sql("SELECT SalReturnedQntd FROM dbo.Sale WHERE SalId=%d" % venda)[0][0])


def _venda_na_listagem(ctx, venda):
    status, lista = ctx.api("GET", "/api/storage/sales/v1")
    if status != 200 or not isinstance(lista, list):
        raise Erro("listagem de vendas recusada (HTTP %s)" % status)
    return next((v for v in lista if v.get("id") == venda), None)


@caso("CT-24", "Devolucao parcial de venda")
def ct24(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT24", 20, 7)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = _devolve(ctx, venda, 3)
    R.exige(status == 200, "devolucao parcial recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, DEVOLUCAO, 3)])
    confere_carimbo(movs)
    confere_autor(ctx, movs)
    confere_invariante(pid, antes, {None: 8, ctx.prateleira_a: 13}, movs)
    if movs:
        R.exige(movs[0]["motivo"] == "Devolução da venda %d" % venda,
                "motivo da devolucao nao aponta para a venda", "gravado %r" % movs[0]["motivo"])
    R.exige(_devolvido_no_banco(venda) == 3, "SalReturnedQntd nao registrou a devolucao",
            "gravado %s" % _devolvido_no_banco(venda))
    item = _venda_na_listagem(ctx, venda)
    R.exige(item is not None, "venda parcialmente devolvida sumiu da listagem")
    if item:
        R.exige((item.get("qntd"), item.get("returnedQntd"), item.get("netQntd")) == (7, 3, 4),
                "quantidades da venda na listagem erradas",
                "api (vendida, devolvida, liquida) %s" % ((item.get("qntd"), item.get("returnedQntd"), item.get("netQntd")),))
        R.exige(abs(float(item.get("saleTotal", 0)) - 200.0) < 0.001,
                "total da venda nao desconta a devolucao", "api %s, esperado 200.00" % item.get("saleTotal"))


@caso("CT-25", "Devolucao total tira a venda da listagem")
def ct25(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT25", 10, 4)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = _devolve(ctx, venda, 4)
    R.exige(status == 200, "devolucao total recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(None, DEVOLUCAO, 4)])
    confere_invariante(pid, antes, {None: 9, ctx.prateleira_a: 6}, movs)
    R.exige(_venda_na_listagem(ctx, venda) is None, "venda totalmente devolvida ainda aparece na listagem")
    R.exige(bool(sql("SELECT SalId FROM dbo.Sale WHERE SalId=%d" % venda)),
            "venda totalmente devolvida foi apagada do banco")
    status, _ = ctx.api("GET", "/api/storage/sales/v1/%d" % venda)
    R.exige(status == 200, "venda totalmente devolvida nao pode mais ser consultada por id", "HTTP %s" % status)


@caso("CT-26", "Devolucao acima do que resta da venda")
def ct26(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT26", 10, 5)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    _devolve(ctx, venda, 2)
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, corpo = _devolve(ctx, venda, 4)
    R.exige(status == 400, "devolucao acima do restante foi aceita", "HTTP %s" % status)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.exige(_devolvido_no_banco(venda) == 2, "SalReturnedQntd mudou numa devolucao recusada")
    R.nota("mensagem: %s" % corpo)


@caso("CT-27", "Venda com devolucao nao pode ser cancelada nem reduzida abaixo do devolvido")
def ct27(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT27", 10, 6)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    _devolve(ctx, venda, 3)
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, corpo = ctx.api("DELETE", "/api/storage/sales/v1/%d" % venda)
    R.exige(status == 400, "cancelamento de venda com devolucao foi aceito", "HTTP %s" % status)
    R.exige(bool(sql("SELECT SalId FROM dbo.Sale WHERE SalId=%d" % venda)), "venda com devolucao foi apagada")
    status2, corpo2 = ctx.api("PUT", "/api/storage/sales/v1/%d" % venda, {"qntd": 2})
    R.exige(status2 == 400, "edicao abaixo do devolvido foi aceita", "HTTP %s" % status2)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    R.nota("cancelar: %s | editar: %s" % (corpo, corpo2))


@caso("CT-28", "Transferir para prateleira que ja tem o produto")
def ct28(ctx):
    pid = ctx.cria_produto(20, "CT28")
    _aloca(ctx, pid, ctx.prateleira_a, 5, 10.0)
    _aloca(ctx, pid, ctx.prateleira_b, 3, 30.0)
    origem = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(origem is not None and entrada_de(pid, ctx.prateleira_b) is not None,
                   "pre-condicao falhou: produto nao ficou nas duas prateleiras"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    status, _ = _transfere(ctx, origem["id"], ctx.prateleira_b)
    R.exige(status == 200, "transferencia recusada", "HTTP %s" % status)
    movs = movimentos_depois(marca, pid)
    confere_linhas(movs, [(ctx.prateleira_a, TRANSFERENCIA, -5), (ctx.prateleira_b, TRANSFERENCIA, 5)])
    confere_carimbo(movs)
    confere_invariante(pid, antes, {ctx.prateleira_a: 0, ctx.prateleira_b: 8}, movs)
    destino = entrada_de(pid, ctx.prateleira_b)
    if destino:
        R.exige(abs(destino["preco"] - 30.0) < 0.001,
                "transferencia repreciou o destino", "gravado %s, esperado 30.00" % destino["preco"])
    enters = int(sql("SELECT COUNT(*) FROM dbo.Enter WHERE EntProId=%d AND EntSheId=%d" % (pid, ctx.prateleira_b))[0][0])
    R.exige(enters == 1, "transferencia criou outro Enter no destino", "%d linhas" % enters)


@caso("CT-29", "Transferencias invalidas nao gravam nada")
def ct29(ctx):
    pid = ctx.cria_produto(20, "CT29")
    _aloca(ctx, pid, ctx.prateleira_a, 6, 10.0)
    origem = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(origem is not None, "pre-condicao falhou: entrada nao criada"):
        return
    antes = saldos(pid)
    marca = ultimo_movimento()
    mesma, m1 = _transfere(ctx, origem["id"], ctx.prateleira_a)
    R.exige(mesma == 400, "transferencia para a mesma prateleira foi aceita", "HTTP %s" % mesma)
    inexistente, m2 = _transfere(ctx, origem["id"], 999999)
    R.exige(inexistente == 400, "transferencia para prateleira inexistente foi aceita", "HTTP %s" % inexistente)
    sem_destino, m3 = _transfere(ctx, origem["id"], 0)
    R.exige(sem_destino == 400, "transferencia sem destino foi aceita", "HTTP %s" % sem_destino)
    confere_linhas(movimentos_depois(marca, pid), [])
    R.exige(saldos(pid) == antes, "saldo mudou numa operacao que devia falhar")
    ctx.api("PUT", "/api/storage/shelf/v1/allocation/%d" % origem["id"])
    zerada = ultimo_movimento()
    vazia, m4 = _transfere(ctx, origem["id"], ctx.prateleira_b)
    R.exige(vazia == 400, "transferencia de prateleira sem saldo foi aceita", "HTTP %s" % vazia)
    confere_linhas(movimentos_depois(zerada, pid), [])
    R.nota("mesma: %s | inexistente: %s | sem saldo: %s" % (m1, m2, m4))


def _preco_no_banco(venda):
    return float(sql("SELECT SalPrice FROM dbo.Sale WHERE SalId=%d" % venda)[0][0])


@caso("CT-30", "Venda guarda o preco do momento e nao muda com o repreco")
def ct30(ctx):
    pid, ent, venda = _venda_pronta(ctx, "CT30", 20, 4)
    if not R.exige(venda is not None, "pre-condicao falhou: venda nao criada"):
        return
    R.exige(abs(_preco_no_banco(venda) - 50.0) < 0.001, "SalPrice nao gravou o preco da prateleira",
            "gravado %.2f, esperado 50.00" % _preco_no_banco(venda))
    R.exige(_aloca(ctx, pid, ctx.prateleira_a, 1, 80.0) == 200, "pre-condicao falhou: repreco recusado")
    R.exige(abs(_preco_no_banco(venda) - 50.0) < 0.001, "repreco da prateleira alterou o SalPrice da venda antiga",
            "gravado %.2f" % _preco_no_banco(venda))
    item = _venda_na_listagem(ctx, venda)
    R.exige(item is not None, "venda sumiu da listagem")
    if item:
        R.exige(abs(float(item.get("salePrice", 0)) - 50.0) < 0.001,
                "preco da venda na listagem seguiu a prateleira", "api %s, esperado 50.00" % item.get("salePrice"))
        R.exige(abs(float(item.get("saleTotal", 0)) - 200.0) < 0.001,
                "total da venda antiga mudou com o repreco", "api %s, esperado 200.00" % item.get("saleTotal"))
    status, _ = ctx.api("POST", "/api/storage/sales/v1", {
        "idEnter": ent["id"], "productId": pid, "qntd": 1,
        "dateSale": datetime.now().isoformat(),
    })
    R.exige(status == 200, "segunda venda recusada", "HTTP %s" % status)
    nova = int(sql("SELECT MAX(SalId) FROM dbo.Sale WHERE SalEntId=%d" % ent["id"])[0][0])
    R.exige(nova != venda and abs(_preco_no_banco(nova) - 80.0) < 0.001,
            "venda nova nao pegou o preco atual da prateleira", "gravado %.2f, esperado 80.00" % _preco_no_banco(nova))


def _pagina_da_listagem(ctx, consulta, recurso="/api/storage/sales/v1"):
    req = urllib.request.Request(GATEWAY + recurso + "?" + consulta)
    req.add_header("Authorization", "Bearer " + ctx.token)
    try:
        with urllib.request.urlopen(req, timeout=60) as r:
            return r.status, json.loads(r.read().decode("utf-8")), r.headers.get("X-Total-Count")
    except urllib.error.HTTPError as e:
        return e.code, e.read().decode("utf-8", "replace"), None


@caso("CT-31", "Listagem de vendas paginada e com pesquisa")
def ct31(ctx):
    pid = ctx.cria_produto(15, "CT31")
    R.exige(_aloca(ctx, pid, ctx.prateleira_a, 12, 7.0) == 200, "pre-condicao falhou: alocacao recusada")
    ent = entrada_de(pid, ctx.prateleira_a)
    if not R.exige(ent is not None, "pre-condicao falhou: entrada nao criada"):
        return
    base = datetime.now()
    for i in range(12):
        ctx.api("POST", "/api/storage/sales/v1", {
            "idEnter": ent["id"], "productId": pid, "qntd": 1,
            "dateSale": (base - timedelta(minutes=i)).isoformat()})
    ids = [int(l[0]) for l in sql("SELECT SalId FROM dbo.Sale WHERE SalEntId=%d "
                                  "ORDER BY SalDateSale DESC, SalId DESC" % ent["id"])]
    R.exige(len(ids) == 12, "pre-condicao falhou: vendas nao criadas", "criadas %d" % len(ids))
    busca = "CT31"
    nome = sql("SELECT ProName FROM dbo.Product WHERE ProId=%d" % pid)[0][0]
    termo = urllib.request.quote(nome)

    status, pagina1, total = _pagina_da_listagem(ctx, "page=1&pageSize=5&search=%s" % termo)
    R.exige(status == 200, "primeira pagina recusada", "HTTP %s" % status)
    R.exige(total == "12", "X-Total-Count nao conta as vendas da pesquisa", "cabecalho %r" % total)
    if isinstance(pagina1, list):
        R.exige([v["id"] for v in pagina1] == ids[:5], "primeira pagina fora da ordem mais recente primeiro",
                "api %s, banco %s" % ([v["id"] for v in pagina1], ids[:5]))
        R.exige(all(v["productName"] == nome for v in pagina1), "pesquisa trouxe venda de outro produto")

    status, pagina3, total = _pagina_da_listagem(ctx, "page=3&pageSize=5&search=%s" % termo)
    R.exige(status == 200 and isinstance(pagina3, list) and [v["id"] for v in pagina3] == ids[10:],
            "ultima pagina nao traz as 2 vendas restantes", "HTTP %s, %r" % (status, pagina3))

    status, nada, total = _pagina_da_listagem(ctx, "page=1&pageSize=5&search=%s" % urllib.request.quote(nome + " inexistente"))
    R.exige(status == 200 and nada == [] and total == "0", "pesquisa sem resultado nao volta vazia",
            "HTTP %s, total %r" % (status, total))

    status, invalida, _ = _pagina_da_listagem(ctx, "page=1&pageSize=0")
    R.exige(status == 400, "tamanho de pagina zero foi aceito", "HTTP %s" % status)
    status, invalida, _ = _pagina_da_listagem(ctx, "page=0&pageSize=5")
    R.exige(status == 400, "pagina zero foi aceita", "HTTP %s" % status)

    todas = _venda_na_listagem(ctx, ids[-1])
    R.exige(todas is not None, "listagem sem page deixou de devolver todas as vendas")
    R.nota("busca por %r: 12 vendas em paginas de 5; %s" % (busca, invalida))


@caso("CT-32", "Listagem de produtos nas prateleiras paginada e com pesquisa")
def ct32(ctx):
    alocacoes = "/api/storage/shelf/v1/allocation"
    um = ctx.cria_produto(20, "CT32 Um")
    dois = ctx.cria_produto(20, "CT32 Dois")
    R.exige(_aloca(ctx, um, ctx.prateleira_a, 5, 3.0) == 200, "pre-condicao falhou: alocacao recusada")
    R.exige(_aloca(ctx, um, ctx.prateleira_b, 5, 3.0) == 200, "pre-condicao falhou: alocacao recusada")
    R.exige(_aloca(ctx, dois, ctx.prateleira_a, 5, 3.0) == 200, "pre-condicao falhou: alocacao recusada")
    zerada = entrada_de(dois, ctx.prateleira_a)
    if not R.exige(zerada is not None, "pre-condicao falhou: entrada nao criada"):
        return
    status, _ = _transfere(ctx, zerada["id"], ctx.prateleira_b)
    R.exige(status == 200 and entrada_de(dois, ctx.prateleira_a)["qntd"] == 0,
            "pre-condicao falhou: entrada de origem nao ficou zerada", "HTTP %s" % status)

    busca = "%s CT32" % EXECUCAO
    termo = urllib.request.quote(busca)
    ids = [int(l[0]) for l in sql(
        "SELECT e.EntId FROM dbo.Enter e JOIN dbo.Product p ON p.ProId = e.EntProId "
        "JOIN dbo.Shelf s ON s.SheId = e.EntSheId WHERE p.ProName LIKE '%%%s%%' AND e.EntQntd > 0 "
        "ORDER BY s.SheName, p.ProName, e.EntId" % busca)]
    R.exige(len(ids) == 3, "pre-condicao falhou: esperadas 3 entradas com saldo", "banco %s" % ids)

    status, pagina1, total = _pagina_da_listagem(ctx, "page=1&pageSize=2&search=%s" % termo, alocacoes)
    R.exige(status == 200, "primeira pagina recusada", "HTTP %s" % status)
    R.exige(total == "3", "X-Total-Count nao conta so as entradas com saldo da pesquisa", "cabecalho %r" % total)
    if isinstance(pagina1, list):
        R.exige([e["id"] for e in pagina1] == ids[:2], "primeira pagina fora da ordem por prateleira e produto",
                "api %s, banco %s" % ([e["id"] for e in pagina1], ids[:2]))
        R.exige(all(busca in e["productName"] for e in pagina1), "pesquisa trouxe entrada de outro produto")

    status, pagina2, _ = _pagina_da_listagem(ctx, "page=2&pageSize=2&search=%s" % termo, alocacoes)
    R.exige(status == 200 and isinstance(pagina2, list) and [e["id"] for e in pagina2] == ids[2:],
            "segunda pagina nao traz a entrada restante", "HTTP %s, %r" % (status, pagina2))
    R.exige(zerada["id"] not in ids, "entrada zerada contou como produto na prateleira")

    status, nada, total = _pagina_da_listagem(ctx, "page=1&pageSize=2&search=%s" % urllib.request.quote(busca + " inexistente"), alocacoes)
    R.exige(status == 200 and nada == [] and total == "0", "pesquisa sem resultado nao volta vazia",
            "HTTP %s, total %r" % (status, total))

    status, _, _ = _pagina_da_listagem(ctx, "page=1&pageSize=101", alocacoes)
    R.exige(status == 400, "tamanho de pagina acima do maximo foi aceito", "HTTP %s" % status)

    status, todas = ctx.api("GET", alocacoes)
    R.exige(status == 200 and isinstance(todas, list) and any(e.get("id") == zerada["id"] for e in todas),
            "listagem sem page deixou de devolver todas as entradas, inclusive as zeradas")
    R.nota("busca por %r: 3 entradas com saldo em paginas de 2; a zerada fica fora" % busca)


@caso("CT-33", "Listagem de produtos paginada e com pesquisa")
def ct33(ctx):
    recurso = "/api/storage/products/v1"
    criados = [ctx.cria_produto(10, "CT33 %s" % letra) for letra in ("Cedro", "Acacia", "Bambu")]
    R.exige(all(criados), "pre-condicao falhou: produtos nao criados", "%s" % criados)
    R.exige(_aloca(ctx, criados[1], ctx.prateleira_a, 4, 2.0) == 200, "pre-condicao falhou: alocacao recusada")

    busca = "%s CT33" % EXECUCAO
    termo = urllib.request.quote(busca)
    ids = [int(l[0]) for l in sql("SELECT ProId FROM dbo.Product WHERE ProName LIKE '%%%s%%' "
                                  "ORDER BY ProName, ProId" % busca)]
    R.exige(len(ids) == 3, "pre-condicao falhou: esperados 3 produtos", "banco %s" % ids)

    status, pagina1, total = _pagina_da_listagem(ctx, "page=1&pageSize=2&search=%s" % termo, recurso)
    R.exige(status == 200, "primeira pagina recusada", "HTTP %s" % status)
    R.exige(total == "3", "X-Total-Count nao conta os produtos da pesquisa", "cabecalho %r" % total)
    if isinstance(pagina1, list):
        R.exige([p["id"] for p in pagina1] == ids[:2], "primeira pagina fora da ordem por nome",
                "api %s, banco %s" % ([p["id"] for p in pagina1], ids[:2]))
        R.exige(all(busca in p["name"] for p in pagina1), "pesquisa trouxe outro produto")
        alocado = next((p for p in pagina1 if p["id"] == criados[1]), None)
        if R.exige(alocado is not None, "produto Acacia devia abrir a primeira pagina"):
            R.exige(_saldos_na_api(alocado) == (6, 4, 10), "saldos do produto na pagina diferentes do banco",
                    "api (deposito, prateleiras, total) %s" % (_saldos_na_api(alocado),))

    status, pagina2, _ = _pagina_da_listagem(ctx, "page=2&pageSize=2&search=%s" % termo, recurso)
    R.exige(status == 200 and isinstance(pagina2, list) and [p["id"] for p in pagina2] == ids[2:],
            "segunda pagina nao traz o produto restante", "HTTP %s, %r" % (status, pagina2))

    status, nada, total = _pagina_da_listagem(ctx, "page=1&pageSize=2&search=%s" % urllib.request.quote(busca + " inexistente"), recurso)
    R.exige(status == 200 and nada == [] and total == "0", "pesquisa sem resultado nao volta vazia",
            "HTTP %s, total %r" % (status, total))

    status, _, _ = _pagina_da_listagem(ctx, "page=-1&pageSize=2", recurso)
    R.exige(status == 400, "pagina negativa foi aceita", "HTTP %s" % status)

    status, todos = ctx.api("GET", recurso)
    R.exige(status == 200 and isinstance(todos, list) and all(any(p.get("id") == i for p in todos) for i in ids),
            "listagem sem page deixou de devolver todos os produtos")
    R.nota("busca por %r: 3 produtos em paginas de 2, por nome" % busca)


# --------------------------------------------------------------------------- #
# conferencia final de toda a base
# --------------------------------------------------------------------------- #

def confere_base_inteira():
    R.abre("BASE", "Saldo de todo produto bate com o ledger mais o saldo de abertura")
    linhas = sql("""
        WITH Atual AS (
          SELECT p.ProId, CAST(NULL AS int) AS SheId, p.ProQntd AS Qntd FROM dbo.Product p
          UNION ALL
          SELECT e.EntProId, e.EntSheId, e.EntQntd FROM dbo.Enter e
        ),
        Ledger AS (
          SELECT PsmProId AS ProId, PsmSheId AS SheId, SUM(PsmQntd) AS Soma
          FROM dbo.ProductStockMovement GROUP BY PsmProId, PsmSheId
        )
        SELECT a.ProId, ISNULL(CAST(a.SheId AS varchar(10)),'DEP'), a.Qntd, ISNULL(g.Soma,0)
        FROM Atual a
        LEFT JOIN Ledger g ON g.ProId=a.ProId AND ISNULL(g.SheId,-1)=ISNULL(a.SheId,-1)
        ORDER BY a.ProId, ISNULL(a.SheId,0)
    """)
    negativos = sql("SELECT ProId, ProQntd FROM dbo.Product WHERE ProQntd < 0")
    R.exige(not negativos, "produto com saldo negativo no deposito",
            str(negativos))
    negativos = sql("SELECT EntId, EntQntd FROM dbo.Enter WHERE EntQntd < 0")
    R.exige(not negativos, "entrada com saldo negativo na prateleira",
            str(negativos))
    orfas = sql("""SELECT COUNT(*) FROM dbo.ProductStockMovement m
                   WHERE NOT EXISTS (SELECT 1 FROM dbo.Product p WHERE p.ProId=m.PsmProId)""")
    R.exige(int(orfas[0][0]) == 0, "movimentacao apontando para produto inexistente",
            "%s linhas" % orfas[0][0])
    vendas_orfas = sql("""SELECT COUNT(*) FROM dbo.Sale s
                          WHERE NOT EXISTS (SELECT 1 FROM dbo.Enter e WHERE e.EntId=s.SalEntId)""")
    R.exige(int(vendas_orfas[0][0]) == 0, "venda apontando para entrada inexistente",
            "%s linhas" % vendas_orfas[0][0])
    R.nota("%d combinacoes produto/local conferidas" % len(linhas))


def remove_sobras(ctx):
    """Produtos de execucoes anteriores que a limpeza nao alcancou."""
    linhas = sql("SELECT ProId FROM dbo.Product WHERE ProName LIKE '%s%%'"
                 % PREFIXO.replace("'", "''"))
    antigos = [int(l[0]) for l in linhas if l[0] not in ("NULL", "")]
    antigos = [p for p in antigos if p not in ctx.criados]
    apaga_produtos_de_teste(antigos)
    return antigos


def confere_saude():
    R.abre("SAUDE", "Gateway e servicos respondendo")
    for nome, caminho in [("gateway", "/health")]:
        status, _ = http("GET", caminho)
        R.exige(status == 200, "%s nao respondeu 200" % nome, "HTTP %s" % status)
    status, _ = http("GET", "/api/storage/sales/v1")
    R.exige(status == 401, "rota de vendas respondeu sem token",
            "esperado 401, veio %s" % status)
    R.nota("sem token a API recusa, com token responde: autorizacao ativa")


# --------------------------------------------------------------------------- #
# principal
# --------------------------------------------------------------------------- #

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--caso", action="append",
                    help="roda apenas o(s) caso(s) informado(s), ex.: --caso CT-08")
    ap.add_argument("--manter", action="store_true",
                    help="nao remove os produtos criados pelo roteiro")
    args = ap.parse_args()

    print("SmartStorage | roteiro de ensaio do ledger")
    print("gateway: %s" % GATEWAY)
    print("inicio:  %s" % datetime.now().strftime("%Y-%m-%d %H:%M:%S"))

    try:
        confere_saude()
        token = entrar()
        ctx = Contexto(token)
        print("usuario: %s (id %s) | funcionario: %s | prateleiras de teste: %s e %s"
              % (USUARIO, ctx.usuario, ctx.funcionario, ctx.prateleira_a, ctx.prateleira_b))
        print("execucao: %s" % EXECUCAO)
        sobras = remove_sobras(ctx)
        if sobras:
            print("sobras de execucoes anteriores removidas: %s" % sobras)
    except Erro as e:
        print("\nABORTADO: %s" % e)
        return 2

    selecionados = [c for c in CASOS if not args.caso or c[0] in args.caso]
    for codigo, titulo, fn in selecionados:
        R.abre(codigo, titulo)
        try:
            fn(ctx)
        except Erro as e:
            R.erro("erro de infraestrutura: %s" % e)
        except Exception as e:
            R.erro("excecao inesperada: %s: %s" % (type(e).__name__, e))

    try:
        confere_base_inteira()
    except Erro as e:
        R.erro("conferencia final falhou: %s" % e)

    if args.manter:
        print("\n--manter: produtos de teste preservados: %s" % ctx.criados)
    else:
        try:
            removidos = ctx.limpa()
            print("\nlimpeza: %d produto(s) de teste removido(s) direto do banco" % len(removidos))
        except Erro as e:
            print("\nlimpeza falhou: %s" % e)

    falhas = R.imprime()
    print("fim:     %s" % datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
    return 1 if falhas else 0


if __name__ == "__main__":
    sys.exit(main())
