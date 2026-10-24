# Especificação: geração automática de marcadores de anime

## 1. Objetivo

Adicionar um comando para varrer uma pasta de mídia montada pelo usuário, identificar animes e episódios pelo caminho/nome dos arquivos, consultar APIs públicas para obter marcadores de abertura e encerramento e gerar um `markers.json` compatível com o serviço atual.

O recurso gera **marcadores temporais** (segmentos de intro e encerramento), não arquivos de metadados descritivos nem alterações na biblioteca Plex.

## 2. Escopo do MVP

- Anime em arquivos de vídeo locais acessíveis pelo container.
- Consulta de dados externos somente por APIs públicas documentadas.
- Geração local de `markers.json`; sem escrita ou atualização da biblioteca Plex.
- Execução explícita por CLI, com modo de prévia e confirmação antes de gravar.
- Reutilização do formato de marcadores já lido por `JsonMarkerRepository`.
- Nenhuma dependência de API privada do Plex, scraping de páginas ou credencial de usuário Plex para buscar timestamps.

Fora do MVP: filmes, séries não-anime, download/edição de vídeo, identificação por visão computacional, submissão automática de timestamps e sincronização contínua em background.

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
6. Não inferir temporadas especiais, cour, episódios divididos ou recaps automaticamente. Se o número da temporada local não mapear de forma inequívoca à temporada/obra de anime da API, exigir configuração ou confirmação manual.
7. A raiz deverá ser fornecida explicitamente ao comando e montada no container em modo somente leitura. O programa não deve percorrer caminhos fora dessa raiz.

## 4. APIs públicas

### 4.1 Jikan REST API v4 — pesquisa e identificação

- Documentação: <https://docs.api.jikan.moe/>
- Base: `https://api.jikan.moe/v4`
- Endpoint de pesquisa: `GET /anime?q=<título>&limit=<limite>`
- Endpoint de detalhes: `GET /anime/{mal_id}/full`
- Autenticação: nenhuma chave de API.
- Jikan é uma API pública não oficial para dados do MyAnimeList; disponibilidade e limites podem mudar.

Usar o título da pasta como consulta primária. A busca retorna candidatos com `mal_id`, títulos alternativos e dados básicos. O usuário deverá escolher um candidato quando houver mais de uma correspondência plausível. A escolha será persistida localmente para não repetir a pesquisa a cada execução.

### 4.2 AniSkip — timestamps de abertura e encerramento

- API pública: <https://api.aniskip.com/>.
- Endpoint de leitura: `GET https://api.aniskip.com/v2/skip-times/{mal_id}/{episode}?types=op&types=ed&episodeLength={segundos}`
- Autenticação para leitura: nenhuma.
- Os tipos `op` e `ed` representam abertura e encerramento; os intervalos retornados usam segundos.
- Respostas podem não encontrar segmentos; limites podem retornar `429`.

O formato de resposta observado contém `found` e `results`; cada resultado contém `skipType`, `interval.startTime`, `interval.endTime` e `episodeLength`. O contrato, parâmetros e comportamento de erros devem ser revalidados contra a API pública no momento da implementação. Não assumir que temporada local × episódio identifica diretamente o episódio esperado pelo AniSkip.

### 4.3 Duração do arquivo

O parâmetro `episodeLength` deve ser a duração real da mídia em segundos. Preferência:

1. Ler duração do objeto de mídia já reconhecido pelo Plex, quando disponível no fluxo.
2. Para a CLI que recebe uma pasta, usar `ffprobe` instalado no host/container, ou outra biblioteca de leitura de container de mídia validada antes da implementação.
3. Se não for possível obter uma duração válida, não consultar AniSkip para esse arquivo; reportar como pendente.

Não enviar arquivos ou conteúdo de mídia para provedores externos.

## 5. Fluxo funcional

1. Usuário executa uma varredura indicando a raiz de mídia e o arquivo de destino.
2. O sistema percorre somente a raiz indicada e extrai título, temporada local, episódio e extensão.
3. O sistema agrupa arquivos pela série e evita consultas repetidas usando cache local.
4. Para séries ainda não mapeadas, consulta Jikan e apresenta candidatos com título, ano/formato e `mal_id`.
5. Sem candidato ou com resultado ambíguo, deixa a série pendente; não escolhe automaticamente um resultado incerto.
6. Resolve explicitamente a relação entre a temporada local e a série/temporada/numeração usada pelo AniSkip. Uma temporada/cour pode ser um anime separado no MAL; nesses casos, o usuário confirma o título/ID e o offset ou mapeamento dos episódios.
7. Obtém a duração real do arquivo e consulta AniSkip para `op` e `ed`.
8. Valida e normaliza os intervalos em segundos.
9. Exibe uma prévia indicando IDs, episódio, duração, segmentos encontrados, ausentes e conflitos.
10. Com confirmação, mescla os resultados ao `markers.json` sem apagar entradas manuais e grava o arquivo de forma atômica.

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
python -m plex_autoplay metadata scan \
  --media-root /media/anime \
  --output /config/markers.json \
  --cache /config/metadata-cache.json \
  --dry-run
```

Comportamento:

- `--dry-run` é o padrão; não escreve arquivos.
- `--apply` habilita a gravação após mostrar o resumo.
- `--mapping <arquivo>` carrega confirmações de títulos e mapeamentos de temporada/episódio.
- Exit code diferente de zero para erro de configuração, diretório inválido ou falha de gravação; itens sem timestamp permanecem como pendências e são listados no relatório.

Os flags e o formato do arquivo de mapeamento devem ser finalizados antes da implementação da CLI.

## 9. Arquitetura prevista

Seguindo as camadas atuais:

- `domain`: modelos de mídia/segmentos e validação de intervalos; parser puro de nomes/caminhos.
- `application`: caso de uso de scan, resolução explícita de ambiguidades, merge e relatório.
- `application/ports.py`: contratos para pesquisa de anime, consulta de segmentos, leitura de duração e persistência/cache.
- `infrastructure/anime`: adaptador HTTP para Jikan.
- `infrastructure/segments`: adaptador HTTP para AniSkip.
- `infrastructure/media`: leitor de duração.
- `infrastructure/markers`: merge seguro e gravação atômica.
- `__main__.py`/CLI: comando separado do watcher de reprodução.

Os testes unitários devem usar respostas HTTP simuladas; nenhum teste automatizado deve depender de chamadas reais às APIs.

## 10. Critérios de aceite

1. O exemplo de pasta e arquivo acima é interpretado como título, temporada 1 e episódio 1.
2. Um arquivo cujo nome não termina no padrão `-<episódio>.<extensão>` é ignorado com motivo claro.
3. O parser não ultrapassa a raiz informada e não altera mídias.
4. Resultado Jikan ambíguo ou mapeamento de temporada incerto não gera timestamps sem confirmação.
5. A consulta AniSkip recebe MAL ID, episódio resolvido e duração real em segundos.
6. Resposta com `op` e `ed` gera `start`, `end` e `credits` em segundos no JSON compatível.
7. Ausência de dados, erro de rede, `404` e `429` não produzem marcadores falsos nem apagam marcadores existentes.
8. Entradas manuais são preservadas por padrão; conflitos são mostrados antes de qualquer substituição.
9. Modo de prévia não cria nem altera arquivos.
10. Execução sem conectividade mantém resultados anteriores e informa itens pendentes.
11. Existem testes para filme/arquivo inválido, nomes longos, zeros à esquerda, episódio sem segmento, erro de API, throttling, conflito manual e escrita atômica.

## 11. Questões a resolver antes da implementação

- Como identificar continuations/cours e especiais da biblioteca: confirmação interativa ou arquivo de mapeamento por série?
- Como obter `episodeLength` no container final: incluir `ffprobe` na imagem ou exigir Plex como fonte?
- A saída de encerramento deve continuar se chamando `credits` para compatibilidade, mesmo que AniSkip classifique a sequência como `ed`?
- O usuário quer somente anime no primeiro lançamento ou deve haver uma arquitetura genérica para provedores de filmes/séries futuramente?

## 12. Referências

- [Jikan REST API v4](https://docs.api.jikan.moe/)
- [AniSkip API](https://api.aniskip.com/)
- [Python PlexAPI: objetos de vídeo](https://python-plexapi.readthedocs.io/en/latest/modules/video.html)
