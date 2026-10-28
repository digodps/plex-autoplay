<script setup>
import AppIcon from "./AppIcon.vue";

defineProps({
  page: { type: Number, required: true },
  totalPages: { type: Number, required: true },
  pageWindow: { type: Array, required: true },
});

defineEmits(["go"]);
</script>

<template>
  <nav class="pagination" aria-label="Paginação de séries">
    <button
      class="page-btn"
      :disabled="page <= 1"
      aria-label="Página anterior"
      @click="$emit('go', page - 1)"
    >
      <AppIcon name="chevron-left" size="sm" />
    </button>
    <button
      v-for="p in pageWindow"
      :key="p"
      class="page-btn"
      :class="{ active: p === page }"
      @click="$emit('go', p)"
    >
      {{ p }}
    </button>
    <button
      class="page-btn"
      :disabled="page >= totalPages"
      aria-label="Próxima página"
      @click="$emit('go', page + 1)"
    >
      <AppIcon name="chevron-right" size="sm" />
    </button>
  </nav>
</template>

<style scoped src="./series-pagination.css"></style>
