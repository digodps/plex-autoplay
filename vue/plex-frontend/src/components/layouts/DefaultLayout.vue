<script setup>
import { computed, onMounted, ref, watch } from "vue";
import { useRoute } from "vue-router";
import AppHeader from "@/components/AppHeader.vue";
import { fetchLibraries } from "@/api";
import { buildBreadcrumb } from "@/lib/breadcrumb";

const route = useRoute();
const libraries = ref([]);
const loading = ref(true);
const scroller = ref(null);

const breadcrumbs = computed(() =>
  buildBreadcrumb(route, libraries.value, { loading: loading.value }),
);

watch(
  () => route.fullPath,
  () => {
    if (scroller.value) scroller.value.scrollTop = 0;
  },
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
  <div class="flex h-dvh flex-col overflow-hidden bg-gray-50 dark:bg-gray-900">
    <AppHeader :items="breadcrumbs" />
    <main class="min-h-0 flex-1 p-2 md:p-3">
      <div
        ref="scroller"
        data-testid="content-card"
        class="h-full overflow-x-hidden overflow-y-auto rounded-2xl bg-white shadow-default no-scrollbar dark:bg-gray-800"
      >
        <div class="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
          <slot></slot>
        </div>
      </div>
    </main>
  </div>
</template>
