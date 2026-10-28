<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "./AppIcon.vue";
import StatusCardSkeleton from "./StatusCardSkeleton.vue";
import StatusCardError from "./StatusCardError.vue";

const props = defineProps({
  /** Nome do ícone (do sprite /icons.svg#ui-*) */
  icon: { type: String, required: true },
  /** Rótulo principal do card */
  label: { type: String, required: true },
  /** Valor principal (número ou string) */
  value: { type: [String, Number], default: "" },
  /** Linhas de detalhamento opcionais: array de {label, value, class?} */
  breakdown: { type: Array, default: () => [] },
  /** Badge opcional: { text, variant } — variant: 'success' | 'warning' | 'danger' | 'neutral' */
  badge: { type: Object, default: null },
  /** Rota de destino (se clicável). Se não informado, card não é link. */
  href: { type: String, default: null },
  /** Estado de carregamento: mostra skeleton */
  loading: { type: Boolean, default: false },
  /** Estado de erro: mostra badge de erro */
  error: { type: Boolean, default: false },
});

const route = useRoute();

const isActiveRoute = computed(() => props.href && route.path === props.href);

const badgeClasses = computed(() => {
  if (!props.badge) return "";
  const base = "badge";
  const variants = {
    success: "badge-success",
    warning: "badge-warning",
    danger: "badge-danger",
    neutral: "",
  };
  return `${base} ${variants[props.badge.variant] ?? ""}`.trim();
});

function handleClick(e) {
  // Se for link, o router-link cuida da navegação.
  // Se não, emite evento para pai decidir (ex.: abrir modal).
  if (!props.href) {
    e.preventDefault();
    // opcional: emitir evento customizado
  }
}
</script>

<template>
  <component
    :is="href ? 'router-link' : 'article'"
    :to="href"
    class="status-card"
    :class="{
      'status-card--clickable': href,
      'status-card--active': isActiveRoute,
      'status-card--error': error,
    }"
    :aria-label="href ? `Ir para ${label}` : undefined"
    @click="handleClick"
  >
    <!-- Skeleton loading -->
    <StatusCardSkeleton v-if="loading" />

    <!-- Conteúdo real -->
    <template v-else>
      <header class="card-header">
        <AppIcon :name="icon" size="lg" aria-hidden="true" />
        <span class="card-label">{{ label }}</span>
        <span v-if="badge" :class="badgeClasses">{{ badge.text }}</span>
      </header>

      <div class="card-value">{{ value }}</div>

      <dl v-if="breakdown.length" class="card-breakdown">
        <div v-for="(row, idx) in breakdown" :key="idx" class="breakdown-row">
          <dt :class="row.class">{{ row.label }}</dt>
          <dd>{{ row.value }}</dd>
        </div>
      </dl>

      <StatusCardError v-if="error" />
    </template>
  </component>
</template>

<style scoped>
.status-card {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 1rem 1.25rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow-1);
  transition:
    box-shadow var(--speed) var(--ease),
    border-color var(--speed) var(--ease);
  min-height: 140px; /* altura mínima para alinhar cards no grid */
}

.status-card:hover {
  box-shadow: var(--shadow-2);
  border-color: var(--border-strong);
}

.status-card--clickable {
  cursor: pointer;
  text-decoration: none;
  color: inherit;
}

.status-card--clickable:focus-visible {
  outline: 2px solid var(--accent);
  outline-offset: 2px;
}

.status-card--active {
  border-color: var(--accent);
  box-shadow:
    0 0 0 1px var(--accent),
    var(--shadow-1);
}

.status-card--error {
  border-color: var(--danger);
}

.card-header {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.card-label {
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.card-value {
  font-size: 2rem;
  font-weight: 700;
  color: var(--text);
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}

.card-breakdown {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
  gap: 0.25rem 0.5rem;
  margin-top: 0.25rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--border);
}

.breakdown-row {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
}

.breakdown-row dt {
  font-size: 0.65rem;
  font-weight: 600;
  color: var(--text-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}

.breakdown-row dd {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--text);
  font-variant-numeric: tabular-nums;
}

/* Badge variants (reusa .badge do style.css global) */
.badge-success {
  color: var(--success);
  border-color: color-mix(in srgb, var(--success) 35%, transparent);
}

.badge-warning {
  color: var(--accent);
  border-color: color-mix(in srgb, var(--accent) 40%, transparent);
}

.badge-danger {
  color: var(--danger);
  border-color: color-mix(in srgb, var(--danger) 35%, transparent);
}
</style>
