# Delta — plex-scan-real

## MODIFIED Requirements

### Requirement: Mapear caminho do episódio para arquivo local

O sistema SHALL resolver o caminho local do arquivo de mídia de cada episódio combinando o valor MediaPart.file do Plex com a lista de raízes de mídia configuradas (`MEDIA_ROOTS`), testando cada raiz com sondagem de sufixo (do sufixo mais específico ao caminho completo) até encontrar um arquivo existente.

#### Scenario: Caminho resolvido via MEDIA_ROOT
- **WHEN** o MediaPart.file de um episódio é /Volumes/Media/Series/Show/S01E01.mkv e MEDIA_ROOT=/media
- **THEN** o sistema resolve o caminho local como /media/Volumes/Media/Series/Show/S01E01.mkv

#### Scenario: Caminho resolvido na segunda raiz
- **WHEN** o MediaPart.file é /Volumes/Media/Series/Show/S01E01.mkv, `MEDIA_ROOTS=["/media","/mnt/nas"]` e o arquivo existe apenas em `/mnt/nas/Series/Show/S01E01.mkv`
- **THEN** o sistema resolve o caminho local como `/mnt/nas/Series/Show/S01E01.mkv`

#### Scenario: Arquivo de mídia não acessível
- **WHEN** o caminho local resolvido não existe em disco
- **THEN** o sistema loga o arquivo ausente e ignora a detecção de marcadores para aquele episódio

#### Scenario: Scan agendado usa todas as raízes
- **WHEN** o job de sincronização periódica processa episódios
- **THEN** ele usa a lista `MEDIA_ROOTS` corrente lida do banco, não um valor fixo de ambiente
