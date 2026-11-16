<script setup>
import { RouterView, useRoute } from "vue-router";
import { computed, onMounted, ref } from "vue";
import AppHeader from "@/components/AppHeader.vue";
import { fetchLibraries } from "@/api";

const route = useRoute();

const showShell = computed(() => route.name !== "login" && route.name !== "setup");

const libraries = ref([]);
const loading = ref(true);

// Breadcrumb is built in AppHeader via nav-cards pattern

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
  <div class="app">
    <div v-if="showShell" class="shell">
      <AppHeader />
      <main id="main-content" class="main" role="main">
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

<style>
/* Global styles using Taste system.css tokens */
:root {
  --nav-h: 64px;
  --wrap-max: 1440px;
}

.app {
  min-height: 100vh;
  background: var(--p-bg);
  color: var(--p-ink);
}

.shell {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.main {
  flex: 1;
  padding-top: calc(var(--nav-h) + var(--s-6));
  padding-bottom: var(--s-8);
  width: 100%;
  max-width: var(--wrap-max);
  margin: 0 auto;
}

/* Page transitions */
.page-enter-active,
.page-leave-active {
  transition: opacity var(--speed) var(--ease), transform var(--speed) var(--ease);
}
.page-enter-from,
.page-leave-to {
  opacity: 0;
  transform: translateY(8px);
}

/* Focus visible for all interactive elements */
:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .page-enter-active,
  .page-leave-active,
  * {
    transition-duration: 0.01ms !important;
    animation-duration: 0.01ms !important;
  }
}

/* Skip link base (AppHeader has full implementation) */
.skip-link {
  position: absolute;
  top: -100%;
  left: 50%;
  transform: translateX(-50%);
  padding: var(--s-2) var(--s-4);
  background: var(--p-accent);
  color: var(--p-accent-ink);
  border-radius: var(--r-md);
  z-index: 200;
  font-weight: 500;
}
.skip-link:focus {
  top: var(--s-4);
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

/* Smooth scroll */
html {
  scroll-behavior: smooth;
}
@media (prefers-reduced-motion: reduce) {
  html {
    scroll-behavior: auto;
  }
}
</style>