<script setup>
import { computed } from "vue";

const props = defineProps({
  name: { type: String, required: true },
  size: { type: String, default: "md" }, // 'sm', 'md', 'lg', 'xl'
});

const sizeMap = {
  sm: "16",
  md: "24",
  lg: "32",
  xl: "40",
};

const size = computed(() => sizeMap[props.size] || "24");

const iconPath = computed(() => {
  // Custom Plex Autoplay icon; Lucide UI icons stored with 'ui-' prefix
  const name = props.name === 'plex' ? 'plex-autoplay' : `ui-${props.name}`;
  return `/assets/icons/${name}.svg`;
});
</script>

<template>
  <img
    :src="iconPath"
    :data-icon="name"
    class="icon"
    :width="size"
    :height="size"
    :alt="name"
    aria-hidden="true"
    loading="lazy"
  />
</template>

<style scoped>
.icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}
</style>