<script setup>
import AppIcon from "./AppIcon.vue";

defineProps({
  series: { type: Object, default: null },
  // Estado de carregamento: renderiza card de skeleton sem dados.
  loading: { type: Boolean, default: false },
});

defineEmits(["open"]);
</script>

<template>
  <!-- skeleton (carregamento) -->
  <div v-if="loading" class="card">
    <div class="thumb skeleton" />
    <div class="card-body">
      <div class="skeleton line" />
      <div class="skeleton line short" />
    </div>
  </div>

  <button v-else class="card" @click="$emit('open')">
    <div class="thumb">
      <img v-if="series?.thumb" :src="series.thumb" :alt="series?.title" loading="lazy" />
      <div v-else class="thumb-fallback">
        <AppIcon name="tv" size="lg" />
      </div>
    </div>
    <div class="card-body">
      <h3 class="card-title">
        {{ series?.title }}
      </h3>
      <p class="card-summary">
        {{ series?.summary || "Sem descrição." }}
      </p>
    </div>
    <span class="card-cta"><AppIcon name="edit" size="sm" /> Editar marcadores</span>
  </button>
</template>

<style scoped src="./series-card.css"></style>
