# Design

## Context

O projeto já possui uma camada de persisting de marcadores via `JsonMarkerRepository`, que lê `markers.json` e no fluxo atual não gera nenhum marcador automático. A proposta precisa fitar a arquitetura leve/hexagonal do projeto, mantendo regras de domínio testáveis sem rede e respeitando a convenção de "introduções" e "créditos" já consumida pelo serviço de playback.

## Goals / Non-Goals

**Goals:**
- Gerar marcadores de intro e credits a partir de arquivos de anime em uma pasta local informada pelo usuário.
- Resolver anime/episódio com APIs públicas documentadas e manter uma operação de preview antes da gravação.
- Preservar compatibilidade do formato de `markers.json` e evitar sobrescrita involuntária de dados manuais.

**Non-Goals:**
- Alterar a biblioteca Plex ou escrever metadados em arquivos de mídia.
- Suportar filmes, séries não-anime, reconhecimento por visão computacional ou sincronização contínua em background.
- Submeter automaticamente timestamps para serviços externos ou exigir credenciais de usuário.

## Decisions

- A geração será comandada por CLI, em vez de disparar automaticamente no bootstrap do serviço principal. Isso mantém o comportamento atual do projeto estável e exige uma ação explícita do usuário para varrer uma raiz de mídia.
- A lógica de identificação será separada em ports/adapters: um serviço de domínio para descoberta, uma porta para Jikan e outra para AniSkip. Isso preserva a testabilidade sem rede e impede acoplamento com requisições HTTP na camada de regras.
- O fluxo de seleção do candidato será orientado por confirmação manual em cenários ambíguos. Em vez de assumir uma resposta "boa o bastante", o sistema marca o item como pendente e exige ação humana quando a correspondência não é inequívoca.
- A gravação do arquivo será atômica: gerar um arquivo temporário no mesmo diretório, validar o conteúdo e renomear no final. Isso preserva o estado atual do arquivo e reduz risco de corrupção em falhas de I/O.
- O cache local ficará em um arquivo separado de `markers.json` para não conflitar com o formato de saída do serviço e garantir migração clara, se o esquema for ampliado no futuro.

## Risks / Trade-offs

- [Rate limiting público] → Mitigação com timeout, retry controlado, respeito a `Retry-After` e cache de respostas válidas.
- [Ambiguidade de títulos/temporadas] → Mitigação com confirmação manual, marcação de pendência e sem escolha automática de resultado incerto.
- [Duração do arquivo indisponível] → Mitigação com fallback para não consultar AniSkip e registrar o episódio como pendente.
- [Formato de `markers.json` sem metadados externos] → Mitigação com uso de arquivo de metadata-cache separado e sem alterar a estrutura atual do contrato de marcadores.

## Migration Plan

1. Introduzir a descoberta e o parser de caminho sem alterar o comportamento atual do monitoramento do Plex.
2. Adicionar as portas para Jikan e AniSkip e os adaptadores com tratamento de erro e cache local.
3. Implementar a etapa de preview e confirmação, integrando a lógica a um comando explícito do CLI.
4. Validar a mesclagem do resultado em `markers.json` e manter o arquivo atual intacto sem sobrescrita sem confirmação.
5. Executar a suíte de validação do projeto para confirmar compatibilidade e estabilidade.

## Open Questions

- O comando CLI final deve usar um valor padrão para `--output` ou exigir a passagem explícita do caminho do arquivo de marcadores em todas as execuções?
- A carga de cache local deve ser controlada por um TTL fixo por padrão ou por um valor configurável por ambiente?
