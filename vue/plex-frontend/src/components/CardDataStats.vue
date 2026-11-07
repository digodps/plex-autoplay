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
  href: { type: String, default: null },
  loading: { type: Boolean, default: false },
  error: { type: Boolean, default: false },
});

const route = useRoute();
const isActive = computed(() => props.href && route.path === props.href);

const badgeClass = computed(() => {
  const map = {
    success: "bg-meta-3/10 text-meta-3",
    warning: "bg-meta-6/10 text-meta-6",
    danger: "bg-meta-1/10 text-meta-1",
    neutral: "bg-gray-2 text-body dark:bg-meta-4 dark:text-bodydark",
  };
  return map[props.badge?.variant] ?? map.neutral;
});
</script>

<template>
  <component
    :is="href ? 'router-link' : 'article'"
    :to="href"
    class="rounded-sm border bg-white px-7.5 py-6 shadow-default transition dark:bg-boxdark"
    :class="[
      isActive
        ? 'border-primary ring-1 ring-primary'
        : 'border-stroke dark:border-strokedark',
      error ? '!border-meta-1' : '',
      href ? 'block hover:shadow-2' : '',
    ]"
  >
    <template v-if="loading">
      <div class="h-11.5 w-11.5 animate-pulse rounded-full bg-gray-2 dark:bg-meta-4"></div>
      <div class="mt-4 space-y-2">
        <div class="h-7 w-64 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
        <div class="h-4 w-32 animate-pulse rounded bg-gray-2 dark:bg-meta-4"></div>
      </div>
    </template>

    <template v-else>
      <div class="flex items-center justify-between">
        <div
          class="flex h-11.5 w-11.5 items-center justify-center rounded-full bg-meta-2 text-primary dark:bg-meta-4 dark:text-white"
        >
          <AppIcon :name="icon" size="lg" />
        </div>
        <span
          v-if="badge"
          class="inline-flex rounded-full px-2.5 py-0.5 text-xs font-medium capitalize"
          :class="badgeClass"
        >
          {{ badge.text }}
        </span>
      </div>

      <div class="mt-4 flex items-end justify-between">
        <div class="min-w-0">
          <h4 class="truncate text-title-md font-bold text-black dark:text-white">
            {{ value }}
          </h4>
          <span class="text-sm font-medium text-body dark:text-bodydark">{{ label }}</span>
        </div>
      </div>

      <dl
        v-if="breakdown.length"
        class="mt-4 grid grid-cols-2 gap-x-4 gap-y-2 border-t border-stroke pt-3 dark:border-strokedark"
      >
        <div v-for="(row, idx) in breakdown" :key="idx" class="flex items-baseline justify-between gap-2">
          <dt class="text-xs text-body dark:text-bodydark">{{ row.label }}</dt>
          <dd class="text-sm font-semibold text-black dark:text-white" :class="row.class">
            {{ row.value }}
          </dd>
        </div>
      </dl>

      <p v-if="error" class="mt-3 text-xs font-medium text-meta-1">
        Falha ao carregar este dado.
      </p>
    </template>
  </component>
</template>