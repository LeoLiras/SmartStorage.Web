# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Visão geral

SmartStorage é um sistema de gestão de estoque em .NET 10 dividido em microsserviços, com front Blazor WebAssembly, gateway Ocelot, SQL Server, RabbitMQ e Docker Compose. O README na raiz descreve o produto e como subir tudo; este arquivo cobre o que só se descobre lendo o código.

**A raiz do repositório não é a raiz da solution.** O `docker-compose.yml`, o `.env` e o `README.md` ficam na raiz; todos os projetos e a `SmartStorageWeb.sln` ficam em `SmartStorage-API/`. Dentro dela existe ainda `SmartStorage-API/SmartStorage-API/`, que é o projeto da API core (`SmartStorage.API.csproj`). Todo `context` de build no compose é `./SmartStorage-API`.

## Comandos

```bash
# Stack completo (baixa as imagens de hub.docker.com/u/lelep0)
docker compose up -d
docker compose up -d --build          # buildar localmente em vez de baixar
docker compose logs -f <serviço>

# Build da solution
dotnet build SmartStorage-API/SmartStorageWeb.sln -c Release

# Rodar um serviço isolado (a partir de SmartStorage-API/)
dotnet run --project SmartStorage-API/SmartStorage.API.csproj
```

A verificação tem duas camadas, e elas cobrem coisas diferentes de propósito.

### Testes de componente (`dotnet test`)

`SmartStorage.Blazor.Tests` usa bUnit e NSubstitute para renderizar as telas e afirmar sobre o **JSON que o Blazor serializa**, com os serviços reais sobre um `HttpMessageHandler` falso (`ApiFalsa`, que também entrega um `HttpClient` pronto em `Cliente()`). É a camada que pega bug de front, que o roteiro de API não vê.

Três atritos do bUnit 2 com MudBlazor, já resolvidos em `RegistroDeVendaTests` e que vale copiar ao escrever teste novo: o contexto é `BunitContext` (não `TestContext`) e a autorização é `AddAuthorization()` (não `AddTestAuthorization()`); toda tela com `MudDatePicker` exige um `MudPopoverProvider` na árvore, que **não** pode ser wrapper por não ter `ChildContent` — renderize os dois como irmãos num mesmo fragmento; e a classe de teste precisa de `IAsyncLifetime`, senão o descarte síncrono estoura em `MudBlazor.PointerEventsNoneService`, que só implementa `IAsyncDisposable`.

### Roteiro fim-a-fim (`tests/ledger_e2e.py`)

Script Python de biblioteca padrão que entra pelo gateway como cliente e confere o banco a cada passo:

```bash
python tests/ledger_e2e.py              # os 44 casos, ~8 min
python tests/ledger_e2e.py --caso CT-08 # um caso só
python tests/ledger_e2e.py --manter     # preserva os produtos criados, para inspeção
```

Exige o stack de pé. Cria os próprios produtos (prefixo `ZZ Ensaio Automatizado` mais um identificador por execução, para ser reexecutável) e remove o que criou direto no banco — a aplicação não exclui produto —, varrendo também sobras de execuções anteriores. Os produtos do roteiro nascem com 0,01 L, para não esgotar a capacidade das prateleiras A1 e A2 durante a execução; as prateleiras com o mesmo prefixo saem depois dos produtos. `--caso` pode ser repetido (`--caso CT-31 --caso CT-32`), e cada execução apaga as sobras da anterior, então `--manter` só preserva os dados da última. Depois de cada operação confere quatro coisas: as linhas que o ledger gravou (local, tipo e quantidade com sinal), a invariante `saldo_depois == saldo_antes + SUM(PsmQntd)` por produto e por local, que o `PsmDate` esteja no relógio do servidor — por causa de uma regressão em que a venda gravava o horário local do navegador, três horas atrás das demais movimentações — e que o `PsmUseId` seja o usuário que fez login no roteiro. No fim procura saldo negativo, movimentação órfã e venda sem entrada. Devolve exit code não-zero em falha, mas **não está no CI**: o workflow só builda e publica imagens.

O roteiro bate na API, não no Blazor — bugs que vivem só no front, no que a tela monta e envia, passam por ele sem serem notados. Dois já aconteceram assim: `ProductId` zero vindo do estado global (os holders de `VariablesExtensions` eram inicializados com `new()`, o que anulava todo `is null`) e a data perdendo o `Kind` no `MudDatePicker`. Essa classe é coberta pelos testes de componente, não por aqui.

Nenhum caso do roteiro sai pulado ou como lacuna: qualquer resultado diferente de `PASSOU` é regressão.

### Migrations

O projeto de migrations é `SmartStorage.Infraestructure`, com startup em `SmartStorage.API.csproj`:

```bash
cd SmartStorage-API
dotnet ef migrations add <Nome> -p SmartStorage.Infraestructure -s SmartStorage-API/SmartStorage.API.csproj
dotnet ef migrations has-pending-model-changes -p SmartStorage.Infraestructure -s SmartStorage-API/SmartStorage.API.csproj
```

`SmartStorageContextFactory` (`IDesignTimeDbContextFactory`) existe justamente para as ferramentas do EF não subirem o host da aplicação — que exigiria `TokenConfigurations:Secret`. Em tempo de design a connection string não precisa ser real.

Em runtime **nenhuma aplicação aplica migrations**: quem faz isso é o container `migrator`, que roda um EF migrations bundle autocontido e sai. Os cinco serviços que usam banco esperam por ele com `service_completed_successfully`.

### Segredos fora do Docker

Cada projeto executável tem seu próprio `UserSecretsId`. Os `appsettings.json` têm as chaves vazias de propósito. Necessários: `ConnectionStrings:SqlServerConnection` e `TokenConfigurations:Secret` (o mesmo valor em todos — ele assina e valida), mais `Email:Username`, `Email:Password` e `Email:Destinatario` na EmailAPI. No Docker tudo vem do `.env` por variável de ambiente.

## Arquitetura

### O gateway é a única porta de entrada

O Blazor não conhece o endereço de nenhum serviço — as cinco chaves `ServiceUrls:*` em `wwwroot/appsettings*.json` apontam todas para o gateway (4480). São 40 rotas em `SmartStorage.APIGateway/appsettings.json` (dev) e `appsettings.Docker.json` (compose, ativado por `ASPNETCORE_ENVIRONMENT: Docker`).

**Os dois arquivos precisam declarar as rotas na mesma ordem.** A configuração JSON do .NET faz merge de arrays por índice, então `appsettings.Docker.json` só sobrescreve corretamente se cada rota estiver na mesma posição e com todos os campos redeclarados. Ao adicionar um endpoint, edite os dois.

Cuidado com as portas: as rotas de **dev** apontam para as portas **HTTPS** dos serviços (5100, 5102, 5104, 5106, 5202), que são diferentes das portas publicadas pelo compose (5100, 5103, 5105, 5107, 5202).

### Camadas da API core (`SmartStorage-API/`)

`Controller → Business → SmartStorageContext`, com `Converter` traduzindo Model ↔ VO. Não há repositório aqui — as `*BusinessImplementation` recebem o `DbContext` direto e instanciam o converter no construtor. A AuthenticationAPI é a exceção: ela tem `Repositories/` com um `GenericRepository`.

Converter que precisa de dados de outra tabela resolve a lista inteira em lote: o `Parse(List<>)` busca os nomes ou quantidades de todos os itens numa consulta (dicionário por id) e monta cada VO num `Parse` privado, como fazem `ProductConverter`, `SaleConverter` e `EnterConverter`. Consultar dentro do `Parse` de um item faz a listagem crescer em consultas por linha — as vendas faziam 61 consultas para 20 itens.

Listagem paginada é opcional por query string: `GET /sales/v1?page=1&pageSize=10&search=texto` devolve só a página, na ordem mais recente primeiro, e o total no cabeçalho `X-Total-Count` (constantes em `SmartStorage.Shared/VO/Pagination.cs`, `pageSize` até 100). Sem `page` o endpoint devolve a lista inteira como antes — o Insights, o roteiro e2e e as telas que montam selects dependem disso. O corpo continua sendo uma lista, então o HATEOAS e o gateway não mudam; o que muda é o CORS, que precisa expor o cabeçalho (`WithExposedHeaders` em `PolicyExtensions`), senão o navegador não o entrega ao Blazor. No front, `ReadApiPageAsync` (em `HttpResponseExtensions`) nos serviços, com `MudTable` e `ServerData` nas tabelas e `MudPagination` na tela de produtos em cards. Usam as telas de vendas, de produtos (`GET /products/v1`, por nome) e de produtos nas prateleiras (#10); `GET /shelf/v1/allocation` paginado só traz entrada com saldo, ordenada por prateleira e produto, enquanto a lista inteira continua incluindo as zeradas.

**Pastas e namespaces divergem.** Os arquivos de negócio vivem em `Business/`, mas o namespace é `SmartStorage_API.Service`. Vários projetos usam `_` no lugar do `.` no namespace (`SmartStorage_API`, `SmartStorage_Shared`). Siga o namespace do arquivo vizinho, não o nome da pasta.

### HATEOAS

`HyperMediaFilter` é aplicado por `[TypeFilter(typeof(HyperMediaFilter))]` em cada action que retorna recurso. Ele percorre `HyperMediaFilterOptions.ContentResponseEnricherList` — montada à mão no `Program.cs` da API core — e o primeiro enricher que responde `CanEnrich` injeta os `Links`. **Um VO novo que precise de links exige um enricher novo registrado no `Program.cs`.**

### Chamadas do Blazor à API

Cada domínio da API principal tem um serviço tipado, como os de IA, relatórios, e-mail e autenticação; o antigo `ApiExtensions` genérico, que escolhia o endpoint pelo tipo do VO, saiu com a #22. `ISaleService`/`SaleService` cobre vendas, carrinho, cancelamento e devolução; `IShelfService`/`ShelfService` cobre prateleiras e alocações (listas, página, alocação individual e em lote, desfazer com `PUT` sem corpo, transferência e inventário); `IProductService`/`ProductService` cobre produtos (o ajuste de estoque segue dentro do `PUT` do produto); `IEmployeeService`/`EmployeeService` só lista funcionários. Cada serviço expõe apenas as rotas que as telas usam.

Cada serviço tem interface em `Services/IServices`, é registrado no `Program.cs` com `AddHttpClient`, `AuthHandler` (token do `localStorage` por requisição) e `SessionExpiredHandler` (401 leva ao login), e trata a resposta com `HttpResponseExtensions`: `ReadApiAsync` lança `ApiException` com o status e a mensagem da API, `ReadApiPageAsync` lê o `X-Total-Count` e `WithPage` monta a query de paginação. Nos testes, registre o serviço sobre `api.Cliente()`.

### VOs (`SmartStorage.Shared/VO/`)

Cada VO implementa `ISupportHyperMedia`, herda `BaseMessage` (do `SmartStorage.MessageBus`, o que dá o `Id` e permite publicar o VO na fila) e expõe `static Parse(VO)` / `static ParseList(List<VO>)` retornando o Model.

### Configuração compartilhada

`builder.Configuration.AddSharedConfiguration()` (em `SmartStorage.Configurations`) carrega `appsettings.Shared.json` **e readiciona as variáveis de ambiente logo depois**. A ordem importa: o arquivo é a fonte mais recente e sobrescreveria `TokenConfigurations__Secret` vindo do compose se as env vars não voltassem ao topo. Pelo mesmo motivo, uma chave que deva vir de user secrets precisa ser **removida** de `appsettings.Shared.json`, não deixada em branco — string vazia no arquivo vence o user secret.

`AddAuthConfiguration` lança se `TokenConfigurations:Secret` estiver vazio, então configuração errada falha alto no startup.

### Mensageria

Criar produto publica o `ProductVO` na fila `sendemailqueue` (`RabbitMQMessageSender` na API core) e retorna imediatamente. `RabbitMQEmailConsumer` na EmailAPI é um `BackgroundService` que consome e só dá `BasicAck` depois do envio SMTP. Cada tipo de mensagem tem a sua fila, porque o consumidor desserializa o corpo pelo tipo da fila.

O alerta de estoque mínimo (#8) usa a fila `lowstockemailqueue` com `LowStockAlertVO`. `Product.ProMinimumStock` (0 desativa) é comparado com o **saldo total** (depósito + prateleiras) por `StockAlertBusinessImplementation`, chamado depois da venda, da edição que aumenta a venda e da alocação — esta nunca muda o total, então na prática quem avisa é a venda. O aviso só sai ao **cruzar** o limite (antes no mínimo ou acima, depois abaixo), para não repetir a cada venda com o estoque já baixo. Falha ao publicar é só logada: a venda já foi gravada e não deve voltar erro por causa do e-mail. O CT-34 confere o alerta pelos logs de `smartstorage-api` e `emailapi` e, com o `.env` preenchido, **envia um e-mail real** a cada execução do roteiro.

### AIAPI e ReportsAPI

Os dois serviços só leem o banco: referenciam `SmartStorage.Infraestructure` e consultam o `SmartStorageContext` direto de um `*Repository`, sem business, converter nem ledger. Ambos exigem token (`[Authorize]`) e são chamados só pela tela de Insights do Blazor (`Services/AiService.cs` e `Services/ReportsService.cs`), sempre pelo gateway.

- **AIAPI** — `POST /api/storage/ai/v1/analyse-sales` com `AiRequest { aiQuestion }`. `AiRepository` serializa as **10 vendas mais recentes** como entidade `Sale` crua (quantidades, datas e `SalPrice`, sem nome de produto nem prateleira, porque `Enter` não é incluído) e manda pergunta e JSON ao Gemini (`gemini-2.5-flash`, pacote `Google.GenAI`), devolvendo só o texto da primeira resposta. A chave vem de `Environment.GetEnvironmentVariable("GOOGLE_API_KEY")`, **não** do `IConfiguration`: user secrets e `appsettings` não funcionam, fora do Docker precisa ser variável de ambiente. No compose ela vem do `.env`.
- **ReportsAPI** — `GET /api/storage/reports/v1/export-excel` (ClosedXML) e `export-pdf` (QuestPDF, licença Community declarada no `Program.cs`), com gráficos do ScottPlot renderizados em PNG. Quantidades e valores são **líquidos de devolução** (`SalQntd - SalReturnedQntd`, vezes `SalPrice`), inclusive nos gráficos, e a venda totalmente devolvida fica fora, como na listagem de vendas. O recorte é o mês corrente do ano corrente, pelo `SalDateSale`; mês sem vendas responde 400 com aviso em vez de arquivo vazio. O resumo por IA dentro do PDF está comentado — é a issue #5.

O roteiro e2e não chama esses endpoints. `InsightsTests` (bUnit) cobre só o lado da tela: `AiService` e `ReportsService` lançam `ApiException` em resposta de erro, como os demais serviços, e o Insights mostra o aviso sem deixar o carregamento girando nem pôr o erro da IA no chat.

### Papéis

`SmartStorage.Shared/Auth/Role.cs` é canônico: `Admin = "Administrador"`, `Client = "Usuario"` — os valores em português, porque três `.razor` (`Home`, `Account`, `EditUser`) comparam contra os literais. Endpoints `[Authorize(Roles = Role.Admin)]` só funcionam se a claim emitida no login usar exatamente essas constantes.

## Banco

Modelos com prefixo de três letras por tabela (`ProId`, `ProName`, `EmpId`, `EntQntd`, `SalEntId`). **`Product` não tem FK para `Shelf`** — `Enter` é a tabela de junção (produto alocado em prateleira, com quantidade e preço), e `Sale` referencia `Enter`, não `Product`.

O autor de cada movimentação (`PsmUseId`, FK para `User`) e o instante (`PsmDate`) são decididos pelo servidor dentro de `ProductStockMovementRepository`: o autor sai do `unique_name` do token via `IHttpContextAccessor`, e nenhum VO carrega esses campos. A FK é `Restrict`, então a AuthenticationAPI recusa excluir usuário que já movimentou estoque. Pelo mesmo motivo **produto não tem exclusão** — nem tela, nem endpoint, nem rota no gateway: excluir apagava as movimentações junto e o histórico sumia.

Devolução de venda (`POST /sales/{id}/return`) soma em `Sale.SalReturnedQntd` e lança `Devolucao` com sinal positivo **no depósito**, não na prateleira, com motivo `Devolução da venda {id}`. A listagem de vendas mostra quantidade e total líquidos e esconde a venda totalmente devolvida, que continua no banco. Venda com devolução não pode ser cancelada nem editada abaixo do devolvido: o estorno do cancelamento voltaria para a prateleira unidades que já voltaram ao depósito.

A venda guarda o preço do momento em `Sale.SalPrice`, copiado do `EntPrice` da prateleira na criação e nunca mais alterado; o total da venda (API, Excel e PDF) sai dele. Como a alocação repreça a prateleira, calcular pelo `EntPrice` fazia o repreço reescrever o faturamento de vendas antigas. Sem FIFO nem custo por lote: um produto tem um preço por prateleira. As vendas anteriores à migration `AddSalePrice` receberam o `EntPrice` vigente na hora da migration.

O carrinho (#21) grava várias vendas num único `POST /sales/v1/batch` (`SaleBatchVO`), **tudo ou nada**: `CreateNewSales` valida todos os itens antes de escrever — entrada existente, quantidade positiva e saldo da entrada somando os itens repetidos dela — e responde 400 com `Item N (produto na prateleira): motivo` sem gravar nada; a gravação corre numa transação, com o mesmo `PsmDate` em todos os lançamentos. Cada item continua uma `Sale` independente (não há pedido), e o alerta de estoque mínimo sai uma vez por produto, depois do commit. No Blazor o carrinho é o `SaleCart` (scoped), salvo no `localStorage` na chave `saleCart:{usuário}` — por usuário, para sobreviver à sessão expirada sem passar para outro login no mesmo navegador. A tela `/products/sales/cart` confere preço e saldo na API ao abrir e depois de uma recusa, e só esvazia o carrinho quando a API aceita.

Transferência entre prateleiras (`POST /shelf/allocation/{enterId}/transfer`) move **todo o saldo** do `Enter` de origem para a prateleira de destino, em dois lançamentos `Transferencia` com o mesmo instante. O destino que já tem o produto mantém o próprio preço; o destino novo herda o preço da origem. O `Enter` de origem fica com saldo zero, porque as vendas apontam para ele.

O inventário cíclico (#12) é `POST /shelf/v1/{shelfId}/inventory` (`InventoryCountVO`: motivo e, por entrada, a quantidade contada), **tudo ou nada** como o lote: `CountShelfInventory` recusa entrada de outra prateleira, entrada repetida e contagem sem nenhuma diferença, com `Item N (produto): motivo`. A quantidade contada é absoluta e a diferença sai do saldo do momento no servidor, não do que a tela carregou; cada entrada com diferença vira um lançamento `Ajuste` na prateleira, todos com o mesmo instante e o motivo `Inventário da {prateleira}: {motivo}` (motivo até 180 caracteres, para caber nos 300 do `PsmReason`). Não checa capacidade — a contagem registra o que já está lá — e dispara o alerta de estoque mínimo por produto depois do commit. No Blazor, o botão Inventário de Produtos nas prateleiras leva a `/products/shelves/inventory`: escolhida a prateleira, carrega as entradas com saldo dela e envia só as linhas alteradas; numa recusa, mantém as contagens e atualiza o saldo do sistema.

Um produto só fica em **uma prateleira por vez**: `EnsureSingleShelf` recusa a alocação quando o produto tem saldo (`EntQntd > 0`) em outra prateleira. Complementar a mesma prateleira continua permitido, e `Enter` zerado não conta, então depois de desfazer a alocação ou transferir o saldo o produto pode ir para outra prateleira. A transferência não passa por essa checagem, porque move o saldo inteiro e o produto continua numa prateleira só.

A alocação em lote (#20) é `POST /shelf/v1/allocation/batch` (`AllocationBatchVO`), **tudo ou nada** como o carrinho: `AllocateProductsToShelves` valida todos os itens antes de escrever — produto e prateleira existentes, produto repetido no lote (recusado), quantidade e preço positivos, saldo do depósito, `EnsureSingleShelf` e `EnsureShelfFits` com o volume dos itens anteriores do lote na mesma prateleira já descontado — e responde `Item N (produto na prateleira): motivo`. Grava numa transação, dois lançamentos `Alocacao` por item com o mesmo instante, e repreça cada prateleira como a alocação individual. `Product.ProPrecoInicial` (`decimal(18,2)`, opcional, `PrecoInicial` no VO) é só o preço padrão das telas de alocação e nunca muda ao alocar; a migration `AddProductInitialPrice` copiou para os produtos existentes o `EntPrice` da prateleira com saldo, ou da entrada mais recente. No Blazor, os cards de Produtos em estoque têm seleção e levam a `/product/allocation/batch?ids=…`, que abre cada produto com quantidade 1, o preço inicial e a prateleira onde ele já tem saldo, e avisa teto somado e segunda prateleira — quem recusa é a API.

Capacidade de prateleira (#11) é por **volume em litros**: `Product.ProVolume` e `Shelf.SheVolume`, `decimal(18,3)` e opcionais. O teto útil é **90% do volume da prateleira** (`ShelfVO.UsableFraction`), porque volume não considera encaixe. `EnsureShelfFits` em `ShelfBusinessImplementation` recusa a alocação e a transferência quando o produto ou a prateleira destino não têm volume, ou quando `quantidade × volume do produto` passa do que resta do teto — a prateleira no limite não aceita mais nada. O volume usado é `SUM(EntQntd × ProVolume)` das entradas da prateleira, calculado em lote no `ShelfConverter`, que devolve `volume`, `usedVolume`, `usableVolume`, `freeVolume` e `occupancy` no `ShelfVO`. Cancelar venda e desfazer alocação não checam capacidade: devolvem o que já estava lá. Não há tela de cadastro de prateleiras; o volume entra pelo POST/PUT de `/shelf/v1` e pelo seed. A tela de alocação sempre busca as prateleiras na API, porque a ocupação em cache ficaria velha.

O seed vive em `Context/Seed/SeedData.cs` via `HasData` no `OnModelCreating`, então viaja com as migrations e é aplicado pelo `migrator`. Todas as datas do seed são `static readonly` constantes: qualquer `DateTime.Now` em `HasData` faz o EF detectar mudança de modelo a cada `migrations add`. As imagens dos produtos são PNGs em base64 em `SeedImages.cs`, porque `Product.ProImage` é `varbinary(max)`.

Logins do seed: `admin` / `admin123` (Administrador) e `usuario` / `usuario123` (Usuário comum, para exercitar o que é barrado fora do papel de admin). A senha é SHA-256 puro sem salt (`Sha256PasswordHasher`).

## Docker

Todo Dockerfile de serviço .NET segue o mesmo template: `sdk:10.0` → `aspnet:10.0`, `useradd --no-create-home appuser` (a imagem aspnet não tem `adduser`), `EXPOSE 8080`. Mantenha o padrão ao adicionar serviço.

O Blazor é WebAssembly: a imagem final é `nginx-unprivileged:alpine` servindo `wwwroot`, com `try_files $uri $uri/ /index.html`. Não adicione bloco `types { }` no `nginx.conf` — a `mime.types` da imagem já mapeia `application/wasm`, e um `types` em contexto de server substitui a tabela inteira. **A configuração do WASM é escolhida no build** (`cp appsettings.Docker.json appsettings.json` no Dockerfile), não por `ASPNETCORE_ENVIRONMENT`: um ambiente novo exige imagem nova.

Healthchecks: a âncora `x-dotnet-healthcheck` abre `/dev/tcp` pelo bash porque as imagens aspnet não têm curl nem wget. O check do SQL Server exige que nenhum banco de usuário esteja fora de `ONLINE` (um `SELECT 1` no `master` responde antes do banco da aplicação terminar a recuperação) e o `-b` do `sqlcmd` é obrigatório, senão erro de T-SQL sai com código 0.

O SQL Server e os serviços .NET rodam com `TZ: America/Sao_Paulo`. O Blazor manda as datas com o fuso do navegador (`-03:00`) e o model binding converte para a hora local do container: em UTC, a venda das 09:00 era gravada como 12:00, e `DateTime.Now` e `GETDATE()` carimbavam o ledger três horas adiantado. Linhas gravadas antes dessa mudança continuam em UTC.

Env var só entra no container na **criação**: depois de mexer no `.env`, `docker compose up -d --force-recreate <serviço>`. E `docker compose exec <svc> printenv` **não** prova o ambiente do processo rodando — leia `/proc/1/environ` dentro do container.

Antes de publicar imagens, sempre `docker compose build` — uma imagem local velha já quase subiu com um segredo removido do código.

## CI

`.github/workflows/ci.yml`: job `build` (restore + build Release da solution) e job `images` (`docker compose build`, login e push só em `push`, nunca em PR). A tag é o SHA curto, e o push enumera as imagens próprias com `docker compose config --images | grep '^lelep0/'`.

`SmartStorage.MessageBus` ainda tem como alvo `net6.0` enquanto o resto é `net10.0`. Compila sob o SDK 10, só emite aviso de fora de suporte — **não transforme warnings em erros no CI**.

## Convenções

- Commits em português, presente do indicativo na terceira pessoa: "Implementa o .yml de CI/CD", "Corrige o remetente e torna o destinatário do e-mail configurável".
- Comentários no código explicam **decisões não óbvias** (o porquê de uma ordem de configuração, de um healthcheck, de um `ENV`), não o que a linha faz. Vários já existem no compose e nos Dockerfiles; siga esse padrão em vez de narrar o código.
- Mensagens de validação e de exceção são em português.
- `#region Propriedades / Construtores / Métodos` é o padrão nas classes de negócio e controllers.
