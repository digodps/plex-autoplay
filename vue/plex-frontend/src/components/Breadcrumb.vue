<script setup>
<<<<<<< HEAD
import { RouterLink } from "vue-router";

const emit = defineEmits(["select"]);

defineProps({
=======
import { computed } from "vue";
import { useRouter } from "vue-router";
import AppIcon from "./AppIcon.vue";

const props = defineProps({
  /**
   * Array de itens do breadcrumb
   * @type {Array<{label: string, to?: string|object, current?: boolean}>}
   */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  items: {
    type: Array,
    required: true,
  },
});

<<<<<<< HEAD
function handleSelect(item) {
  if (item.to && !item.current) {
    emit("select", item.to);
=======
const router = useRouter();

const breadcrumbItems = computed(() =>
  props.items.map((item, index, arr) => ({
    ...item,
    isLast: index === arr.length - 1,
    isCurrent: item.current ?? index === arr.length - 1,
  })),
);

function navigate(to) {
  if (typeof to === "string") {
    router.push(to);
  } else {
    router.push(to);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  }
}
</script>

<template>
<<<<<<< HEAD
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
=======
  <nav v-if="breadcrumbItems.length > 0" class="breadcrumb" aria-label="Navegação">
    <ol class="breadcrumb-list">
      <li v-for="(item, index) in breadcrumbItems" :key="index" class="breadcrumb-item">
        <template v-if="!item.isCurrent && item.to">
          <router-link :to="item.to" class="breadcrumb-link" @click.prevent="navigate(item.to)">
            <AppIcon v-if="index === 0" name="chevron-left" size="xs" class="breadcrumb-icon" />
            {{ item.label }}
          </router-link>
        </template>

        <span v-else class="breadcrumb-current" :aria-current="item.isCurrent ? 'page' : undefined">
          <AppIcon v-if="index === 0" name="chevron-left" size="xs" class="breadcrumb-icon" />
          {{ item.label }}
        </span>

        <!-- Separador entre itens (exceto o último) -->
        <template v-if="index < breadcrumbItems.length - 1">
          <AppIcon name="chevron-right" size="xs" class="breadcrumb-separator" aria-hidden="true" />
        </template>
      </li>
    </ol>
  </nav>
</template>

<style scoped>
.breadcrumb {
  padding: 0.5rem 0;
  margin-bottom: 0.5rem;
}

.breadcrumb-list {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.25rem;
  margin: 0;
  padding: 0;
  list-style: none;
  font-size: 0.875rem;
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  white-space: nowrap;
}

.breadcrumb-link {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--text-secondary);
  text-decoration: none;
  font-weight: 500;
  padding: 0.25rem 0.5rem;
  border-radius: var(--radius-sm);
  transition:
    color var(--speed) var(--ease),
    background var(--speed) var(--ease);
}

.breadcrumb-link:hover {
  color: var(--accent);
  background: var(--surface);
  text-decoration: none;
}

.breadcrumb-current {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--text);
  font-weight: 600;
  padding: 0.25rem 0.5rem;
}

.breadcrumb-icon {
  flex-shrink: 0;
  color: var(--accent);
}

.breadcrumb-separator {
  color: var(--text-tertiary);
  flex-shrink: 0;
}
</style>
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
