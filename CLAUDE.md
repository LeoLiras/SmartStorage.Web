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

**Não existe projeto de testes na solution.** Não há `dotnet test` a rodar. A verificação é o build mais o roteiro `tests/ledger_e2e.py`, um script Python de biblioteca padrão que entra pelo gateway como cliente e confere o banco a cada passo:

```bash
python tests/ledger_e2e.py              # os 20 casos, ~5 min
python tests/ledger_e2e.py --caso CT-08 # um caso só
python tests/ledger_e2e.py --manter     # preserva os produtos criados, para inspeção
```

Exige o stack de pé. Cria os próprios produtos (prefixo `ZZ Ensaio Automatizado` mais um identificador por execução, para ser reexecutável) e remove o que criou, varrendo também sobras de execuções anteriores. Depois de cada operação confere três coisas: as linhas que o ledger gravou (local, tipo e quantidade com sinal), a invariante `saldo_depois == saldo_antes + SUM(PsmQntd)` por produto e por local, e que o `PsmDate` esteja no relógio do servidor — este último por causa de uma regressão em que a venda gravava o horário local do navegador, três horas atrás das demais movimentações. No fim procura saldo negativo, movimentação órfã e venda sem entrada. Devolve exit code não-zero em falha, mas **não está no CI**: o workflow só builda e publica imagens.

O roteiro bate na API, não no Blazor — bugs que vivem só no front, no que a tela monta e envia, passam por ele sem serem notados. Dois já aconteceram assim: `ProductId` zero vindo do estado global (os holders de `VariablesExtensions` eram inicializados com `new()`, o que anulava todo `is null`) e a data perdendo o `Kind` no `MudDatePicker`. Para essa classe, só teste manual pelo front ou bUnit.

Dois resultados do roteiro não são falha e são esperados: `CT-19` sai como lacuna, porque excluir produto apaga as movimentações em vez de registrar a saída, e `CT-20` sai pulado, porque `TransferProductBetweenLocations` aceita prateleira nos dois lados mas nenhum endpoint ou tela chama assim.

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

O Blazor não conhece o endereço de nenhum serviço — as cinco chaves `ServiceUrls:*` em `wwwroot/appsettings*.json` apontam todas para o gateway (4480). São 35 rotas em `SmartStorage.APIGateway/appsettings.json` (dev) e `appsettings.Docker.json` (compose, ativado por `ASPNETCORE_ENVIRONMENT: Docker`).

**Os dois arquivos precisam declarar as rotas na mesma ordem.** A configuração JSON do .NET faz merge de arrays por índice, então `appsettings.Docker.json` só sobrescreve corretamente se cada rota estiver na mesma posição e com todos os campos redeclarados. Ao adicionar um endpoint, edite os dois.

Cuidado com as portas: as rotas de **dev** apontam para as portas **HTTPS** dos serviços (5100, 5102, 5104, 5106, 5202), que são diferentes das portas publicadas pelo compose (5100, 5103, 5105, 5107, 5202).

### Camadas da API core (`SmartStorage-API/`)

`Controller → Business → SmartStorageContext`, com `Converter` traduzindo Model ↔ VO. Não há repositório aqui — as `*BusinessImplementation` recebem o `DbContext` direto e instanciam o converter no construtor. A AuthenticationAPI é a exceção: ela tem `Repositories/` com um `GenericRepository`.

**Pastas e namespaces divergem.** Os arquivos de negócio vivem em `Business/`, mas o namespace é `SmartStorage_API.Service`. Vários projetos usam `_` no lugar do `.` no namespace (`SmartStorage_API`, `SmartStorage_Shared`). Siga o namespace do arquivo vizinho, não o nome da pasta.

### HATEOAS

`HyperMediaFilter` é aplicado por `[TypeFilter(typeof(HyperMediaFilter))]` em cada action que retorna recurso. Ele percorre `HyperMediaFilterOptions.ContentResponseEnricherList` — montada à mão no `Program.cs` da API core — e o primeiro enricher que responde `CanEnrich` injeta os `Links`. **Um VO novo que precise de links exige um enricher novo registrado no `Program.cs`.**

### VOs (`SmartStorage.Shared/VO/`)

Cada VO implementa `ISupportHyperMedia`, herda `BaseMessage` (do `SmartStorage.MessageBus`, o que dá o `Id` e permite publicar o VO na fila) e expõe `static Parse(VO)` / `static ParseList(List<VO>)` retornando o Model. O Blazor chama esses métodos **por reflexão** em `Utils/API/ApiExtensions.cs`, então mudar a assinatura quebra o front em runtime, sem erro de compilação.

### Configuração compartilhada

`builder.Configuration.AddSharedConfiguration()` (em `SmartStorage.Configurations`) carrega `appsettings.Shared.json` **e readiciona as variáveis de ambiente logo depois**. A ordem importa: o arquivo é a fonte mais recente e sobrescreveria `TokenConfigurations__Secret` vindo do compose se as env vars não voltassem ao topo. Pelo mesmo motivo, uma chave que deva vir de user secrets precisa ser **removida** de `appsettings.Shared.json`, não deixada em branco — string vazia no arquivo vence o user secret.

`AddAuthConfiguration` lança se `TokenConfigurations:Secret` estiver vazio, então configuração errada falha alto no startup.

### Mensageria

Criar produto publica o `ProductVO` na fila `sendemailqueue` (`RabbitMQMessageSender` na API core) e retorna imediatamente. `RabbitMQEmailConsumer` na EmailAPI é um `BackgroundService` que consome e só dá `BasicAck` depois do envio SMTP.

### Papéis

`SmartStorage.Shared/Auth/Role.cs` é canônico: `Admin = "Administrador"`, `Client = "Usuario"` — os valores em português, porque três `.razor` (`Home`, `Account`, `EditUser`) comparam contra os literais. Endpoints `[Authorize(Roles = Role.Admin)]` só funcionam se a claim emitida no login usar exatamente essas constantes.

## Banco

Modelos com prefixo de três letras por tabela (`ProId`, `ProName`, `EmpId`, `EntQntd`, `SalEntId`). **`Product` não tem FK para `Shelf`** — `Enter` é a tabela de junção (produto alocado em prateleira, com quantidade e preço), e `Sale` referencia `Enter`, não `Product`.

O seed vive em `Context/Seed/SeedData.cs` via `HasData` no `OnModelCreating`, então viaja com as migrations e é aplicado pelo `migrator`. Todas as datas do seed são `static readonly` constantes: qualquer `DateTime.Now` em `HasData` faz o EF detectar mudança de modelo a cada `migrations add`. As imagens dos produtos são PNGs em base64 em `SeedImages.cs`, porque `Product.ProImage` é `varbinary(max)`.

Logins do seed: `admin` / `admin123` (Administrador) e `usuario` / `usuario123` (Usuário comum, para exercitar o que é barrado fora do papel de admin). A senha é SHA-256 puro sem salt (`Sha256PasswordHasher`).

## Docker

Todo Dockerfile de serviço .NET segue o mesmo template: `sdk:10.0` → `aspnet:10.0`, `useradd --no-create-home appuser` (a imagem aspnet não tem `adduser`), `EXPOSE 8080`. Mantenha o padrão ao adicionar serviço.

O Blazor é WebAssembly: a imagem final é `nginx-unprivileged:alpine` servindo `wwwroot`, com `try_files $uri $uri/ /index.html`. Não adicione bloco `types { }` no `nginx.conf` — a `mime.types` da imagem já mapeia `application/wasm`, e um `types` em contexto de server substitui a tabela inteira. **A configuração do WASM é escolhida no build** (`cp appsettings.Docker.json appsettings.json` no Dockerfile), não por `ASPNETCORE_ENVIRONMENT`: um ambiente novo exige imagem nova.

Healthchecks: a âncora `x-dotnet-healthcheck` abre `/dev/tcp` pelo bash porque as imagens aspnet não têm curl nem wget. O check do SQL Server exige que nenhum banco de usuário esteja fora de `ONLINE` (um `SELECT 1` no `master` responde antes do banco da aplicação terminar a recuperação) e o `-b` do `sqlcmd` é obrigatório, senão erro de T-SQL sai com código 0.

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
