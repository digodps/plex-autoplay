<script setup>
defineProps({
  title: { type: String, default: "Plex Autoplay" },
  subtitle: { type: String, default: "" },
});
</script>

<template>
  <div
    class="w-full max-w-md rounded-sm border border-stroke bg-white shadow-default dark:border-strokedark dark:bg-boxdark"
  >
    <div class="p-6 sm:p-12">
      <div class="mb-6 text-center">
        <h1 class="text-title-md font-bold text-black dark:text-white">{{ title }}</h1>
        <p v-if="subtitle" class="mt-2 text-sm font-medium text-body dark:text-bodydark">
          {{ subtitle }}
        </p>
      </div>
      <slot></slot>
    </div>
  </div>
</template>