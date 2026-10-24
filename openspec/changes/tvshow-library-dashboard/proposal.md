# Proposal

## Why

O dashboard deve atender bibliotecas de TV shows do Plex em geral, não apenas Anime. O painel precisa inventariar séries e episódios, mostrar cobertura de marcadores, registrar falhas e permitir edição manual para qualquer série; integrações de Anime são um enriquecimento opcional, não uma dependência do fluxo genérico.

## What Changes

- Substituir o escopo planejado em `anime-scan-dashboard` por esta mudança genérica; preservar os comportamentos anteriores de autenticação inicial, gestão de bibliotecas na interface, estado persistente, nova varredura e acesso Docker.
- Criar dashboard autenticado, com estilo escuro limpo inspirado na organização visual do Plex, que liste bibliotecas de TV shows, séries, temporadas e episódios descobertos, marcadores presentes/ausentes, pendências e erros.
- Permitir cadastrar/remover bibliotecas pela interface, selecionando subpastas dentro de uma raiz do host montada uma única vez como somente leitura durante a instalação; a classificação da biblioteca determina apenas quais enriquecimentos específicos podem ser executados.
- Descobrir episódios em padrões comuns do Plex, incluindo `S01E02`, `S01E02-E03`, `1x02`, `E02` dentro de pastas de temporada `Season 1`/`Temporada 1` e o padrão existente `<série>/Temporada n/<arquivo>-NN`.
- Permitir adicionar, editar e remover manualmente tempos de intro e créditos em episódios de qualquer TV show, preservando o formato compatível de `markers.json` e exigindo confirmação antes de substituir valores existentes. Manter Jikan/AniSkip como geração automática opcional e exclusiva para bibliotecas classificadas como Anime.
- Persistir bibliotecas e relatório da última varredura em `/config`; permitir atualização periódica do painel e disparo de nova varredura por administrador autenticado, sem execuções concorrentes.
- Configurar primeiro acesso para criação da conta administrativa, login nos acessos seguintes e porta web configurável publicada em Docker com rede bridge. Manter Plex acessível pelo `PLEX_URL`.

## Capabilities

### New Capabilities
- `tvshow-library-dashboard`: gestão e monitoramento de bibliotecas de TV shows, descoberta de episódios, cobertura de marcadores, edição manual e experiência web autenticada.

### Modified Capabilities
- None

## Impact

- Este plano genérico substitui o plano ainda não implementado `anime-scan-dashboard`; os artefatos daquela mudança permanecem como histórico até que sejam explicitamente arquivados.
- Novo servidor web e UI no processo `plex_autoplay`, API para estado/bibliotecas/varredura e operações protegidas de edição de marcadores.
- Generalização de `plex_autoplay/infrastructure/media/directory_monitor.py`, hoje limitado a uma estrutura específica de pastas e nomes de episódio; preservação do adaptador Jikan/AniSkip como enriquecimento exclusivo do fluxo Anime.
- Alterações em `JsonMarkerRepository` ou serviço de marcadores para persistir operações manuais no contrato existente sem sobrescrita acidental.
- Alterações em `Dockerfile`, dependências, `plex_autoplay/config.py`, `plex_autoplay/main.py`, Compose local/ZimaOS, `.env.example` e README para raiz somente leitura, persistência em `/config`, porta publicada, rede bridge e conexão Plex.
- Sem acesso ao socket Docker, sem escrita nas mídias e sem dependência de APIs externas para inventário, cobertura ou edição manual de TV shows. A geração automática via APIs permanece limitada a Anime.
