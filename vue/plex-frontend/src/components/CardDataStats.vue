<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import AppIcon from "@/components/AppIcon.vue";

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

// Sempre reage ao estado do badge quando nenhum tone explícito é passado.
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
    class="group relative flex flex-col rounded-2xl border bg-white p-6 shadow-sm transition duration-200 dark:bg-gray-800"
    :class="[
      isActive
        ? 'border-primary ring-1 ring-primary'
        : 'border-stroke hover:border-gray-400 hover:shadow-md dark:border-gray-700',
      error ? '!border-meta-1' : '',
      href ? 'block focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900' : '',
    ]"
  >
    <template v-if="loading">
      <div class="flex items-start justify-between">
        <div class="h-12 w-12 animate-pulse rounded-xl bg-gray-2 dark:bg-meta-4"></div>
        <div class="h-5 w-16 animate-pulse rounded-full bg-gray-2 dark:bg-meta-4"></div>
      </div>
      <div class="mt-6 space-y-2">
        <div class="h-6 w-2/3 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
        <div class="h-4 w-1/3 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
      </div>
    </template>

    <template v-else>
      <div class="flex items-start justify-between gap-3">
        <span
          class="flex h-12 w-12 flex-none items-center justify-center rounded-xl transition-transform duration-200 group-hover:scale-105"
          :style="tileStyle"
        >
          <AppIcon :name="icon" size="lg" />
        </span>
        <span
          v-if="badge"
          class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium capitalize"
          :style="pillStyle"
        >
          <span class="h-1.5 w-1.5 rounded-full" :style="{ backgroundColor: toneColor }"></span>
          {{ badge.text }}
        </span>
      </div>

      <div class="mt-5">
        <p class="text-xs font-semibold uppercase tracking-wider text-gray-600 dark:text-bodydark">
          {{ label }}
        </p>
        <p
          class="mt-1 truncate text-2xl font-bold tracking-tight text-black tabular-nums dark:text-white xl:text-3xl"
        >
          {{ value }}
        </p>
      </div>

      <dl
        v-if="breakdown.length"
        class="mt-5 grid grid-cols-2 gap-x-4 gap-y-3 border-t border-stroke pt-4 dark:border-gray-700"
      >
        <div
          v-for="(row, idx) in breakdown"
          :key="idx"
          class="flex flex-col"
          :class="row.value === '0' || row.value === '0%' ? 'opacity-60' : ''"
        >
          <dt class="text-[11px] font-medium uppercase tracking-wide text-gray-600 dark:text-bodydark">
            {{ row.label }}
          </dt>
          <dd
            class="mt-0.5 text-sm font-semibold text-black tabular-nums dark:text-white"
            :class="row.class"
          >
            {{ row.value }}
          </dd>
        </div>
      </dl>

      <p v-if="error" class="mt-4 text-xs font-medium text-meta-1">
        Falha ao carregar este dado.
      </p>
    </template>
  </component>
</template>