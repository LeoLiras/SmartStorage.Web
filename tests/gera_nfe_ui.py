#!/usr/bin/env python3
"""
Gera os XMLs de NF-e usados no roteiro de tela (tests/roteiro_ui.md).

Uso:
    python tests/gera_nfe_ui.py <pasta> [--gtin-existente 7891234567895]

Escreve na pasta:
    nfe_ui.xml           1 item com o GTIN existente (ou novo, se nao vier) e 1 item novo em CX
    nfe_gtin_invalido.xml 1 item com cEAN de digito verificador errado
    nao_e_nfe.xml        XML valido que nao e NF-e

A chave e os GTINs novos mudam a cada execucao, para a nota nao sair como ja importada.
"""

import argparse
import os
from datetime import datetime

EXECUCAO = datetime.now().strftime("%m%d%H%M%S")


def gtin(sufixo):
    base = "3" + EXECUCAO + "%01d" % sufixo
    soma = sum(int(d) * (3 if i % 2 == 0 else 1) for i, d in enumerate(reversed(base)))
    return base + str((10 - soma % 10) % 10)


def nota(itens, sequencia):
    """itens: lista de (codigo, descricao, unidade, quantidade, valor_total)"""
    chave = "35261012345678000195550010" + EXECUCAO + "%08d" % sequencia
    dets = "".join(
        '<det nItem="%d"><prod><cProd>UI-%d</cProd><cEAN>%s</cEAN><xProd>%s</xProd><uCom>%s</uCom>'
        '<qCom>%s</qCom><vUnCom>%.4f</vUnCom><vProd>%.2f</vProd></prod></det>'
        % (n, n, codigo or "SEM GTIN", descricao, unidade, quantidade, total / float(quantidade), total)
        for n, (codigo, descricao, unidade, quantidade, total) in enumerate(itens, start=1))
    return ('<?xml version="1.0" encoding="UTF-8"?>'
            '<nfeProc xmlns="http://www.portalfiscal.inf.br/nfe" versao="4.00"><NFe>'
            '<infNFe Id="NFe%s" versao="4.00"><ide><mod>55</mod><serie>1</serie><nNF>%d</nNF>'
            '<dhEmi>2026-10-22T09:00:00-03:00</dhEmi></ide>'
            '<emit><CNPJ>12345678000195</CNPJ><xNome>ZZ Roteiro UI Fornecedor</xNome></emit>%s</infNFe></NFe></nfeProc>'
            % (chave, sequencia, dets))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("pasta")
    parser.add_argument("--gtin-existente")
    args = parser.parse_args()
    os.makedirs(args.pasta, exist_ok=True)

    existente = args.gtin_existente or gtin(1)
    invalido = gtin(3)[:-1] + str((int(gtin(3)[-1]) + 1) % 10)
    arquivos = {
        "nfe_ui.xml": nota([(existente, "ZZ Roteiro UI Item Existente", "UN", 5, 50.00),
                            (gtin(2), "ZZ Roteiro UI Caixa Nova", "CX", 2, 120.00)], 1),
        "nfe_gtin_invalido.xml": nota([(invalido, "ZZ Roteiro UI Codigo Errado", "UN", 3, 30.00)], 2),
        "nao_e_nfe.xml": '<?xml version="1.0" encoding="UTF-8"?><pedido><numero>1</numero></pedido>',
    }
    for nome, conteudo in arquivos.items():
        with open(os.path.join(args.pasta, nome), "w", encoding="utf-8") as f:
            f.write(conteudo)
        print(os.path.join(args.pasta, nome))
    print("GTIN do item existente:", existente)


if __name__ == "__main__":
    main()
