<script setup>
import { RouterView, useRoute } from "vue-router";
import { computed } from "vue";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import { useUiStore } from "@/stores/ui";
import Breadcrumb from "@/components/Breadcrumb.vue";
import AppLogo from "@/components/AppLogo.vue";

const route = useRoute();
const theme = useThemeStore();
const auth = useAuthStore();
const ui = useUiStore();

const showShell = computed(() => route.name !== "login" && route.name !== "setup");

const cycleMode = () => theme.cycleMode();

function logout() {
  auth.logout();
  window.location.href = "/login";
}

const breadcrumbItems = computed(() => {
  const name = route.name;
  const params = route.params || {};
  const key = params.key || route.query.libraryKey;
  const home = { label: "Início", to: { name: "dashboard" } };
  if (name === "dashboard") return [{ ...home, current: true }];
  if (name === "libraries")
    return [
      { ...home },
      { label: "Bibliotecas", current: true },
    ];
  if (name === "series")
    return [
      { ...home },
      { label: "Bibliotecas", to: { name: "libraries" } },
      { label: ui.seriesTitle || "Séries", current: true },
    ];
  if (name === "series-markers")
    return [
      { ...home },
      { label: "Bibliotecas", to: { name: "libraries" } },
      {
        label: ui.seriesTitle || "Série",
        to: { name: "series", params: { key } },
      },
      { label: "Marcadores", current: true },
    ];
  if (name === "sync")
    return [
      { ...home },
      { label: "Sincronização", current: true },
    ];
  if (name === "settings")
    return [
      { ...home },
      { label: "Ajustes", current: true },
    ];
  return [];
});
</script>

<template>
  <div>
    <div v-if="showShell" class="shell">
      <header class="topbar">
        <div class="topbar-left">
          <RouterLink :to="{ name: 'dashboard' }" aria-label="Plex Autoplay — Início">
            <AppLogo :size="26" />
            <span class="font-semibold text-bodydark dark:text-body dark:hover:text-primary">
              Plex Autoplay
            </span>
          </RouterLink>
          <Breadcrumb :items="breadcrumbItems" />
        </div>
        <div class="topbar-right">
          <button
            type="button"
            class="btn btn-ghost flex items-center gap-1"
            :aria-label="theme.mode === 'light' ? 'Alterar para modo escuro' : 'Alterar para modo claro'"
            @click="cycleMode"
          >
            <AppIcon v-if="theme.mode === 'light'" name="moon" size="xs" />
            <AppIcon v-else-if="theme.mode === 'dark'" name="sun" size="xs" />
            <AppIcon v-else name="moon" size="xs" />
          </button>
          <button type="button" class="btn btn-ghost" @click="logout">
            <AppIcon name="user" size="xs" />
            <span class="hidden text-sm">Sair</span>
          </button>
        </div>
      </header>
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