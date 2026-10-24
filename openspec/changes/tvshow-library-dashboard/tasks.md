# Tasks

## 1. Descoberta genérica de episódios

- [x] 1.1 Criar modelo neutro de biblioteca, série, temporada e episódio, e implementar parsing de `S01E02`, faixas `S01E02-E03` e `1x02`; testar reconhecimento, faixas sem duplicação e nomes inválidos.
- [x] 1.2 Implementar parsing contextual de `E02` em pastas `Season 1`/`Temporada 1` e preservar o padrão legado `<série>/Temporada n/<arquivo>-NN`; testar precedência, conflitos de temporada e compatibilidade.
- [x] 1.3 Implementar descoberta recursiva para bibliotecas de qualquer tipo, isolando arquivos não reconhecidos e falhas por arquivo; testar árvores com múltiplas séries/temporadas e atualizar a documentação dos padrões suportados.

## 2. Bibliotecas e segurança da raiz de mídia

- [x] 2.1 Implementar persistência atômica das bibliotecas com caminho relativo e classificação, incluindo validação de duplicatas e caminhos inexistentes; testar salvar, carregar, remover e reiniciar.
- [x] 2.2 Implementar listagem e seleção segura de subpastas sob `/media-root`, rejeitando traversal, arquivos e symlinks que escapem da raiz; testar caminhos válidos e tentativas fora do escopo.
- [x] 2.3 Implementar serviços e rotas autenticadas para adicionar, classificar e remover bibliotecas sem tocar nos arquivos de mídia; testar operações e documentar o cadastro pela interface.

## 3. Cobertura e edição de marcadores

- [x] 3.1 Integrar o inventário ao resolvedor atual de marcadores para apresentar cobertura por biblioteca, série, temporada e episódio; testar marcadores existentes, campos ausentes e nomes de série equivalentes.
- [x] 3.2 Implementar adição, edição e remoção manual de `start`, `end` e `credits` em qualquer biblioteca, com validação numérica, confirmação de substituição, lock e gravação atômica; testar valores inválidos, conflitos e preservação do formato de `markers.json`.
- [x] 3.3 Garantir que mesclagem automática preserve marcadores manuais e que apenas bibliotecas Anime usem Jikan/AniSkip; testar sem chamadas externas para outras bibliotecas e atualizar a documentação funcional de marcadores.

## 4. Autenticação e proteção da aplicação

- [x] 4.1 Implementar configuração inicial de administrador e persistência de hash/salt e segredo de sessão em `/config`; testar que nenhuma senha em texto claro seja gravada e que a configuração inicial seja bloqueada após o primeiro uso.
- [x] 4.2 Proteger páginas, dados e mutações com sessão, expiração, cookies seguros, CSRF e limitação de tentativas; testar acesso anônimo, login, logout e requisições sem CSRF.
- [x] 4.3 Documentar login inicial, proteção do volume `/config` e recomendação de HTTPS via proxy para acesso remoto; conferir as instruções no README.

## 5. Estado e execução das varreduras

- [x] 5.1 Implementar serviço compartilhado de varredura para startup e dashboard, com execução assíncrona e exclusão mútua; testar estados de execução, conclusão, falha e rejeição de disparo concorrente.
- [x] 5.2 Persistir atomicamente o relatório mais recente com status, cobertura, itens adicionados, ausentes, ambíguos e erros por biblioteca; testar recuperação de relatório inválido e preservação da última versão válida.
- [x] 5.3 Manter o watcher Plex independente das varreduras, continuar após falha isolada de biblioteca e aplicar enriquecimento somente a Anime; testar falhas parciais e documentar comportamento de startup e nova varredura.

## 6. Dashboard e experiência do administrador

- [x] 6.1 Implementar páginas responsivas em português com tema escuro, resumo de cobertura e navegação por biblioteca/série/temporada/episódio; testar renderização de dados, erros e estado vazio em desktop e mobile.
- [x] 6.2 Implementar gestão de bibliotecas pela interface, atualização periódica autenticada e ação para iniciar varredura com feedback de progresso e concorrência; testar os fluxos pelo cliente de testes web e documentar a operação no README.
- [x] 6.3 Apresentar e permitir edição manual dos marcadores no detalhe do episódio, solicitando confirmação antes de sobrescrever valores; testar atualização refletida no painel e remoção seletiva.

## 7. Docker e integração final

- [x] 7.1 Atualizar Compose local e ZimaOS para rede bridge, raiz `${MEDIA_ROOT_HOST}:/media-root:ro` e porta `${WEB_HOST_PORT:-8080}:8080`, mantendo `PLEX_URL` acessível pelo container; validar com `docker compose config`.
- [x] 7.2 Atualizar `.env.example`, Dockerfile e README com dependências web, volume `/config`, configuração inicial, raiz da mídia, porta publicada e orientação de migração; conferir consistência dos exemplos de implantação.
- [x] 7.3 Executar `python -m unittest discover -v`, `python -m compileall -q plex_autoplay tests` e validar os Compose; verificar integração de autenticação, cadastro de bibliotecas, descoberta, cobertura, edição manual, relatório persistente e acesso ao Plex.
