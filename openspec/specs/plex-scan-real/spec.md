# plex-scan-real Specification

## Purpose

Define o comportamento do scanner real de bibliotecas Plex que lista episódios, detecta marcadores de intro e créditos via ffprobe e os persiste em SQLite e no arquivo custom.json no formato PlexAutoSkip.

## Requirements

### Requirement: Listar episódios de uma biblioteca de séries
O sistema SHALL listar todos os episódios de uma biblioteca de séries do Plex consultando a API do servidor, incluindo o rating key e o caminho do arquivo de mídia (MediaPart.file) de cada episódio.

#### Scenario: Episódios listados com sucesso
- **WHEN** o sistema consulta a API do Plex para episódios de uma biblioteca de tipo "show"
- **THEN** o sistema retorna uma lista de episódios com rating key, título, temporada, episódio e caminho do arquivo de mídia

#### Scenario: Biblioteca inacessível
- **WHEN** o servidor Plex não está acessível na URL configurada
- **THEN** o sistema retorna um erro indicando que o servidor Plex está indisponível

### Requirement: Mapear caminho do episódio para arquivo local
O sistema SHALL resolver o caminho local do arquivo de mídia de cada episódio combinando o valor MediaPart.file do Plex com a raiz de mídia configurada na variável de ambiente MEDIA_ROOT.

#### Scenario: Caminho resolvido via MEDIA_ROOT
- **WHEN** o MediaPart.file de um episódio é /Volumes/Media/Series/Show/S01E01.mkv e MEDIA_ROOT=/media
- **THEN** o sistema resolve o caminho local como /media/Volumes/Media/Series/Show/S01E01.mkv

#### Scenario: Arquivo de mídia não acessível
- **WHEN** o caminho local resolvido não existe em disco
- **THEN** o sistema loga o arquivo ausente e ignora a detecção de marcadores para aquele episódio

### Requirement: Detectar marcadores via ffprobe
O sistema SHALL analisar o arquivo de mídia de cada episódio usando ffprobe detecção de silêncio e identificar segmentos de silêncio contíguos no início (intro) e próximo do final (créditos) como candidatos a marcadores.

#### Scenario: Intro detectado no início do episódio
- **WHEN** ffprobe encontra silêncio de 0ms até aproximadamente 90000ms em um episódio de série
- **THEN** o sistema cria um marcador do tipo "intro" com start_ms=0 e end_ms igual ao fim do silêncio

#### Scenario: Créditos detectados no final do episódio
- **WHEN** ffprobe encontra silêncio de aproximadamente 85% da duração até o final do episódio
- **THEN** o sistema cria um marcador do tipo "credits" com start_ms no início do silêncio e end_ms na duração do episódio

#### Scenario: Episódio menor que o limite mínimo de créditos
- **WHEN** a duração do episódio é menor que 300000ms
- **THEN** o sistema não tenta detecção de créditos para aquele episódio

### Requirement: Persistir marcadores em SQLite e custom.json
O sistema SHALL gravar os marcadores detectados na tabela de marcadores do SQLite e reescrever o arquivo custom.json no formato PlexAutoSkip, keyed por rating key, sem duplicar marcadores já existentes salvo re-scan explícito.

#### Scenario: Novo marcador gravado
- **WHEN** um marcador é detectado para um episódio com rating key 12345
- **THEN** o SQLite contém o marcador e custom.json tem uma entrada para rating key 12345 com start_ms, end_ms e kind

#### Scenario: Marcador existente não duplicado
- **WHEN** custom.json já contém um marcador para rating key 12345
- **THEN** o sistema não sobrescreve salvo que um re-scan manual seja disparado

### Requirement: Pular episódios que já têm marcadores
O sistema SHALL pular a detecção via ffprobe para episódios que já possuem marcadores de intro e créditos armazenados em custom.json ou SQLite.

#### Scenario: Episódio já tem marcadores
- **WHEN** o rating key de um episódio existe em custom.json com marcadores intro e créditos
- **THEN** o sistema não executa ffprobe para aquele episódio

#### Scenario: Episódio sem marcadores
- **WHEN** um episódio não tem marcadores em custom.json
- **THEN** o sistema executa ffprobe e armazena quaisquer marcadores detectados
