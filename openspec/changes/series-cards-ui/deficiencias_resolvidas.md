# Deficiências resolvidas (EGM Goldfish Review A/S/C)

## Revisor A — Arquitetura, dependências e integração

- Achado 1: Alteração restrita à camada de apresentação frontend (card de série). Sem impacto em contratos de API. **ACEITO.**
- Achado 2: Não há novas dependências. **ACEITO.**

## Revisor S — SOLID e coesão

- Achado 1: Card permanece um componente único de apresentação, coeso, sem lógica de negócio. **ACEITO.**

## Revisor C — Clean Code, escopo e testabilidade

- Achado 1: Escopo limitado ao markup/classes do card; não altera composables nem store. **ACEITO.**
- Achado 2: Navegação por clique mantida via `@click="openMarkers"`; acessível (teclado/foco). **ACEITO.**

Nenhum achado exige alteração de requisitos ou artefatos antes da implementação.
