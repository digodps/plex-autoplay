<script setup>
defineProps({
  size: { type: Number, default: 28 },
})
</script>

<template>
  <img
    :src="`/assets/icons/plex.svg`"
    :width="size"
    :height="size"
    alt="Plex"
    aria-hidden="true"
    class="shrink-0"
    loading="lazy"
  />
</template>