# Spec Delta

## Purpose

Permite administrar e monitorar qualquer biblioteca de TV shows em uma interface web autenticada, descobrir episódios e acompanhar a cobertura de marcadores. Inventário e edição manual são genéricos; integrações de geração automática específicas de Anime são opcionais.

## ADDED Requirements

### Requirement: Configuração inicial e autenticação administrativa
O dashboard MUST exigir a criação de uma conta administrativa no primeiro acesso e autenticar acessos seguintes. Credenciais MUST ser armazenadas de forma protegida e dados das bibliotecas MUST permanecer privados sem sessão válida.

#### Scenario: Criar administrador no primeiro acesso
- **WHEN** o dashboard é acessado sem credencial administrativa previamente configurada
- **THEN** o sistema apresenta um assistente para criar usuário e senha, salva somente um verificador seguro e passa a exigir login

#### Scenario: Bloquear acesso não autenticado
- **WHEN** uma solicitação sem sessão válida tenta acessar páginas, dados ou operações
- **THEN** o sistema solicita autenticação sem revelar nomes de bibliotecas, relatórios ou marcadores

### Requirement: Gerenciar bibliotecas de TV shows
O administrador MUST poder adicionar, classificar e remover bibliotecas de TV shows pela interface, selecionando pastas dentro de uma única raiz montada em modo somente leitura. A classificação pode identificar Anime para habilitar enriquecimento específico, mas não limita o inventário nem a edição manual de marcadores.

#### Scenario: Adicionar biblioteca
- **WHEN** o administrador seleciona uma subpasta disponível e define seu tipo como Anime ou Série
- **THEN** o sistema persiste a biblioteca e a exibe na lista, sem exigir alteração ou recriação do container

#### Scenario: Impedir acesso fora da raiz
- **WHEN** uma seleção contém traversal, link simbólico externo ou caminho resolvido fora da raiz permitida
- **THEN** o sistema rejeita a operação e não lê nem altera arquivos externos à raiz

#### Scenario: Remover biblioteca
- **WHEN** o administrador remove uma biblioteca configurada
- **THEN** ela não participa das varreduras seguintes e seus arquivos não são apagados nem modificados

### Requirement: Descobrir episódios de TV shows
O sistema MUST identificar séries, temporadas e episódios em todas as bibliotecas de TV shows configuradas, independentemente da classificação, e distinguir episódios com marcadores de episódios sem marcadores.

#### Scenario: Descoberta por padrão Plex
- **WHEN** uma biblioteca contém arquivos com padrões `S01E02`, `S01E02-E03` ou `1x02` sob uma pasta de série
- **THEN** o sistema reconhece temporada e episódio(s), sem duplicar o episódio no inventário

#### Scenario: Descoberta por pasta de temporada
- **WHEN** a biblioteca contém subpastas `Season 1` ou `Temporada 1` e arquivos com padrão `E02`
- **THEN** o sistema associa o episódio à temporada indicada pela pasta e à série pai

#### Scenario: Compatibilidade com o padrão legado de episódios
- **WHEN** uma biblioteca contém `<série>/Temporada n/<arquivo>-NN.ext`
- **THEN** o sistema continua reconhecendo o episódio e a temporada como antes

#### Scenario: Arquivo auxiliar ou episódio sem padrão reconhecido
- **WHEN** a varredura encontra conteúdo que não corresponde aos padrões suportados
- **THEN** o sistema o ignora ou registra como não reconhecido, sem interromper a varredura da biblioteca

### Requirement: Gerar e editar marcadores de episódios
O sistema MUST manter o formato atual de `markers.json`, oferecer edição manual de intro e créditos para episódios de qualquer TV show e limitar geração automática por APIs públicas específicas ao tipo Anime.

#### Scenario: Gerar marcadores automaticamente para Anime
- **WHEN** uma biblioteca classificada como Anime é varrida e existem dados compatíveis nas APIs configuradas
- **THEN** o sistema consulta Jikan/AniSkip, registra estimativas ou pendências e mescla os timestamps no formato compatível

#### Scenario: Inventariar TV shows sem enriquecimento de Anime
- **WHEN** uma biblioteca de TV shows não classificada como Anime é varrida
- **THEN** seus episódios entram no inventário e na cobertura de marcadores, sem consultas Jikan/AniSkip ou geração automática de timestamps por esses serviços

#### Scenario: Adicionar ou editar marcador manualmente
- **WHEN** o administrador informa timestamps de intro e/ou créditos para um episódio de qualquer série
- **THEN** o sistema valida valores numéricos válidos, grava apenas os campos fornecidos e reflete a alteração no dashboard

#### Scenario: Proteger marcador existente
- **WHEN** uma operação manual ou automática conflita com valores já existentes
- **THEN** o sistema preserva os valores por padrão e solicita confirmação explícita antes de substituí-los

#### Scenario: Remover marcador manual
- **WHEN** o administrador confirma a remoção de um marcador customizado de episódio
- **THEN** o sistema remove somente a entrada selecionada do arquivo de marcadores por escrita atômica, sem alterar a mídia

### Requirement: Apresentar estado e resultados da varredura
O dashboard MUST persistir e exibir o estado da última varredura, os episódios descobertos, marcadores adicionados, episódios sem marcadores, estimativas e erros.

#### Scenario: Mostrar resumo por biblioteca e série
- **WHEN** existe resultado de varredura
- **THEN** o dashboard mostra horário, estado, contagens e navegação por biblioteca, série, temporada e episódio

#### Scenario: Mostrar cobertura e itens ausentes
- **WHEN** episódios descobertos não possuem todos os campos de marcador disponíveis
- **THEN** o dashboard identifica quais episódios têm marcadores e quais permanecem sem intro/créditos, distinguindo dados ausentes de falhas

#### Scenario: Mostrar pendências e erros
- **WHEN** a varredura encontra ambiguidade, arquivo ilegível, API indisponível ou falha de persistência
- **THEN** o resultado exibe o item afetado e o motivo, separando pendência recuperável de erro de execução

#### Scenario: Atualizar painel periodicamente
- **WHEN** uma varredura está em andamento ou terminou enquanto o dashboard permanece aberto
- **THEN** o painel atualiza o estado sem exigir recarga manual

#### Scenario: Ausência de varredura registrada
- **WHEN** ainda não existe relatório persistido
- **THEN** a interface exibe estado vazio e não apresenta valores de sucesso presumidos

### Requirement: Disparar varredura pela interface
O dashboard MUST permitir que um administrador autenticado inicie uma varredura sob demanda, sem bloquear a interface nem iniciar execuções concorrentes.

#### Scenario: Iniciar varredura
- **WHEN** o administrador solicita uma varredura sem outra execução ativa
- **THEN** o sistema enfileira o trabalho e apresenta progresso e resultados por biblioteca

#### Scenario: Rejeitar varredura concorrente
- **WHEN** uma execução já está ativa
- **THEN** nova solicitação não inicia outro worker e a interface informa que a varredura está em andamento

#### Scenario: Continuar após falha de uma biblioteca
- **WHEN** uma biblioteca falha durante a execução
- **THEN** o sistema registra o erro e continua as outras bibliotecas elegíveis

### Requirement: Publicar dashboard no Docker
Os Compose MUST publicar o dashboard em porta configurável e montar uma raiz de mídia do host em modo somente leitura, sem depender de `network_mode: host`.

#### Scenario: Acessar interface pela porta configurada
- **WHEN** o container é iniciado com a porta host configurada
- **THEN** o administrador pode acessar o dashboard pelo endereço do host e essa porta

#### Scenario: Usar rede bridge e conectar ao Plex
- **WHEN** o serviço inicia em rede bridge
- **THEN** ele conecta ao Plex pelo endereço configurado que seja alcançável pelo container

#### Scenario: Persistir dados administrativos e resultados
- **WHEN** o container é reiniciado
- **THEN** conta administrativa, bibliotecas selecionadas, marcadores e último relatório permanecem disponíveis no volume persistente de configuração
