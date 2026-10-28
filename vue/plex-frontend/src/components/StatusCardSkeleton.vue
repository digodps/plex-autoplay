<script setup>
/** Skeleton loader para StatusCard - altura fixa para evitar layout shift */
</script>

<template>
  <div class="card-skeleton">
    <div class="skeleton icon-skeleton" />
    <div class="skeleton label-skeleton" />
    <div class="skeleton value-skeleton" />
    <div v-for="i in 3" :key="i" class="skeleton line-skeleton" />
  </div>
</template>

<style scoped>
.card-skeleton {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  width: 100%;
}

.icon-skeleton {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-sm);
}

.label-skeleton {
  width: 60%;
  height: 10px;
  border-radius: var(--radius-sm);
}

.value-skeleton {
  width: 40%;
  height: 28px;
  border-radius: var(--radius-sm);
}

.line-skeleton {
  width: 100%;
  height: 12px;
  border-radius: var(--radius-sm);
}
</style>
