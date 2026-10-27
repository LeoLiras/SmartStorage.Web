# SmartStorage

Especificação do produto. O sistema já está em desenvolvimento: cada requisito
traz a situação entre colchetes.

- **[feito]** já implementado; o número indica a issue de origem, quando houver
- **[planejado #N]** descrito numa issue aberta no GitHub, ainda não implementado
- **[decidido]** decidido numa revisão desta spec, ainda sem issue e sem
  implementação; a issue registra a discussão quando for aberta

O que o código ainda faz de errado fica em "Lacunas conhecidas", no fim, fora
dos requisitos, apontando o requisito que o corrige quando ele já existe.

## Problema
Um almoxarifado pequeno controla estoque em planilha ou de memória. Não sabe
quanto tem de cada produto nem onde está, não descobre quem mexeu no saldo nem
quando, vê o faturamento das vendas antigas mudar quando o preço muda e só
percebe a falta do produto quando o cliente pede.

## Objetivo
Controlar o ciclo do produto, da entrada da mercadoria à venda: depósito,
prateleiras com capacidade, vendas e devoluções. Cada movimentação fica
registrada com autor e instante, o saldo de qualquer local se reconstrói pelo
histórico, e o gestor recebe relatórios, alertas de estoque baixo e análise
das vendas por IA.

## Usuários
- **Operador** (papel `Usuario`): cadastra produtos, recebe mercadoria, aloca,
  transfere, vende, devolve, faz inventário e consulta relatórios
- **Administrador** (papel `Administrador`): tudo o que o operador faz, mais
  cadastrar, editar e excluir usuários, e cadastrar, editar e excluir
  prateleiras e colaboradores [planejado #32]
- **Administrador geral** (papel `AdministradorGeral`) [planejado #28]:
  cadastra empresas e gerencia os administradores de cada uma, sem ver o
  estoque nem as vendas delas (RF-49)
- **Destinatário dos avisos**: recebe por e-mail o cadastro de produto e o
  alerta de estoque baixo; hoje é um endereço único configurado no sistema, e
  com empresas passa a ser os administradores de cada uma (RF-48)

## Histórias
- Como operador, quero importar o XML da NF-e para dar entrada na mercadoria
  sem digitar item por item
- Como operador, quero alocar vários produtos nas prateleiras de uma vez, sem
  passar do espaço que cada uma tem
- Como operador, quero juntar itens de prateleiras diferentes num carrinho e
  registrar a venda inteira de uma vez
- Como operador, quero registrar a devolução parcial de uma venda
- Como operador, quero contar uma prateleira e corrigir o saldo com um motivo
  registrado
- Como gestor, quero saber quem mexeu no estoque, quando e por quê
- Como gestor, quero ser avisado por e-mail quando um produto ficar abaixo do
  mínimo
- Como gestor, quero o relatório do mês em Excel e PDF e perguntar à IA sobre
  as vendas
- Como administrador, quero controlar quem acessa o sistema e com qual papel
- Como operador, quero trabalhar o turno inteiro sem entrar de novo
  [planejado #31]
- Como usuário, quero recuperar minha senha sozinho pelo e-mail [planejado #29]
- Como empresa, quero ver só os meus dados quando outras empresas usam o mesmo
  sistema [planejado #28]
- Como operador, quero ler o código de barras pela câmera [planejado #13]

## Fluxo principal
O caminho da mercadoria, da compra à venda. Cada passo que muda saldo grava a
movimentação com autor e instante.

1. O operador entra com usuário e senha e cai na home, com o menu do seu papel
2. **Entrada**: importa o XML da NF-e do fornecedor, confere a prévia (itens
   reconhecidos pelo código de barras, itens novos, fator e quantidade) e
   confirma; os produtos novos são criados e o saldo entra no depósito. Sem
   nota, cadastra o produto à mão com a quantidade inicial
3. **Alocação**: em Produtos em estoque, seleciona os produtos e os aloca nas
   prateleiras com quantidade, preço e responsável; o saldo sai do depósito e
   entra na prateleira, dentro do espaço livre dela
4. **Venda**: em Produtos nas prateleiras, põe os itens no carrinho, confere
   o total, escolhe a data (hoje ou um dia anterior do mês) e registra; o saldo sai das prateleiras e o preço
   fica congelado em cada venda
5. **Alerta**: se a venda deixou um produto abaixo do mínimo, o destinatário
   recebe um e-mail
6. **Pós-venda**: na listagem de vendas, registra a devolução (o saldo volta
   ao depósito) ou cancela a venda sem devolução (o saldo volta à prateleira)
7. **Manutenção**: transfere o saldo entre prateleiras, desfaz alocações e
   conta as prateleiras no inventário, corrigindo diferenças com motivo
8. **Análise**: no Insights, vê os gráficos, exporta o relatório do mês em
   Excel ou PDF e pergunta à IA sobre as vendas

## Requisitos funcionais

### Acesso
- RF-01 [feito]: login por usuário e senha devolve JWT, válido por 60
  minutos, e refresh token; a AuthenticationAPI tem refresh e revoke, mas o
  Blazor ainda não os usa (RF-44)
- RF-44 [planejado #31]: a sessão dura até 9 horas desde o login; o Blazor
  renova o token pelo refresh sem o operador perceber, a renovação não estende
  esse limite, e sair revoga o refresh token no servidor
- RF-02 [feito]: toda API, exceto login e refresh, exige token válido;
  resposta 401 que o refresh não resolve leva o Blazor de volta ao login com o
  aviso de sessão expirada. Pela #29, a confirmação de e-mail e a recuperação
  de senha também são anônimas (RN-35)
- RF-03 [feito]: o administrador cadastra, edita, lista e exclui usuários e
  define o papel de cada um
- RF-03a [planejado #30]: cada usuário altera o próprio nome completo e a
  própria senha, informando a senha atual; username e papel só mudam pelo
  administrador. Hoje a tela existe, mas o que ela chama só aceita
  administrador
- RF-04 [feito]: a interface esconde as ações fora do papel, e a API verifica
  o papel de novo
- RF-41 [planejado #30]: os endpoints de usuário respondem só com id,
  usuário, nome completo e papel (com a #33, também se está ativo; com a
  #29, o e-mail e se está confirmado), nunca com hash de senha nem refresh
  token;
  listar e buscar usuários por id exige administrador, `user-by-username` sai
  da API e do gateway, e cada usuário lê os próprios dados por
  `GET /api/auth/v1/me`
- RF-42 [planejado #30]: a atualização de usuário recebe só os campos que
  podem mudar, nunca o refresh token
- RF-43 [planejado #30]: o refresh emite as claims a partir do usuário no
  banco, então a troca de papel vale no próximo refresh
- RF-46 [planejado #33]: excluir ou desativar o usuário, desativar a empresa e
  redefinir a senha encerram as sessões dele na hora: a próxima requisição
  recebe 401, mesmo com o token ainda no prazo. Redefinir a senha é tanto o
  administrador definir a senha de outro usuário quanto o "Esqueci minha
  senha"; o próprio usuário trocar a senha pela conta não derruba a sessão
  dele. A troca de papel não encerra a sessão e vale no próximo refresh, em
  até 60 minutos (RF-43). A empresa depende da #28, e o "Esqueci minha senha"
  da #29

### Produtos
- RF-05 [feito]: cadastro de produto com nome, descrição, imagem (PNG ou
  JPEG), colaborador responsável obrigatório, volume em litros, estoque
  mínimo, preço inicial, código de barras (GTIN), fator de conversão e
  quantidade inicial, que entra no depósito como movimentação `Entrada`
- RF-06 [feito]: listagem com saldo total, saldo do depósito e saldo nas
  prateleiras, com busca por nome e paginação
- RF-07 [feito]: edição dos dados do produto sem tocar no saldo; a correção do
  saldo do depósito é uma ação própria, em que o operador informa o saldo real
  e o motivo, e o sistema lança a diferença como `Ajuste` (#6)
- RF-08 [feito]: cadastrar produto pela tela avisa o destinatário por
  e-mail, sem a tela esperar o envio; o produto criado pela NF-e não avisa
- RF-08a [feito]: colaboradores (nome, CPF único e RG) são cadastrados,
  editados e excluídos pela API e pelo seed; o Blazor só os lista, para
  escolher o responsável
- RF-45 [planejado #32]: só o administrador cadastra e exclui prateleiras e
  colaboradores; a leitura continua aberta a qualquer usuário logado
- RF-47 [planejado #32]: a edição de prateleiras e colaboradores também exige
  administrador, como o cadastro, porque o volume da prateleira é o que limita
  a alocação (RN-08, RN-40)
- RF-53 [planejado #51]: a API confere o que hoje só o Blazor confere: a
  imagem do produto é PNG ou JPEG, pela assinatura do arquivo, e tem até
  10 MB, no cadastro, na edição e no produto novo da NF-e; e a alocação
  individual recusa preço menor ou igual a zero, como o lote

### Entrada de mercadoria (NF-e) (#25)
- RF-09 [feito]: o operador envia o XML da NF-e modelo 55 e recebe uma prévia
  com número, série, fornecedor, chave de acesso e itens, sem gravar nada; se
  a nota já foi importada, a prévia mostra quando
- RF-10 [feito]: cada item da prévia vem ligado ao produto cujo código é o
  `cEAN` do item, com a quantidade sugerida `qCom × fator de conversão`
- RF-11 [feito]: na prévia o operador liga o item a um produto existente ou
  cria um produto novo, e corrige fator e quantidade de entrada
- RF-12 [feito]: a importação lança uma entrada no depósito por item, cria os
  produtos novos e grava a nota e seus itens; o produto existente passa a usar
  o fator do item e, se não tinha código, recebe o da nota
- RF-13 [feito]: a importação recalcula o custo médio ponderado do produto

### Prateleiras e alocação
- RF-14 [feito]: prateleira tem nome e volume em litros, e mostra volume
  usado, livre e ocupação (#11); é cadastrada, editada e excluída só pela API
  e pelo seed, e a exclusão é recusada se a prateleira já teve entradas;
  cadastro e exclusão passam a exigir administrador pelo RF-45, e a edição
  pelo RF-47
- RF-15 [feito]: alocação move quantidade do depósito para uma prateleira, com
  preço de venda e colaborador responsável
- RF-16 [feito]: alocação em lote de vários produtos selecionados nos cards,
  cada um com quantidade, preço e prateleira (#20)
- RF-17 [feito]: desfazer alocação devolve o saldo da prateleira ao depósito
- RF-18 [feito]: transferência move todo o saldo de uma entrada para outra
  prateleira
- RF-19 [feito]: inventário de uma prateleira recebe a contagem física por
  entrada e lança o ajuste das diferenças, com motivo (#12)
- RF-20 [feito]: listagem dos produtos nas prateleiras, com busca e paginação

### Vendas
- RF-21 [feito]: venda de uma entrada de prateleira, com quantidade e data;
  os limites da data estão na RN-39
- RF-22 [feito]: carrinho com itens de prateleiras diferentes, quantidade
  editável, total, e registro de todos os itens num único envio (#21)
- RF-23 [feito]: o carrinho fica salvo no navegador, por usuário, até ser
  registrado ou limpo
- RF-24 [feito]: listagem de vendas com quantidade e total líquidos de
  devolução, busca e paginação
- RF-25 [feito]: edição da quantidade e cancelamento de venda, com estorno
  para a prateleira; o cancelamento apaga a venda, e o ledger guarda a venda e
  o estorno
- RF-26 [feito]: devolução parcial ou total de venda, que volta ao depósito

### Histórico e alertas
- RF-27 [feito]: toda movimentação (entrada, alocação, venda, perda, ajuste,
  devolução, transferência) grava local, tipo, quantidade com sinal, autor,
  instante e motivo (#6, #7)
- RF-28 [feito]: alerta por e-mail quando uma operação que reduz o saldo
  total do produto o leva para abaixo do estoque mínimo (RN-28): venda,
  carrinho, edição que aumenta a venda e inventário (#8). O ajuste do depósito
  também avalia o alerta [planejado #38], inclusive quando registra uma perda;
  a alocação não, porque não muda o saldo total
- RF-48 [planejado #39]: os avisos de produto novo e de
  estoque baixo de uma empresa vão para os administradores dela com e-mail
  confirmado, e não mais para um endereço único

### Relatórios e IA
- RF-29 [feito]: exportação das vendas do mês corrente em Excel e em PDF; o
  PDF traz os 10 produtos mais vendidos do mês e o total por mês do ano
- RF-50 [planejado #36]: o gestor escolhe o mês e o ano do relatório, com o mês
  corrente como padrão, para tirar o fechamento depois que o mês acabou
- RF-30 [feito]: tela de Insights com gráficos e um chat que responde
  perguntas sobre as 10 vendas mais recentes pelo Gemini
- RF-51 [planejado #46]: o Insights é um dashboard do mês escolhido, com
  indicadores (faturamento e variação sobre o mês anterior, unidades, ticket
  médio, valor do estoque a custo médio, produtos abaixo do mínimo e ocupação
  média) e gráficos de vendas por dia, produtos mais vendidos em unidades,
  vendidas e devolvidas por mês, ocupação das prateleiras, dias de cobertura e
  capital parado, todos líquidos de devolução
- RF-52 [planejado #50]: a venda guarda o custo do produto no momento, e o
  dashboard mostra a margem bruta do mês sobre as vendas com custo, dizendo
  quantas ficaram de fora
- RF-31 [planejado #5]: o PDF inclui um resumo das vendas escrito pela IA
- RF-32 [planejado #27]: o provedor de IA é escolhido por configuração entre
  Gemini e um modelo local no Ollama
- RF-33 [planejado #27]: a IA recebe as 100 vendas mais recentes, com nomes de
  produto e prateleira em vez de IDs, e agregados já calculados por produto e
  por mês, sem dado pessoal

### Código de barras (#13)
- RF-34 [planejado #13]: o operador lê o código de barras pela câmera do
  dispositivo para localizar o produto

### Empresas (#28)
- RF-35 [planejado #28]: cadastro de empresas com nome, CNPJ e situação,
  criando junto o primeiro administrador e o colaborador padrão
- RF-36 [planejado #28]: todo usuário e todo dado pertencem a uma empresa, e
  a aplicação só mostra os dados da empresa do usuário logado, inclusive
  relatórios, IA e alertas
- RF-37 [planejado #28]: empresa inativa não faz login nem renova sessão, e
  as sessões abertas caem na hora (RF-46)
- RF-49 [planejado #40]: o administrador geral cria, desativa e
  redefine a senha dos administradores de qualquer empresa, sem ver o estoque,
  as vendas nem os demais usuários dela; assim a empresa cujo único
  administrador saiu não fica sem acesso

### E-mail do usuário e senha (#29)
- RF-38 [planejado #29]: o usuário tem e-mail, confirmado por link de uso
  único válido por 24 horas; trocar o e-mail exige confirmar o novo endereço,
  e até lá o antigo continua valendo
- RF-39 [planejado #29]: "Esqueci minha senha" envia um link de uso único
  válido por 30 minutos, que define a senha nova e derruba as sessões abertas
  na hora (RF-46)
- RF-40 [planejado #29]: os e-mails de confirmação, de redefinição e de aviso
  de senha alterada vão para o endereço do usuário

### Interface
- RF-54 [feito]: o Blazor tem um tema escuro único, com a tipografia, o
  esqueleto de carregamento e a transição entre telas do layout, e desliga as
  animações quando o sistema pede movimento reduzido (#19)
- RF-55 [feito]: toda tela cabe na largura do celular, inclusive as tabelas,
  sem rolagem horizontal da página (#19)
- RF-56 [planejado #47]: o sistema tem ícone próprio, legível em 16 px, na
  aba do navegador, na barra superior e no login
- RF-57 [planejado #48]: o campo só leitura se distingue do editável sem ser
  preciso clicar nele

## Regras de negócio

### Estoque e histórico
- RN-01: o saldo do depósito fica em `ProQntd` e o de cada prateleira em
  `EntQntd`; só o repositório do ledger os altera, junto com a movimentação, e
  eles sempre valem a soma das movimentações do local. O saldo total é
  depósito mais prateleiras
- RN-02: nenhum saldo fica negativo, e nenhuma movimentação tem quantidade
  zero; quem recusa é o repositório do ledger, para qualquer operação
- RN-03: autor e instante da movimentação vêm do token e do relógio do
  servidor, nunca da requisição
- RN-04: produto não é excluído, e usuário que já movimentou estoque também
  não, para o histórico não sumir
- RN-05: correção de saldo e inventário exigem motivo; o motivo do inventário
  tem até 180 caracteres
- RN-06: operação de vários itens (carrinho, alocação em lote, inventário,
  NF-e) é tudo ou nada; um item inválido recusa o envio inteiro e a resposta
  aponta o item e o motivo
- RN-07: as movimentações de uma operação de vários itens têm o mesmo instante

### Prateleiras
- RN-08: o teto útil de uma prateleira é 90% do seu volume
- RN-09: alocação e transferência são recusadas quando o produto ou a
  prateleira não têm volume, ou quando `quantidade × volume do produto` passa
  do que resta do teto; no lote, somam-se os itens que vão para a mesma
  prateleira
- RN-10: um produto tem saldo em uma prateleira por vez; complementar a mesma
  prateleira é permitido, e entrada zerada não conta
- RN-11: transferência leva todo o saldo e o preço da origem. Como o produto
  só tem saldo numa prateleira (RN-10), o destino que já tem o produto é
  sempre uma entrada zerada, de quando ele esteve lá; ela passa a usar o
  preço da origem [planejado #37], e não o preço antigo que guardava
- RN-40 [planejado #32]: o volume da prateleira não pode ser reduzido abaixo do
  volume já usado. Reduzido a um valor em que o usado passa de 90% sem passar
  de 100%, é aceito: a prateleira fica acima do teto e só recusa novas
  alocações
- RN-12: desfazer alocação e cancelar venda não checam capacidade, porque
  devolvem o que já estava lá
- RN-13: inventário não checa capacidade; a contagem é absoluta e a diferença
  é calculada sobre o saldo do momento no servidor

### Preço e custo
- RN-14: o preço de venda é por prateleira; alocar reprecifica a prateleira
- RN-15: a venda congela o preço do momento, e o faturamento de vendas antigas
  nunca muda
- RN-16: o preço inicial do produto é só o padrão das telas de alocação e não
  muda ao alocar
- RN-17: o custo do produto é a média ponderada pelo saldo total antes da
  entrada; o custo do item da nota é `vProd ÷ quantidade de entrada`
- RN-41 [planejado #50]: a venda congela o custo médio do produto no momento,
  e uma nota importada depois não muda a margem de vendas antigas; venda de
  produto sem custo, e as anteriores à #50, ficam sem custo e fora da margem

### Vendas e devoluções
- RN-18: a venda não passa do saldo da entrada; no carrinho, somam-se os itens
  repetidos da mesma entrada
- RN-19: devolução volta ao depósito, não à prateleira
- RN-20: venda com devolução não pode ser cancelada nem editada abaixo do
  devolvido
- RN-21: venda totalmente devolvida some da listagem e dos relatórios, mas
  continua no banco; a série de devolvidas do dashboard (RF-51) a conta, no
  mês da venda, para não subcontar as devoluções
- RN-22: cada item do carrinho é uma venda independente
- RN-39 [planejado #35]: a data da venda é hoje ou um dia anterior do mês
  corrente, pelo relógio do servidor; data futura ou de mês anterior é
  recusada, para uma venda nova não mudar o faturamento de um mês fechado

### NF-e
- RN-23: a mesma nota (chave de acesso) não é importada duas vezes
- RN-24: o código do produto é um GTIN de 8, 12, 13 ou 14 dígitos com dígito
  verificador válido, único quando preenchido
- RN-25: item `SEM GTIN`, sem `cEAN` ou com GTIN de dígito inválido não casa
  automaticamente
- RN-25a: o mesmo produto em dois itens da nota precisa do mesmo fator
- RN-26: produto novo sem colaborador recebe o colaborador padrão "Admin"; sem
  ele cadastrado, o item é recusado
- RN-27: só NF-e modelo 55 é aceita

### Alertas e relatórios
- RN-28: o alerta compara o mínimo com o saldo total e só sai quando a
  operação leva o saldo de igual ou acima do mínimo para abaixo dele; saldo
  igual ao mínimo não é abaixo, mínimo zero desliga o alerta e, com o estoque
  já baixo, não repete
- RN-29: falha ao enviar o alerta não desfaz a venda
- RN-30: relatórios usam quantidade e total líquidos de devolução e cobrem um
  mês de um ano: hoje só o corrente, e pelo RF-50 o escolhido; mês sem vendas
  responde com aviso, sem arquivo

### Planejadas
- RN-31 [planejado #28]: código de produto e chave de NF-e são únicos por
  empresa; username e e-mail continuam únicos no sistema todo
- RN-32 [planejado #28]: a empresa de um dado vem do token, nunca do corpo da
  requisição; ID de outra empresa é tratado como inexistente
- RN-33 [planejado #28]: empresa não é excluída, só desativada; a empresa do
  administrador geral não pode ser desativada
- RN-34 [planejado #34]: senha com no mínimo 8
  caracteres, ao menos uma letra e um número, diferente da atual; a API
  confere em todo cadastro e troca de senha, e a tela repete a mesma regra
- RN-35 [planejado #29]: os endpoints anônimos de senha e confirmação
  respondem igual exista o usuário ou não, e limitam as tentativas
- RN-36 [planejado #29]: o banco guarda só o hash do token enviado por e-mail
- RN-37 [planejado #29]: quem tem e-mail cadastrado e não confirmado não faz
  login; usuário antigo sem e-mail continua entrando
- RN-38 [planejado #30]: só a recusa de regra de negócio devolve o motivo ao
  cliente, com 400; erro inesperado responde 500 com mensagem genérica e o
  detalhe vai para o log

## Casos de borda e falhas
Regra geral: a API recusa com 400 e uma mensagem em português dizendo o
motivo, nada é gravado, e a tela mostra a mensagem num diálogo sem perder o
que o usuário já preencheu. As exceções estão descritas em cada caso.

### Sessão e acesso
- Token vencido, revogado ou adulterado: qualquer API responde 401, o Blazor
  avisa que a sessão expirou, limpa o estado e volta ao login; o carrinho fica
  salvo para o mesmo usuário. Pela #31, o token vencido dentro das 9 horas é
  renovado pelo refresh e a requisição é repetida, e só o refresh recusado
  leva ao login
- Várias requisições recebem 401 ao mesmo tempo [planejado #31]: o Blazor faz
  um refresh só, porque o refresh troca o refresh token e uma segunda
  renovação com o token antigo seria recusada
- Sair com a rede fora ou o token já vencido [planejado #31]: o revoke falha,
  mas o Blazor limpa a sessão local mesmo assim
- Usuário sem papel de administrador chama endpoint de administrador: 403,
  mesmo que a tela tenha sido contornada
- Usuário e senha errados: o login responde 401 sem dizer qual dos dois errou
- Cadastro de usuário com username já usado: hoje responde 500 com o erro do
  banco; pela #30 passa a responder 400 com "Já existe um usuário com esse
  nome."
- Usuário comum pede a lista de usuários: hoje recebe todos, com o hash da
  senha; pela #30 recebe 403
- Excluir usuário que já movimentou estoque: recusado, para o histórico não
  perder o autor
- Usuário comum cadastra, edita ou exclui prateleira ou colaborador pela API:
  hoje é aceito; pela #32 e pelo RF-47 recebe 403 e nada é gravado
- Usuário excluído ou desativado com a sessão aberta [planejado #33]: a próxima
  requisição recebe 401 e o Blazor volta ao login (RF-46)
- Administrador tenta desativar a si mesmo [planejado #33]: recusado, para a
  empresa não ficar sem quem a administre por engano
- Login de usuário desativado [planejado #33]: 401 com a mesma mensagem de
  usuário ou senha errados
- Login de empresa desativada [planejado #28]: com usuário e senha certos, a
  tela avisa que a empresa está inativa; com a senha errada, a mensagem é a
  genérica, para não revelar usuários
- Refresh com access token adulterado ou de outro emissor [planejado #30]: 401,
  e não 500
- Usuário altera os próprios dados com a senha atual errada [planejado #30]:
  recusado
- Senha sem letra, sem número, com menos de 8 caracteres ou igual à atual
  [planejado #34]: recusada pela API, mesmo que a tela tenha sido contornada

### Produtos
- Nome com menos de 5 ou mais de 100 caracteres, descrição fora de 5 a 300,
  volume, preço inicial ou fator menor ou igual a zero, estoque mínimo
  negativo: recusado pela validação
- Código de barras com tamanho errado, dígito verificador inválido ou já usado
  por outro produto: recusado
- Ajuste de estoque sem motivo, com quantidade negativa ou igual ao saldo
  atual do depósito: recusado; a quantidade informada é o saldo real, e o
  sistema lança a diferença
- Nome igual ao de outro produto: recusado
- Imagem que não é PNG ou JPEG: o seletor não a aceita; a imagem é
  redimensionada no navegador e recusada se ainda passar de 10 MB. Pela #51,
  a API recusa também quando a tela é contornada (RF-53)

### Entrada por NF-e
- Arquivo que não é NF-e, ou NF-e modelo 65: recusado na prévia, nada é lido
  como item
- Item `SEM GTIN` ou com código desconhecido: aparece na prévia como item a
  resolver; a importação recusa enquanto ele não for ligado ou criado
- `qCom × fator` não dá inteiro: a quantidade sugerida vem vazia e o operador
  precisa informá-la
- Nota já importada (mesma chave de acesso): recusada
- Código do item que pertence a outro produto, produto novo com nome repetido
  ou sem colaborador padrão cadastrado: recusada a nota inteira, apontando o
  item
- Item ligado a um produto que já tem outro código: recusada a nota inteira;
  a importação não troca o código de um produto

### Alocação e prateleiras
- Quantidade maior que o saldo do depósito: recusada
- Produto com saldo em outra prateleira: recusado; é preciso transferir ou
  desfazer a alocação antes
- Produto ou prateleira sem volume: recusado; o card do produto mostra o selo
  "sem volume"
- Quantidade que passa do teto de 90% da prateleira, inclusive somando os
  itens do mesmo lote: recusada, com o espaço livre na tela antes do envio
- Produto repetido no mesmo lote: recusado o lote inteiro
- Transferência de entrada zerada, para a própria prateleira, para
  prateleira inexistente ou que não cabe no destino: recusada
- Volume da prateleira editado para menos que o volume já usado [planejado #32]:
  recusado (RN-40)
- Inventário sem nenhuma diferença, com entrada de outra prateleira ou com
  entrada repetida: recusado; a tela mantém as contagens e atualiza o saldo do
  sistema para o operador conferir de novo
- Contagem do inventário feita enquanto outra operação mexeu na prateleira: a
  diferença é calculada sobre o saldo do servidor no momento do envio, não
  sobre o que a tela carregou

### Vendas
- Quantidade zero ou negativa: recusada
- Data futura ou de mês anterior [planejado #35]: recusada (RN-39)
- Quantidade maior que o saldo da entrada, somando os itens repetidos da
  mesma entrada no carrinho: recusado o carrinho inteiro, apontando o item
- Carrinho reaberto depois que preço ou saldo mudaram: a tela confere na API
  ao abrir, mostra o preço e o saldo atuais e avisa o item acima do saldo; o
  carrinho só é esvaziado quando a API aceita
- Recusa do carrinho: o carrinho continua salvo para corrigir e reenviar
- Devolução maior que o vendido menos o já devolvido: recusada
- Cancelar venda com devolução, ou editá-la abaixo do devolvido: recusado
- Venda totalmente devolvida: some da listagem e dos relatórios, mas continua
  no banco

### Serviços dependentes
- Falha ao publicar o alerta de estoque mínimo na fila: só é registrada em
  log; a venda já gravada não volta erro
- E-mail que falha no envio SMTP: a mensagem fica sem confirmação na fila e
  só volta a ser entregue quando o consumidor reconecta; não há nova
  tentativa automática nem aviso na tela
- Relatório de mês sem vendas: 400 com o aviso "ainda não há vendas no mês
  corrente", sem arquivo; pelo RF-50, o aviso fala do mês escolhido, e mês
  futuro cai no mesmo caso
- Empresa sem nenhum administrador com e-mail confirmado [planejado #39]: o aviso
  não sai e a falha só é registrada em log, como na RN-29 (RF-48)
- Chave do Gemini ausente, cota esgotada ou AIAPI fora do ar: o Insights
  mostra o aviso, para o carregamento e não põe o erro no chat
- ReportsAPI fora do ar: o Insights mostra o aviso e libera o botão de
  exportar
- IA fora do ar ou lenta ao gerar o PDF [planejado #5]: o PDF sai sem o resumo,
  com uma linha avisando, em vez de falhar o relatório inteiro (RF-31)
- Cadastro de produto com o RabbitMQ fora do ar: hoje responde erro com o
  produto já gravado (ver as lacunas abaixo)
- Banco ainda aplicando migrations: os serviços não sobem até o `migrator`
  terminar; migration com erro impede o stack de subir

## Fora de escopo
Fica fora até virar requisito numa revisão desta spec.

- **Vendas**: pedido agrupando as vendas, pagamento, desconto, troco, emissão
  de nota de saída e devolução do carrinho inteiro de uma vez
- **Custo**: FIFO, custo por lote e preço por cliente (a margem por venda
  entrou como RF-52, pelo custo médio do momento)
- **Compras**: consulta da NF-e na SEFAZ, validação da assinatura do XML,
  cadastro de fornecedores, pedido de compra e contas a pagar
- **Catálogo**: consulta do produto em base externa (Cosmos, GS1), variações
  de produto (cor, tamanho) e exclusão de produto
- **Armazenamento**: tela de cadastro de prateleiras (o volume entra pela API
  e pelo seed), sugestão automática de posição, produto em mais de uma
  prateleira ao mesmo tempo, transferência parcial e transferência ou desfazer
  em lote
- **Acesso**: troca do hash de senha, dois fatores, bloqueio da conta por
  tentativas erradas, autocadastro e login pelo e-mail
- **Empresas**: usuário em mais de uma empresa, troca de empresa sem novo
  login, banco separado por empresa, planos e cobrança, visual por empresa
- **IA**: consulta ao banco por tool calling ou SQL livre, e qualquer decisão
  tomada pela IA sem um operador
- **Plataforma**: aplicativo nativo, uso offline e integração com ERP ou
  marketplace

## Lacunas conhecidas
Encontradas na verificação do código em 2026-10-14 e revisadas em
2026-10-15 e 2026-10-19. Uma lacuna vira requisito quando ganha issue ou é
decidida numa revisão desta spec ([decidido]).

### Segurança (#30)
Descritas e planejadas na #30, que deve entrar antes da #28 e da #29:
- hash de senha e refresh token expostos a qualquer usuário logado
  (RF-41)
- `update-credentials` recebendo e devolvendo a entidade `User` (RF-42)
- refresh reaproveitando as claims antigas (RF-43)
- mensagem de qualquer exceção devolvida ao cliente, e username repetido
  respondendo 500 (RN-38)
- métodos sem uso que executam SQL recebido como texto no
  `GenericRepository` da AuthenticationAPI

### Sessão (#31)
Planejadas na #31, que deve entrar depois da #30 (RF-44):
- o gateway não tem rota de `refresh` nem de `revoke`, e o Blazor guarda só o
  access token: a sessão cai em 60 minutos, no meio do turno
- o refresh grava um prazo novo de 7 dias a cada renovação, então a sessão
  nunca teria fim
- sair só apaga o token do navegador; o refresh token continua válido no banco

### Papéis (#32)
- cadastro, edição e exclusão de prateleiras e colaboradores aceitam qualquer
  usuário logado, inclusive o papel `Usuario` (RF-45, RF-47)
- o volume da prateleira pode ser reduzido abaixo do usado (RN-40)

### Revisão de 2026-10-19 (#33 a #38)
- excluir o usuário não encerra a sessão dele: o token segue válido até
  vencer, e não há como desativar quem já movimentou estoque (#33, RF-46)
- a política de senha (8 caracteres, maiúscula, minúscula e número) só existe
  no Blazor, com uma regra diferente da RN-34, e a API aceita qualquer senha
  (#34)
- a venda aceita qualquer data, inclusive futura ou de um mês já fechado
  (#35, RN-39)
- o relatório só sai para o mês corrente (#36, RF-50)
- transferir para a prateleira onde o produto já esteve mantém o preço antigo
  dela, e não o da origem (#37, RN-11)
- o ajuste de estoque do depósito não avalia o alerta de estoque mínimo
  (#38, RF-28)

### Regras (sem issue)
- O tipo de movimentação `Perda` existe no ledger, mas nenhuma tela ou
  endpoint o lança
- A alocação individual aceita preço zero ou negativo; o lote recusa (#51,
  RF-53)
- Toda recusa responde 400, inclusive recurso inexistente, que seria 404
- A imagem do produto só é conferida no Blazor: a API grava qualquer
  conteúdo, sem checar PNG ou JPEG nem o limite de 10 MB (#51, RF-53)

### Código (sem issue)
- As mensagens de validação da AuthenticationAPI estão em inglês ("Username
  and password are required.", "Invalid client request!")
- `POST /api/storage/email/v1/new-product` está roteado no gateway, mas
  ninguém o chama: a chamada em `CreateProduct.razor` está comentada e o
  `IEmailService` do Blazor ficou sem uso. Ele deixa o administrador
  disparar o e-mail de produto novo pelo navegador, por fora da fila. Sai na
  #39, junto com o `Email:Destinatario` (RF-48)

### Falhas (sem issue)
- Cadastrar produto com o RabbitMQ fora do ar grava o produto e responde 400:
  o operador vê erro, e tentar de novo é recusado como produto já cadastrado
- O consumidor do e-mail de produto novo não trata exceção: um envio que
  falha deixa a mensagem sem confirmação até o consumidor reconectar

## Critérios de aceite
- Depois de qualquer operação, o saldo de cada produto em cada local é igual
  ao saldo anterior mais a soma das movimentações gravadas
- Toda movimentação tem o usuário do token como autor e o instante no relógio
  do servidor
- Nenhum saldo fica negativo, nenhuma movimentação fica órfã e nenhuma venda
  existe sem entrada
- Um envio de vários itens com um item inválido não grava nada e aponta o item
- Repreçar a prateleira não muda o total de vendas já registradas
- A mesma NF-e importada duas vezes é recusada na segunda
- Nenhuma alocação passa de 90% do volume da prateleira
- O alerta de estoque mínimo sai uma vez ao cruzar o limite e não se repete
  enquanto o saldo segue baixo
- O relatório do mês não inclui vendas de outros anos nem unidades devolvidas
- Usuário sem papel de administrador não cadastra nem exclui usuários, nem
  pela tela nem pela API
- [planejado #30] Nenhuma resposta da API traz hash de senha, refresh token
  ou texto de erro interno
- [planejado #31] O operador trabalha 9 horas sem entrar de novo; depois
  delas, ou depois de sair, o refresh é recusado
- [planejado #32] Usuário sem papel de administrador não cadastra, não edita
  nem exclui prateleiras e colaboradores
- [planejado #33] Usuário excluído perde o acesso na requisição seguinte, sem
  esperar o token vencer
- [planejado #35] Nenhuma venda nova tem data futura nem cai num mês já encerrado
- [planejado #34] A API recusa senha fora da RN-34, mesmo com a tela contornada
- Todos os casos de `tests/ledger_e2e.py` e os testes de `dotnet test` passam
