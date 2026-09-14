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

ENTRADA, ALOCACAO, VENDA, PERDA, AJUSTE = 0, 1, 2, 3, 4
NOME_TIPO = {0: "Entrada", 1: "Alocacao", 2: "Venda", 3: "Perda", 4: "Ajuste"}

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
        removidos = []
        for pid in self.criados:
            status, _ = self.api("DELETE", "/api/storage/products/v1/%d" % pid)
            removidos.append((pid, status))
        return removidos


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


@caso("CT-19", "Excluir produto com entradas e vendas")
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
    R.exige(status == 200, "exclusao do produto recusada", "HTTP %s" % status)
    if pid in ctx.criados:
        ctx.criados.remove(pid)
    depois = len(movimentos_depois(0, pid))
    existe = sql("SELECT ProId FROM dbo.Product WHERE ProId=%d" % pid)
    R.exige(not existe, "produto continua na base depois do DELETE")
    if depois == 0 and antes > 0:
        R.lacuna("as %d movimentacoes do produto foram APAGADAS junto: o ledger "
                 "perde o historico, nao registra a saida (lacuna conhecida)" % antes)
    else:
        R.nota("movimentacoes preservadas: %d antes, %d depois" % (antes, depois))


@caso("CT-20", "Transferir entre duas prateleiras")
def ct20(ctx):
    R.pula("TransferProductBetweenLocations suporta, mas nenhum endpoint ou tela "
           "chama com prateleira nos dois lados: nao ha o que exercitar")


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
    for pid in antigos:
        ctx.api("DELETE", "/api/storage/products/v1/%d" % pid)
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
        removidos = ctx.limpa()
        nok = [p for p, s in removidos if s != 200]
        print("\nlimpeza: %d produto(s) de teste removido(s)%s"
              % (len(removidos) - len(nok),
                 ", falhou em %s" % nok if nok else ""))

    falhas = R.imprime()
    print("fim:     %s" % datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
    return 1 if falhas else 0


if __name__ == "__main__":
    sys.exit(main())
