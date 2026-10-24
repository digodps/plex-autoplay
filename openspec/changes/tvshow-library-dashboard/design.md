# Design

## Context

Ver `proposal.md` para motivação e `specs/tvshow-library-dashboard/spec.md` para o contrato funcional. Hoje o scanner em `plex_autoplay/infrastructure/media/directory_monitor.py` reconhece uma estrutura limitada de pastas e nomes de episódios, e a descoberta está acoplada ao enriquecimento Anime via Jikan/AniSkip. `JsonMarkerRepository` já lê um JSON compatível com marcadores por série, temporada/episódio e valores padrão. O serviço não tem UI web nem armazenamento de estado de varredura; `/config` é persistido nos Compose.

## Goals / Non-Goals

**Goals:**
- Tornar descoberta e inventário de episódios independentes do gênero ou tipo da biblioteca.
- Manter identificação e timestamps obtidos por provedores como um enriquecimento opcional, específico de Anime.
- Reutilizar um inventário comum para cobertura de marcadores e telas por biblioteca/série/temporada/episódio.
- Centralizar adições manuais e geração automática em operações de persistência validadas, atômicas e sem sobrescrita acidental.
- Usar o mesmo processo do watcher Plex e dashboard, evitando um segundo container de aplicação.

**Non-Goals:**
- Buscar automaticamente timestamps de abertura/créditos de séries comuns em provedores externos.
- Reproduzir, analisar visualmente, transcodificar ou modificar arquivos de vídeo.
- Dar suporte a filmes, bibliotecas fora da raiz montada, múltiplos administradores ou edição da configuração Docker a partir da aplicação.
- Alterar o formato de marcadores já consumido pelo serviço de playback.

## Decisions

### Descoberta genérica em camada separada

Extrair um modelo neutro de episódio de TV (biblioteca, série, temporada, episódio, caminho e duração quando disponível) e separar parsing/descoberta de enriquecimento. O parser aplica padrões explícitos e testados:

- `S01E02` e faixa `S01E02-E03` em qualquer componente do nome do arquivo;
- `1x02`;
- `E02` quando uma pasta ancestral imediata fornece `Season 1` ou `Temporada 1`;
- formato legado `<série>/Temporada n/<arquivo>-NN`.

O título da série é derivado da pasta de série relativa à biblioteca, removendo níveis de temporada. Arquivos fora dos padrões não são classificados como episódio e aparecem como não reconhecidos ou ignorados, sem parar a descoberta. Parsers específicos de nome de arquivo prevalecem sobre contexto de pasta; um conflito entre temporada do arquivo e pasta é registrado como ambiguidade, não resolvido silenciosamente. Faixas multi-episódio geram associações para cada episódio sem duplicar o arquivo no inventário.

Uma função genérica de descoberta é preferível a ampliar o scanner atual em `if` específicos para Anime, porque o inventário e a cobertura serão comuns a todos os tipos de biblioteca.

### Inventário genérico com enriquecimento opcional por tipo

O serviço de varredura descobre episódios em todas as bibliotecas cadastradas e calcula a cobertura de marcadores sem depender da classificação. Bibliotecas classificadas como Anime também podem passar pelo enriquecimento Jikan/AniSkip existente; as demais continuam totalmente disponíveis no inventário, painel de cobertura e edição manual, sem chamadas a esses provedores. Interfaces dos providers não recebem caminhos nem conteúdo de mídia.

### Serviço de marcadores centralizado

Adicionar operações de leitura e escrita para marcadores manuais por título/temporada/episódio, mantendo `start`, `end` e `credits` em segundos e o esquema atual. Validar que os valores são numéricos, finitos, não negativos e coerentes (`start <= end` quando ambos existirem). Uma alteração requer confirmação se campos existentes forem substituídos. Remoção afeta somente a entrada/campos escolhidos. Toda escrita usa lock do processo, temporário no mesmo diretório e rename atômico. O dashboard mostra marcadores existentes pelo mesmo resolvedor que informa cobertura; marcadores nativos Plex não são editados por esta interface.

### Dashboard e autenticação

Adicionar Flask com templates Jinja próprios e servir com Waitress. Um layout responsivo de tema escuro, navegação lateral, cartões de contagem, pesquisa/filtros e detalhes por episódio fornece o visual pedido sem framework frontend, build separado ou assets externos. JavaScript leve consulta o endpoint autenticado de status periodicamente e atualiza progresso; o botão de varredura usa POST.

No primeiro acesso, um wizard cria um administrador; guardar em `/config` salt e hash PBKDF2-HMAC-SHA256, nunca a senha. As sessões são assinadas por segredo aleatório persistido em `/config`, com cookie `HttpOnly`, `SameSite=Lax`, expiração e proteção CSRF nas mutações. Bloquear setup após a criação, limitar tentativas e exigir login para dados, seleção de diretórios, varredura e edição de marcadores. O volume `/config` deve ser protegido pelo operador.

### Gestão segura de bibliotecas dentro de uma raiz

O Compose monta uma raiz única de host em `/media-root:ro`. A UI apresenta diretórios filhos dessa raiz e salva caminhos relativos e tipo da biblioteca em JSON sob `/config`. A validação resolve caminhos reais e rejeita traversal, symlinks externos, arquivos, caminhos inexistentes e duplicatas. A descoberta percorre apenas pastas de bibliotecas cadastradas. O container não recebe socket Docker, mounts graváveis ou capacidade para criar bind mounts após iniciar.

Para colocar uma biblioteca que esteja fora da raiz escolhida, o operador deverá alterar o mount na implantação; a UI não controla paths arbitrários no host.

### Estado persistente e varredura assíncrona

Compartilhar um serviço de varredura entre startup e dashboard. Ele mantém uma única execução por processo, registra status (`queued`, `running`, `completed`, `failed`), timestamps, cobertura e listas de adicionados, ausentes, ambíguos e erros. Um relatório JSON atômico em `/config` guarda apenas a execução mais recente; bibliotecas selecionadas têm arquivo separado. A escrita do relatório não substitui o resultado anterior por JSON inválido.

O watcher Plex e o web server inicializam independentemente do trabalho de varredura. No startup, a execução é submetida em background. Um disparo manual durante a execução retorna conflito e não cria outro worker. Erro em uma biblioteca é capturado por biblioteca, persistido e não impede processar as demais.

### Rede Docker

Remover `network_mode: host`, ouvir em `0.0.0.0:8080` dentro do container e mapear `${WEB_HOST_PORT:-8080}:8080`. Montar a raiz host por variável de implantação como `${MEDIA_ROOT_HOST}:/media-root:ro`. Configurar `PLEX_URL` para endereço acessível pela bridge, documentando endereço LAN do Plex no ZimaOS e `host.docker.internal`/`host-gateway` quando aplicável ao Compose local. Não presumir que `localhost` aponta para o host.

## Risks / Trade-offs

- [Nomes e estruturas de episódios Plex variam e podem produzir interpretações conflitantes] → Usar precedência determinística, casos de teste para formatos suportados e apresentar ambiguidade/conflito em vez de escolher valores invisivelmente.
- [Título de pasta pode não coincidir com a chave já usada em markers.json] → Normalizar comparação sem diferenciar caixa para exibição/cobertura, mas preservar a chave original; permitir escolher/criar vínculo explícito antes de uma gravação manual quando houver mais de uma correspondência.
- [Raiz host ampla revela nomes de diretórios] → Requerer autenticação, montar somente leitura e recomendar o menor diretório pai que contenha as bibliotecas desejadas.
- [Bridge quebra configurações Plex baseadas em localhost] → Exigir `PLEX_URL` alcançável pelo container e documentar host-gateway/endereço LAN antes de atualizar.
- [Falha ao gravar marcador ou relatório pode causar divergência] → Fazer validação e escrita atômica; manter a última versão válida e exibir o erro operacional.
- [Aplicação manual pode sobrescrever dado existente por erro de UI] → Mostrar valor atual, exigir confirmação de substituição e testar atualização concorrente sob lock.
- [HTTP sem TLS expõe credenciais em redes não confiáveis] → Restringir recomendação à LAN confiável e documentar proxy HTTPS para uso remoto.

## Migration Plan

1. Atualizar configuração Docker para raiz única de mídia somente leitura, porta publicada e rede bridge; definir `PLEX_URL` endereçável desde o container.
2. Manter `/config/markers.json` e cache existentes; criar os novos arquivos de bibliotecas, credenciais e relatório sem alterar arquivos anteriores.
3. No primeiro boot, cadastrar administrador e bibliotecas no dashboard. Confirmar que todas as bibliotecas aparecem no inventário e na cobertura; somente Anime usa o enriquecimento existente.
4. Rollback: restaurar imagem e Compose anteriores. O `markers.json` permanece no contrato antigo; os arquivos novos sob `/config` não são consumidos pela versão antiga e podem ser preservados.

## Open Questions

Não há decisões funcionais em aberto. O reconhecimento de padrões fica limitado aos formatos especificados; novos padrões podem ser adicionados posteriormente com testes.
