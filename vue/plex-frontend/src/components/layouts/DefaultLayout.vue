<script setup>
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import SidebarArea from "@/components/Sidebar/SidebarArea.vue";
import AppHeader from "@/components/AppHeader.vue";
import { useSidebarStore } from "@/stores/sidebar";
import { fetchLibraries } from "@/api";
import { buildBreadcrumb } from "@/lib/breadcrumb";

const sidebar = useSidebarStore();
const route = useRoute();
const libraries = ref([]);
const loading = ref(true);
const error = ref("");

const breadcrumbs = computed(() =>
  buildBreadcrumb(route, libraries.value, { loading: loading.value }),
);

onMounted(async () => {
  try {
    const data = await fetchLibraries();
    libraries.value = Array.isArray(data) ? data : [];
  } catch (e) {
    error.value = e?.message || "Falha ao carregar bibliotecas.";
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <div class="flex h-screen flex-col overflow-hidden">
    <AppHeader :items="breadcrumbs" />
    <div class="flex min-h-0 flex-1">
      <SidebarArea
        :items="libraries"
        :collapsed="sidebar.isCollapsed"
        :active-path="route.path"
        :error="error"
      />
      <main class="relative flex-1 overflow-y-auto overflow-x-hidden">
        <div class="mx-auto max-w-screen-2xl p-4 md:p-6 2xl:p-10">
          <slot></slot>
        </div>
      </main>
    </div>
  </div>
</template>
