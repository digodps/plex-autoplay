# Tasks

## 1. Definir contrato de CLI e descoberta de mídia

- [x] 1.1 Definir o comando CLI para varredura da raiz de mídia, saída do `markers.json`, modo de pré-visualização e confirmação explícita antes da gravação. Verificar com `python -m plex_autoplay --help`.
- [x] 1.2 Implementar o parser de caminhos e nomes de arquivo para extrair título, temporada e episódio, ignorando arquivos não elegíveis e registrando motivos informativos. Verificar com testes unitários para padrões válidos e inválidos.
- [x] 1.3 Modelar entidades de domínio para série, temporada e episódio e centralizar as regras de normalização de temporada e episódios. Verificar com testes unitários de mapeamento e conversão de `01` para `1`.

## 2. Integrar APIs públicas e cache local

- [x] 2.1 Criar adaptadores para Jikan e AniSkip com chamadas HTTP controladas, timeout, e tratamento explícito de falhas (`404`, `429`, indisponibilidade e JSON inválido). Verificar com testes de contrato usando respostas simuladas sem rede real.
- [x] 2.2 Implementar cache local para pesquisas, escolhas do usuário e respostas válidas com TTL e invalidação simples. Verificar com testes de persistência e reuso do cache.
- [x] 2.3 Definir o fluxo de confirmação para casos ambíguos e mapeamentos de temporada/episódio não inequívocos, deixando itens pendentes sem gravar dados incertos. Verificar com saída de prévia e cenários de lista de pendências.

## 3. Gerar preview, mesclar e validar segurança

- [x] 3.1 Implementar a normalização dos intervalos de `op` e `ed` em segundos e a junção com `markers.json` preservando entradas manuais e sem sobrescrita sem confirmação. Verificar com testes unitários de merge e compatibilidade de formato.
- [x] 3.2 Implementar a gravação segura em arquivo temporário e rename atômico, garantindo que a escrita permaneça dentro da raiz configurada e que o arquivo atual seja preservado em falhas. Verificar com testes de escrita atômica e integração do CLI em modo de prévia.
- [x] 3.3 Executar a validação do projeto com `python -m unittest discover -v`, `python -m compileall -q plex_autoplay tests` e `docker compose config` para confirmar compatibilidade e ausência de regressões.

## 4. Configurar bibliotecas e geração no Docker

- [x] 4.1 Configurar variáveis e mounts independentes, somente leitura, para Anime e Séries comuns no Docker Compose local e ZimaOS; persistir marcadores e cache em `/config`.
- [x] 4.2 Executar a varredura automática de Anime no startup, mantendo Séries comuns fora das APIs de anime, mesclando marcadores e registrando estimativas/pendências.
- [x] 4.3 Atualizar documentação e validar testes, compilação e as configurações Compose para ambas as bibliotecas.
