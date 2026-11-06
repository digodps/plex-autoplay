<script setup>
import { computed } from "vue";
import AppIcon from "./AppIcon.vue";

const props = defineProps({
  items: {
    type: Array,
    required: true,
  },
});

const emit = defineEmits(["select"]);

const breadcrumbItems = computed(() =>
  props.items.map((item, index, arr) => ({
    ...item,
    isCurrent: item.current ?? index === arr.length - 1,
  })),
);
</script>

<template>
  <nav v-if="breadcrumbItems.length > 0" class="breadcrumb" aria-label="Navegação em trilha" data-testid="breadcrumb">
    <ol class="breadcrumb__list">
      <li v-for="(item, index) in breadcrumbItems" :key="index" class="breadcrumb__item">
        <button
          v-if="!item.isCurrent && item.to"
          type="button"
          class="breadcrumb__link"
          data-testid="breadcrumb-item"
          @click="emit('select', item.to)"
        >
          <AppIcon v-if="index === 0" name="chevron-left" size="xs" />
          <span>{{ item.label }}</span>
        </button>

        <span
          v-else
          class="breadcrumb__current"
          data-testid="breadcrumb-current"
          :aria-current="item.isCurrent ? 'page' : undefined"
        >
          <span>{{ item.label }}</span>
        </span>

        <span v-if="index < breadcrumbItems.length - 1" class="breadcrumb__sep" aria-hidden="true">
          <AppIcon name="chevron-right" size="xs" />
        </span>
      </li>
    </ol>
  </nav>
</template>
