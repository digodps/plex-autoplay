# Deficiências Resolvidas — dashboard-botoes-tamanhos

EGM Goldfish Review (A/S/C) executado em paralelo. Cada achado classificado: **aceito**, **rejeitado** (com motivo), ou **parcial**.

---

## Architecture (A) — 6 achados

| # | Achado | Decisão | Justificativa |
|---|--------|---------|---------------|
| A1 | `tokens.css` ausente; design system manda criar | **Parcial** | `tokens.css` pertence ao redesign-frontend-neutral (não mergeado). Escopo deste change é apenas Dashboard. Solução: classes `@layer components` em `style.css` como *single source of truth* local. Tokens globais adiados. |
| A2 | Botões repetidos em 4 views; recomenda `AppButton` | **Rejeitado** | YAGNI. Uma tela, 4 botões, zero consumidores atuais. Componente adiciona complexidade sem payoff. Classes compartilhadas em `style.css` resolvem DRY. |
| A3 | `px-*`/`py-*` não consome CSS vars; plano troca hardcode por hardcode | **Parcial** | Aceito o princípio (não hardcodar padding nas views). Implementação: valores fixos nas classes `.btn-md` em `style.css` (uma fonte). Tailwind `@apply` resolve. |
| A4 | Nomenclatura de tokens inconsistente entre spec/tasks | **Aceito** | Alinhado: spec/tasks agora usam `.btn`/`.btn-md` (classes), sem variáveis `--btn-*`. |
| A5 | `marker-row.css` refere variáveis indefinidas (`--radius-sm`, `--bg-sunken`, `--mono`) | **Rejeitado** | Pré-existente, não relacionado a este change. Não bloqueia. |
| A6 | Estados focus/disabled acoplados por botão | **Aceito** | Centralizados em `.btn` base class. Todos os 4 botões herdam. |

---

## SOLID (S) — 5 achados

| # | Achado | Decisão | Justificativa |
|---|--------|---------|---------------|
| S1 | Scope creep (SRP): change também exige tokens cross-app | **Aceito** | Escopo restrito a `DashboardView.vue` + classes utilitárias em `style.css`. Tokens globais removidos do spec/design/tasks. |
| S2 | DRY duplicação real mas contida (3 conjuntos de classes) | **Aceito** | Extrair 3–4 classes utilitárias compartilhadas (`.btn`, `.btn-md`, variantes). Resolvido no design. |
| S3 | Novo `AppButton` seria YAGNI | **Aceito** | Rejeitado componente. Classes utilitárias bastam. Reavaliar só quando ≥2 views compartilharem markup. |
| S4 | Padronização não unifica app (SyncView usa tamanhos diferentes) | **Parcial** | Aceito como dado: este change é piloto no Dashboard. Promoção a tokens cross-app só quando outra view adotar. |
| S5 | Spec re-especifica navegação só para anexar "token padronizado" | **Aceito** | MODIFIED removido. Substituído por ADDED focado apenas em estilo (classes + altura computada). Comportamento de navegação permanece no spec principal. |

---

## Clean Code (C) — 10 achados

| # | Achado | Decisão | Justificativa |
|---|--------|---------|---------------|
| C1 | `--btn-*` ausentes no design system | **Parcial** | Substituído por classes `.btn`/`.btn-md` em `style.css` (fonte única). |
| C2 | Padding inconsistente nos 4 botões | **Aceito** | Core do change. Todos viram `px-4 py-2` via `.btn-md`. |
| C3 | Retry buttons sem `focus-visible` | **Aceito** | `.btn` base inclui `focus-visible:ring-2` + offset; herdado por todos. |
| C4 | Retry buttons sem estado `disabled` | **Aceito** | Adicionado `:disabled="store.isLoading"` + `disabled:opacity-60` nos 2 retry buttons. |
| C5 | Dark focus ring offset incompleto no botão secundário | **Aceito** | `.btn-primary`/`.btn-secondary`/`.btn-danger`/`.btn-warning` todos incluem `dark:focus-visible:ring-offset-gray-900`. |
| C6 | Critérios de aceite vagos ("altura idêntica", "usa CSS vars") | **Aceito** | Spec ADDED reescrito: cenários testáveis via presença de classes + altura computada em pixels. |
| C7 | Tasks referência classes que não existem | **Aceito** | Tasks atualizadas: referenciam `.btn`/`.btn-md`/`.btn-{primary,secondary,danger,warning}` definidas no design. |
| C8 | Retry buttons sem `text-sm` | **Aceito** | `.btn` base define `text-sm`; herdado. |
| C9 | `rounded-xl` hardcoded; spec exige token `--radius-md` | **Parcial** | `rounded-xl` movido para `.btn` base class (uma fonte). Token `--radius-md` adiado com tokens globais. |
| C10 | `sm` size proposto mas Dashboard não usa | **Aceito** | `.btn-sm` deferido (ponytail). Apenas `.btn-md` implementado agora. |

---

## Resumo das decisões arquiteturais

1. **Sem `tokens.css`** — classes `@layer components` em `style.css` são a fonte única deste change.
2. **Sem `AppButton.vue`** — YAGNI; utilitários CSS resolvem.
3. **Apenas tamanho `md`** — `.btn-sm` adiado até surgir necessidade real.
4. **Spec ADDED apenas** — cenários testáveis via classes + altura computada; nada de re-especificar navegação.
5. **Paridade total de estados** — todos 4 botões herdam focus-visible, disabled, dark-mode, hover, transição da `.btn` base.