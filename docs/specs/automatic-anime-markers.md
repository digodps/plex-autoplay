# Especificação: geração automática de marcadores de anime

## 1. Objetivo

Permitir configurar bibliotecas independentes de Anime e Séries comuns no Docker, varrer Anime ao iniciar o serviço, identificar episódios pelo caminho/nome dos arquivos, consultar APIs públicas para obter marcadores de abertura e encerramento e gerar um `markers.json` compatível com o serviço atual. Uma CLI também oferece prévia e geração manual.

O recurso gera **marcadores temporais** (segmentos de intro e encerramento), não arquivos de metadados descritivos nem alterações na biblioteca Plex.

## 2. Escopo do MVP

- Anime em arquivos de vídeo locais acessíveis pelo container.
- Consulta de dados externos somente por APIs públicas documentadas.
- Geração local de `markers.json`; sem escrita ou atualização da biblioteca Plex.
- Varredura automática no startup do Docker Compose e execução manual por CLI, com modo de prévia e confirmação antes de gravar pela CLI.
- Mounts de Anime e Séries comuns independentes e somente leitura; somente Anime é varrida e consultada nas APIs de anime.
- Reutilização do formato de marcadores já lido por `JsonMarkerRepository`.
- Nenhuma dependência de API privada do Plex, scraping de páginas ou credencial de usuário Plex para buscar timestamps.

Fora do escopo: geração para séries comuns ou filmes, download/edição de vídeo, identificação por visão computacional, submissão automática de timestamps e monitoramento contínuo das alterações na pasta.

## 3. Requisito de caminho e nomenclatura

Estrutura obrigatória para descoberta automática:

```text
<raiz da biblioteca>/
└── <nome da série>/
    └── Temporada <número>/
        └── <qualquer texto> -<episódio>.<extensão>
```

Exemplo aceito:

```text
Animes/
└── Kyouran Reijou Nia Liston/
    └── Temporada 1/
        └── Kyouran Reijou Nia Liston Byoujaku Reijou ni Tensei shita Kamigoroshi no Bujin no Karei Naru Musouroku -01.mkv
```

Regras de parsing:

1. O diretório imediatamente abaixo da raiz de mídia fornece o título usado na busca inicial.
2. `Temporada <número>` é case-insensitive; espaços adicionais são permitidos. A temporada é um número inteiro positivo.
3. O nome do arquivo deve terminar com hífen, número do episódio (um a três dígitos, zeros à esquerda aceitos) e extensão. O texto anterior ao sufixo é apenas uma pista para confirmar a identificação.
4. A extensão deve pertencer a uma lista configurada de formatos de vídeo comuns. Arquivos auxiliares e não correspondentes ao padrão são ignorados com registro informativo.
5. Episódio `01` é normalizado para o inteiro `1`.
6. No modo de CLI sem uma escolha inequívoca, deixar a série pendente ou solicitar confirmação interativa. No startup automatizado, estimar candidato e offset de temporada e registrar cada suposição.
7. As raízes são fornecidas por configuração Docker e montadas em modo somente leitura; a raiz de Séries comuns não é percorrida pelo gerador. O programa não deve percorrer caminhos fora da raiz de Anime.

## 4. APIs públicas

### 4.1 Jikan REST API v4 — pesquisa e identificação

- Documentação: <https://docs.api.jikan.moe/>
- Base: `https://api.jikan.moe/v4`
- Endpoint de pesquisa: `GET /anime?q=<título>&limit=<limite>`
- Endpoint de detalhes: `GET /anime/{mal_id}/full`
- Autenticação: nenhuma chave de API.
- Jikan é uma API pública não oficial para dados do MyAnimeList; disponibilidade e limites podem mudar.

Usar o título da pasta como consulta primária. A busca retorna candidatos com `mal_id`, títulos alternativos e dados básicos. A CLI solicita seleção quando não houver correspondência inequívoca; no startup automatizado, o primeiro candidato é usado como estimativa e a suposição é registrada. O mapeamento é persistido localmente para reduzir pesquisas repetidas.

### 4.2 AniSkip — timestamps de abertura e encerramento

- API pública: <https://api.aniskip.com/>.
- Endpoint de leitura: `GET https://api.aniskip.com/v2/skip-times/{mal_id}/{episode}?types=op&types=ed&episodeLength={segundos}`
- Autenticação para leitura: nenhuma.
- Os tipos `op` e `ed` representam abertura e encerramento; os intervalos retornados usam segundos.
- Respostas podem não encontrar segmentos; limites podem retornar `429`.

O formato de resposta contém `found` e `results`; cada resultado contém `skipType`, `interval.startTime`, `interval.endTime` e `episodeLength`. Não assumir que temporada local × episódio identifica diretamente o episódio esperado pelo AniSkip; a CLI permite informar o offset e o startup usa offset zero estimado quando não há mapeamento em cache.

### 4.3 Duração do arquivo

O parâmetro `episodeLength` deve ser a duração real da mídia em segundos. A implementação usa `ffprobe` no host/container para a CLI e para a varredura no startup. Se não for possível obter uma duração válida, não consulta AniSkip para esse arquivo e o reporta como pendente.

Não enviar arquivos ou conteúdo de mídia para provedores externos.

## 5. Fluxo funcional

1. No Docker Compose, o usuário configura os caminhos host separados para Anime e Séries comuns; ambas são montadas em modo somente leitura, e marcadores/cache persistem em `/config`.
2. Ao iniciar o serviço, se `AUTO_GENERATE_ANIME_MARKERS` estiver habilitada, o sistema percorre somente a raiz de Anime e extrai título, temporada local, episódio e extensão. A raiz de Séries comuns não é varrida nem consultada nas APIs de anime.
3. O sistema agrupa arquivos pela série e evita consultas repetidas usando cache local.
4. Para séries ainda não mapeadas, consulta Jikan; no startup, seleciona uma estimativa quando não houver correspondência exata única e registra a suposição.
5. Na CLI interativa, apresenta candidatos e permite confirmação; em modo não interativo sem `--auto-guess`, deixa a série pendente.
6. Resolve a relação entre a temporada local e a numeração usada pelo AniSkip. No startup, usa offset zero como estimativa quando não houver mapeamento em cache e registra a suposição; a CLI pode solicitar confirmação.
7. Obtém a duração real do arquivo e consulta AniSkip para `op` e `ed`.
8. Valida e normaliza os intervalos em segundos.
9. A CLI exibe uma prévia indicando IDs, episódio, duração, segmentos encontrados, ausentes e conflitos; o startup registra o resumo, as estimativas e as pendências nos logs.
10. No startup, mescla automaticamente os resultados ao `markers.json` preservando valores já existentes e grava o arquivo de forma atômica. Na CLI, a gravação requer confirmação explícita; `--force` permite substituir valores existentes.

A varredura acontece uma vez por inicialização do processo/container, não acompanha continuamente mudanças na biblioteca. Se a geração falhar, o erro é registrado e o serviço Plex continua a inicialização.

## 6. Formato de saída

Manter compatibilidade com o formato existente:

```json
{
  "Nome da série": {
    "season_1": {
      "1": {
        "start": 310.571,
        "end": 400.571,
        "credits": 1396.006
      }
    }
  }
}
```

`start`/`end` representam o intervalo de abertura (`op`) e `credits` representa o início do encerramento (`ed`). Os valores são segundos. O repositório atual consome esses campos e converte os tempos para milissegundos. Se a resposta não incluir um tipo, omitir apenas o campo correspondente; não inventar timestamp nem usar zero como dado ausente.

O formato atual não registra ID externo, fonte, data de busca, duração ou confiança. Para evitar mudar o contrato de `markers.json`, guardar mapeamentos e estado de cache em arquivo separado, por exemplo `metadata-cache.json`. Qualquer mudança de esquema dos marcadores exige migração e testes de compatibilidade dedicados.

## 7. Regras de segurança e robustez

- Montar mídias somente em leitura; nunca renomear, remover ou modificar arquivos de vídeo.
- Validar que os caminhos resolvidos continuam dentro da raiz informada, inclusive diante de symlinks.
- Nunca sobrescrever um marcador manual existente sem opção explícita de substituição e confirmação.
- Fazer escrita atômica: arquivo temporário no mesmo diretório, flush e rename.
- Aplicar timeout, tratamento explícito de erros HTTP e respeito aos limites publicados/`Retry-After`.
- Usar cache persistente para buscas, IDs e respostas válidas; ter TTL configurável e evitar chamadas por arquivo repetido.
- Em `404`, distinguir ausência de dados de falha de rede. Em `429`, interromper/adiar consultas respeitando o limite. Em indisponibilidade, preservar arquivo atual e deixar itens pendentes.
- Não registrar tokens ou conteúdo sensível; as APIs deste escopo não precisam de segredo de usuário.
- Limitar tamanho e tipos de conteúdo HTTP; validar JSON, campos numéricos, intervalos positivos e limites coerentes com duração do episódio.

## 8. Interface CLI proposta

```text
python -m plex_autoplay \
  --media-root /media/anime \
  --output /config/markers.json \
  --dry-run
```

Opções disponíveis:

- `--media-root` seleciona a raiz de Anime; `--output` e `--cache-file` definem os arquivos de marcadores e cache.
- `--dry-run` mostra a prévia sem gravar marcadores; sem essa opção, a CLI solicita confirmação antes da mesclagem.
- `--auto-guess` permite estimar candidatos e offsets também pela CLI; sem isso, ambiguidade permanece pendente ou é resolvida pela seleção interativa.
- `--yes` confirma a gravação explicitamente em modo não interativo; `--force` autoriza substituir campos existentes, mas a gravação ainda exige confirmação.
- `--cache-ttl-hours` define o TTL do cache.
- A CLI retorna status diferente de zero para diretório inválido, ausência de marcadores graváveis ou falha de gravação; itens sem timestamp são listados como pendências.

## 9. Implementação

O fluxo de descoberta, consulta Jikan/AniSkip, cache e merge está implementado em `plex_autoplay/infrastructure/media/directory_monitor.py`. O entry point `plex_autoplay/main.py` compõe o comando CLI e chama a geração de Anime no startup quando `AUTO_GENERATE_ANIME_MARKERS` estiver habilitada. A rotina de startup recebe as configurações de caminhos/cache de `plex_autoplay/config.py`; os watchers de reprodução do Plex continuam separados desse fluxo.

Os testes unitários devem usar respostas HTTP simuladas; nenhum teste automatizado deve depender de chamadas reais às APIs.

## 10. Critérios de aceite

1. O exemplo de pasta e arquivo acima é interpretado como título, temporada 1 e episódio 1.
2. Um arquivo cujo nome não termina no padrão `-<episódio>.<extensão>` é ignorado com motivo claro.
3. O parser não ultrapassa a raiz informada e não altera mídias.
4. A CLI sem confirmação ou `--auto-guess` não gera timestamps com mapeamentos ambíguos; o modo automático usa estimativas e as registra.
5. A consulta AniSkip recebe MAL ID, episódio resolvido e duração real em segundos.
6. Resposta com `op` e `ed` gera `start`, `end` e `credits` em segundos no JSON compatível.
7. Ausência de dados, erro de rede, `404` e `429` não produzem marcadores falsos nem apagam marcadores existentes.
8. Entradas manuais são preservadas por padrão; conflitos são mostrados antes de qualquer substituição.
9. Modo de prévia não cria nem altera arquivos.
10. Execução sem conectividade mantém resultados anteriores e informa itens pendentes.
11. A geração automática é executada somente na raiz de Anime no startup do Docker; falhas não impedem a inicialização do serviço Plex.
12. Existem testes para arquivo inválido, zeros à esquerda, episódio sem segmento, erro de API, throttling, conflito manual e escrita atômica.

## 11. Configuração de implantação

- Compose local recebe caminhos host por `ANIME_LIBRARY_PATH` e `SERIES_LIBRARY_PATH` no `.env`.
- No ZimaOS, os caminhos host são configurados nos bind mounts em `docker-compose.zimaos.yml`.
- `/config/markers.json` e `/config/metadata-cache.json` são persistidos pelo volume de configuração.
- `AUTO_GENERATE_ANIME_MARKERS` está habilitada nos arquivos Compose fornecidos e desabilitada por padrão fora deles.

## 12. Referências

- [Jikan REST API v4](https://docs.api.jikan.moe/)
- [AniSkip API](https://api.aniskip.com/)
- [Python PlexAPI: objetos de vídeo](https://python-plexapi.readthedocs.io/en/latest/modules/video.html)
