# Spec Delta

## Purpose

Este capability permite que o sistema identifique animes em uma árvore de mídia local, obtenha os intervalos de abertura e encerramento de APIs públicas e produza um `markers.json` compatível com o serviço atual, sem alterar a biblioteca Plex. No Docker, as raízes de Anime e de Séries comuns são configuradas separadamente; apenas Anime é varrida e consultada nas APIs.

## ADDED Requirements

### Requirement: Geração automática de marcadores de anime

O sistema deve permitir configurar raízes independentes de Anime e Séries comuns no Docker, montá-las em modo somente leitura e executar automaticamente a varredura da raiz de Anime na inicialização. A raiz de Séries comuns não deve ser consultada nas APIs Jikan/AniSkip. A CLI também deve permitir varredura manual e prévia.

#### Scenario: Bibliotecas configuradas separadamente no Docker
- **WHEN** o usuário configurar os caminhos de Anime e Séries comuns no Compose
- **THEN** cada raiz deve ser montada em seu destino interno próprio e somente leitura, mesmo que as origens do host sejam diferentes

#### Scenario: Varredura automática na inicialização
- **WHEN** a geração automática estiver habilitada e a raiz de Anime estiver montada
- **THEN** o serviço deve varrer somente Anime, mesclar marcadores gerados sem apagar valores existentes e persistir saída/cache na configuração do container

#### Scenario: Falha na geração automática
- **WHEN** a raiz de Anime estiver indisponível ou uma consulta falhar
- **THEN** a falha deve ser registrada e o serviço Plex deve continuar sua inicialização

#### Scenario: Séries comuns não consultadas nas APIs de anime
- **WHEN** a raiz de Séries comuns estiver montada
- **THEN** ela deve permanecer disponível como biblioteca montada, sem varredura ou chamadas a Jikan/AniSkip

#### Scenario: Descoberta válida de arquivos e estrutura de pastas
- **WHEN** a raiz de mídia contiver uma estrutura compatível com `<título>/Temporada <número>/<arquivo -episódio.ext>`
- **THEN** o sistema deve reconhecer a série, a temporada local e o episódio, ignorar arquivos auxiliares e não corresponder a outros tipos de conteúdo sem bloquear a execução

#### Scenario: Identificação ambígua de anime em execução automatizada
- **WHEN** a busca por título retornar candidatos sem correspondência inequívoca durante a inicialização automática
- **THEN** o sistema deve usar a estimativa automática, registrar o candidato presumido nos logs e expor a suposição no resultado da prévia

#### Scenario: Identificação ambígua de anime na CLI
- **WHEN** a busca por título retornar candidatos sem correspondência inequívoca durante a CLI sem seleção confirmada
- **THEN** o sistema deve deixar a série pendente em vez de gravar marcadores incertos

#### Scenario: Consulta de timestamps públicos
- **WHEN** uma série/episódio válido for identificado e a duração real do arquivo puder ser obtida
- **THEN** o sistema deve consultar APIs públicas documentadas para os intervalos de abertura e encerramento e normalizar os valores para segundos

#### Scenario: Preservação do contrato de marcadores
- **WHEN** o sistema for preparar a saída para `markers.json`
- **THEN** deve manter o formato existente de `show -> season_x -> ep -> { start, end, credits }`, omitindo apenas campos ausentes e nunca inventando valores para dados inexistentes

#### Scenario: Gravação segura e sem sobrescrita indiscriminada
- **WHEN** o usuário confirmar a geração de marcadores
- **THEN** o sistema deve mesclar os resultados em `markers.json` sem apagar marcadores manuais existentes sem confirmação explícita, gravando em arquivo temporário e renomeando de forma atômica

#### Scenario: Tratamento de falhas externas
- **WHEN** a API pública falhar, responder 404 ou 429, ou não encontrar dados para um episódio
- **THEN** o sistema deve registrar a pendência, preservar o estado atual do arquivo e não escrever dados incompletos ou inconsistentes

### Requirement: Limites de segurança e alcance

O sistema deve operar em modo somente leitura sobre a raiz informado, limitar todos os acessos ao caminho resolvido, e não realizar ações fora desse escopo (sem renomear, remover ou alterar arquivos de vídeo, sem consultar conteúdo de mídia externo e sem persistir segredos).

#### Scenario: Validação de caminho
- **WHEN** o sistema resolva um caminho ou link simbólico dentro da árvore permitida
- **THEN** deve validar que a entrada permanece dentro da raiz configurada e rejeitar qualquer tentativa de atravessar limites de diretório

#### Scenario: Cache persistente
- **WHEN** nomes, escolhas de usuário e respostas de APIs forem reutilizados em execuções futuras
- **THEN** o sistema deve armazenar esses dados em um arquivo de cache separado, com TTL e invalidação simples para reduzir chamadas repetidas e manter rastreabilidade local
