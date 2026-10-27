# Constituição do projeto

Regras que valem para toda especificação, plano e tarefa do SmartStorage. O
`CLAUDE.md` da raiz explica o código como ele é; esta constituição diz o que
nenhuma mudança pode quebrar.

## Stack
- .NET 10 e C# em todos os serviços, exceto o `SmartStorage.MessageBus`, que
  ainda tem como alvo `net6.0`
- Front em Blazor WebAssembly com MudBlazor, servido por nginx
- API core em controllers ASP.NET Core versionados (`Asp.Versioning`), com
  HATEOAS
- SQL Server com EF Core; migrations em `SmartStorage.Infraestructure`
- RabbitMQ para mensageria entre serviços, MailKit para e-mail
- Gateway Ocelot como única porta de entrada do navegador
- Docker Compose para o stack inteiro, com imagens publicadas em
  `hub.docker.com/u/lelep0`
- Nenhuma dependência nova entra sem justificativa escrita no plano

## Arquitetura e camadas
- Cada responsabilidade vive em um serviço próprio, com sua imagem Docker:
  AuthenticationAPI, API core, ReportsAPI, AIAPI, EmailAPI, gateway e Blazor
- O Blazor só conhece o gateway; rota nova entra no `appsettings.json` e no
  `appsettings.Docker.json` do gateway, na mesma posição dos dois arquivos, e
  na coleção do Postman em `docs/postman/`; rota alterada ou removida muda
  nos três no mesmo commit
- Na API core o fluxo é `Controller → Business → SmartStorageContext`, com
  `Converter` traduzindo Model ↔ VO:
  - o controller valida a entrada, chama o business e traduz o resultado em
    resposta; não tem regra de negócio
  - o business tem as regras e recebe o `DbContext` direto, sem repositório
    genérico; a exceção é o ledger
  - o converter que precisa de outra tabela resolve a lista inteira em lote,
    nunca uma consulta por item
- AIAPI e ReportsAPI só leem o banco, por um `*Repository`, sem business nem
  ledger
- O que vale para todos os serviços (autenticação, checagem da sessão,
  tratamento de erro) vive no `SmartStorage.Configurations`; a checagem da
  sessão consulta direto o usuário do token, por índice único, e é a única
  leitura de banco fora do `*Repository` na AIAPI e na ReportsAPI
- Comunicação entre serviços que não precisa de resposta vai pela fila do
  RabbitMQ, uma fila por tipo de mensagem; a requisição não espera o consumidor
- Todo efeito de estoque passa pelo `ProductStockMovementRepository`, que
  grava a linha em `ProductStockMovement`, com sinal, e atualiza na mesma
  gravação o saldo materializado (`ProQntd` no depósito, `EntQntd` na
  prateleira); nenhum outro código escreve nesses saldos, e o saldo de cada
  local é sempre `SUM(PsmQntd)` daquele local
- Operação com vários itens é tudo ou nada: valida todos antes de escrever,
  grava numa transação e responde `Item N (...): motivo` na recusa
- Histórico não se apaga: produto não tem exclusão, e usuário que movimentou
  estoque não pode ser excluído
- Nenhuma aplicação aplica migration em runtime; quem aplica é o container
  `migrator`, e o seed viaja nas migrations
- O Blazor fala com cada domínio por um serviço tipado com interface,
  registrado com `AuthHandler` e `SessionExpiredHandler`, que lança
  `ApiException` em resposta de erro

## Padrão de código
- VO novo que espelha uma entidade implementa `ISupportHyperMedia`, herda
  `BaseMessage` e expõe `Parse`/`ParseList`; se precisar de links, ganha um
  enricher registrado no `Program.cs`. VO que só responde um agregado
  calculado, sem entidade por trás (como o dashboard), dispensa o
  `Parse`/`ParseList`
- Listagem paginada é opcional por query string (`page`, `pageSize` até 100,
  `search`) e devolve o total em `X-Total-Count`; sem `page`, devolve a lista
  inteira
- `#region Propriedades / Construtores / Métodos` em business e controllers
- Comentário só explica decisão não óbvia, nunca narra o código
- Datas de seed são constantes; `DateTime.Now` em `HasData` é proibido
- Warnings não viram erro no build

## Testes e cobertura mínima
- Três camadas, cada uma cobrindo o que as outras não veem:
  - **unidade** (`SmartStorage.API.Tests` e o projeto `.Tests` de cada
    serviço, xUnit): funções puras, como parser, cálculo e validação
  - **componente** (`SmartStorage.Blazor.Tests`, bUnit e NSubstitute): o que
    a tela monta e envia, afirmando sobre o JSON serializado na `ApiFalsa`
  - **fim a fim** (`tests/ledger_e2e.py`): o ledger pelo gateway, conferindo
    as linhas gravadas, a invariante de saldo, o `PsmDate` e o `PsmUseId`
- Mínimo exigido de cada mudança:
  - regra de negócio nova ou alterada tem teste de unidade, ou caso no
    roteiro quando depende do banco
  - toda regra que mexe no ledger tem caso no roteiro, com o caminho feliz e
    ao menos uma recusa
  - tela nova ou alterada tem teste bUnit do envio e do erro da API
  - bug corrigido ganha o teste que o teria pego
- O CI roda build e `dotnet test` da solution a cada push e PR; o roteiro roda
  local, com o stack de pé, antes de cada merge que toca estoque
- Nenhum teste é removido para a suíte passar, e qualquer caso do roteiro
  diferente de `PASSOU` é regressão
- Mudança de tela é conferida no navegador, em largura de desktop e de
  celular, antes do commit

## Segurança e tratamento de dados
- Toda API exige JWT, exceto login, refresh e, a partir da #29, os endpoints
  de confirmação de e-mail e de recuperação de senha
- A sessão dura no máximo 9 horas desde o login: o token vale 60 minutos e o
  Blazor o renova pelo refresh até esse limite, que a renovação não estende;
  sair revoga o refresh token no servidor
- Papel é verificado na API com `[Authorize(Roles = ...)]` e as constantes de
  `Role.cs`; esconder o botão no Blazor é conveniência, não proteção
- Endpoint que lê ou altera dado de outro usuário exige administrador
- Cadastrar e excluir prateleiras e colaboradores exige administrador
- Identidade, autor e instante vêm do token e do relógio do servidor, nunca do
  corpo da requisição
- Segredos nunca entram no repositório: no Docker vêm do `.env` (ignorado pelo
  git), fora dele de user secrets; os `appsettings.json` deixam as chaves
  vazias, e configuração faltando derruba o serviço no startup
- Chaves de serviços externos, como a do Gemini, ficam no servidor; o
  WebAssembly nunca as recebe
- CORS só libera as origens configuradas
- Acesso ao banco só pelo EF com parâmetros; SQL montado por concatenação de
  texto é proibido
- Senha, hash de senha, token e chave nunca aparecem em resposta, log ou
  mensagem de erro; a API responde com VO, nunca com a entidade do banco
- A API não recebe entidade do banco no corpo da requisição: cada endpoint
  recebe um VO só com os campos que o usuário pode alterar
- Erro inesperado responde 500 com mensagem genérica e vai para o log; só a
  recusa de regra de negócio devolve o motivo ao cliente
- Endpoint anônimo responde igual exista o recurso ou não, para não revelar
  usuários
- Token enviado por e-mail é guardado só como hash, de uso único e com prazo
- Dados pessoais guardados se limitam ao necessário para operar: nome e
  usuário de quem acessa (e, a partir da #29, o e-mail); nome, CPF e RG dos
  colaboradores; CNPJ ou CPF e nome do fornecedor, que ficam com a nota
- CPF e RG de colaborador só aparecem nas telas e endpoints de colaborador,
  nunca em relatório, e-mail ou contexto enviado à IA
- A IA recebe só dados de venda e produto, sem dado pessoal
- Imagem de produto só em PNG ou JPEG, redimensionada no navegador e
  limitada a 10 MB
- Toda validação existe na API; o Blazor repete para dar retorno rápido, mas
  quem recusa é a API
- O hash de senha atual (SHA-256 sem salt) é dívida conhecida; nenhum código
  novo o copia para outro fim
- Onde o código atual descumpre esta seção, a violação está listada em
  "Lacunas conhecidas" da `spec.md` e é corrigida antes de feature nova na
  mesma área

## Convenções
### Nomes
- Classes, métodos, arquivos e rotas em inglês; mensagens de validação,
  exceção e interface em português
- Tabelas com nome em inglês e colunas com prefixo de três letras por tabela
  (`ProId`, `SalEntId`); prefixo novo não pode repetir um existente
- Enums de domínio em português (`TipoMovimentacao`, `TipoUsuario`), como os
  existentes
- Namespaces seguem o arquivo vizinho, não a pasta (`SmartStorage_API.Service`
  dentro de `Business/`)

### Commits
- Em português, no presente do indicativo na terceira pessoa ("Implementa o
  carrinho de vendas"), um assunto por commit
- Citam a issue quando houver e, depois que a feature tiver spec, os
  requisitos (`RF-xx`, `RN-xx`) que atendem
- Mudança da constituição sai em commit separado
- Branches seguem o GitFlow: `main` só recebe merge de `release/*` e
  `hotfix/*`, `develop` integra o trabalho, e cada issue é desenvolvida numa
  `feature/<número>-<descrição>` criada a partir de `develop`
- Nada é commitado em `main` sem o build e os testes passando

### Pastas
- Raiz: `docker-compose.yml`, `.env`, `README.md`, `CLAUDE.md`, `docs/`,
  `specs/` e `tests/`
- `src/`: um projeto por serviço e a `SmartStorageWeb.sln`; todo `context` de
  build do compose é `./src`
- API core: `Controllers/`, `Business/`, `Data/Converter/`, `Hypermedia/`,
  `Repository/` (só o ledger) e `RabbitMQSender/`
- Modelos, VOs e enums compartilhados: `SmartStorage.Shared/Model`,
  `SmartStorage.Shared/VO` e `SmartStorage.Shared/Enum`
- Blazor: telas em `Pages/<Domínio>/`, serviços em `Services/` com interfaces
  em `Services/IServices/`
- Testes: `src/SmartStorage.API.Tests`, `src/SmartStorage.Blazor.Tests`, um
  `src/SmartStorage.<Serviço>.Tests` por serviço que tenha regra própria (a
  partir da #30 o da AuthenticationAPI e o do `SmartStorage.Configurations`,
  da #36 o da ReportsAPI, da #27 o da AIAPI, da #29 o da EmailAPI e da #52
  o do gateway), e o roteiro em `tests/`
- Especificações: `specs/constitution.md`, `specs/spec.md`, `specs/plan.md`
  (decisões técnicas) e `specs/tasks.md` (tarefas de cada issue)

## Governança
- Feature nova entra primeiro na `spec.md`, com requisitos numerados, e só
  depois ganha plano e tarefas; a issue no GitHub registra a discussão, a
  spec registra o que foi decidido
- Requisitos e regras nunca são renumerados: um item novo recebe o próximo
  número livre (`RF-44`), e o que sai deixa o número sem uso
- Se o plano contradiz a constituição, quem muda é o plano
- Se a constituição precisa mudar, muda em commit separado, com o motivo
- Spec que muda depois de implementada é atualizada no mesmo commit do código
