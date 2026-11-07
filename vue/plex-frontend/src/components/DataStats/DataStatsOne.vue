<script setup>
import { computed } from "vue";
import { useDashboardCards } from "@/composables/useDashboardCards";
import CardDataStats from "@/components/CardDataStats.vue";

const { serviceCard, syncCard, librariesCard, markersCard } = useDashboardCards();

const cards = computed(() => [serviceCard.value, syncCard.value, librariesCard.value, markersCard.value]);
</script>

<template>
  <div class="grid grid-cols-1 gap-4 md:grid-cols-2 md:gap-6 xl:grid-cols-4 2xl:gap-7.5">
    <CardDataStats
      v-for="(card, idx) in cards"
      :key="idx"
      v-bind="card"
    />
  </div>
</template>