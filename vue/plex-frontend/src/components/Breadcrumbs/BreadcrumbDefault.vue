<script setup>
import AppIcon from "@/components/AppIcon.vue";

defineProps({
  items: { type: Array, required: true },
});
defineEmits(["select"]);
</script>

<template>
  <nav class="mb-6" aria-label="Navegação em trilha">
    <ol class="flex flex-wrap items-center gap-2 text-sm font-medium text-body dark:text-bodydark">
      <li v-for="(item, index) in items" :key="index" class="flex items-center gap-2">
        <button
          v-if="!item.isCurrent && item.to"
          type="button"
          class="transition hover:text-primary"
          @click="$emit('select', item.to)"
        >
          {{ item.label }}
        </button>
        <span
          v-else
          :class="item.isCurrent ? 'text-black dark:text-white' : ''"
          :aria-current="item.isCurrent ? 'page' : undefined"
        >
          {{ item.label }}
        </span>
        <span v-if="index < items.length - 1" class="text-bodydark2" aria-hidden="true">
          <AppIcon name="chevron-right" size="xs" />
        </span>
      </li>
    </ol>
  </nav>
</template>