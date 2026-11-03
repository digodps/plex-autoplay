<script setup>
import { useAppShell } from "./composables/useAppShell";
import { useAuthStore } from "./stores/auth";
import AppIcon from "./components/AppIcon.vue";

const authStore = useAuthStore();
const { isLoginRoute, navItems, handleLogout } = useAppShell();
</script>

<template>
  <div class="app-shell">
    <transition name="fade" mode="out-in">
      <div v-if="isLoginRoute" key="login" class="app-shell__center">
        <router-view />
      </div>

      <div v-else-if="authStore.isAuthenticated" key="shell" class="app-shell__body">
        <aside class="sidebar">
          <div class="brand">
            <img src="/favicon.svg" alt="" class="brand-logo" />
            <span class="brand-name">Plex Autoplay</span>
          </div>

          <nav class="nav" aria-label="Navegação principal">
            <router-link
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              class="nav-item"
              active-class="nav-item-active"
            >
              <AppIcon :name="item.icon" class="nav-icon" />
              <span class="nav-label">{{ item.label }}</span>
            </router-link>
          </nav>

          <footer class="sidebar-footer">
            <span class="status-dot" aria-hidden="true"></span>
            <span>PlexAutoPlay em execução</span>
          </footer>

          <button class="logout-btn" title="Sair" @click="handleLogout">
            Sair
          </button>
        </aside>

        <main class="main-content">
          <router-view v-slot="{ Component }">
            <component :is="Component" />
          </router-view>
        </main>
      </div>
    </transition>
  </div>
</template>
