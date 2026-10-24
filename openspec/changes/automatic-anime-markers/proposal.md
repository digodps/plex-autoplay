# Proposal

## Why

A geração automática de marcadores de anime ainda não existe no projeto, mas a arquitetura atual já oferece um ponto natural para integrar isso: o serviço lê e usa `markers.json` para pular intros e créditos, e o repositório atual depende apenas do formato já suportado. O recurso reduz o trabalho manual de localizar episódios, consultar APIs públicas e montar os timestamps sem alterar a biblioteca Plex ou o contrato atual dos marcadores.

## What Changes

- Adicionar um comando de CLI para varrer uma pasta de mídia montada pelo usuário e descobrir séries/animes, temporadas e episódios pelo caminho e nome dos arquivos.
- Resolver o anime e o episódio usando APIs públicas documentadas (Jikan para identificação, AniSkip para timestamps de abertura/encerramento) com escolha explícita do usuário em casos ambíguos.
- Gerar uma prévia dos intervalos encontrados e mesclar o resultado em `markers.json` sem sobrescrever entradas manuais sem confirmação.
- Persistir caches de busca e mapeamento local em arquivo separado para evitar consultas repetidas e respeitar limites de API.
- Preservar compatibilidade com o formato atual de marcadores já consumido por `JsonMarkerRepository`.

## Capabilities

### New Capabilities
- `automatic-anime-markers`: geração automática de marcadores temporais para animes a partir de uma raiz de mídia local, com resolução de identidade usando APIs públicas e confirmação antes da gravação.

### Modified Capabilities
- None

## Impact

- CLI e comandos do ponto de entrada do projeto (`python -m plex_autoplay` e módulos equivalentes).
- Integração em `plex_autoplay/infrastructure/markers/json_repository.py` e em qualquer código que consuma `markers.json`.
- Novos adaptadores de rede para APIs públicas e um cache de metadados local fora do arquivo principal de marcadores.
- Nenhuma alteração na biblioteca do Plex, sem dependência de scraping, sem escrita em arquivos de vídeo e sem atualizações automáticas da biblioteca.
