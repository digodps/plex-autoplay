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
  <nav aria-label="breadcrumb" class="breadcrumb" data-testid="breadcrumb">
    <ol class="breadcrumb__list">
      <li
        v-for="(item, index) in items"
        :key="index"
        class="breadcrumb__item"
      >
        <RouterLink
          v-if="item.to && !item.current"
          :to="item.to"
          class="breadcrumb__link"
          @click="handleSelect(item)"
        >
          <span class="breadcrumb__label">{{ item.label }}</span>
        </RouterLink>
        <span
          v-else-if="item.current"
          class="breadcrumb__label breadcrumb__label--current"
          aria-current="page"
        >
          <span>{{ item.label }}</span>
        </span>
        <span
          v-else
          class="breadcrumb__label"
          @click="handleSelect(item)"
        >
          <span>{{ item.label }}</span>
        </span>
        <span
          v-if="index < items.length - 1"
          class="breadcrumb__separator"
          aria-hidden="true"
        >
          /
        </span>
      </li>
    </ol>
  </nav>
</template>

<style scoped>
.breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  min-width: 0;
  flex: 1;
}

.breadcrumb__list {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  min-width: 0;
  flex: 1;
  list-style: none;
  margin: 0;
  padding: 0;
}

.breadcrumb__item {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  min-width: 0;
}

.breadcrumb__link {
  display: flex;
  align-items: center;
  gap: var(--s-1);
  font-size: var(--t-sm);
  color: var(--p-muted);
  text-decoration: none;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: color var(--speed) var(--ease);
}
.breadcrumb__link:hover {
  color: var(--p-accent);
}
.breadcrumb__link:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
  border-radius: var(--r-sm);
}

.breadcrumb__label {
  display: inline-block;
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.breadcrumb__label--current {
  font-weight: 500;
  color: var(--p-ink);
}

.breadcrumb__separator {
  color: var(--p-muted);
  flex-shrink: 0;
}

/* Mobile: hide breadcrumb on small screens, nav-cards takes over */
@media (max-width: 768px) {
  .breadcrumb { display: none; }
}
</style>