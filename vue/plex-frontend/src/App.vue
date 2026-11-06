<script setup>
import { computed } from "vue";
import { useAppShell } from "./composables/useAppShell";
import { useAuthStore } from "./stores/auth";
import { useUiStore } from "./stores/ui";
import { useThemeStore } from "./stores/theme";
import Breadcrumb from "./components/Breadcrumb.vue";

const authStore = useAuthStore();
const uiStore = useUiStore();
const themeStore = useThemeStore();
const { isLoginRoute, activePath, navigate, handleLogout } = useAppShell();

const breadcrumbItems = computed(() => {
  if (isLoginRoute.value) return [];

  const path = activePath.value;

  if (path === "/dashboard") {
    return [{ label: "Início", current: true }];
  }

  if (path === "/libraries") {
    return [
      { label: "Início", to: "/dashboard" },
      { label: "Bibliotecas", current: true },
    ];
  }

  if (path.startsWith("/libraries/") && path.endsWith("/series")) {
    const libTitle = uiStore.libraryTitle || "Séries";
    return [
      { label: "Início", to: "/dashboard" },
      { label: "Bibliotecas", to: "/libraries" },
      { label: libTitle, current: true },
    ];
  }

  if (path.startsWith("/series/") && path.endsWith("/markers")) {
    const sTitle = uiStore.seriesTitle || "Série";
    return [
      { label: "Início", to: "/dashboard" },
      { label: "Bibliotecas", to: "/libraries" },
      { label: sTitle, current: false },
      { label: "Marcadores", current: true },
    ];
  }

  if (path === "/sync") {
    return [
      { label: "Início", to: "/dashboard" },
      { label: "Sincronização", current: true },
    ];
  }

  if (path === "/settings") {
    return [
      { label: "Início", to: "/dashboard" },
      { label: "Ajustes", current: true },
    ];
  }

  return [{ label: "Início", current: true }];
});

function toggleTheme() {
  themeStore.cycleMode();
}
</script>

<template>
  <div class="app-shell">
    <transition name="page" mode="out-in">
      <div v-if="isLoginRoute" key="login" class="auth-shell">
        <router-view />
      </div>

      <div v-else-if="authStore.isAuthenticated" key="shell" class="app-shell">
        <!-- Top bar (sticky, sem sidebar lateral) -->
        <header class="topbar">
          <div class="topbar__inner">
            <!-- Brand -->
            <button type="button" class="brand" data-testid="brand-home" @click="navigate('/dashboard')">
              <span class="brand__mark">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="20" height="20">
                  <path d="M3 10.5 12 3l9 7.5M5.5 9.5V21h13V9.5M10 21v-6h4v6" />
                </svg>
              </span>
              <span class="brand__text">Plex AutoPlay</span>
            </button>

            <!-- Breadcrumb centralizado -->
            <Breadcrumb :items="breadcrumbItems" @select="navigate" />

            <!-- Ações à direita -->
            <div class="topbar__actions">
              <button
                type="button"
                class="icon-btn"
                data-testid="theme-toggle"
                aria-label="Alternar modo de cor"
                title="Alternar claro/escuro"
                @click="toggleTheme"
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18">
                  <circle cx="12" cy="12" r="5" />
                  <line x1="12" y1="1" x2="12" y2="3" />
                  <line x1="12" y1="21" x2="12" y2="23" />
                  <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
                  <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
                  <line x1="1" y1="12" x2="3" y2="12" />
                  <line x1="21" y1="12" x2="23" y2="12" />
                  <line x1="4.22" y1="19.78" x2="5.64" y2="18.36" />
                  <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
                </svg>
              </button>

              <button
                type="button"
                class="icon-btn"
                data-testid="logout-btn"
                aria-label="Sair da sessão"
                title="Sair"
                @click="handleLogout"
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="18" height="18">
                  <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" />
                  <polyline points="16 17 21 12 16 7" />
                  <line x1="21" y1="12" x2="9" y2="12" />
                </svg>
              </button>
            </div>
          </div>
        </header>

        <!-- Conteúdo principal -->
        <main class="main" data-testid="main-content">
          <router-view v-slot="{ Component }">
            <transition name="page" mode="out-in">
              <component :is="Component" />
            </transition>
          </router-view>
        </main>
      </div>
    </transition>
  </div>
</template>
