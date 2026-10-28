<script setup>
import AppIcon from "./AppIcon.vue";

/** Exibição de erro dentro do StatusCard */
</script>

<template>
  <div class="card-error">
    <AppIcon name="warning" size="sm" />
    <span>Erro ao carregar</span>
  </div>
</template>

<style scoped>
.card-error {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  margin-top: auto;
  padding-top: 0.5rem;
  color: var(--danger);
  font-size: 0.75rem;
  font-weight: 500;
}
</style>
