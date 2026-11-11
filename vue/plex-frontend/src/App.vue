<script setup>
import { RouterView, useRoute } from "vue-router";
import { computed, onMounted, ref } from "vue";
import AppHeader from "@/components/AppHeader.vue";
import { buildBreadcrumb } from "@/lib/breadcrumb";
import { fetchLibraries } from "@/api";

const route = useRoute();

const showShell = computed(() => route.name !== "login" && route.name !== "setup");

const libraries = ref([]);
const loading = ref(true);

const breadcrumbItems = computed(() => buildBreadcrumb(route, libraries.value, { loading: loading.value }));

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
  <div>
    <div v-if="showShell" class="shell">
      <AppHeader :items="breadcrumbItems" />
      <main class="main">
        <router-view v-slot="{ Component }">
          <transition name="page" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>
    </div>
    <RouterView v-else />
  </div>
</template>
