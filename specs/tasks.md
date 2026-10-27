# Tarefas

As tarefas de cada issue, tiradas do `specs/plan.md`. As concluídas foram
reconstruídas do histórico de commits e dos comentários de fechamento das
issues; as abertas seguem a "Ordem de implementação" do plano.

## Como ler
- **ID**: `T-<issue>.<nn>`. `T-00` agrupa o que foi feito sem issue. Tarefa
  nova recebe o próximo número livre da sua issue; nenhuma é renumerada, e a
  que for abandonada deixa o número sem uso
- **Origem**: o requisito (`RF-xx`, `RN-xx`), a decisão do plano (`D-xx`) ou
  a constituição que a tarefa atende
- **Depende de**: as tarefas que precisam estar prontas antes
- **Concluída quando**: o critério de aceite. O teste que a constituição
  exige faz parte dele: regra nova tem teste de unidade ou caso no roteiro,
  regra do ledger tem caso no roteiro com o caminho feliz e ao menos uma
  recusa, tela nova ou alterada tem teste bUnit do envio e do erro da API e é
  conferida no navegador em largura de desktop e de celular. Em toda tarefa
  vale também que `dotnet test` e o roteiro inteiro continuam passando
- **Commit**: preenchido quando a tarefa termina; vazio é tarefa aberta
- O commit que conclui a última tarefa de um requisito troca, na `spec.md`,
  o `[planejado #N]` dele por `[feito]`
- Casos novos do roteiro recebem o próximo `CT-xx` livre (o último é o
  CT-48) quando forem escritos

## Concluídas

### Sem issue (T-00)
O trabalho anterior às issues está agrupado em blocos, um por frente.

| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-00.01 | API core com produtos, prateleiras, entradas, vendas e colaboradores em `Controller → Business`, com VOs, versionamento e HATEOAS | RF-05, RF-08a, RF-14, RF-15, RF-17, RF-21, RF-25 | | As rotas respondem pelo Swagger | `87ffe57..89f173a` |
| T-00.02 | Trocar PostgreSQL por SQL Server e separar `SmartStorage.Infraestructure` e `SmartStorage.Shared` | Constituição (Stack) | T-00.01 | A solution compila com o contexto e as migrations na Infraestructure | `e1ed400`, `9c79aad` |
| T-00.03 | Blazor com MudBlazor: layout, produtos em cards, alocação, vendas e desfazer alocação | RF-05 a RF-07, RF-15, RF-17, RF-21, RF-24, RF-25 | T-00.02 | As telas operam o fluxo de produto, alocação e venda pela API | `043b18f..c832ad8` |
| T-00.04 | Insights com gráficos e chat do Gemini | RF-30 | T-00.03 | O chat responde sobre as vendas recentes | `d203bc8..93d8ec8` |
| T-00.05 | Relatórios do mês em Excel e PDF | RF-29 | T-00.03 | Os dois arquivos saem com as vendas do mês e os gráficos | `ab798a1..1046ef7` |
| T-00.06 | Migrar para .NET 10 | Constituição (Stack) | | A solution compila em `net10.0`, exceto o MessageBus | `07281f4`, `7488b79` |
| T-00.07 | Autenticação JWT com login, cadastro, conta, edição e exclusão de usuário e papéis | RF-01, RF-03, RF-04 | T-00.03 | Login devolve token, e as telas de usuário respeitam o papel | `f1247fc..7b3b9aa` |
| T-00.08 | Separar AIAPI, ReportsAPI, EmailAPI e AuthenticationAPI, com a configuração comum em `SmartStorage.Configurations` | Constituição (Arquitetura) | T-00.07 | Cada serviço sobe sozinho e valida o mesmo token | `5ae251d..a155d0a` |
| T-00.09 | Mensageria: MessageBus, e-mail de produto novo pelo RabbitMQ e consumidor na EmailAPI | RF-08 | T-00.08 | Criar produto publica na fila e o consumidor envia o e-mail | `515e8d1..d5f4939` |
| T-00.10 | Gateway Ocelot como única porta do Blazor | Constituição (Arquitetura) | T-00.08 | As `ServiceUrls` do Blazor apontam todas para o gateway | `e080deb..67dc2e0`, `d7ff4cd`, `cadbb90` |
| T-00.11 | Dockerizar os serviços, aplicar migrations pelo `migrator` e pôr healthchecks | Constituição (Stack), D-13 | T-00.10 | `docker compose up -d` sobe o stack inteiro, com os serviços esperando o `migrator` | `ec00568..13ca48c`, `213adb4`, `83ae915`, `6f1be73`, `6986d6a` |
| T-00.12 | Seed por `HasData` com administrador, usuário comum e estoque de exemplo | D-13 | T-00.11 | O `migrator` aplica o seed, e `admin` e `usuario` entram | `751173d`, `e9f9252`, `29de47a` |
| T-00.13 | Segredos fora do repositório: JWT pela configuração e user secrets | Constituição (Segurança) | T-00.11 | Serviço sem `TokenConfigurations:Secret` não sobe | `3b33a41`, `1bdc5fd`, `ca83fb2` |
| T-00.14 | Alinhar as constantes de papel com a claim do login | RF-04 | T-00.07 | `[Authorize(Roles = Role.Admin)]` aceita o administrador do seed | `31e66b9` |
| T-00.15 | Remetente corrigido e destinatário do e-mail configurável | RF-08 | T-00.09 | O e-mail sai para `Email:Destinatario` | `d5592cb` |
| T-00.16 | CI com build, testes e publicação das imagens | Constituição (Testes) | T-00.11 | Push publica as imagens com a tag do SHA; PR não publica | `0452e38`, `39b095e` |
| T-00.17 | Guia do repositório e README | Constituição (Governança) | | `CLAUDE.md` e README descrevem o stack e as telas | `634ffb8`, `1068956`, `f31fc30`, `c6050c4`, `7511257` |
| T-00.18 | Corrigir a devolução de estoque ao reduzir uma venda | RF-25 | | Reduzir a venda devolve a diferença à prateleira | `f209077` |
| T-00.19 | Roteiro fim a fim do ledger | Constituição (Testes) | T-06.03 | `python tests/ledger_e2e.py` passa e confere linhas, invariante, `PsmDate` e `PsmUseId` | `2a034f6`, `e7eea3c` |
| T-00.20 | Testes de componente com bUnit, no CI | Constituição (Testes) | T-00.16 | `dotnet test` roda o bUnit local e no CI | `19b8102`, `a27e7a8`, `fdb7d45` |
| T-00.21 | Corrigir o produto não preenchido na venda pela prateleira e validar a entrada, e não o produto | RF-21 | T-06.05 | A venda pela prateleira grava a entrada certa | `71d79c3`, `8285b97` |
| T-00.22 | Tela de carregamento ao desfazer alocação | RF-17 | | A tela mostra o carregamento enquanto a alocação é desfeita | `5749534` |
| T-00.23 | Token vencido leva ao login | RF-02 | T-00.20 | `SessaoExpiradaTests` passa | `31a0f78`, `a8160f4` |
| T-00.24 | Um produto em uma prateleira por vez | RN-10 | T-11.01 | CT-36 passa | `05d3f5e`, `c212010`, `8eba6cf` |
| T-00.25 | Prateleira de destino da transferência sem o 0 | RF-18 | T-07.07 | O campo abre vazio | `ef92031` |
| T-00.26 | Mover os projetos para `src` e renomear a pasta da API core | Constituição (Pastas) | | Build da solution e `docker compose build` com `context: ./src` passam | `a001bde` |
| T-00.27 | Telas de tabela no celular | RF-55 | | As tabelas cabem na largura do celular | `e76391d` |
| T-00.28 | Constituição, spec, revisões e plano do SDD | Constituição (Governança) | | `specs/` com constituição, spec e plano, e as issues #30 a #40 ligadas à spec | `02071b6..b124e4d` |
| T-00.29 | Criar a `develop` a partir da `main` | Constituição (GitFlow) | | `develop` existe no GitHub, criada da ponta da `main`, e a `claude-studies` já integrada saiu do remoto | `2240e86` (ponta) |

### #1 Portas dos microsserviços
Fechada como já atendida pela T-00.10 e pela T-00.11: no Docker todo serviço
escuta 8080 e só o gateway e o Blazor ficam expostos.

### #2 Configuração do banco (T-02)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-02.01 | `AddDatabase` em `SmartStorage.Configurations`, usado pelos cinco serviços | Constituição (Arquitetura) | T-00.08 | Os cinco serviços leem `ConnectionStrings:SqlServerConnection` pelo mesmo método | `464fc0b`, `62544e0`, `503a4ac`, `c7d0e71` |

### #3 Autenticação em todos os serviços (T-03)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-03.01 | `[Authorize]` em todo o `AuthController`, com `create`, `update-credentials` e `DELETE` só para administrador | RF-02, RF-03 | T-00.14 | Pelo gateway: anônimo recebe 401, usuário comum 403 no cadastro e na exclusão, administrador 200 | `490d0be` |

### #4 HyperMediaFilter nos serviços
Fechada como não planejada: ninguém consome os links, e os outros serviços não
têm recurso para linkar.

### #6 Ledger de movimentações (T-06)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-06.01 | Tabela `ProductStockMovement`, prefixo `Psm` | RN-01, D-01 | | O `migrator` aplica, e `has-pending-model-changes` não acusa nada | `6baf0f7` |
| T-06.02 | Repartir o seed entre depósito e prateleira, com movimentos de abertura | D-13 | T-06.01 | O saldo do seed é `SUM(PsmQntd)` de cada local | `7c963d8` |
| T-06.03 | `ProductStockMovementRepository`: grava a linha e o saldo materializado juntos e recusa saldo negativo e quantidade zero | RN-01, RN-02, RF-27, D-01 | T-06.01 | Nenhum outro código escreve `ProQntd` nem `EntQntd` | `62611c6` |
| T-06.04 | Alocação e desfazer pelo ledger | RF-15, RF-17, RN-14 | T-06.03 | CT-08 a CT-12 passam | `3f113d3` |
| T-06.05 | Vendas, edição e cancelamento pelo ledger | RF-21, RF-25 | T-06.03 | CT-13 a CT-18 passam | `3b03f89` |
| T-06.06 | Correção do saldo do depósito como ação própria, com motivo, lançada como `Ajuste` | RF-07, RN-05 | T-06.03 | CT-03 a CT-07 passam | `f0dc5e1`, `e370836` |
| T-06.07 | Instante de toda movimentação pelo relógio do servidor | RN-03, RF-27, D-02 | T-06.03 | O roteiro confere o `PsmDate` | `7afc710` |
| T-06.08 | Autor de toda movimentação pelo token | RN-03, RF-27, D-02 | T-06.03 | O roteiro confere o `PsmUseId` | `4b008a9`, `0da66ca`, `6b206f0` |
| T-06.09 | Recusar a exclusão de usuário com movimentações | RN-04 | T-06.08 | A exclusão responde erro e o usuário continua no banco | `7c5548d`, `6e67ad9` |
| T-06.10 | Edição e ajuste de estoque no mesmo envio do produto | RF-07 | T-06.06 | CT-21 e CT-22 e o bUnit do envio único passam | `338bdc4`, `ae0b227`, `9d4d1c4` |
| T-06.11 | Saldo total do produto na API e no Blazor | RF-06 | T-06.03 | CT-23 e o bUnit da listagem passam | `0a89ae6`, `144ca57` |
| T-06.12 | Remover a exclusão de produto | RN-04 | T-06.03 | CT-19 passa, e não há rota de exclusão no gateway | `0afbf38`, `70cb09a` |

### #7 Movimentações além da venda (T-07)
A perda ficou sem tela nem endpoint: é registrada pelo ajuste, com o motivo.

| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-07.01 | Tipo `Devolucao` e `Sale.SalReturnedQntd` | RF-26 | T-06.03 | O `migrator` aplica a coluna | `72e7f74` |
| T-07.02 | `POST /sales/{id}/return`, que volta ao depósito, com rota no gateway | RF-26, RN-19 | T-07.01 | CT-24 a CT-26 passam | `2886a57`, `dfcb2c1` |
| T-07.03 | Recusar cancelar ou reduzir venda que tem devolução | RN-20 | T-07.02 | CT-27 passa | `b4b27b8` |
| T-07.04 | Tela de devolução de venda | RF-26 | T-07.02 | bUnit da devolução passa | `7d105b9`, `f944c5a`, `fc276b5` |
| T-07.05 | Tipo `Transferencia` no ledger | RF-18 | T-06.03 | O enum tem o tipo | `79b9a9e` |
| T-07.06 | `POST /shelf/allocation/{enterId}/transfer`, com dois lançamentos e o mesmo instante, e rota no gateway | RF-18, RN-11, D-06 | T-07.05 | CT-20, CT-28 e CT-29 passam | `4d36a5d`, `0bbe9ea` |
| T-07.07 | Tela de transferência | RF-18 | T-07.06 | bUnit da transferência passa | `8ccb419`, `83d63a2` |
| T-07.08 | Documentar a devolução e a transferência | RF-18, RF-26, RN-11, RN-19 | T-07.04, T-07.07 | O `CLAUDE.md` descreve as duas regras | `24ff488`, `2efbe26` |

### #8 Alerta de estoque mínimo (T-08)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-08.01 | `Product.ProMinimumStock`, com zero desligando o alerta | RF-28 | | Editável no cadastro e na edição do produto | `1aee034` |
| T-08.02 | Publicar o alerta na `lowstockemailqueue` quando o saldo total cruza o mínimo, depois do commit, e só logar a falha | RN-28, RN-29, D-08 | T-08.01 | CT-34 passa | `be9611d`, `1bfb24b` |
| T-08.03 | Consumidor da fila na EmailAPI | RF-28 | T-08.02 | O CT-34 acha o envio nos logs da `emailapi` | `b0da778` |
| T-08.04 | Documentar o alerta | RF-28, RN-28 | T-08.03 | O `CLAUDE.md` descreve a fila e a regra de cruzar o limite | `429530d` |

### #9 Preço na venda (T-09)
Sem FIFO nem margem, como decidido no fechamento.

| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-09.01 | `Sale.SalPrice` copiado do `EntPrice` na criação e nunca alterado; a migration `AddSalePrice` preenche as vendas antigas | RN-15, D-05 | T-06.05 | CT-30 passa | `d915a1a`, `2e1a3d9` |
| T-09.02 | Relatórios pelo preço gravado | RN-15 | T-09.01 | Excel e PDF somam `SalPrice` | `3bd33aa` |
| T-09.03 | Documentar o preço gravado | RN-15 | T-09.02 | O `CLAUDE.md` descreve a regra | `d8e373f` |

### #10 Busca e paginação (T-10)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-10.01 | Converters de lista carregando nomes em lote | D-11 | | A listagem de vendas cai de 61 para 2 consultas | `5cfe610`, `b7d4dfb` |
| T-10.02 | Reduzir a imagem do produto no upload | RF-05 | | A imagem sai com no máximo 800 px | `082577c` |
| T-10.03 | Vendas paginadas e pesquisáveis na API e na tela | RF-24, D-10 | T-10.01 | CT-31 e o bUnit da listagem passam | `55667f1`, `ebfe88c`, `8301d74`, `c5c0b9f` |
| T-10.04 | Produtos nas prateleiras paginados e pesquisáveis | RF-20, D-10 | T-10.01 | CT-32 e o bUnit passam | `87e4ab3`, `051793c`, `3646a90`, `3a652d7` |
| T-10.05 | Produtos paginados e pesquisáveis | RF-06, D-10 | | CT-33 passa | `562cd79`, `2ca8b21`, `611fa05` |
| T-10.06 | Documentar a paginação opcional | RF-06, RF-20, RF-24, D-10 | T-10.05 | O `CLAUDE.md` descreve `page`, `pageSize`, `search` e `X-Total-Count` | `d439f2f`, `ff11e6d`, `c52aeae` |

### #11 Capacidade de prateleira (T-11)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-11.01 | Volume em `Product` e `Shelf`, teto de 90% e `EnsureShelfFits` na alocação e na transferência | RN-08, RN-09, RN-12, D-07 | T-06.04 | CT-35 passa | `c94f659`, `91378f6` |
| T-11.02 | Volume, ocupação e espaço livre na tela de alocação | RF-14 | T-11.01 | bUnit da alocação passa | `033caa8` |
| T-11.03 | Documentar a capacidade | RN-08, RN-09 | T-11.02 | O `CLAUDE.md` descreve o teto e o cálculo do volume usado | `00f24ee` |

### #12 Inventário cíclico (T-12)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-12.01 | `POST /shelf/v1/{shelfId}/inventory`, tudo ou nada, com um `Ajuste` por entrada com diferença | RF-19, RN-05, RN-06, RN-13 | T-06.03 | CT-43 e CT-44 passam | `deb8e61`, `7e6f0a1` |
| T-12.02 | Tela de inventário | RF-19 | T-12.01 | `InventarioDePrateleiraTests` passa, e a tela foi conferida no Chrome | `c87abd2`, `2e1022c` |
| T-12.03 | Documentar o inventário | RF-19 | T-12.02 | O `CLAUDE.md` descreve a regra | `641916f` |

### #14 Documentar AIAPI e ReportsAPI (T-14)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-14.01 | Seção "AIAPI e ReportsAPI" no `CLAUDE.md` | RF-29, RF-30 | | Endpoints, leitura do contexto, chave do Gemini e limitações descritos | `a46da95`, `b3947b6` |

### #15 Relatório do mês com vendas de outros anos (T-15)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-15.01 | Filtrar o relatório do mês também pelo ano | RN-30 | | Com vendas de 09/2025 e 09/2026, o Excel de 09/2026 traz só a de 2026 | `3afae3b`, `bcb42ca` |

### #16 Relatórios sem desconto de devolução (T-16)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-16.01 | Quantidade e total líquidos de devolução nas tabelas e nos gráficos, sem a venda totalmente devolvida | RN-21, RN-30 | T-07.02 | Venda de 5 com 2 devolvidas sai como 3; a totalmente devolvida não aparece | `cf1a512`, `919c5c9` |

### #17 Arquivos exportados sem extensão (T-17)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-17.01 | Extensão `.xlsx` e `.pdf` no nome dos arquivos | RF-29 | | O `Content-Disposition` das duas respostas traz a extensão | `d44dfe3` |

### #18 Erros de relatório e IA no Insights (T-18)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-18.01 | `ReportsService` e `AiService` lançam `ApiException`; o Insights avisa o erro, para o carregamento e não põe o erro da IA no chat | RF-30 | | `InsightsTests` passa, e os 3 testes de erro falham no código anterior | `b0c1dd1`, `ffccee6` |
| T-18.02 | Mês sem vendas responde 400 com aviso | RN-30 | T-18.01 | Sem vendas no mês, os dois exports respondem 400 "Ainda não há vendas no mês corrente." | `45165bd` |
| T-18.03 | Atualizar a AIAPI e a ReportsAPI no `CLAUDE.md` | RF-30, RN-30 | T-18.02 | As limitações resolvidas saem do texto | `7742f4b` |

### #19 Layout moderno (T-19)
Cada diretório só passou ao seguinte com o bUnit verde e a tela conferida no
navegador.

| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-19.01 | `Layout`: paleta escura, tipografia, transição entre telas e menu | RF-54 | | Modo claro e botão de alternar removidos; animações estáticas com `prefers-reduced-motion` | `e5c2997` |
| T-19.02 | `Loading` em esqueleto e visual do diálogo | RF-54 | T-19.01 | O gif sai de todas as telas | `5803b78` |
| T-19.03 | Telas de produto | RF-54, RF-55 | T-19.02 | Conferidas em desktop e celular | `b81a42a` |
| T-19.04 | Telas de venda | RF-54, RF-55 | T-19.02 | Conferidas em desktop e celular | `6f9b796` |
| T-19.05 | Telas de alocação | RF-54, RF-55 | T-19.02 | Conferidas em desktop e celular | `a19daf7` |
| T-19.06 | Telas de autenticação | RF-54, RF-55 | T-19.02 | Conferidas em desktop e celular | `ac519c5` |
| T-19.07 | Insights | RF-54, RF-55 | T-19.02 | Conferida em desktop e celular | `1ae789c` |
| T-19.08 | Home | RF-54, RF-55 | T-19.02 | Conferida em desktop e celular | `f8efeca` |
| T-19.09 | Dados de pt-BR no Blazor e pesquisa das telas | Constituição (Nomes) | T-19.08 | Preços e totais em pt-BR | `10d9ca4` |

### #20 Alocação em lote (T-20)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-20.01 | `Product.ProPrecoInicial` e a migration `AddProductInitialPrice` | RN-16 | | Os produtos existentes recebem o `EntPrice` da prateleira com saldo ou da entrada mais recente | `8b1c8b5` |
| T-20.02 | `POST /shelf/v1/allocation/batch`, tudo ou nada, com capacidade somada por prateleira | RF-16, RN-06, RN-07, RN-09, RN-14, D-04 | T-20.01, T-11.01 | CT-40 a CT-42 passam | `63b2990`, `7551134` |
| T-20.03 | Preço inicial no formulário de produto e como padrão da alocação | RN-16 | T-20.01 | Alocar não muda o preço inicial | `8a31d03` |
| T-20.04 | Seleção nos cards e tela de lote | RF-16 | T-20.02 | 9 testes bUnit novos passam; tela conferida no Chrome | `85e6ae4`, `0a4a04a` |
| T-20.05 | Navegador revalida os arquivos do Blazor; testes de componente esperam o `await` | Constituição (Stack) | T-20.04 | nginx manda `Cache-Control: no-cache`, e o bUnit passa em 30 execuções seguidas | `16e1613`, `d64de92` |
| T-20.06 | Campo de prateleira vazio na alocação individual | RF-15 | T-20.03 | O campo abre vazio para produto sem prateleira | `57aee3f` |
| T-20.07 | Documentar a alocação em lote | RF-16 | T-20.04 | O `CLAUDE.md` descreve a regra | `f7d72fb` |

### #21 Carrinho de vendas (T-21)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-21.01 | `POST /sales/v1/batch`, tudo ou nada, somando itens repetidos da mesma entrada | RF-22, RN-06, RN-07, RN-18, RN-22, D-04 | T-06.05 | CT-37 a CT-39 passam | `857f6a9`, `a78a9dd` |
| T-21.02 | `SaleCart` salvo no `localStorage` por usuário e tela do carrinho | RF-22, RF-23 | T-21.01 | 8 testes bUnit novos passam; recusa conferida no Chrome com o carrinho mantido | `b3601de`, `9e19e33` |
| T-21.03 | Containers no fuso de São Paulo | D-03 | T-21.02 | A venda das 09:00 é gravada como 09:00 | `1a665dc` |
| T-21.04 | Documentar o carrinho | RF-22, RF-23 | T-21.02 | O `CLAUDE.md` descreve a regra | `bba54ae` |

### #22 Serviços tipados no Blazor (T-22)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-22.01 | `ISaleService` no lugar do `ApiExtensions` nas telas de venda | D-12 | | bUnit verde, telas de venda conferidas no navegador | `f8dba04`, `20bc57f`, `910a892`, `f832a48` |
| T-22.02 | `IShelfService` para prateleiras e alocações | D-12 | T-22.01 | bUnit verde, telas de alocação conferidas | `8c6618f`, `a683552`, `2165d12`, `4d92905` |
| T-22.03 | `IProductService` | D-12 | T-22.02 | bUnit verde, telas de produto conferidas | `0645533`, `4560ced`, `2b6328a`, `97a1aff`, `2b21484` |
| T-22.04 | `IEmployeeService` e remoção do `ApiExtensions` | D-12 | T-22.03 | O `ApiExtensions` não existe mais; 101 testes bUnit verdes em 20 execuções | `c3b2484`, `561a756`, `e546b1b`, `dcfaff8`, `0c500e1`, `eeca934` |
| T-22.05 | Remover o `HttpClient` que só o `AuthStateProvider` usava | D-12 | T-22.04 | `SessaoExpiradaTests` passa | `d97ae6e` |

### #25 Importação de NF-e (T-25)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-25.01 | `ProCodigo`, `ProFatorConversao`, `ProCusto` e as tabelas `Invoice` e `InvoiceItem` | RN-23, RN-24 | | O `migrator` aplica, com `UQ_Product_codigo` filtrado e `UQ_Invoice_chave` | `ffdbf0b` |
| T-25.02 | `NfeXmlReader` | RF-09, RN-24, RN-25, RN-27 | | Testes de unidade cobrem cabeçalho, itens, `SEM GTIN`, dígito verificador, modelo 65 e arquivo que não é NF-e | `ffdbf0b`, `d196a9b` |
| T-25.03 | `POST /invoices/v1/preview` e `POST /invoices/v1`, tudo ou nada, com custo médio ponderado | RF-10 a RF-13, RN-17, RN-25a, D-09 | T-25.01, T-25.02 | CT-45 a CT-47 passam, e o teste de unidade cobre `AverageCost` | `ffdbf0b`, `36a1f6a` |
| T-25.04 | Responsável do produto nas duas telas de alocação | RF-15 | T-25.03 | CT-48 e o bUnit passam | `af44a72`, `1d9d807` |
| T-25.05 | Tela de importação | RF-09, RF-11 | T-25.03 | bUnit passa; conferida no Chrome com uma NF-e de exemplo | `af44a72`, `1d9d807` |
| T-25.06 | Colaborador padrão da NF-e achado por consulta pelo nome | RN-26 | T-25.03 | Sem o colaborador "Admin", o item é recusado | `32e5ab3` |
| T-25.07 | Documentar a importação | RF-09 a RF-13 | T-25.05 | O `CLAUDE.md` descreve a regra | `684150c` |

## Abertas
Na ordem de implementação do plano. Cada issue é desenvolvida numa
`feature/<número>-<descrição>` criada a partir de `develop`.

### #37 Transferência leva o preço da origem (T-37)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-37.01 | A transferência passa `shelfPrice: enter.EntPrice` ao ledger, exista ou não entrada do produto no destino | RN-11, D-34 | T-00.29 | Caso novo: alocar na A1, transferir para a A2, repreçar a A2 e voltar para a A1 deixa na A1 o preço da A2, com os dois `Transferencia` no mesmo instante; o CT-28 passa a esperar o preço da origem; o CT-29 continua passando e vale como a recusa que a constituição exige, porque a regra não cria recusa nova | |
| T-37.02 | Atualizar a transferência no `CLAUDE.md` | RN-11 | T-37.01 | O `CLAUDE.md` não diz mais que o destino existente mantém o próprio preço | |

### #38 Alerta no ajuste do depósito (T-38)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-38.01 | `ProductBusinessImplementation` recebe `IStockAlertBusiness` e avalia o alerta no `adjust-stock` e no ajuste do `PUT`, com o saldo total de antes | RF-28, RN-28, D-35 | T-00.29 | Caso novo no padrão do CT-34: ajustar o depósito para abaixo do mínimo gera um aviso nos logs de `smartstorage-api` e `emailapi`, e um segundo ajuste ainda abaixo não gera outro | |
| T-38.02 | Tirar a avaliação do alerta da alocação individual e da em lote | RF-28, D-35 | T-38.01 | `ShelfBusinessImplementation` só avalia o alerta no inventário; CT-34, CT-40 e CT-43 passam | |
| T-38.03 | Atualizar os gatilhos do alerta no `CLAUDE.md` | RF-28 | T-38.02 | O `CLAUDE.md` lista venda, carrinho, aumento de venda, inventário e ajuste, e não a alocação | |

### #41 Valor do item da NF-e na importação (T-41)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-41.01 | Campo "Valor na nota", só leitura, em cada item da prévia, com o `vProd` em pt-BR ao lado do custo unitário | RF-11, D-09 | T-00.29 | bUnit: o item 2 da nota de exemplo mostra 120,00, e o valor não muda com o fator em 12 enquanto o custo unitário vai a 5,0000; tela conferida em desktop e celular | |

### #42 Espaço entre a imagem do produto e os botões (T-42)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-42.01 | Margem entre a moldura da imagem e a linha de botões no `ProductImagePicker` | RF-05 | T-00.29 | Cadastro e edição de produto e produto novo na NF-e conferidos em desktop e celular, com e sem o nome do arquivo: os botões não encostam na moldura | |

### #43 Página de produtos com a linha cheia (T-43)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-43.01 | Colunas da grade de cards por faixa de largura, com contagens que dividem o tamanho da página, e o `pageSize` de Produtos em estoque ajustado a elas | RF-06, D-10 | T-00.29 | bUnit: a tela pede a página com o novo `pageSize`; conferida em celular, tablet, desktop e tela larga, com toda página que não é a última terminando com a linha cheia | |

### #44 Botão Voltar no formulário de produto (T-44)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-44.01 | Botão Voltar no `ProductForm`, com o destino por parâmetro: Produtos em estoque na edição e no cadastro, Produtos nas prateleiras na transferência e Vendas na devolução | RF-05, RF-07, RF-18, RF-26 | T-00.29 | bUnit: na edição, Voltar navega para Produtos em estoque sem enviar o `PUT`, e na transferência e na devolução para a listagem de cada uma sem enviar nada; as quatro telas conferidas em desktop e celular | |

### #45 Campo numérico vermelho sem o motivo (T-45)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-45.01 | O `ErrorText` da quantidade na alocação em lote aparece, como já aparece no carrinho | RF-16 | T-00.29 | bUnit, falhando antes da correção: lote acima do depósito mostra "Há N no depósito"; testes do carrinho ("Há N na prateleira") e da NF-e (quantidade zero mostra o motivo) seguem passando; tela conferida em desktop e celular | |

### #47 Ícone próprio (T-47)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-47.01 | Ícone em SVG nas cores do layout, legível em 16 px | RF-56 | T-00.29 | O SVG está em `wwwroot` e foi aprovado pelo usuário | |
| T-47.02 | O ícone no lugar do `.brand-mark` na barra superior e no login; `favicon.svg`, `favicon.png` de reserva e `apple-touch-icon`; título "Smart Storage"; o `icon-192.png` do template trocado ou removido | RF-56 | T-47.01 | Aba, barra superior e login conferidos em desktop e celular, sem o "@" do template; a imagem do Docker serve o `favicon.svg` como `image/svg+xml` | |

### #48 Campo só leitura distinto do editável (T-48)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-48.01 | Estilo de campo só leitura em `app.css` (classe `app-readonly`, tokens do tema escuro), mostrado num campo editável ao lado de um só leitura | RF-57, D-48 | T-00.29 | Proposta aprovada pelo usuário no tema escuro | |
| T-48.02 | A classe nos campos sempre só leitura (`ProductForm`, alocação individual e em lote, NF-e, transferência, devolução) e nos do `ProductForm`, `EmployeeSelect`, `MudDatePicker` e `ProductImagePicker` quando `ReadOnly`; o Produto da venda individual troca `Disabled` por `ReadOnly` | RF-05, RF-57, D-48 | T-48.01 | bUnit: na transferência, os campos do produto saem com `app-readonly` e a quantidade a transferir sem; as sete telas conferidas em desktop e celular, com o só leitura reconhecível sem clicar | |

### #49 Texto do preço no lote (T-49)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-49.01 | O `HelperText` do preço na alocação em lote diz "Preço inicial do produto" só quando o valor é o preço inicial, e "Preço inicial: R$ …" quando é outro | RF-16, RN-16 | T-00.29 | bUnit: o texto troca ao mudar o preço e volta ao restaurar o valor; produto sem preço inicial mostra "Produto sem preço inicial"; tela conferida em desktop e celular | |

### #50 Custo na venda (T-50)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-50.01 | `Sale.SalCusto` e a migration, sem preencher as vendas antigas | RN-41, D-49 | T-00.29 | `has-pending-model-changes` limpo; o `migrator` aplica sobre um banco com vendas, que ficam com `SalCusto` nulo | |
| T-50.02 | A venda individual e o carrinho copiam o `ProCusto` para o `SalCusto`; o `SaleVO` expõe o custo | RF-52, RN-41, D-49 | T-50.01 | Caso novo no `ledger_e2e.py`: venda individual e pelo carrinho gravam o custo do momento, uma NF-e depois não muda o `SalCusto` e produto sem custo vende com nulo; o roteiro inteiro passa | |
| T-50.03 | Card de margem bruta do mês no dashboard, só sobre as vendas com custo e com a contagem das que ficaram de fora | RF-52, D-49 | T-50.02, T-46.03 | Teste de unidade da agregação com vendas com e sem custo e com devolução; bUnit do card; tela conferida em desktop e celular | |

### #51 Validação da imagem e do preço na API (T-51)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-51.01 | Função pura em `SmartStorage.Shared` que confere a imagem: assinatura de PNG ou JPEG e até 10 MB, com imagem vazia permitida | RF-53, D-53 | T-00.29 | Testes de unidade: PNG, JPEG e vazia passam; GIF, texto e 10 MB + 1 byte são recusados, com a mensagem em português | |
| T-51.02 | O cadastro e a edição do produto e o produto novo da NF-e chamam a conferência | RF-53, D-53 | T-51.01 | Caso novo no roteiro: imagem que não é PNG nem JPEG responde 400 sem gravar o produto, e na NF-e recusa a nota com `Item N (...)`; PNG passa | |
| T-51.03 | A alocação individual recusa preço menor ou igual a zero, com a mesma mensagem do lote | RF-53, D-53 | T-00.29 | Caso novo no roteiro: preço 0 e negativo respondem 400 sem movimentação nem repreço; preço positivo passa | |
| T-51.04 | Tirar as duas lacunas da spec e descrever a validação no `CLAUDE.md` | RF-53 | T-51.02, T-51.03 | A spec não lista mais as lacunas, e o RF-53 está `[feito]` | |

### #52 Coleção do Postman (T-52)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-52.01 | Criar `SmartStorage.APIGateway.Tests` (xUnit) na solution, com o teste de paridade entre o `appsettings.json` e o `appsettings.Docker.json` do gateway | D-55 | T-00.29 | `dotnet test` da solution roda o projeto novo, local e no CI; trocar duas rotas de lugar num dos arquivos faz o teste falhar apontando a posição; se a paridade de hoje falhar, a correção entra na mesma tarefa | |
| T-52.02 | Coleção em `docs/postman/` com as 42 rotas em pastas por domínio, a requisição "Entrar" guardando o token e os ambientes Local e Docker | D-54 | T-00.29 | Com o stack do Docker de pé, depois do "Entrar" como `admin`, cada requisição responde com o status esperado para o exemplo do seed; nenhum segredo nos arquivos | |
| T-52.03 | Teste de cobertura: cada rota do gateway tem uma requisição na coleção, e nenhuma requisição fica sem rota | D-55 | T-52.01, T-52.02 | Tirar uma requisição da coleção, ou acrescentar uma sem rota, faz o teste falhar apontando método e caminho | |
| T-52.04 | Documentar a coleção | D-54 | T-52.03 | O README explica como importar a coleção e os ambientes e que no Local é preciso desligar a verificação de SSL do Postman; o `CLAUDE.md` diz que rota nova entra nos dois `appsettings` e na coleção, e que o teste confere | |

### #30 Segurança da AuthenticationAPI (T-30)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-30.01 | Criar `SmartStorage.AuthenticationAPI.Tests` (xUnit, NSubstitute e `Microsoft.AspNetCore.Mvc.Testing`) na solution | D-21, D-52 | T-00.29 | `dotnet test` da solution roda o projeto novo, local e no CI, com um teste que sobe a API em memória e recebe 401 sem token | |
| T-30.02 | `BusinessException` em `SmartStorage.Shared` e `IExceptionHandler` em `SmartStorage.Configurations`, ainda sem registro | RN-38, D-15, D-16 | T-30.20 | Teste de unidade: `BusinessException` vira 400 com a mensagem em texto puro; outra exceção vira 500 com "Erro inesperado. Tente novamente." e o detalhe no log | |
| T-30.03 | Trocar os `throw new Exception(` de regra por `BusinessException` no ledger e nos business de produtos e colaboradores | RN-38, D-15 | T-30.02 | Nenhum `throw new Exception(` nesses arquivos; mensagens iguais às de antes | |
| T-30.04 | O mesmo no business de prateleiras | RN-38, D-15 | T-30.02 | Nenhum `throw new Exception(` em `ShelfBusinessImplementation` | |
| T-30.05 | O mesmo no business de vendas, no de NF-e e no `NfeXmlReader` | RN-38, D-15 | T-30.02 | Nenhum `throw new Exception(` nesses arquivos; os testes do `NfeXmlReader` esperam `BusinessException` | |
| T-30.06 | O mesmo nos controllers e nos demais serviços (AuthenticationAPI, ReportsAPI, AIAPI e EmailAPI) | RN-38, D-15 | T-30.02 | O `throw new Exception(` que sobrar é erro inesperado de propósito, com o motivo no review | |
| T-30.07 | Registrar o handler nos cinco serviços e tirar os `catch (Exception) → BadRequest(ex.Message)` dos nove controllers | RN-38, D-15 | T-30.03 a T-30.06 | Nenhum `catch (Exception` nos controllers; o roteiro inteiro passa com as mesmas mensagens de recusa, e o `InsightsTests` também | |
| T-30.08 | `UserAccountVO`, `UserUpdateVO` (sem senha) e `MyAccountUpdateVO` em `SmartStorage.Shared/VO` | D-14 | T-30.01 | Os três seguem o padrão de VO da constituição; nenhum tem refresh token, e o `UserUpdateVO` não tem senha | |
| T-30.09 | Leituras de usuário devolvem `UserAccountVO`; listar e buscar por id exigem administrador | RF-41, D-19 | T-30.08, T-52.03 | Testes: o JSON não tem `password` nem `refreshToken`; usuário comum recebe 403 em `GET /api/auth/v1` e `GET /api/auth/v1/{id}`; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-30.10 | `GET /api/auth/v1/me` com rota no gateway; `user-by-username` sai da API e do gateway | RF-41, D-19 | T-30.09, T-52.03 | Teste: usuário comum recebe 200 com os próprios dados; os dois `appsettings` do gateway têm `/me` na mesma posição e nenhum `user-by-username`; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-30.11 | `update-credentials` recebe `UserUpdateVO` e responde `UserAccountVO` | RF-42, D-14 | T-30.08, T-52.03 | Teste: corpo com `refreshToken`, senha ou campo fora do VO não muda nada além do VO; a resposta não traz senha; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-30.12 | `PUT /api/auth/v1/me` com `MyAccountUpdateVO`, conferindo a senha atual, com rota no gateway | RF-03a, D-14, D-19 | T-30.10, T-52.03 | Testes: o usuário comum troca o próprio nome e a própria senha; senha atual errada responde 400; o corpo não troca username nem papel; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-30.13 | Username repetido conferido antes de gravar, e a `DbUpdateException` do `UQ_users_user_name` vira `BusinessException` | D-17 | T-30.07 | Teste: cadastro repetido responde 400 "Já existe um usuário com esse nome." | |
| T-30.14 | O refresh monta as claims do usuário no banco, pelo `GenerateToken` do login; access token inválido no refresh responde 401 | RF-43, D-18 | T-30.07 | Testes: refresh depois de rebaixar o papel emite `Usuario`; token adulterado responde 401, e não 500 | |
| T-30.15 | Remover `FindWithPagedSearch` e `GetCount` do `GenericRepository` e do `IRepository` | D-19 | T-30.01 | Nenhum `FromSqlRaw` na AuthenticationAPI; o build passa | |
| T-30.16 | Blazor: `IAuthService` com `UserAccountVO`, `VariablesExtensions.User` vira `UserAccountVO`, e a conta carrega por `/me` | D-20 | T-30.10 | bUnit: a tela de conta chama `/me` e o estado não tem campo de senha; `SessaoExpiradaTests` passa | |
| T-30.17 | Blazor: a conta salva por `PUT /me`, com a senha atual, e o `EditUser` do administrador envia `UserUpdateVO` e, quando a senha é preenchida, chama a rota de senha | RF-03a, RF-42, D-50 | T-30.12, T-30.16, T-30.19 | bUnit: o JSON enviado tem só os campos do VO, e o erro da API aparece no diálogo; telas conferidas em desktop e celular | |
| T-30.18 | Conferir no Chrome, logado como `usuario`, e documentar o tratamento de erro | RF-41, RN-38 | T-30.17 | Na aba de rede, nenhuma resposta traz hash, refresh token ou texto interno, e a listagem de usuários é recusada; o `CLAUDE.md` descreve a `BusinessException` | |
| T-30.19 | `PUT /api/auth/v1/{userId}/password`, só administrador, com rota no gateway; o `update-credentials` deixa de mexer em senha | RF-46, D-50 | T-30.11, T-52.03 | Testes: o administrador define a senha de outro usuário; usuário comum recebe 403; os dois `appsettings` do gateway têm a rota na mesma posição; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-30.20 | Criar `SmartStorage.Configurations.Tests` (xUnit, NSubstitute e `Microsoft.AspNetCore.Mvc.Testing`) na solution | D-52 | T-00.29 | `dotnet test` da solution roda o projeto novo, local e no CI | |

### #34 Regra de senha na API (T-34)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-34.01 | `PasswordPolicy` em `SmartStorage.Shared`: no mínimo 8 caracteres, uma letra e um número, com mensagem em português | RN-34, D-22 | T-30.01 | Testes de unidade: 7 caracteres, só letras e só números são recusados; `admin123` passa | |
| T-34.02 | A API aplica a política no `create`, na rota de senha do administrador e no `PUT /me`, e confere "diferente da atual" pelo `IPasswordHasher.Verify` nas duas trocas | RN-34, D-22 | T-34.01, T-30.12, T-30.19 | Testes: senha fora da regra responde 400 sem gravar; senha igual à atual responde 400 no `PUT /me` e na rota do administrador | |
| T-34.03 | Caso no roteiro do cadastro com senha fora da regra | RN-34 | T-34.02 | Caso novo: 400 e nenhum usuário gravado | |
| T-34.04 | `Register`, `EditUser` e a conta usam a `PasswordPolicy` e o mesmo texto de ajuda | RN-34 | T-34.01, T-30.17 | bUnit: a mensagem da regra nova aparece, e senha sem maiúscula é aceita; telas conferidas em desktop e celular | |

### #31 Sessão de 9 horas (T-31)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-31.01 | `TokenConfigurations:SessionHours` (9) no lugar de `DaysToExpiry`; o login grava `RefreshTokenExpiryTime` com login + 9 h | RF-44, D-23 | T-30.14 | Teste: o prazo gravado no login é de 9 horas; `appsettings.Shared.json` sem `DaysToExpiry` | |
| T-31.02 | O refresh troca o refresh token e mantém o prazo do login; depois dele, 401; o access token vence no que vier primeiro, 60 minutos ou o fim da sessão | RF-44, D-23 | T-31.01 | Testes: o prazo depois do refresh é o do login; refresh vencido e refresh depois do revoke são recusados; refresh a 30 minutos do fim emite token que vence no fim da sessão, e não 60 minutos depois | |
| T-31.03 | Rotas de `refresh` (anônima) e `revoke` no gateway | D-25 | T-00.29, T-52.03 | As duas nos dois `appsettings`, na mesma posição; o refresh pelo gateway devolve um `TokenVO` novo; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-31.04 | O login do Blazor guarda o refresh token em `refreshTokenKey` | D-24 | T-00.29 | bUnit: depois do login, o `localStorage` tem as duas chaves | |
| T-31.05 | `RefreshHandler` com `SemaphoreSlim` singleton e o refresh por um `HttpClient` nomeado sem os handlers | RF-44, D-24 | T-31.03, T-31.04 | Testes sobre a `ApiFalsa`: 401 com refresh aceito repete a requisição com o token novo; refresh recusado leva à sessão expirada; dois 401 simultâneos fazem um refresh só | |
| T-31.06 | Registrar o `RefreshHandler` entre o `AuthHandler` e o `SessionExpiredHandler` em todos os serviços tipados | D-24 | T-31.05 | Todo `AddHttpClient` com `AuthHandler` no `Program.cs` tem o `RefreshHandler`; `SessaoExpiradaTests` passa | |
| T-31.07 | O logout chama o revoke e limpa o `localStorage` mesmo se o revoke falhar | RF-44, D-25 | T-31.03 | bUnit: o revoke é chamado; com o revoke em 500 ou sem rede, o `localStorage` é limpo e a tela vai ao login | |
| T-31.08 | Conferir no Chrome com `Minutes` em 1 num ambiente local | RF-44 | T-31.06, T-31.07 | A aba de rede mostra o 401, o refresh e a repetição sem a tela de login, e o logout mostra o revoke | |

### #32 Prateleiras e colaboradores só para administrador (T-32)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-32.01 | O roteiro lê as credenciais do usuário comum por variável de ambiente, com o seed como padrão | D-27 | T-00.29 | Sem as variáveis, o roteiro usa `usuario` / `usuario123` | |
| T-32.02 | `[Authorize(Roles = Role.Admin)]` no `POST`, `PUT` e `DELETE` de `shelf` e `employees` | RF-45, RF-47, D-26 | T-32.01, T-52.03 | Caso novo: como `usuario`, os seis respondem 403 e nada é gravado em `Shelf` nem em `Employee`; como administrador, o cadastro de prateleira do roteiro passa; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-32.03 | Extrair o cálculo do volume usado do `EnsureShelfFits` para um método só | D-26 | T-00.29 | `EnsureShelfFits` e `ShelfConverter` usam o mesmo cálculo; CT-35 e CT-41 passam | |
| T-32.04 | O `UpdateShelf` recusa volume menor que o usado | RN-40, D-26 | T-32.03 | Caso novo: volume abaixo do usado responde 400 e não muda; volume entre o usado e o usado ÷ 0,9 é aceito, e a alocação seguinte é recusada | |
| T-32.05 | Documentar que `SMARTSTORAGE_USER` exige administrador e conferir as telas como `usuario` | RF-45 | T-32.02 | O `CLAUDE.md` diz isso; no Chrome, como `usuario`, alocação e venda ainda listam prateleiras e colaboradores | |

### #33 Encerrar a sessão na hora (T-33)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-33.01 | `User.UseActive` (padrão verdadeiro) e `User.UseSessionStamp` (GUID), com migration | D-28 | T-31.02 | O `migrator` aplica; os usuários existentes ficam ativos e com carimbo | |
| T-33.02 | Claim `session_stamp` no login e no refresh | D-28 | T-33.01 | Teste: a claim é o carimbo do banco | |
| T-33.03 | `OnTokenValidated` no `AddAuthConfiguration`: usuário ausente, inativo ou com carimbo diferente falha a autenticação | RF-46, D-29, D-52 | T-33.02, T-30.20 | Testes: os três casos respondem 401, e o usuário ativo com o carimbo certo passa | |
| T-33.04 | `PATCH /api/auth/v1/{userId}/active`, só administrador, com rota no gateway: troca o carimbo, zera o refresh token e recusa desativar a si mesmo; a rota de senha do administrador também troca o carimbo e zera o refresh; o `UserAccountVO` ganha `active` | RF-41, RF-46, D-14, D-28, D-30, D-31 | T-33.03, T-30.19, T-52.03 | Testes: desativar troca o carimbo e zera o refresh; desativar a si mesmo responde 400; usuário comum recebe 403; a rota de senha do administrador troca o carimbo, e o `PUT /me` não; trocar o papel pelo `update-credentials` não troca o carimbo; a lista de usuários traz `active`; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-33.05 | Login e refresh recusam o usuário inativo | D-31 | T-33.04 | Teste: 401 com a mesma mensagem de usuário ou senha errados | |
| T-33.06 | Botão de desativar e reativar nos cards de usuário do administrador, com confirmação pelo `Dialog` | RF-46 | T-33.04 | bUnit: o `PATCH` enviado e o erro da API; tela conferida em desktop e celular | |
| T-33.07 | Caso no roteiro da sessão derrubada | RF-46 | T-33.05 | Caso novo: desativado, o usuário recebe 401 na requisição seguinte e o refresh é recusado; reativado, entra de novo; excluir usuário sem movimentações também derruba a sessão | |
| T-33.08 | Documentar a checagem da sessão | D-29 | T-33.07 | O `CLAUDE.md` descreve a consulta por requisição e que, depois do deploy, todos entram de novo uma vez | |

### #35 Data da venda no mês corrente (T-35)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-35.01 | A API recusa data da venda antes do dia 1º do mês corrente ou depois de hoje, pelo relógio do servidor, na venda e no carrinho | RN-39, D-32 | T-30.07 | Teste de unidade da regra no dia 1º e no último dia do mês; caso novo: amanhã e o último dia do mês anterior respondem 400 sem gravar venda nem movimentação, e ontem dentro do mês passa | |
| T-35.02 | `MinDate` e `MaxDate` no `MudDatePicker` da venda e do carrinho, com hoje como padrão | RN-39 | T-35.01 | bUnit: o seletor não oferece dia fora do intervalo; telas conferidas em desktop e celular | |

### #36 Mês e ano do relatório (T-36)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-36.01 | Criar `SmartStorage.ReportsAPI.Tests` (xUnit, sem banco) na solution | D-46 | T-00.29 | `dotnet test` da solution roda o projeto novo, local e no CI | |
| T-36.02 | O `ReportRepository` recebe `(year, month)` no recorte, na planilha e no gráfico anual, sem ler `DateTime.Now` | RF-50, RN-30, D-33 | T-36.01 | Teste de unidade do recorte: vendas de outro mês e de outro ano ficam fora, e a quantidade é líquida de devolução | |
| T-36.03 | `month` e `year` opcionais na query dos dois exports, com o mês corrente como padrão; mês fora de 1 a 12 responde 400 | RF-50, D-33 | T-36.02, T-52.03 | Pelo gateway, com vendas no mês anterior, Excel e PDF desse mês saem com as vendas dele e o nome do mês escolhido; mês sem vendas responde "Não há vendas em 09/2026."; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-36.04 | Seletor de mês e ano no Insights, ao lado dos botões de exportar | RF-50 | T-36.03 | bUnit (`InsightsTests`): o mês e o ano vão na requisição, e o aviso de mês sem vendas não trava o botão; tela conferida em desktop e celular | |

### #46 Dashboard de estatísticas (T-46)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-46.01 | `DashboardVO` e as agregações como funções puras: indicadores, vendas por dia, mais vendidos em unidades, vendidas e devolvidas por mês, ocupação, dias de cobertura e capital parado | RF-51, RN-21, RN-30, D-47 | T-36.02 | Testes de unidade: líquido de devolução, cobertura com e sem vendas nos 30 dias, produto sem custo fora do capital parado, prateleira sem volume fora da ocupação, e a venda totalmente devolvida contada nas devolvidas do mês da venda e fora das vendidas | |
| T-46.02 | `GET /api/storage/reports/v1/dashboard?year=&month=` na ReportsAPI, com `[Authorize]`, e a rota no gateway | RF-51, D-47 | T-46.01, T-36.03, T-52.03 | Pelo gateway, um mês do seed devolve os números conferidos no banco; mês fora de 1 a 12 responde 400; os dois `appsettings` têm a rota na mesma posição; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-46.03 | `IReportsService` busca o dashboard; o Insights mostra os cards e os seis gráficos novos e tira os dois atuais do mesmo endpoint, sem baixar a lista inteira de vendas | RF-51, D-47 | T-46.02, T-36.04 | bUnit (`InsightsTests`): o mês e o ano vão na requisição, cards e gráficos aparecem, e o erro da API aparece sem travar a tela; o chat da IA continua funcionando | |
| T-46.04 | Conferir no navegador e documentar | RF-51 | T-46.03 | Tela conferida em desktop e celular; a seção "AIAPI e ReportsAPI" do `CLAUDE.md` descreve o endpoint e que margem fica de fora | |

### #28 Empresas (T-28)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-28.01 | Tabela `Company`, empresa padrão, `TipoUsuario.AdministradorGeral = 2` e a constante do papel em `Role.cs` | RF-35, D-36 | T-33.01 | O `migrator` aplica, com a empresa padrão e CNPJ de teste válido | |
| T-28.02 | `ComId` em `User`, `Product`, `Shelf`, `Employee` e `Invoice`: anulável, preenchido com a empresa padrão e só então `NOT NULL` com FK; índices únicos compostos | RF-36, RN-31, D-36 | T-28.01 | Migration ensaiada numa cópia do banco: todas as linhas na empresa padrão, nada perdido | |
| T-28.03 | Seed com o `AdministradorGeral` e o colaborador padrão por empresa | RF-35 | T-28.02 | O `migrator` aplica, e o administrador geral entra | |
| T-28.04 | Claims `company_id` e `company_name` no login e no refresh | D-36 | T-28.02, T-30.14 | Teste: as claims vêm do usuário no banco | |
| T-28.05 | `ICompanyContext` lido da claim; requisição autenticada sem a claim responde 401 | RN-32, D-36 | T-28.04 | Teste: sem a claim, 401 | |
| T-28.06 | Global query filter em `Product`, `Shelf`, `Employee`, `Invoice` e `User`, e pelo produto em `Enter`, `Sale`, `ProductStockMovement` e `InvoiceItem` | RF-36, D-36, D-52 | T-28.05 | Caso novo no roteiro, com a segunda empresa criada direto no banco: cada entidade de uma empresa é invisível pelo gateway para a outra | |
| T-28.07 | O `SaveChanges` preenche o `ComId` das entidades raiz pela empresa do token | RN-32, D-36, D-52 | T-28.05 | Caso novo no roteiro: o que cada empresa cria grava o `ComId` dela no banco, mesmo com outro `ComId` no corpo; nenhum VO de entrada tem o campo | |
| T-28.08 | Referência a outra empresa é tratada como inexistente: produto, prateleira, entrada e colaborador | RN-32, D-52 | T-28.06 | Caso novo no roteiro: alocar produto da A em prateleira da B, vender entrada da B e escolher colaborador da B são recusados como inexistentes, sem movimentação | |
| T-28.09 | Unicidade por empresa de `ProCodigo` e da chave da NF-e; colaborador padrão da NF-e procurado na empresa | RN-31, D-52 | T-28.06 | Caso novo no roteiro: o mesmo código e a mesma chave em empresas diferentes são aceitos, e na mesma empresa recusados | |
| T-28.10 | Empresa inativa: login e refresh recusados, com a mensagem própria só depois de conferir usuário e senha, e a checagem da sessão lê `ComActive` | RF-37, D-29, D-51 | T-28.05, T-33.03 | Testes: com a senha certa, o login responde "empresa inativa"; com a senha errada, a mensagem genérica; sessão aberta de empresa desativada recebe 401 na requisição seguinte | |
| T-28.11 | AuthenticationAPI: o administrador lista, edita, desativa e exclui só usuários da própria empresa | RF-36 | T-28.06 | Teste: usuário de outra empresa é inexistente para ele | |
| T-28.12 | `/api/companies/v1`: lista, busca, cadastro com colaborador padrão e primeiro administrador (tudo ou nada), edição e `PATCH {id}/active`; só `AdministradorGeral`; rotas no gateway | RF-35, RN-33, D-36 | T-28.10, T-52.03 | Testes: CNPJ inválido e repetido recusados; cadastro com administrador inválido não grava a empresa; a empresa do administrador geral não é desativada; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-28.13 | ReportsAPI e AIAPI filtradas pela empresa do token, com o `Authorization` chegando pelo gateway | RF-36, D-37 | T-28.06 | Pelo gateway, o relatório e o contexto da IA de uma empresa não têm vendas da outra | |
| T-28.14 | O `LowStockAlertVO` leva a empresa, e o alerta só considera produtos dela | RF-36, D-37 | T-28.06 | CT-34 passa | |
| T-28.15 | Blazor: `CompanyService`, telas `/companies`, `/companies/new` e `/companies/edit/{id}` com `CompanyForm`, e o menu "Empresas" só para `AdministradorGeral` | RF-35 | T-28.12 | bUnit: lista, cadastro, edição, ativar e desativar, e o menu escondido para os outros papéis; telas conferidas em desktop e celular | |
| T-28.16 | Blazor: nome da empresa no cabeçalho, seletor de empresa no cadastro de usuário só para `AdministradorGeral` e mensagem de empresa inativa no login, quando a API a devolve | RF-36, RF-37, D-51 | T-28.15 | bUnit dos três; telas conferidas em desktop e celular | |
| T-28.17 | Roteiro com duas empresas | RF-36, RN-32 | T-28.14 | Caso novo: entrada, alocação, venda, devolução e NF-e nas duas, sem uma ver saldo, venda ou relatório da outra; ID de outra empresa responde como inexistente | |
| T-28.18 | Conferir no Chrome com um usuário de cada empresa e documentar | RF-36 | T-28.17 | Conferido; o `CLAUDE.md` descreve o filtro e onde o `IgnoreQueryFilters()` é permitido | |

### #29 E-mail do usuário e senha (T-29)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-29.01 | `UseEmail` (único), `UseEmailConfirmed`, `UseEmailConfirmedAt` e a tabela `UserToken`; seed com e-mails confirmados | RF-38, RN-36, D-38 | T-28.02 | O `migrator` aplica; usuários antigos ficam sem e-mail e continuam entrando | |
| T-29.02 | Token de e-mail: 32 bytes aleatórios em Base64Url, gravado só como hash, com prazo, tipo e uso único | RN-36, D-38 | T-29.01 | Testes de unidade: gerado, gravado só como hash, vencido, reutilizado e de tipo trocado | |
| T-29.03 | `EmailConfirmationVO`, `PasswordResetVO` e `PasswordChangedVO`, e a AuthenticationAPI publicando no RabbitMQ pelo `SmartStorage.MessageBus` | RF-40, D-38 | T-29.01 | O build passa com o MessageBus em `net6.0` | |
| T-29.04 | EmailAPI: destinatário por mensagem, consumidores das três filas novas, HTML com versão em texto e porta SMTP pela configuração | RF-40 | T-29.03, T-29.16 | Testes: as três mensagens saem para o destinatário certo; o alerta de estoque ainda vai para `Email:Destinatario` | |
| T-29.05 | Mailpit no compose de desenvolvimento e `Frontend:BaseUrl` na configuração | D-38, D-39 | T-29.04 | Os e-mails aparecem em `http://localhost:8025`, e os links usam `Frontend:BaseUrl`, nunca o `Host` | |
| T-29.06 | Cadastro com e-mail obrigatório dispara a confirmação; `confirm-email` e `resend-confirmation` anônimos; o `UserAccountVO` ganha `email` e `emailConfirmed` | RF-38, RF-41, RN-35, D-14 | T-29.02, T-29.05 | Testes: link de 24 h e uso único; `resend-confirmation` responde 204 exista o usuário ou não; a lista de usuários traz o e-mail e a situação, sem o token | |
| T-29.07 | O login recusa e-mail pendente e aceita usuário sem e-mail | RN-37 | T-29.06 | Testes dos dois casos | |
| T-29.08 | `forgot-password` e `reset-password`: link de 30 min, `PasswordPolicy`, senha diferente da atual, troca do `UseSessionStamp`, refresh zerado, tokens anteriores invalidados e aviso de senha alterada | RF-39, RF-46, RN-34, RN-35, D-22, D-38 | T-29.06, T-33.03, T-34.01 | Testes: mesma resposta para usuário inexistente, sem e-mail, com e-mail pendente e válido; senha igual à atual responde 400 e o token continua válido; depois do reset, a sessão aberta recebe 401 | |
| T-29.09 | `AddRateLimiter` nos quatro endpoints anônimos | RN-35, D-39 | T-29.08 | Teste: a sexta tentativa no minuto e o quarto e-mail na hora para o mesmo usuário são recusados | |
| T-29.10 | Rotas dos quatro endpoints no gateway | D-38 | T-29.08, T-52.03 | Nos dois `appsettings`, na mesma posição; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-29.11 | A troca de e-mail só vale depois de confirmada | RF-38 | T-29.06 | Teste: até a confirmação, o e-mail antigo continua em uso | |
| T-29.12 | Blazor: `ForgotPassword`, `ResetPassword` e `ConfirmEmail`, e no login o link de recuperação e o reenvio da confirmação | RF-38, RF-39 | T-29.10 | bUnit: token inválido, sucesso e regra de senha; telas conferidas em desktop e celular | |
| T-29.13 | Blazor: e-mail no cadastro e na edição, situação e reenvio na conta e na lista de usuários, e aviso para quem não tem e-mail | RF-38 | T-29.12 | bUnit dos campos e do reenvio; telas conferidas em desktop e celular | |
| T-29.14 | Roteiro de autenticação com o Mailpit | RF-38, RF-39 | T-29.13 | Casos novos: cadastro, e-mail no Mailpit, confirmação e login; esqueci a senha, link, senha nova, login com ela e refresh antigo recusado | |
| T-29.15 | Documentar o fluxo de e-mail | RF-38 a RF-40 | T-29.14 | `CLAUDE.md`, README e `.env.example` descrevem o Mailpit e `Frontend:BaseUrl` | |
| T-29.16 | Criar `SmartStorage.EmailAPI.Tests` (xUnit e NSubstitute, sem banco) na solution | D-52 | T-00.29 | `dotnet test` da solution roda o projeto novo, local e no CI | |

### #39 Avisos para os administradores da empresa (T-39)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-39.01 | A API core resolve os destinatários (administradores ativos da empresa com e-mail confirmado) e os manda em `Recipients` nos avisos de produto novo e de estoque baixo | RF-48, D-40 | T-28.14, T-29.07 | Teste: a lista só tem administradores ativos e confirmados da empresa | |
| T-39.02 | Lista vazia não publica e vai para o log | RF-48 | T-39.01 | Teste: sem destinatário, nada é publicado e a venda não volta erro | |
| T-39.03 | A EmailAPI envia para `Recipients`; `Email:Destinatario` sai da configuração, do `.env` e do `CLAUDE.md` | RF-48, D-40 | T-39.01, T-39.05 | Nenhuma referência a `Email:Destinatario` no código | |
| T-39.04 | Roteiro do aviso por empresa | RF-48 | T-39.03 | O CT-34 confere o envio ao administrador da empresa do roteiro; o aviso de uma empresa não chega à outra | |
| T-39.05 | Remover o `POST /api/storage/email/v1/new-product`: a rota dos dois `appsettings` do gateway, o endpoint da EmailAPI e o `IEmailService` sem uso do Blazor; a lacuna sai da spec | RF-48, D-40 | T-00.29, T-52.03 | Nenhuma referência a `new-product` no gateway, na EmailAPI nem no Blazor; o e-mail de produto novo continua saindo pela fila no CT-34; bUnit e build passam; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |

### #40 Administrador geral e os administradores das empresas (T-40)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-40.01 | `GET` e `POST /api/companies/v1/{companyId}/admins`, só `AdministradorGeral`, com `IgnoreQueryFilters()` e o filtro `UseType = Administrador` explícito | RF-49, D-41 | T-28.12 | Testes: lista só administradores; usuário comum da empresa não aparece | |
| T-40.02 | Desativar, reativar e redefinir a senha (rota da D-50) de administrador de outra empresa, só quando o alvo é `Administrador` | RF-49, D-41, D-50 | T-40.01, T-33.04, T-34.02 | Testes: alvo `Usuario` responde como inexistente; o desativado e o que teve a senha redefinida perdem a sessão na requisição seguinte; senha igual à atual responde 400 | |
| T-40.03 | Rotas no gateway e isolamento do administrador geral | RF-49, RN-32 | T-40.02, T-52.03 | Testes: administrador de empresa recebe 403 nas rotas do geral; o geral não lê produtos, vendas nem usuários comuns de outra empresa; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-40.04 | Administradores de cada empresa na tela de empresas | RF-49 | T-40.03, T-28.15 | bUnit: criar, desativar e redefinir a senha; tela conferida em desktop e celular | |
| T-40.05 | Conferir os usos de `IgnoreQueryFilters()` | D-41 | T-40.04 | Só aparece nos lugares documentados no `CLAUDE.md` | |

### #27 Ollama como provedor alternativo (T-27)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-27.01 | Criar `SmartStorage.AIAPI.Tests` (xUnit, sem banco) na solution | D-46 | T-00.29 | `dotnet test` da solution roda o projeto novo, local e no CI | |
| T-27.02 | `IChatClient` injetado por DI e escolhido por `AI:Provider` (Gemini ou Ollama), com adaptador do Gemini se o pacote não expuser `IChatClient` | RF-32, D-42 | T-27.01 | Teste: a configuração escolhe o provedor; o `AiRepository` não cria mais o `Client` | |
| T-27.03 | Serviço `ollama` no compose atrás do profile `ollama`, com volume dos modelos e `num_ctx` que caiba o contexto | RF-32 | T-27.02 | `docker compose --profile ollama up -d` sobe, e o Insights responde com `AI:Provider=Ollama` | |
| T-27.04 | Contexto com as 100 vendas mais recentes em CSV: data, produto, prateleira, quantidade e total líquido | RF-33, D-43 | T-27.01 | Teste de unidade: com 150 vendas, saem as 100 mais recentes; nomes em vez de IDs, total líquido de devolução e nenhuma coluna de colaborador ou dado pessoal | |
| T-27.05 | Agregados por produto e por mês calculados em C# no contexto | RF-33, D-43 | T-27.04 | Teste de unidade dos totais | |
| T-27.06 | Erro do Ollama (fora do ar, modelo não baixado) aparece no Insights como os da #18 | RF-32 | T-27.03 | `InsightsTests` cobre o erro, e o erro não vai para o chat | |
| T-27.07 | Documentar o provedor | RF-32 | T-27.06 | README, `CLAUDE.md` e `.env.example` descrevem `AI:Provider` e o profile | |

### #5 Resumo da IA no PDF (T-05)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-05.01 | A ReportsAPI chama a AIAPI pelo endereço interno, repassando o `Authorization` do usuário | RF-31, D-44 | T-27.05, T-36.03 | Teste de unidade com a AIAPI falsa: o cabeçalho é repassado; a chamada tem prazo | |
| T-05.02 | O PDF traz o resumo da IA; se a IA falhar ou passar do prazo, sai sem ele, com uma linha avisando | RF-31, D-44 | T-05.01 | Teste de unidade dos dois casos; pelo gateway, com a AIAPI parada, o PDF sai com o aviso | |

### #13 Código de barras pela câmera (T-13)
| ID | Tarefa | Origem | Depende de | Concluída quando | Commit |
|---|---|---|---|---|---|
| T-13.01 | `GET /api/storage/products/v1?code=` busca pelo `ProCodigo` | RF-34, D-45 | T-00.29, T-52.03 | Caso novo: código existente devolve o produto, e código desconhecido devolve lista vazia; a coleção do Postman acompanha a mudança e o teste de cobertura passa | |
| T-13.02 | Busca por código no `IProductService` | RF-34 | T-13.01 | bUnit: a query vai com `code` | |
| T-13.03 | Leitura pela câmera por JS interop: `BarcodeDetector` quando existe e `@zxing/browser` servido do `wwwroot` | RF-34, D-45 | T-00.29 | Lê um EAN-13 no Chrome do celular e no Firefox | |
| T-13.04 | Botão de leitura em Produtos em estoque que abre o produto lido | RF-34 | T-13.02, T-13.03 | bUnit do código enviado e do produto não encontrado; tela conferida em desktop e celular | |
