# Design

## Context

O projeto já possui uma camada de persisting de marcadores via `JsonMarkerRepository`, que lê `markers.json` e no fluxo atual não gera nenhum marcador automático. A proposta precisa fitar a arquitetura leve/hexagonal do projeto, mantendo regras de domínio testáveis sem rede e respeitando a convenção de "introduções" e "créditos" já consumida pelo serviço de playback.

## Goals / Non-Goals

**Goals:**
- Gerar marcadores de intro e credits automaticamente a partir das pastas de Anime montadas pelo Docker.
- Permitir que múltiplas raízes de Anime e Séries comuns sejam configuradas separadamente e montadas somente leitura.
- Consultar as APIs Jikan/AniSkip somente para Anime; estimar e registrar associações/offsets incertos no modo de inicialização.
- Preservar compatibilidade do formato de `markers.json` e evitar sobrescrita involuntária de dados manuais.

**Non-Goals:**
- Alterar a biblioteca Plex ou escrever metadados em arquivos de mídia.
- Gerar marcadores para séries comuns, filmes, reconhecimento por visão computacional ou sincronização contínua em background.
- Submeter automaticamente timestamps para serviços externos ou exigir credenciais de usuário.

## Decisions

- O Docker habilita a geração de Anime no bootstrap do serviço; a CLI continua disponível para prévia e execução manual. A geração automática pode ser desabilitada por configuração e falhas na varredura não impedem a inicialização do serviço Plex.
- A lógica de identificação será separada em ports/adapters: um serviço de domínio para descoberta, uma porta para Jikan e outra para AniSkip. Isso preserva a testabilidade sem rede e impede acoplamento com requisições HTTP na camada de regras.
- A CLI interativa pode solicitar seleção/offset quando necessário. No bootstrap não interativo, o modo `auto_guess` escolhe candidato/offset estimados e registra cada suposição nos logs; itens sem dados utilizáveis permanecem pendentes.
- A gravação do arquivo será atômica: gerar um arquivo temporário no mesmo diretório, validar o conteúdo e renomear no final. Isso preserva o estado atual do arquivo e reduz risco de corrupção em falhas de I/O.
- O cache local ficará em um arquivo separado de `markers.json` para não conflitar com o formato de saída do serviço e garantir migração clara, se o esquema for ampliado no futuro.

## Risks / Trade-offs

- [Rate limiting público] → Mitigação com timeout, retry controlado, respeito a `Retry-After` e cache de respostas válidas.
- [Estimativas incorretas de títulos/temporadas] → Mitigação com registro explícito das suposições no resultado e nos logs; a CLI interativa mantém a opção de confirmar manualmente.
- [Duração do arquivo indisponível] → Mitigação com fallback para não consultar AniSkip e registrar o episódio como pendente.
- [Formato de `markers.json` sem metadados externos] → Mitigação com uso de arquivo de metadata-cache separado e sem alterar a estrutura atual do contrato de marcadores.

## Migration Plan

1. Introduzir a descoberta e o parser de caminho sem alterar o comportamento atual do monitoramento do Plex.
2. Adicionar as integrações para Jikan e AniSkip com tratamento de erro, cache local e registro de estimativas.
3. Montar cada biblioteca informada pelo usuário em um destino somente leitura próprio e executar a varredura automática em todas as raízes de Anime.
4. Validar a mesclagem do resultado em `markers.json`, preservando marcadores existentes, e manter a CLI de prévia/execução manual.
5. Executar a suíte de validação do projeto e validar as configurações Docker Compose/ZimaOS.

## Resolved Deployment Configuration

- No Compose local, as origens padrão são informadas por `ANIME_LIBRARY_PATH` e `SERIES_LIBRARY_PATH` no `.env`; raízes adicionais exigem mounts próprios.
- No ZimaOS, as origens são configuradas em bind mounts próprios; `ANIME_MEDIA_DIRS` e `SERIES_MEDIA_DIRS` listam os destinos internos, e `/config` persiste os marcadores e o cache.
