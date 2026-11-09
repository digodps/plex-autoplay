<script setup>
import { RouterLink } from "vue-router";

const emit = defineEmits(["select"]);

defineProps({
  items: {
    type: Array,
    required: true,
  },
});

function handleSelect(item) {
  if (item.to && !item.current) {
    emit("select", item.to);
  }
}
</script>

<template>
  <nav aria-label="Breadcrumb" class="flex items-center gap-1 min-w-0 flex-1">
    <ol class="flex items-center gap-1 min-w-0 flex-1">
      <li
        v-for="(item, index) in items"
        :key="index"
        class="flex items-center gap-1 min-w-0"
      >
        <RouterLink
          v-if="item.to && !item.current"
          :to="item.to"
          class="breadcrumb-item flex items-center gap-1 text-sm text-body hover:text-primary dark:hover:text-bodydark truncate"
          @click="handleSelect(item)"
        >
          <span class="truncate">{{ item.label }}</span>
        </RouterLink>
        <span
          v-else-if="item.current"
          class="breadcrumb-item flex items-center gap-1 text-sm font-medium text-bodydark dark:text-body truncate"
          aria-current="page"
        >
          <span class="truncate">{{ item.label }}</span>
        </span>
        <span
          v-else
          class="breadcrumb-item flex items-center gap-1 text-sm text-body hover:text-primary truncate"
          @click="handleSelect(item)"
        >
          <span class="truncate">{{ item.label }}</span>
        </span>
        <span
          v-if="index < items.length - 1"
          class="text-body mx-1"
          aria-hidden="true"
        >
          /
        </span>
      </li>
    </ol>
  </nav>
</template>