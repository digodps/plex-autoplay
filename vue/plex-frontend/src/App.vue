<script setup>
import { useAppShell } from "./composables/useAppShell";
import { useAuthStore } from "./stores/auth";
import AppSidebar from "./components/AppSidebar.vue";

const authStore = useAuthStore();
const { isLoginRoute, navItems, activePath, navigate, handleLogout } = useAppShell();
</script>

<template>
  <div class="app-shell">
    <transition name="fade" mode="out-in">
      <div v-if="isLoginRoute" key="login" class="app-shell__center">
        <router-view />
      </div>

      <div v-else-if="authStore.isAuthenticated" key="shell" class="app-shell__body">
        <AppSidebar
          :items="navItems"
          :active-path="activePath"
          status-label="PlexAutoPlay em execução"
          @select="navigate"
          @logout="handleLogout"
        />

        <main class="main-content">
          <router-view v-slot="{ Component }">
            <component :is="Component" />
          </router-view>
        </main>
      </div>
    </transition>
  </div>
</template>
