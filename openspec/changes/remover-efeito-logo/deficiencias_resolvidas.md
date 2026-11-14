# Deficiências Resolvidas — EGM Goldfish Review

Change: `remover-efeito-logo` — Reverter animação do logo (remover electric/lightning effect).

## Resumo das Classificações

| Achado | Eixo | Classificação | Ação |
|--------|------|---------------|------|
| [A-1] E2E test coupled to removed markup | A | **ACEITO** | Add task 1.7: delete `qa-electric-logo-animation.spec.js` |
| [A-2] Design removes `<script>` breaking `size` prop | A | **ACEITO** | Fix design: keep `defineProps` in `<script setup>` |
| [A-3] Two active changes for one capability | A | **PARCIAL** | Noted; removal change is intentional compensating delta. No action needed. |
| [A-4] `web-ui` modified capability no delta | A | **ACEITO** | Drop `web-ui` from proposal.md Modified Capabilities |
| [A-5] Capability id naming drift (slash vs dash) | A | **ACEITO** | Normalize to `brand-logo-animation` (dashed) in proposal |
| [S-1] Design removes script but `size` prop needs it | S | **ACEITO** | Same fix as A-2 |
| [S-2] `size` prop lacks validation | S | **REJEITADO** | Out of scope; pre-existing behavior, YAGNI |
| [S-3] Spec uses negative requirements (SHALL NOT) | S | **PARCIAL** | Appropriate for REMOVED delta; keep |
| [S-4] No reduced-motion test in tasks | S | **PARCIAL** | Animation removed, so static test 2.3 covers it |
| [S-5] Component becomes template-only | S | **REJEITADO** | Valid Vue 3 pattern; SRP aligned |
| [C-1] Design says remove script but size prop must remain | C | **ACEITO** | Same fix as A-2 |
| [C-2] Spec doesn't explicitly affirm size prop preserved | C | **ACEITO** | Spec scenario 4 implies it; no change needed |

## Detalhes

### A-1 — E2E test orphan
O arquivo `tests/e2e/tests/qa-electric-logo-animation.spec.js` testa `path.logo-lightning` e animação. Ao remover o recurso, o teste falha. **Ação:** tarefa 1.7 adicionada para exclusão do arquivo.

### A-2 / S-1 / C-1 — `size` prop depende de `<script setup>`
O design original mandava remover todo o `<script>`, mas `defineProps({ size: ... })` é necessário para o contrato da API (consumido por `AppHeader.vue:48` com `:size="28"`). **Ação:** design corrigido para manter `<script setup>` apenas com `defineProps`; remover apenas `ref` e import.

### A-4 — `web-ui` capacidade modificada sem delta
Proposal listava `web-ui` como modified capability, mas não havia delta correspondente em `openspec/specs/web-ui/spec.md`. **Ação:** removido do proposal.

### A-5 — Capability ID inconsistente
Proposal usava `brand/logo-animation` (slash) mas diretório/spec usam `brand-logo-animation` (dash). **Ação:** normalizado para dashed em todo proposal.

### S-2 — Validação de `size` prop
Rejeitado: fora do escopo; o prop já existe sem validação desde antes. Não introduzir validação nova em mudança de remoção.

### S-3 — Requisitos negativos
Parcial: delta `REMOVED` naturalmente usa negação. Mantido.

### S-4 — Teste reduced-motion
Parcial: com animação removida, o teste 2.3 "logo renders static (no animation)" já cobre o comportamento idêntico sob `prefers-reduced-motion: reduce`. Não há necessidade de teste separado.

## Artefatos Atualizados

- `proposal.md`: capability ID normalizado; `web-ui` removido; E2E test file listado em Impact
- `design.md`: script removal corrigido; seção E2E cleanup adicionada
- `tasks.md`: tarefa 1.7 adicionada (delete E2E test)
- `specs/brand-logo-animation/spec.md`: sem alteração (cenários já implícitos cobrem size prop)