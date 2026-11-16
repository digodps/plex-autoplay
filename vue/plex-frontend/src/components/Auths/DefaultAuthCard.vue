<script setup>
defineProps({
  title: { type: String, default: "Plex Autoplay" },
  subtitle: { type: String, default: "" },
});
</script>

<template>
  <article class="card panel" style="--p-h: auto; max-width: 480px; width: 100%; margin: 0 auto;">
    <div class="panel__content" style="padding: var(--s-6);">
      <header class="text-center" style="margin-bottom: var(--s-6);">
        <h1 class="t-h3" style="margin: 0 0 var(--s-2); font: var(--font-display);">{{ title }}</h1>
        <p v-if="subtitle" class="t-body muted" style="margin: 0;">{{ subtitle }}</p>
      </header>
      <slot></slot>
    </div>
  </article>
</template>

<style scoped>
.card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-lg);
}
.panel__content {
  display: flex;
  flex-direction: column;
}
</style>