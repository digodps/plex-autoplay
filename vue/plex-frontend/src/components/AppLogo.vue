<script setup>
defineProps({
  size: { type: Number, default: 28 },
});
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
  </svg>
</template>
