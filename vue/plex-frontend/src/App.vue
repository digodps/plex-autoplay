<script setup>
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
</template>
