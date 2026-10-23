# Roteiro de testes manuais — SmartStorage

Checklist para testar pela tela as features de set/2026 (NF-e, alocação em lote, carrinho, devolução, transferência, inventário) e as correções de 22/10/2026 (vírgula decimal e hora da venda). Começa do zero e cria os próprios dados. Complementa o `roteiro_ui.md`, que é a versão com o resultado esperado no banco.

**Antes de começar**
- Stack de pé (`docker compose up -d`). Abra `http://localhost:5000` e entre com `admin` / `admin123`.
- Gere as notas fiscais de teste:
  ```
  python tests/gera_nfe_ui.py C:\temp\nfe1
  ```
  O script cria `nfe_ui.xml`, `nfe_gtin_invalido.xml` e `nao_e_nfe.xml`, e imprime o GTIN do item 1. **Anote esse GTIN.**
- Os produtos saem da nota: **não** use "Adicionar produto", porque ele manda e-mail real.

## 1. Importação de NF-e

**Aprovado em 23/10/2026.**

- [x] **1.1** Abra Produtos em estoque → **Importar NF-e** → escolha `C:\temp\nfe1\nfe_ui.xml`.
  **Esperado:** número 1/1, fornecedor "ZZ Roteiro UI Fornecedor", "0 existente(s) · 2 novo(s)".
- [x] **1.2** No item 2 (CX, 2 caixas), mude o **Fator de conversão** para 12.
  **Esperado:** quantidade de entrada vira 24 e o custo unitário vira 5,0000.
- [x] **1.3** Item 1: nome "ZZ Roteiro UI Produto A", volume `0,01`, preço inicial `15,90`. Item 2: nome "ZZ Roteiro UI Produto B", volume `0,01`.
  **Esperado:** os campos mostram `0,01` e `15,90`, sem erro.
- [x] **1.4** Apague o nome do item 2 e digite só `ABC`.
  **Esperado:** campo vermelho ("Entre 5 e 100 caracteres") e **Importar nota** desabilitado. Depois volte o nome.
- [x] **1.5** Clique em **Importar nota**.
  **Esperado:** "2 item(ns), 2 produto(s) novo(s)". Em Produtos em estoque, A aparece com 5 e B com 24.
- [x] **1.6** Gere a segunda nota com o GTIN anotado e importe:
  ```
  python tests/gera_nfe_ui.py C:\temp\nfe2 --gtin-existente <GTIN anotado>
  ```
  **Esperado:** item 1 já ligado ao Produto A, com "Encontrado pelo código de barras".
- [x] **1.7** No item 2, escolha **Produto B** no seletor e clique em importar.
  **Esperado:** erro "o produto … Produto B tem o código …, e a nota traz …" e nada gravado.
- [x] **1.8** Volte o item 2 para "Cadastrar novo produto", dê o nome "ZZ Roteiro UI Produto C" e volume `0,01`, e importe.
  **Esperado:** sucesso. A passa a ter 10 em estoque.
- [x] **1.9** Abra de novo `C:\temp\nfe1\nfe_ui.xml`.
  **Esperado:** alerta vermelho "Esta nota já foi importada em …" e botão desabilitado.
- [x] **1.10** Abra `nfe_gtin_invalido.xml`.
  **Esperado:** aviso amarelo "O código … não é um GTIN válido e foi ignorado". **Não importe.**
- [x] **1.11** Abra `nao_e_nfe.xml`.
  **Esperado:** erro "O arquivo não é uma NF-e" e volta à escolha de arquivo.

## 2. Campos decimais e datas (correção de 22/10)

**Aprovado em 23/10/2026.**

- [x] **2.1** Edite o Produto B. Volume `0,02`, preço inicial `8,50` → **Atualizar** → reabra o produto.
  **Esperado:** mostra `0,02` e `8,50`, e não 2 e 850.
- [x] **2.2** No mesmo campo de volume, digite `0.02` (com ponto).
  **Esperado:** campo vermelho; o valor não vira 2. Saia sem salvar.
- [x] **2.3** Repare na data de registro.
  **Esperado:** formato dia/mês/ano (`22/10/2026`).

## 3. Alocação em lote

**Aprovado em 23/10/2026.**

- [x] **3.1** Em Produtos em estoque, pesquise "ZZ Roteiro UI", marque A, B e C e clique em **Alocar selecionados (3)**.
  **Esperado:** cada produto com quantidade 1; A já com preço 15,90 ("Preço inicial do produto"); responsável Admin.
- [x] **3.2** No Produto A, digite quantidade 11.
  **Esperado:** campo vermelho (o depósito tem 10).
- [x] **3.3** Preencha A = 3 a `15,90`, B = 4 a `9,90` e C = 2 a `7,50`, todos na **Prateleira B2**. No C, troque o responsável para Bruno.
  **Esperado:** o painel "Prateleiras do lote" mostra B2 com 0,09 L.
- [x] **3.4** Clique em **Alocar 3 produto(s)**.
  **Esperado:** sucesso. Em Produtos nas prateleiras os três aparecem na B2 com esses preços. Ao editar o C, o responsável é Bruno.
- [x] **3.5** Selecione só o A de novo, alocação em lote, e troque a prateleira para a C1.
  **Esperado:** alerta vermelho "O produto já está na Prateleira B2…". Clique em alocar: a API recusa com "Item 1 (…)" e nada é gravado.

## 4. Carrinho

**Aprovado em 23/10/2026.** No 4.4, a primeira tentativa reabriu o carrinho depois da venda na outra aba, e ele bloqueou o envio com o saldo novo; refeito com o carrinho aberto, a API recusou como esperado. Por isso o Produto A teve uma venda avulsa a mais, e o 4.5 registrou A = 1.

- [x] **4.1** Em Produtos nas prateleiras, clique no ícone de carrinho de A e de B.
  **Esperado:** badge 2 no topo e um aviso a cada item.
- [x] **4.2** Abra o carrinho e aperte F5.
  **Esperado:** os itens continuam; o total mostra R$ 25,80; a data aparece como dia/mês/ano.
- [x] **4.3** Mude a quantidade de A para 5.
  **Esperado:** alerta "Há item acima do saldo…" e **Registrar vendas** desabilitado.
- [x] **4.4** Ponha A = 3 e B = 2. Em **outra aba**, venda 1 unidade de A pelo ícone **Vender**. Volte à aba do carrinho e clique em **Registrar vendas**.
  **Esperado:** "Nenhuma venda foi registrada. Item 1 … há 2 e o carrinho pede 3"; o carrinho é mantido e o campo mostra "Há 2 na prateleira".
- [x] **4.5** Mude A para 2 e registre.
  **Esperado:** "2 venda(s) registrada(s)", carrinho vazio e as vendas aparecem em Vendas com os preços 15,90 e 9,90.
- [x] **4.6** Ponha C no carrinho. Em Conta → **Sair**, entre como `usuario` / `usuario123` e abra o carrinho.
  **Esperado:** vazio. Saia, volte como `admin`: o C continua no carrinho.

## 5. Venda individual e hora gravada (correção de 22/10)

- [ ] **5.1** Anote a hora, clique em **Vender** no Produto B, espere 1 a 2 minutos com a tela aberta e venda 1 unidade.
  **Esperado:** em Vendas, a hora da venda é a do clique em Vender, não a da abertura da tela.

## 6. Devolução e cancelamento

- [ ] **6.1** Em Vendas, na venda de 2 unidades do Produto B, clique em **Registrar devolução** e devolva 1.
  **Esperado:** "O saldo voltou para o depósito"; a listagem mostra 1 unidade e R$ 9,90.
- [ ] **6.2** Clique em **Cancelar venda** nessa mesma venda e confirme.
  **Esperado:** erro "Não é possível cancelar a venda pois ela já tem devoluções registradas".
- [ ] **6.3** Devolva a unidade restante.
  **Esperado:** a venda some da listagem.

## 7. Transferência

- [ ] **7.1** Em Produtos nas prateleiras, no Produto C, clique em **Transferir** e escolha a Prateleira C1.
  **Esperado:** "2 unidade(s) transferida(s)"; C aparece na C1 com preço 7,50.
- [ ] **7.2** Abra o carrinho (o C ainda está lá, apontando para a B2).
  **Esperado:** "Há 0 na prateleira" e botão bloqueado. Clique em **Limpar carrinho** e confirme.

## 8. Inventário

- [ ] **8.1** Em Produtos nas prateleiras → **Inventário**, escolha a C1.
  **Esperado:** C listado com 2 no sistema e **Registrar inventário** desabilitado (nenhuma diferença).
- [ ] **8.2** Mude a contagem para 1 e escreva o motivo "Contagem de teste" → registrar.
  **Esperado:** "1 ajuste(s)"; C passa a ter 1 na C1.

## 9. Listagens

- [ ] **9.1** Em Vendas, Produtos em estoque e Produtos nas prateleiras: vá à página 2 e pesquise "ZZ Roteiro UI".
  **Esperado:** volta para a página 1 e mostra só os produtos do teste.

## Encerramento

Os produtos `ZZ Roteiro UI` saem direto do banco (a aplicação não exclui produto), na ordem: itens e notas da NF-e, vendas, movimentações, entradas e produtos.
