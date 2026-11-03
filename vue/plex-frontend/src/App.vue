<script setup>
import { useAppShell } from "./composables/useAppShell";
import { useAuthStore } from "./stores/auth";
import AppIcon from "./components/AppIcon.vue";
import "./redesign.css";

const authStore = useAuthStore();
const { isLoginRoute, navItems, handleLogout } = useAppShell();
</script>

<template>
  <div class="app-shell" style="font-family: var(--heading-font); min-height: 100vh; display: flex; flex-direction: column;">
    <transition name="fade" mode="out-in">
      <div v-if="isLoginRoute" key="login">
        <component :is="Component" />
      </div>

      <div v-else-if="authStore.isAuthenticated" key="shell" class="max-width-center">
        <div style="width: 100%; display: flex; flex-direction: column; min-height: 100vh;">
          <aside class="sidebar" style="height: 100vh; flex-shrink: 0; width: 240px;">
            <div class="brand" style="padding: 1rem 1.25rem; border-bottom: 1px solid var(--border);">
              <img src="/favicon.svg" alt="" class="brand-logo" style="width: 28px; height: 28px; vertical-align: middle; margin-right: 0.75rem;" />
              <span class="brand-name" style="font-size: 1.125rem; font-weight: 600; letter-spacing: -0.02em; color: var(--text);">Plex Autoplay</span>
            </div>

            <nav class="nav" style="margin: 1rem 0; padding: 0; gap: 0.5rem;">
              <router-link
                v-for="item in navItems"
                :key="item.to"
                :to="item.to"
                class="nav-item"
                style="display: flex; align-items: center; gap: 0.5rem; padding: 0.5rem 0.75rem; border-radius: var(--radius-sm); color: var(--text); text-decoration: none; transition: var(--transition-fast);"
                active-class="nav-item-active"
              >
                <AppIcon :name="item.icon" style="width: 18px; height: 18px; flex-shrink: 0;" />
                <span style="font-size: 0.875rem; font-weight: 500; color: var(--text);">{{ item.label }}</span>
              </router-link>
            </nav>

            <footer class="sidebar-footer" style="margin-top: auto; padding-top: 1rem; border-top: 1px solid var(--border); font-size: 0.75rem; color: var(--text-tertiary);">
              <span class="status-dot" aria-hidden="true" style="width: 8px; height: 8px; border-radius: 50%; background: var(--accent); margin-right: 0.25rem; vertical-align: middle;"></span>
              <span>PlexAutoPlay em execução</span>
            </footer>

            <button class="logout-btn" style="margin-top: 1rem; width: 100%; padding: 0.75rem 1rem; border: none; border-radius: var(--radius-sm); background: var(--accent); color: var(--accent-contrast); font-weight: 500; font-size: 0.875rem; cursor: pointer; transition: var(--transition-fast);" title="Sair" @click="handleLogout">
              Sair
            </button>
          </aside>

          <main class="main-content" style="flex: 1; width: 100%; padding: 1.5rem;">
            <router-view v-slot="{ Component }">
              <div v-if="!isLoginRoute" key="content">
                <component :is="Component" />
              </div>
            </router-view>
          </main>
        </div>
      </div>
    </transition>
  </div>
</template>