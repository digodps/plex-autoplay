<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";

const props = defineProps({
  icon: { type: String, required: true },
  label: { type: String, required: true },
  value: { type: [String, Number], default: "" },
  breakdown: { type: Array, default: () => [] },
  badge: { type: Object, default: null },
  href: { type: String, default: null },
  loading: { type: Boolean, default: false },
  error: { type: Boolean, default: false },
});

const route = useRoute();

const isActiveRoute = computed(() => props.href && route.path === props.href);

const badgeClasses = computed(() => {
  if (!props.badge) return "";
  const variants = {
    success: "badge badge--success",
    warning: "badge badge--warning",
    danger: "badge badge--danger",
    neutral: "badge badge--neutral",
  };
  return variants[props.badge.variant] ?? "badge badge--neutral";
});

function handleClick(e) {
  if (!props.href) {
    e.preventDefault();
  }
}
</script>

<template>
  <component
    :is="href ? 'router-link' : 'article'"
    :to="href"
    class="panel"
    :class="{
      'panel--clickable': href,
      'panel--active': isActiveRoute,
      'panel--error': error,
    }"
    :aria-label="href ? `Ir para ${label}` : undefined"
    @click="handleClick"
    role="listitem"
  >
    <!-- Skeleton loading -->
    <div v-if="loading" class="panel__skeleton" aria-hidden="true">
      <div class="skeleton skeleton--icon" />
      <div class="skeleton skeleton--label" />
      <div class="skeleton skeleton--value" />
      <div class="skeleton skeleton--breakdown" v-if="breakdown.length" />
    </div>

    <!-- Conteúdo real -->
    <template v-else>
      <header class="panel__header">
        <img
          :src="`/assets/icons/${icon}.svg`"
          :data-icon="icon"
          class="panel__icon"
          :width="32"
          :height="32"
          aria-hidden="true"
          loading="lazy"
        />
        <span class="panel__label">{{ label }}</span>
        <span v-if="badge" :class="badgeClasses">{{ badge.text }}</span>
      </header>

      <div class="panel__value">{{ value }}</div>

      <dl v-if="breakdown.length" class="panel__breakdown">
        <div v-for="(row, idx) in breakdown" :key="idx" class="panel__breakdown-row">
          <dt :class="row.class">{{ row.label }}</dt>
          <dd>{{ row.value }}</dd>
        </div>
      </dl>

      <div v-if="error" class="panel__error">
        <img src="/assets/icons/warning.svg" data-icon="warning" class="icon" width="16" height="16" aria-hidden="true" />
        Erro ao carregar
      </div>
    </template>
  </component>
</template>

<style scoped>
.panel {
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
  padding: var(--s-4) var(--s-5);
  background: var(--p-surface);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-1);
  min-height: var(--p-h, 180px);
  transition:
    transform var(--speed) var(--ease),
    box-shadow var(--speed) var(--ease),
    border-color var(--speed) var(--ease);
}

.panel:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-2);
  border-color: var(--p-accent);
}

.panel--clickable {
  cursor: pointer;
  text-decoration: none;
  color: inherit;
}

.panel--clickable:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

.panel--active {
  border-color: var(--p-accent);
  box-shadow:
    0 0 0 1px var(--p-accent),
    var(--shadow-1);
}

.panel--error {
  border-color: var(--danger);
}

.panel__header {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  flex-wrap: wrap;
}

.panel__icon {
  flex-shrink: 0;
  color: var(--p-accent);
}

.panel__label {
  font: var(--font-text);
  font-size: var(--t-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--p-muted);
}

.panel__value {
  font: var(--font-display);
  font-size: var(--t-2xl);
  font-weight: 700;
  line-height: 1.2;
  color: var(--p-ink);
  font-variant-numeric: tabular-nums;
}

.panel__breakdown {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(80px, 1fr));
  gap: var(--s-2) var(--s-3);
  margin-top: var(--s-2);
  padding-top: var(--s-3);
  border-top: 1px solid var(--border);
}

.panel__breakdown-row {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.panel__breakdown-row dt {
  font-size: var(--t-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--p-muted);
}

.panel__breakdown-row dd {
  font: var(--font-text);
  font-size: var(--t-sm);
  font-weight: 600;
  color: var(--p-ink);
  font-variant-numeric: tabular-nums;
}

.panel__error {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  margin-top: var(--s-2);
  padding: var(--s-2);
  background: color-mix(in srgb, var(--danger) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--danger) 25%, transparent);
  border-radius: var(--r-md);
  color: var(--danger);
  font-size: var(--t-sm);
}

.badge {
  display: inline-flex;
  align-items: center;
  padding: 2px var(--s-2);
  font-size: var(--t-xs);
  font-weight: 600;
  border-radius: var(--r-full);
  border: 1px solid transparent;
}
.badge--success {
  color: var(--success);
  background: color-mix(in srgb, var(--success) 10%, transparent);
  border-color: color-mix(in srgb, var(--success) 35%, transparent);
}
.badge--warning {
  color: var(--warning);
  background: color-mix(in srgb, var(--warning) 10%, transparent);
  border-color: color-mix(in srgb, var(--warning) 35%, transparent);
}
.badge--danger {
  color: var(--danger);
  background: color-mix(in srgb, var(--danger) 10%, transparent);
  border-color: color-mix(in srgb, var(--danger) 35%, transparent);
}
.badge--neutral {
  color: var(--p-muted);
  background: var(--p-muted);
  border-color: var(--border);
}

/* Skeleton */
.panel__skeleton {
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
  height: 100%;
}
.skeleton {
  background: linear-gradient(
    90deg,
    var(--p-muted) 25%,
    color-mix(in srgb, var(--p-muted) 50%, transparent) 50%,
    var(--p-muted) 75%
  );
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s ease-in-out infinite;
  border-radius: var(--r-sm);
}
.skeleton--icon { width: 32px; height: 32px; border-radius: var(--r-full); }
.skeleton--label { width: 60%; height: 12px; }
.skeleton--value { width: 40%; height: 28px; margin-top: var(--s-2); }
.skeleton--breakdown { width: 100%; height: 40px; margin-top: var(--s-3); }

@keyframes skeleton-loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .panel,
  .skeleton {
    animation: none;
    transition: none;
  }
  .panel:hover {
    transform: none;
  }
}
</style>