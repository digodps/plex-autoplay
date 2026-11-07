# Deficiências Resolvidas (EGM - Goldfish Review)

## Revisor A: Arquitetura & Integração
- **A1: Preservação de contrato de proxy de imagem**
  - *Classificação*: Aceito.
  - *Resolução*: O endpoint mapeia `/api/images/{Thumb}` em vez de expor URLs externas do Plex. Segue o mesmo padrão estabelecido no catálogo de séries.

## Revisor S: SOLID & Coesão
- **S1: Separação de responsabilidades da miniatura**
  - *Classificação*: Aceito.
  - *Resolução*: `MarkerRow` permanece restrito aos controles de cada tipo de marcador (`intro` / `credits`), enquanto a miniatura do episódio é posicionada no nível do card do episódio (`<article>` no `MarkerEditorView.vue`). Evita duplicar requisições e DOM desnecessário.

## Revisor C: Clean Code & Testabilidade
- **C1: Fallback visual e resiliência**
  - *Classificação*: Aceito.
  - *Resolução*: Imagem lazy com dimensão reservada e fallback para `<AppIcon name="tv" />` quando `thumb` for nulo, garantindo que a ausência de miniatura no Plex não degrade layout ou funcionalidade.
