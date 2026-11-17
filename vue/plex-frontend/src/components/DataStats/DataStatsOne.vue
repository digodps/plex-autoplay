<script setup>
import { computed } from "vue";
import { useDashboardCards } from "@/composables/useDashboardCards";
import CardDataStats from "@/components/CardDataStats.vue";

const { serviceCard, syncCard, librariesCard, skipSessionsCard } = useDashboardCards();

const cards = computed(() => [serviceCard.value, syncCard.value, librariesCard.value, skipSessionsCard.value]);
</script>

<template>
  <div class="metrics-grid" style="display: grid; grid-template-columns: 1fr; gap: var(--s-4);">
    <CardDataStats v-for="(card, idx) in cards" :key="idx" v-bind="card" />
  </div>
</template>

<style scoped>
.metrics-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--s-4);
}

@media (min-width: 768px) {
  .metrics-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (min-width: 1024px) {
  .metrics-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}
</style>