# Plano técnico

## Contexto
Implementa a `specs/spec.md` respeitando a `specs/constitution.md`. As
decisões D-01 a D-13 registram como o código já atende os requisitos
[feito], para que nenhuma mudança as quebre sem saber; as seguintes planejam
os requisitos [planejado #N], agrupadas por issue e na ordem de "Ordem de
implementação". As tarefas de cada issue saem daqui para a `specs/tasks.md`.

Decisão nova recebe o próximo número livre, como os requisitos da spec; a
que for abandonada deixa o número sem uso.

## Arquitetura
- O navegador só fala com o gateway Ocelot (4480), que repassa para
  AuthenticationAPI, API core, ReportsAPI, AIAPI e EmailAPI; todos usam o
  mesmo SQL Server pelo mesmo `SmartStorageContext`
  (`SmartStorage.Infraestructure`)
- API core: `Controller → Business → SmartStorageContext`, com `Converter`
  traduzindo Model ↔ VO e HATEOAS por enricher; o ledger
  (`ProductStockMovementRepository`) é o único que escreve saldo
- AuthenticationAPI: `Controller → Service → UserRepository`; emite JWT HS256
  de 60 minutos e o refresh token
- ReportsAPI e AIAPI: só leitura, por um `*Repository`
- EmailAPI: um consumidor RabbitMQ por fila, com `BasicAck` só depois do envio
  SMTP
- `SmartStorage.Configurations` é usado pelos cinco serviços e já referencia a
  Infraestructure: é onde entram o tratamento de erro (D-15) e a checagem da
  sessão (D-29), comuns a todos

## Decisões

### Já implementadas
- D-01: saldo materializado (`ProQntd` no depósito, `EntQntd` na prateleira),
  escrito só pelo ledger, na mesma gravação da linha com sinal (RN-01, RN-02)
- D-02: autor e instante decididos no ledger, pelo `unique_name` do token via
  `IHttpContextAccessor` e pelo relógio do servidor; nenhum VO os carrega
  (RN-03)
- D-03: containers e SQL Server em `TZ: America/Sao_Paulo`, para a data do
  navegador (`-03:00`) virar hora local no model binding
- D-04: operação de vários itens valida tudo antes de escrever, grava numa
  transação com um só `PsmDate` e responde `Item N (...): motivo` (RN-06,
  RN-07)
- D-05: a venda copia o `EntPrice` para `SalPrice` e nunca mais o altera, e o
  faturamento sai dele (RN-15)
- D-06: `Enter` zerado não é apagado, porque as vendas apontam para ele
  (RN-10, RN-11)
- D-07: volume usado é `SUM(EntQntd × ProVolume)`, calculado em lote no
  `ShelfConverter`; o teto é `ShelfVO.UsableFraction` (RN-08, RN-09)
- D-08: alerta de estoque mínimo em fila própria, publicado depois do commit
  e só ao cruzar o limite; falha ao publicar só vai para o log (RN-28, RN-29)
- D-09: o XML da NF-e é relido no servidor na prévia e na importação, e a
  chave é única em `Invoice` (RN-23)
- D-10: paginação opcional por query string, com `X-Total-Count` exposto no
  CORS; sem `page`, a lista inteira
- D-11: converter que precisa de outra tabela resolve a lista em lote, nunca
  uma consulta por item
- D-12: o Blazor fala com cada domínio por um serviço tipado, com
  `AuthHandler`, `SessionExpiredHandler` e `ApiException`
- D-13: só o container `migrator` aplica migrations; o seed viaja nelas por
  `HasData`

### Segurança da AuthenticationAPI (#30)
- D-14: VOs de usuário em `SmartStorage.Shared/VO`: `UserAccountVO` (id,
  username, fullName, type; `active` a partir da #33, `email` e
  `emailConfirmed` a partir da #29) em toda resposta; `UserUpdateVO`
  (fullName, type, sem senha) na edição pelo administrador, porque a senha
  definida por ele tem rota própria (D-50); `MyAccountUpdateVO` (fullName,
  currentPassword, newPassword) no `PUT /me`. A entidade `User` não entra nem
  sai de controller (RF-03a, RF-41, RF-42)
- D-15: toda recusa de regra lança `BusinessException`, em
  `SmartStorage.Shared`; um `IExceptionHandler` em
  `SmartStorage.Configurations`, registrado nos cinco serviços, responde 400
  com a mensagem dela, e qualquer outra exceção vira 500 com "Erro inesperado.
  Tente novamente." e o detalhe no log (RN-38). Os `catch (Exception) →
  BadRequest(ex.Message)` saem dos controllers e os cerca de 165
  `throw new Exception(...)` de regra viram `BusinessException`. Um ponto só,
  e não um catch por controller, porque assim a falha segura é o 500
  genérico: o que for esquecido deixa de vazar texto interno
- D-16: o corpo do 400 continua o texto puro de hoje, para o `ReadApiAsync`
  do Blazor e o roteiro não mudarem
- D-17: username repetido é conferido antes de gravar, e a corrida no
  `UQ_users_user_name` (`DbUpdateException`) também vira `BusinessException`
- D-18: o refresh monta as claims a partir do usuário no banco, pelo mesmo
  `GenerateToken` do login (RF-43); access token inválido no refresh responde
  401, não 500
- D-19: `GET` e `PUT /me` leem o usuário pelo `unique_name`; listar e buscar
  usuário por id exigem administrador; saem `user-by-username` e os
  `FindWithPagedSearch` e `GetCount` do `GenericRepository`
- D-20: `VariablesExtensions.User` passa a ser `UserAccountVO`
- D-21: projeto novo `SmartStorage.AuthenticationAPI.Tests` (xUnit e
  NSubstitute, sem banco), na solution e portanto no CI; dentro do
  `SmartStorage.API.Tests` ele misturaria dois serviços num projeto
- D-50: o administrador define a senha de outro usuário por
  `PUT /api/auth/v1/{userId}/password`, só administrador, e o
  `update-credentials` deixa de mexer em senha. Rota própria, e não um campo
  opcional na edição, para a troca de senha ter um ponto só onde, a partir da
  #33, troca o `UseSessionStamp` e zera o refresh token (RF-46)

### Regra de senha (#34)
- D-22: `PasswordPolicy` em `SmartStorage.Shared`, chamado pela API e pelo
  Blazor, que já referencia o Shared, para a regra existir num lugar só; o
  "diferente da atual" só a API confere, pelo `IPasswordHasher.Verify`, nos
  três pontos onde a senha muda: o `PUT /me`, a rota de senha do
  administrador (D-50) e, com a #29, o `reset-password` (RN-34)

### Sessão de 9 horas (#31)
- D-23: `TokenConfigurations:SessionHours` (9) substitui `DaysToExpiry`; o
  login fixa `RefreshTokenExpiryTime`, e o refresh troca o refresh token sem
  estender esse prazo (RF-44). O access token vence no que vier primeiro, 60
  minutos ou o `RefreshTokenExpiryTime`, porque com 60 minutos fixos um
  refresh perto do fim levaria a sessão até 10 horas
- D-24: o Blazor guarda o refresh token em `refreshTokenKey`; um
  `RefreshHandler` entre o `AuthHandler` e o `SessionExpiredHandler` renova no
  401 e repete a requisição, com um `SemaphoreSlim` singleton para haver uma
  renovação só; o refresh sai por um `HttpClient` nomeado sem esses handlers,
  para não entrar em laço
- D-25: o logout chama o revoke e limpa o `localStorage` mesmo se o revoke
  falhar; o gateway ganha as rotas `refresh` (anônima) e `revoke`

### Prateleiras e colaboradores (#32)
- D-26: `[Authorize(Roles = Role.Admin)]` no `POST`, `PUT` e `DELETE` de
  `shelf` e `employees` (RF-45, RF-47); o `UpdateShelf` recusa volume menor
  que o usado (RN-40), com o cálculo de volume usado do `EnsureShelfFits`
  extraído para um método só
- D-27: o roteiro lê as credenciais do usuário comum, com o seed como padrão,
  e `SMARTSTORAGE_USER` passa a exigir um administrador

### Encerrar a sessão na hora (#33)
- D-28: `User` ganha `UseActive` (padrão verdadeiro) e `UseSessionStamp`
  (GUID), e o token leva a claim `session_stamp`; desativar e redefinir a
  senha (a rota do administrador da D-50 e, com a #29, o `reset-password`)
  trocam o carimbo, e excluir apaga a linha. A troca pelo `PUT /me` não troca,
  para não derrubar a própria sessão de quem trocou
- D-29: um `OnTokenValidated` no `AddAuthConfiguration` lê, a cada
  requisição, `UseActive` e `UseSessionStamp` do usuário do token (e, com a
  #28, `ComActive`), numa consulta de uma linha pelo índice único do
  username; usuário ausente, inativo, com carimbo diferente ou de empresa
  inativa falha a autenticação, e a requisição recebe 401 (RF-46). Banco a
  cada requisição, e não cache, porque o RF-46 pede "na hora", e a consulta
  por índice único é barata no volume de um almoxarifado. É a leitura direta
  fora do `*Repository` que a constituição permite na AIAPI e na ReportsAPI;
  perguntar à AuthenticationAPI por HTTP faria dela um ponto único de falha
  em toda requisição
- D-30: a troca de papel não troca o carimbo; vale no próximo refresh
  (RF-43)
- D-31: `PATCH /api/auth/v1/{userId}/active`, só administrador; ninguém
  desativa a si mesmo, desativar também zera o refresh token, e o login do
  inativo responde 401 com a mesma mensagem de usuário ou senha errados

### Data da venda (#35)
- D-32: a API recusa `dateSale.Date` antes do primeiro dia do mês corrente ou
  depois de hoje, pelo relógio do servidor, na venda e no lote (RN-39); o
  `MinDate` e o `MaxDate` do `MudDatePicker` são só conveniência, e na virada
  do dia quem decide é a API

### Relatório por mês (#36)
- D-33: `month` e `year` opcionais na query dos dois exports; o
  `ReportRepository` recebe `(year, month)` e deixa de ler `DateTime.Now`, e
  mês fora de 1 a 12 responde 400 (RF-50); o Insights ganha um seletor de mês
  e um de ano

### Dashboard (#46)
- D-47: `GET /api/storage/reports/v1/dashboard?year=&month=` na ReportsAPI,
  que já só lê o banco, devolve um `DashboardVO` (em `SmartStorage.Shared`)
  com os indicadores e as séries agregados no servidor, pelo mesmo `(year,
  month)` da D-33; o `DashboardVO` só responde um agregado, então dispensa o
  `Parse`/`ParseList`, como a constituição permite. A série de devolvidas
  soma o `SalReturnedQntd` de todas as vendas do mês, inclusive as
  totalmente devolvidas, pelo mês da venda, na mesma base das vendidas
  (RN-21). O Insights deixa de baixar a lista inteira de vendas para
  desenhar os gráficos. As agregações são funções puras testadas no
  `SmartStorage.ReportsAPI.Tests`, e os gráficos usam o `MudChart`, sem
  biblioteca nova. Margem fica de fora: a venda não grava o custo do momento,
  e o custo médio atual reescreveria a margem de vendas antigas (RF-51); ela
  entra pela #50 (D-49)

### Validação na API (#51)
- D-53: a conferência da imagem é uma função pura em `SmartStorage.Shared`
  (assinatura de PNG ou JPEG e até 10 MB), chamada no cadastro e na edição
  do produto e no produto novo da NF-e, que recusa com `Item N (...): motivo`
  como o resto da nota; imagem vazia continua permitida. A alocação
  individual recusa `ProductPrice <= 0` com a mesma mensagem do lote (RF-53).
  Pela assinatura, e não pela extensão ou pelo `Content-Type`, porque o corpo
  só traz os bytes

### Custo na venda (#50)
- D-49: `Sale.SalCusto`, `decimal(18,4)` e anulável, copiado do `ProCusto` na
  criação da venda, individual e pelo carrinho, como o `SalPrice` (RN-41); a
  migration deixa as vendas antigas nulas em vez de inventar o custo pelo
  médio de hoje. A margem do dashboard soma só as vendas com custo e conta as
  que ficaram de fora (RF-52)

### Campos só leitura (#48)
- D-48: campo só leitura ganha a classe `app-readonly`, aplicada pela tela e
  não por seletor sobre `input[readonly]`, com fundo preenchido e sem
  destaque de foco nos dois temas; `Disabled` fica para o que está
  indisponível, não para valor que só se mostra

### Transferência (#37)
- D-34: a transferência manda sempre `shelfPrice: enter.EntPrice` ao ledger
  (RN-11)

### Alerta no ajuste (#38)
- D-35: `ProductBusinessImplementation` recebe `IStockAlertBusiness` e avalia
  o alerta nos dois caminhos de ajuste do depósito; as chamadas na alocação
  saem, porque ela não muda o saldo total (RF-28)

### Empresas (#28)
- D-36: segue o desenho da #28 (`Company`, `ComId` nas entidades raiz, global
  query filter por um `ICompanyContext` lido da claim `company_id`,
  `SaveChanges` preenchendo o `ComId`, unicidade por empresa, empresa só
  desativada, nunca excluída; RF-35, RF-36, RN-31 a RN-33), com dois
  ajustes da spec revisada: o administrador geral só vê os administradores
  das outras empresas (RF-49), e a empresa inativa derruba a sessão na hora,
  pela D-29, e não só no refresh (RF-37)
- D-51: o login confere usuário e senha antes da situação da empresa; só com
  os dois certos responde "empresa inativa", e com a senha errada a mensagem
  é a genérica. Quem lê o aviso já provou ter a senha, então ele não revela
  usuários a quem não tem (constituição: endpoint anônimo responde igual
  exista o recurso ou não), e o usuário legítimo entende por que não entra
- D-37: ReportsAPI e AIAPI herdam o filtro por usarem o mesmo contexto; o
  consumidor da EmailAPI não tem token, então empresa e destinatário são
  decididos por quem publica (D-40)

### E-mail do usuário e senha (#29)
- D-38: segue o desenho da #29 (`UseEmail`, `UserToken` só com o hash, filas
  novas, `Frontend:BaseUrl`, limite de tentativas, respostas iguais nos
  endpoints anônimos e Mailpit no compose; RF-38, RF-40, RN-35 a RN-37), com
  dois ajustes: a regra de senha é a `PasswordPolicy` (D-22), e a redefinição
  troca o `UseSessionStamp` além de zerar o refresh token, para a sessão cair
  na hora (RF-39, RF-46)
- D-39: o limite de tentativas usa o `AddRateLimiter` do próprio ASP.NET
  Core, sem pacote novo; o Mailpit é só uma imagem de desenvolvimento

### Avisos (#39)
- D-40: a API core, que publica o aviso, resolve os destinatários (os
  administradores ativos da empresa com e-mail confirmado) e os põe na
  mensagem, em `Recipients`; a EmailAPI só envia para a lista recebida.
  Lista vazia não publica e vai para o log, e `Email:Destinatario` sai da
  configuração (RF-48). Sai junto o `POST /api/storage/email/v1/new-product`,
  que ninguém chama e que enviava para esse endereço por fora da fila: a rota
  nos dois `appsettings` do gateway, o endpoint da EmailAPI e o
  `IEmailService` do Blazor

### Administrador geral (#40)
- D-41: `GET` e `POST /api/companies/v1/{companyId}/admins`, e o `PATCH
  .../active` e a rota de senha (D-50) aceitam o `AdministradorGeral` só
  quando o alvo tem `UseType = Administrador`; esse caminho usa
  `IgnoreQueryFilters()` com o filtro de tipo explícito, e nenhum outro o usa
  (RF-49)

### IA (#27, #5)
- D-42: o provedor fica atrás do `IChatClient` (`Microsoft.Extensions.AI`),
  escolhido por `AI:Provider`, com o `OllamaSharp` como cliente do Ollama; o
  Gemini entra por um adaptador sobre o `Google.GenAI` se o pacote não expuser
  `IChatClient` (RF-32). As dependências se justificam por trocar o provedor
  sem mexer no repositório
- D-43: o contexto da IA leva as 100 vendas mais recentes, número fixo, em
  CSV, com data, produto, prateleira, quantidade e total líquido, mais
  agregados por produto e por mês calculados em C# (RF-33); nada de
  colaborador nem de outro dado pessoal, como manda a constituição
- D-44: a ReportsAPI chama a AIAPI pelo endereço interno, repassando o
  `Authorization` do usuário, para o resumo do PDF (RF-31); se a IA falhar ou
  demorar, o PDF sai sem o resumo e com uma linha avisando. Pela AIAPI, e não
  direto no provedor, para a integração com a IA ficar num ponto só

### Código de barras (#13)
- D-45: leitura pela câmera com o `BarcodeDetector` do navegador quando
  existe e o `@zxing/browser`, servido do próprio `wwwroot`, como
  alternativa, por JS interop; a busca é `GET /products/v1?code=` sobre o
  `ProCodigo` (RF-34). A dependência se justifica porque Safari e Firefox não
  têm `BarcodeDetector`

### Testes da ReportsAPI e da AIAPI (#36, #27)
- D-46: projetos `SmartStorage.ReportsAPI.Tests` (criado na #36) e
  `SmartStorage.AIAPI.Tests` (criado na #27), xUnit e sem banco, como o
  `SmartStorage.API.Tests`: testam as funções puras extraídas do repositório
  (o recorte do mês, a montagem do contexto da IA e os agregados). Um projeto
  por serviço, pelo mesmo motivo da D-21: cada um referencia só o serviço que
  testa

### Infraestrutura de teste
- D-52: o que é preciso do pipeline HTTP para provar (401, 403, a rota
  anônima, o handler de erro e a checagem da sessão) usa o
  `Microsoft.AspNetCore.Mvc.Testing`, com o serviço de pé em memória e as
  dependências de banco trocadas por falsos, e roda no CI; a dependência se
  justifica porque essas são as regras de segurança, e fora do CI elas só
  seriam conferidas quando alguém rodasse o roteiro. O que depende do SQL
  Server de verdade (global query filter, índices únicos por empresa,
  `DbUpdateException`) fica como caso do `ledger_e2e.py`, sem pacote novo. O
  código do `SmartStorage.Configurations` ganha o
  `SmartStorage.Configurations.Tests`, e a EmailAPI, que passa a ter regra
  própria na #29, o `SmartStorage.EmailAPI.Tests`, pelo mesmo motivo da D-21

## Modelo de dados
Atual:
- `Product`, `Shelf`, `Enter`, `Sale`, `Employee`, `Invoice`, `InvoiceItem`,
  `ProductStockMovement` e `User`, com os prefixos de três letras
- Índices únicos: `UQ_Product_codigo` (filtrado por `ProCodigo IS NOT
  NULL`), `UQ_Invoice_chave` e `UQ_users_user_name`
- `ProductStockMovement` com FKs `Restrict` para produto, prateleira e
  usuário; `PsmSheId` nulo é o depósito

Mudanças:
- #50: `Sale.SalCusto`, `decimal(18,4)` e anulável
- #33: `User.UseActive` e `User.UseSessionStamp`
- #28: tabela `Company` (`ComId`, `ComName`, `ComCnpj` único,
  `ComDateRegister`, `ComActive`); `UseComId`, `ProComId`, `SheComId`,
  `EmpComId` e `InvComId`; índices únicos compostos com a empresa;
  `TipoUsuario.AdministradorGeral = 2`
- #29: `UseEmail`, `UseEmailConfirmed`, `UseEmailConfirmedAt` e a tabela
  `UserToken`
- #31: nenhuma coluna nova; muda só a regra do `UseRefreshTokenExpiryTime`

## Contratos
Os atuais são as rotas do gateway (`SmartStorage.APIGateway/appsettings.json`).
Entram ou mudam:
- #50: o `SaleVO` passa a trazer o custo da venda
- #30: `GET` e `PUT /api/auth/v1/me`; `PUT /api/auth/v1/{userId}/password`;
  sai `user-by-username`; `update-credentials` recebe `UserUpdateVO`, sem
  senha; respostas de usuário em `UserAccountVO`
- #31: `POST /api/auth/v1/refresh` e `POST /api/auth/v1/revoke` no gateway
- #32: `POST`, `PUT` e `DELETE` de `shelf` e `employees` exigem administrador
- #33: `PATCH /api/auth/v1/{userId}/active`
- #36: `?month=&year=` em `export-excel` e `export-pdf`
- #46: `GET /api/storage/reports/v1/dashboard?year=&month=`
- #28: `/api/companies/v1` (lista, busca, cadastro, edição e
  `PATCH {id}/active`)
- #29: `confirm-email`, `resend-confirmation`, `forgot-password` e
  `reset-password`, anônimos
- #39: sai `POST /api/storage/email/v1/new-product`
- #40: `/api/companies/v1/{companyId}/admins`
- #13: `?code=` em `GET /api/storage/products/v1`

Toda rota nova entra no `appsettings.json` e no `appsettings.Docker.json` do
gateway, na mesma posição.

## Ordem de implementação
1. #37, #38, #41, #42, #43, #44, #45, #47, #48, #49 e #51: pequenas e
   independentes, com a #51 antes da #13, porque corrige uma lacuna de
   segurança em produtos; e a gravação do custo da #50, cedo, porque venda
   feita sem ela fica para sempre fora da margem. A #50 vem antes da #30 sem
   contrariar a constituição: a lacuna da #30 é o texto de erro devolvido ao
   cliente, e a #50 só copia um valor para uma coluna nova, sem criar recusa
   nem caminho de erro novo
2. #30 → #34 → #31 → #32 → #33: as lacunas de segurança antes de feature
   nova na mesma área, como manda a constituição
3. #35 e #36, e depois a #46, que usa o mês e o ano da #36, e o card de
   margem da #50
4. #28 → #29 → #39 e #40
5. #27 → #5
6. #13, sem prioridade

## Riscos
- Troca dos cerca de 165 `throw` da D-15: um esquecido vira 500 genérico onde
  era 400 com motivo; mitigado por só dar a tarefa como pronta sem nenhum
  `throw new Exception(` em business, ledger e controllers, e com o roteiro
  inteiro passando
- Consulta por requisição da D-29: liga as cinco APIs à tabela `User`; se o
  custo aparecer, entra um cache de poucos segundos e o RF-46 passa a dizer
  esse prazo
- Tokens emitidos antes da #33 não têm `session_stamp` e são recusados: todo
  mundo entra de novo uma vez depois do deploy
- Global query filter da #28: um `IgnoreQueryFilters()` fora do lugar vaza
  dados de outra empresa; mitigado por teste de isolamento por entidade
- Migration da #28 sobre banco com dados; ensaiada antes numa cópia
- Ollama local ocupa de 2 a 6 GB e responde mais devagar; o Gemini continua o
  padrão
- `SmartStorage.MessageBus` ainda em `net6.0`, usado pelas filas novas da #29
  e da #39
