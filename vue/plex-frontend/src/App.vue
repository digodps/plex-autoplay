<script setup>
import { RouterView, useRoute } from "vue-router";
import { computed, onMounted, ref } from "vue";
import AppHeader from "@/components/AppHeader.vue";
import Sidebar from "@/components/Sidebar/Sidebar.vue";
import SidebarOverlay from "@/components/Sidebar/SidebarOverlay.vue";
import { useSidebarStore } from "@/stores/sidebar";
import { buildBreadcrumb } from "@/lib/breadcrumb";
import { fetchLibraries } from "@/api";

const route = useRoute();

const sidebarStore = useSidebarStore();

const showShell = computed(() => route.name !== "login" && route.name !== "setup");

const libraries = ref([]);
const loading = ref(true);

const breadcrumbItems = computed(() => buildBreadcrumb(route, libraries.value, { loading: loading.value }));

const menuGroups = [
  {
    name: "MAIN",
    menuItems: [
      { name: "Dashboard", to: "dashboard", icon: "home" },
      { name: "Libraries", to: "libraries", icon: "library" },
      { name: "Series", to: "series", icon: "tv" },
      { name: "Sync", to: "sync", icon: "sync" },
      { name: "Markers", to: "markers", icon: "marker" },
    ],
  },
  {
    name: "SETTINGS",
    menuItems: [
      { name: "Logs", to: "logs", icon: "terminal" },
      { name: "Settings", to: "settings", icon: "settings" },
    ],
  }
];

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
  <div class="flex min-h-screen">
    <Sidebar v-if="showShell" :menu-groups="menuGroups" />
    <div class="flex-1">
      <SidebarOverlay v-if="sidebarStore.isOpen && showShell" @close="sidebarStore.close()" />
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
  </div>
</template>
