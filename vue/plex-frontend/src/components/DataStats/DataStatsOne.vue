<script setup>
import { computed } from "vue";
import { useDashboardCards } from "@/composables/useDashboardCards";
import CardDataStats from "@/components/CardDataStats.vue";

const { serviceCard, syncCard, librariesCard, skipSessionsCard } = useDashboardCards();

const cards = computed(() => [serviceCard.value, syncCard.value, librariesCard.value, skipSessionsCard.value]);
</script>

<template>
  <div class="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
    <CardDataStats v-for="(card, idx) in cards" :key="idx" v-bind="card" />
  </div>
</template>