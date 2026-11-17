<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useI18n } from "vue-i18n";
import AppIcon from "@/components/AppIcon.vue";

const { t } = useI18n();

const props = defineProps({
  icon: { type: String, default: "library" },
  label: { type: String, required: true },
  value: { type: [String, Number], default: "" },
  breakdown: { type: Array, default: () => [] },
  badge: { type: Object, default: null },
  /** accent | success | warning | danger */
  tone: { type: String, default: "accent" },
  href: { type: String, default: null },
  loading: { type: Boolean, default: false },
  error: { type: Boolean, default: false },
});

const route = useRoute();
const isActive = computed(() => props.href && route.path === props.href);

const resolvedTone = computed(() => {
  if (props.tone !== "accent") return props.tone;
  const variant = props.badge?.variant;
  return variant === "success" || variant === "danger" || variant === "warning"
    ? variant
    : "accent";
});

const toneColor = computed(() => `var(--${resolvedTone.value})`);

const tileStyle = computed(() => ({
  color: toneColor.value,
  backgroundColor: `color-mix(in srgb, ${toneColor.value} 12%, transparent)`,
}));

const pillStyle = computed(() => ({
  color: toneColor.value,
  backgroundColor: `color-mix(in srgb, ${toneColor.value} 10%, transparent)`,
}));
</script>

<template>
  <component
    :is="href ? 'router-link' : 'article'"
    :to="href"
    class="card panel"
    :class="{ 'card--active': isActive, 'card--error': error, 'card--link': href }"
    style="--p-h: 280px; display: flex; flex-direction: column;"
  >
    <template v-if="loading">
      <div class="skeleton-card" style="flex: 1; display: flex; flex-direction: column; gap: var(--s-4);">
        <div class="skeleton-row" style="display: flex; justify-content: space-between; align-items: flex-start;">
          <div class="skeleton skeleton-icon"></div>
          <div class="skeleton skeleton-title"></div>
        </div>
        <div class="skeleton-body" style="display: flex; flex-direction: column; gap: var(--s-2);">
          <div class="skeleton skeleton-label"></div>
          <div class="skeleton skeleton-value"></div>
        </div>
      </div>
    </template>

    <template v-else>
      <div class="panel__content" style="display: flex; flex-direction: column; flex: 1; padding: var(--s-4);">
        <div class="card__header" style="display: flex; align-items: flex-start; justify-content: space-between; gap: var(--s-3);">
          <span class="card__icon" :style="tileStyle">
            <AppIcon :name="icon" size="lg" />
          </span>
          <span v-if="badge" class="card__badge" :style="pillStyle">
            <span class="card__badge-dot" :style="{ backgroundColor: toneColor }"></span>
            {{ badge.text }}
          </span>
        </div>

        <div class="card__body" style="margin-top: var(--s-4); flex: 1; display: flex; flex-direction: column;">
          <p class="card__label" style="margin: 0 0 var(--s-1); font-size: var(--t-small); font-weight: 500; text-transform: uppercase; letter-spacing: 0.05em; color: var(--muted);">{{ label }}</p>
          <p class="card__value" style="margin: 0; font-size: var(--t-h2); font-weight: 700; font-variant-numeric: tabular-nums; color: var(--ink); line-height: 1.1;">{{ value }}</p>
        </div>

        <dl v-if="breakdown.length" class="card__breakdown" style="margin-top: var(--s-4); padding-top: var(--s-4); border-top: 1px solid var(--border); display: grid; grid-template-columns: repeat(2, 1fr); gap: var(--s-3) var(--s-4);">
          <div v-for="(row, idx) in breakdown" :key="idx" class="card__breakdown-item" style="display: flex; flex-direction: column; opacity: 1;" :class="{ 'card__breakdown-item--zero': row.value === '0' || row.value === '0%' }">
            <dt class="card__breakdown-label" style="font-size: 11px; font-weight: 500; text-transform: uppercase; letter-spacing: 0.05em; color: var(--muted);">{{ row.label }}</dt>
            <dd class="card__breakdown-value" style="margin-top: 4px; font-size: var(--t-small); font-weight: 600; font-variant-numeric: tabular-nums; color: var(--ink);" :class="row.class">{{ row.value }}</dd>
          </div>
        </dl>

        <p v-if="error" class="card__error" style="margin-top: var(--s-4); font-size: var(--t-small); font-weight: 500; color: var(--danger, #e03e3e);">{{ t("common.errorLoading") }}</p>
      </div>
    </template>
  </component>
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-sm);
  transition: border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

.card--link {
  cursor: pointer;
}

@media (hover: hover) and (pointer: fine) {
  .card:not(.card--link):hover {
    border-color: color-mix(in oklab, var(--ink) 30%, transparent);
    box-shadow: var(--shadow-md);
  }
  .card--link:hover {
    border-color: color-mix(in oklab, var(--action) 50%, transparent);
    box-shadow: var(--shadow-md);
  }
}

.card--active {
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), var(--shadow-sm);
}

.card--error {
  border-color: var(--danger, #e03e3e) !important;
}

.card--link:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.panel__content {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: var(--s-4);
}

.card__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--s-3);
}

.card__icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  flex-shrink: 0;
  border-radius: var(--r-md);
  transition: transform var(--m-normal) var(--m-ease);
}

@media (hover: hover) and (pointer: fine) {
  .card:hover .card__icon {
    transform: scale(1.05);
  }
}

.card__badge {
  display: inline-flex;
  align-items: center;
  gap: var(--s-1);
  padding: 4px 10px;
  font-size: 11px;
  font-weight: 500;
  text-transform: capitalize;
  border-radius: 999px;
  white-space: nowrap;
}

.card__badge-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  flex-shrink: 0;
}

.card__body {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.card__label {
  font-size: var(--t-small);
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--muted);
  margin: 0 0 var(--s-1);
}

.card__value {
  font-size: var(--t-h2);
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  color: var(--ink);
  line-height: 1.1;
  margin: 0;
}

.card__breakdown {
  margin-top: var(--s-4);
  padding-top: var(--s-4);
  border-top: 1px solid var(--border);
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--s-3) var(--s-4);
}

.card__breakdown-item {
  display: flex;
  flex-direction: column;
  opacity: 1;
  transition: opacity var(--m-fast) var(--m-ease);
}

.card__breakdown-item--zero {
  opacity: 0.6;
}

.card__breakdown-label {
  font-size: 11px;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--muted);
}

.card__breakdown-value {
  margin-top: 4px;
  font-size: var(--t-small);
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--ink);
}

.card__error {
  margin-top: var(--s-4);
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--danger, #e03e3e);
}

/* Skeleton */
.skeleton-card {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: var(--s-4);
}

.skeleton-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.skeleton {
  background: linear-gradient(90deg, var(--surface) 25%, var(--border) 50%, var(--surface) 75%);
  background-size: 200% 100%;
  animation: skeleton-loading 1.5s infinite;
  border-radius: var(--r-md);
}

.skeleton-icon {
  width: 48px;
  height: 48px;
  flex-shrink: 0;
}

.skeleton-title {
  height: 28px;
  width: 80px;
}

.skeleton-body {
  display: flex;
  flex-direction: column;
  gap: var(--s-2);
}

.skeleton-label {
  height: 14px;
  width: 60%;
}

.skeleton-value {
  height: 24px;
  width: 40%;
}

@keyframes skeleton-loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
</style>