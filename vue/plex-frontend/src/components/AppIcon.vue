<script setup>
defineProps({
  name: { type: String, required: true },
  size: { type: String, default: "" }, // '', 'sm', 'lg'
});
</script>

<template>
  <svg class="icon" :class="{ [`icon-${size}`]: size !== '' }" aria-hidden="true">
    <use :href="`/icons.svg#ui-${name}`" />
  </svg>
</template>
