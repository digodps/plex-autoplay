<script setup>
<<<<<<< HEAD
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
=======
import { useAppShell } from "./composables/useAppShell";
import { useAuthStore } from "./stores/auth";
import AppIcon from "./components/AppIcon.vue";
import "./app-shell.css";

const authStore = useAuthStore();
const { isLoginRoute, navItems, handleLogout } = useAppShell();
</script>

<template>
  <router-view v-slot="{ Component }">
    <transition name="fade" mode="out-in">
      <div v-if="isLoginRoute" key="login">
        <component :is="Component" />
      </div>

      <div v-else-if="authStore.isAuthenticated" key="shell" class="app-shell">
        <aside class="sidebar">
          <div class="brand">
            <img src="/favicon.svg" alt="" class="brand-logo" />
            <span class="brand-name">Plex Autoplay</span>
          </div>

          <nav class="nav">
            <router-link
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              class="nav-item"
              active-class="nav-item-active"
            >
              <AppIcon :name="item.icon" />
              <span>{{ item.label }}</span>
            </router-link>
          </nav>

          <footer class="sidebar-footer">
            <span class="status-dot" aria-hidden="true" />
            <span>PlexAutoSkip embutido</span>
          </footer>

          <button class="logout-btn" title="Sair" @click="handleLogout">
            <AppIcon name="logout" size="sm" />
            <span>Sair</span>
          </button>
        </aside>

        <main class="content">
          <component :is="Component" />
        </main>
      </div>

      <template v-else>
        <div style="text-align: center; padding: 2rem">
          <p>Redirecionando para login...</p>
        </div>
      </template>
    </transition>
  </router-view>
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
</template>
