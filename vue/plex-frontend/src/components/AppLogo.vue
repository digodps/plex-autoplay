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
    <defs>
      <linearGradient id="pa-logo-bg" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0" stop-color="#4B5563" />
        <stop offset="1" stop-color="#0B0C0E" />
      </linearGradient>
    </defs>
    <rect width="64" height="64" rx="14" fill="url(#pa-logo-bg)" />
    <rect
      x="0.75"
      y="0.75"
      width="62.5"
      height="62.5"
      rx="13.25"
      fill="none"
      stroke="#9CA3AF"
      stroke-opacity="0.45"
      stroke-width="1.5"
    />
    <path d="M18 18 38 32 18 46Z" fill="#E5E7EB" />
    <rect x="43" y="20" width="4" height="24" rx="2" fill="#9CA3AF" />
  </svg>
</template>
