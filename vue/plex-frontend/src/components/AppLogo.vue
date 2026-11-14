<script setup>
import { ref } from 'vue'

defineProps({
  size: { type: Number, default: 28 },
})

const animationDelay = ref(`${Math.random() * 500}ms`)
</script>

<template>
  <svg
    :width="size"
    :height="size"
    viewBox="0 0 64 64"
    role="img"
    aria-label="Plex Autoplay"
    class="shrink-0"
  >
    <path
      d="M18 18 38 32 18 46Z"
      fill="none"
      stroke="currentColor"
      stroke-width="3"
      stroke-linejoin="round"
    />
    <path
      d="M48 20v24"
      fill="none"
      stroke="currentColor"
      stroke-width="3"
      stroke-linecap="round"
    />
    <path
      class="logo-lightning"
      :style="{ animationDelay }"
      d="M32 12 L26 28 L30 28 L22 48 L38 30 L34 30 L42 12 Z"
      fill="none"
      stroke="currentColor"
      stroke-width="2.5"
      stroke-linejoin="round"
      stroke-linecap="round"
    />
  </svg>
</template>

<style scoped>
.logo-lightning {
  animation: electric-pulse 2.5s ease-in-out infinite;
}

@keyframes electric-pulse {
  0%, 100% { opacity: 0.6; }
  50% { opacity: 1; }
  30%, 31% { opacity: 0.3; }
  60%, 61% { opacity: 0.4; }
  90%, 91% { opacity: 0.35; }
}

@media (prefers-reduced-motion: reduce) {
  .logo-lightning {
    animation: none;
    opacity: 1;
  }
}
</style>