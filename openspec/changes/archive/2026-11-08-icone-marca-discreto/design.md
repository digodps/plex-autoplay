# Design

## Context

Ver `proposal.md` — Why. A marca vive em dois lugares: `AppLogo.vue` (SVG
inline no cabeçalho, `:size="28"`) e `public/favicon.svg`. Ambos usam hoje o
mesmo desenho: `rect` 64×64 com gradiente + borda, triângulo "play"
preenchido e barra preenchida. O restante dos ícones da UI (`AppIcon.vue`)
usa traço (`stroke`) e herda cor do texto.

## Goals / Non-Goals

**Goals:**
- Marca como traço fino, fundo transparente, adaptável ao tema.
- Manter o símbolo (triângulo play + barra) e a proporção para não deslocar
  o layout do cabeçalho.

**Non-Goals:**
- Trocar o símbolo/marca por outra identidade visual.
- Alterar tamanho reservado no cabeçalho ou dependências.
- Mexer em `AppIcon.vue` ou no sprite (fora de escopo).

## Decisions

### 1. Traço (`stroke`) em vez de preenchimento (`fill`)
- **Decisão:** remover gradiente, `rect` de fundo e borda; desenhar o
  triângulo e a barra com `fill="none"` + `stroke="currentColor"`.
- **Rationale:** é o traço que dá o visual discreto pedido e alinha a marca
  ao resto dos ícones. `currentColor` faz a marca seguir a cor do texto
  (preto no claro, branco no escuro), corrigindo também o cinza-claro atual
  que quase some no tema claro.
- **Alternativas:** manter `fill` com uma cor neutra fixa → não adapta ao
  tema; adotar `lucide-vue-next` → dependência nova, desnecessária para 2
  formas.

### 2. Manter `viewBox 0 0 64 64` e `:size`
- **Decisão:** manter o viewBox e o prop `size` como estão.
- **Rationale:** menor diff, zero mudança de layout; só o conteúdo do SVG
  muda.

### 3. Favicon com cor neutra fixa
- **Decisão:** o `favicon.svg` (contexto isolado, sem `currentColor`) usa um
  cinza médio (`#8B949E`) sobre fundo transparente.
- **Rationale:** legível tanto em abas claras quanto escuras.

## Risks / Trade-offs

- Marca perde destaque → aceito: objetivo é discrição. Mitigação: traço
  `stroke-width` 3 no viewBox 64 mantém legibilidade a 28px.
- Favicon neutro pode contrastar pouco em alguns temas → mitigação: cinza
  médio é o compromisso mais seguro entre claro/escuro.

## Migration Plan

Puramente visual. Passos: editar `AppLogo.vue` e `public/favicon.svg`,
rodar build e QA visual. Rollback: `git revert` dos dois arquivos.

## Open Questions

- Nenhuma.
