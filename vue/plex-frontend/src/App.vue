<script setup>
import { RouterView } from "vue-router";
import { onMounted, ref } from "vue";
import AppHeader from "@/components/AppHeader.vue";
import { fetchLibraries } from "@/api";

const libraries = ref([]);
const loading = ref(true);

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
  <RouterView v-slot="{ Component, route: routed }">
    <template v-if="routed.meta.shell !== false">
      <AppHeader />
      <main id="main-content" class="main" role="main">
        <transition name="page" mode="out-in">
          <component :is="Component" />
        </transition>
      </main>
    </template>
    <template v-else>
      <component :is="Component" />
    </template>
  </RouterView>
</template>

<style>
/* Global styles using Taste system.css tokens */
:root {
  --nav-h: 64px;
  --wrap-max: 1440px;
}

.main {
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