# Spec Delta

## Purpose

Este capability permite que o sistema identifique animes em uma árvore de mídia local, obtenha os intervalos de abertura e encerramento de APIs públicas e produza um `markers.json` compatível com o serviço atual, sem alterar a biblioteca Plex.

## ADDED Requirements

### Requirement: Geração automática de marcadores de anime

O sistema deve permitir que um usuário forneça explicitamente uma raiz de mídia somente leitura e execute uma varredura para identificar arquivos de vídeo de anime, mapear título, temporada e episódio e gerar uma prévia de marcadores antes de persistir qualquer alteração.

#### Scenario: Descoberta válida de arquivos e estrutura de pastas
- **WHEN** a raiz de mídia contiver uma estrutura compatível com `<título>/Temporada <número>/<arquivo -episódio.ext>`
- **THEN** o sistema deve reconhecer a série, a temporada local e o episódio, ignorar arquivos auxiliares e não corresponder a outros tipos de conteúdo sem bloquear a execução

#### Scenario: Identificação ambígua de anime
- **WHEN** a busca por título em API pública retornar mais de um candidato plausível ou nenhum candidato inequívoco
- **THEN** o sistema deve deixar a série pendente, informar a ambiguidade e exigir confirmação ou configuração manual antes de gravar marcadores para aquele título

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
