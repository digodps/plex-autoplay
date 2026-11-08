<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import AppHeader from "@/components/AppHeader.vue";
import { fetchLibraries } from "@/api";
import { buildBreadcrumb } from "@/lib/breadcrumb";

const route = useRoute();
const libraries = ref([]);
const loading = ref(true);

const breadcrumbs = computed(() =>
  buildBreadcrumb(route, libraries.value, { loading: loading.value }),
);

onMounted(async () => {
  try {
    const data = await fetchLibraries();
    libraries.value = Array.isArray(data) ? data : [];
  } catch {
    libraries.value = [];
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div class="flex h-screen flex-col overflow-hidden">
    <AppHeader :items="breadcrumbs" />
    <main class="relative min-h-0 flex-1 overflow-x-hidden overflow-y-auto">
      <div class="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
        <slot></slot>
      </div>
    </main>
  </div>
</template>
