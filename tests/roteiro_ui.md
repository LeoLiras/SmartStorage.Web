# Roteiro de testes de tela — features de set/2026

Complementa as duas camadas automáticas (`dotnet test` e `tests/ledger_e2e.py`). O roteiro e2e bate na API e não vê o que a tela monta e envia; o bUnit vê a tela, mas contra uma API falsa. Este roteiro passa pelas duas pontas de uma vez: navegador → Blazor → gateway → API → banco.

**Pré-condições:** stack de pé (`docker compose up -d --build`), Blazor em `http://localhost:5000`, login `admin` / `admin123`. Os produtos criados aqui usam o prefixo `ZZ Roteiro UI` e volume 0,01 L, para não esgotar as prateleiras. Rodar **depois** do `ledger_e2e.py`, não junto: o roteiro e2e confere as movimentações gravadas depois de cada marca e veria as desta sessão.

Os XMLs de NF-e saem do gerador em `tests/gera_nfe_ui.py`, com GTINs válidos e chave nova a cada execução.

| Coluna | Significado |
|---|---|
| Passos | o que fazer na tela |
| Esperado na tela | o que o usuário deve ver |
| Esperado no banco | o que conferir em `ProductStockMovement` / `Sale` / `Enter` |

## 1. Importação de NF-e (#25)

| ID | Passos | Esperado na tela | Esperado no banco |
|---|---|---|---|
| UI-01 | Produtos em estoque → **Importar NF-e** → XML com 1 item cujo GTIN é de um produto existente e 1 item com GTIN novo | Prévia com número/série, fornecedor, CNPJ, emissão, "1 existente(s) · 1 novo(s)"; item existente com "Encontrado pelo código de barras"; item novo com formulário de cadastro | nada gravado ainda |
| UI-02 | Item em `CX` com `qCom` 2: fator 12 | Quantidade sugerida 24; custo unitário = `vProd ÷ 24` | — |
| UI-03 | Preencher nome, descrição, volume e preço inicial do novo → **Importar nota** | Sucesso e volta para Produtos em estoque; o novo aparece com o saldo da nota | 1 `Entrada` no depósito por item, mesmo `PsmDate`, motivo `NF-e {n}/{série} de {fornecedor}`; `ProCusto` = custo médio; `Invoice` com os itens |
| UI-04 | Abrir de novo o mesmo XML | Alerta "Esta nota já foi importada em …" e **Importar nota** desabilitado | nada novo |
| UI-05 | XML com `cEAN` de dígito verificador errado | Aviso "O código … não é um GTIN válido e foi ignorado"; item cai em "Cadastrar novo produto" | — |
| UI-06 | Arquivo XML que não é NF-e | Diálogo de erro com a mensagem da API, tela continua na escolha de arquivo | nada gravado |
| UI-07 | Item novo com nome de 3 letras | Erro no campo e **Importar nota** desabilitado | — |

## 2. Alocação em lote (#20) e responsável (#25)

| ID | Passos | Esperado na tela | Esperado no banco |
|---|---|---|---|
| UI-08 | Produtos em estoque → marcar 2 produtos → **Alocar selecionados (2)** | Tela de lote com cada produto em quantidade 1, preço = preço inicial, responsável atual carregado | — |
| UI-09 | Escolher a mesma prateleira para os dois, quantidade 3 e 2 → alocar | Painel "Prateleiras do lote" soma o volume; sucesso | 2 `Alocacao` por item (−depósito, +prateleira), mesmo `PsmDate`; `EntPrice` = preço informado |
| UI-10 | Quantidade acima do depósito | Erro "Há N no depósito" no campo | — |
| UI-11 | Produto que já tem saldo na prateleira X, escolher Y | Alerta "O produto já está na X…"; se enviar, a API recusa com `Item N (…)` e nada é gravado | nada gravado |
| UI-12 | Alocação individual trocando o responsável | Responsável novo aparece na edição do produto | `ProEmpId` alterado |

## 3. Carrinho de vendas (#21) e venda individual

| ID | Passos | Esperado na tela | Esperado no banco |
|---|---|---|---|
| UI-13 | Produtos nas prateleiras → **Adicionar ao carrinho** em 2 produtos → Carrinho | Itens com preço da prateleira, "Disponível: N", subtotal e total geral | — |
| UI-14 | Recarregar a página (F5) | Carrinho continua com os mesmos itens | — |
| UI-15 | Quantidade acima do disponível | Erro no campo, alerta "Há item acima do saldo…", **Registrar vendas** desabilitado | — |
| UI-16 | Quantidades válidas → **Registrar vendas** | Sucesso, carrinho vazio, vendas listadas em Vendas com preço e total | 1 `Sale` por item com `SalPrice` = `EntPrice`; `Venda` negativa na prateleira, mesmo `PsmDate` no relógio do servidor, `PsmUseId` = admin |
| UI-17 | Pôr no carrinho um item; em outra aba vender o saldo inteiro dele; voltar e registrar | Recusa com `Item N (…): motivo`, carrinho mantido e disponível atualizado | nenhuma venda do carrinho gravada |
| UI-18 | Sair, entrar como `usuario` | Carrinho do admin não aparece | — |
| UI-19 | Venda individual (ícone **Vender**) com data de hoje | Venda listada com a data escolhida | `PsmDate` ≈ agora no servidor (não 3 h antes/depois) |

## 4. Devolução, cancelamento e transferência

| ID | Passos | Esperado na tela | Esperado no banco |
|---|---|---|---|
| UI-20 | Vendas → **Registrar devolução** parcial | Listagem mostra quantidade e total líquidos | `Devolucao` positiva no **depósito**, motivo `Devolução da venda {id}` |
| UI-21 | Tentar cancelar a venda com devolução | Mensagem de recusa da API | nada gravado |
| UI-22 | Devolução do restante | Venda some da listagem | `SalReturnedQntd = SalQntd` |
| UI-23 | Produtos nas prateleiras → **Transferir** para outra prateleira | Saldo inteiro aparece no destino | 2 `Transferencia` com o mesmo instante; origem zerada |

## 5. Inventário cíclico (#12)

| ID | Passos | Esperado na tela | Esperado no banco |
|---|---|---|---|
| UI-24 | **Inventário** → escolher prateleira → mudar a contagem de 1 produto, motivo "Contagem de teste" → registrar | Sucesso | 1 `Ajuste` só para a entrada alterada, motivo `Inventário da {prateleira}: Contagem de teste` |
| UI-25 | Registrar sem mudar nenhuma contagem | Botão desabilitado ou recusa "Nenhuma quantidade contada difere…" | nada gravado |

## 6. Listagens paginadas e capacidade

| ID | Passos | Esperado na tela | Esperado no banco |
|---|---|---|---|
| UI-26 | Vendas, Produtos e Produtos nas prateleiras: próxima página e pesquisa `ZZ Roteiro UI` | Pesquisa volta à página 1 e filtra no servidor; total da paginação bate com o banco | — |
| UI-27 | Alocação individual: prateleira escolhida mostra ocupação, livre e volume necessário; quantidade acima do teto de 90% | Recusa da API com a mensagem de capacidade | nada gravado |

## Execução de 22/10/2026

Stack buildado da `develop` (058b56a). `ledger_e2e.py`: 48/48. `dotnet test`: 21/21 na API; no bUnit, 117/117 em 11 de 16 rodadas. As 5 falhas foram todas com a máquina carregada (build Docker ou e2e rodando junto), e a falha examinada foi em `ListagemDeVendasTests.Pesquisa_vai_para_a_api_a_partir_da_primeira_pagina`: o `WaitForAssertion` usa o timeout padrão de 1 s contra o debounce de 400 ms da pesquisa. Os testes equivalentes de produtos e de prateleiras seguem o mesmo padrão.

Na tela, passaram UI-01 a UI-25, além de um caso extra: o carrinho com item cuja entrada foi zerada por transferência mostra "Há 0 na prateleira" e bloqueia o envio. UI-26 e UI-27 não foram exercitados na tela (cobertos pelo bUnit e pelos CT-31 a CT-35).

Achados:

1. **Vírgula decimal é descartada nos campos `decimal`.** Digitar `0,01` no volume grava `1`; `15,90` no preço grava `1590.00`, sem erro nem aviso. Com ponto funciona. Afeta Volume e Preço inicial (`ProductForm`, `ImportInvoice`) e Preço (`AllocateProductToShelf`, `AllocateProductsBatch`). Os `MudNumericField` usam a cultura invariante, enquanto os textos montados à mão usam `PtBr` — a mesma tela mostra `0.01` no campo e `0,01 L` no texto ao lado. Os `MudDatePicker` também mostram `10/22/2026`.
2. Cosmético: no carrinho e no lote, quantidade acima do saldo digitada na hora não mostra o `ErrorText` no campo (o alerta do resumo aparece e o botão bloqueia); depois de uma recusa da API o texto aparece.
3. Cosmético: a paginação das tabelas está em inglês (`1-3 of 3`).
4. Observação: `SalDateSale` leva a hora em que a tela de venda/carrinho foi aberta, não a do envio (`dateSale = DateTime.Now` na inicialização). O `PsmDate` do ledger está certo.

Corrigidos em seguida: o achado 1 (`InputCulture`), o timeout dos testes de pesquisa (5 s, como os demais) e o achado 4 (`SaleDate.At`). Conferido no navegador: `0,02` e `8,50` gravados como 0,02 L e R$ 8,50, e a venda aberta às 16:42:13 e enviada às 16:43:04 saiu com `SalDateSale` 16:43:04.

## Encerramento

Conferir no banco: nenhum saldo negativo, nenhum `Enter` sem produto, `saldo == SUM(PsmQntd)` por produto e local para os produtos `ZZ Roteiro UI`. Depois remover os produtos do roteiro (mesma limpeza do `ledger_e2e.py`).
