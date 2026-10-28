<script setup>
import { computed } from "vue";
import { useRoute } from "vue-router";
import { useAuthStore } from "./stores/auth";
import AppIcon from "./components/AppIcon.vue";

const authStore = useAuthStore();
const route = useRoute();

// Login ocupa a tela inteira, sem shell
const isLoginRoute = computed(() => route.name === "login");

const navItems = [
  { to: "/dashboard", label: "Início", icon: "home" },
  { to: "/series", label: "Séries", icon: "tv" },
  { to: "/libraries", label: "Bibliotecas", icon: "library" },
  { to: "/sync", label: "Sincronizar", icon: "sync" },
  { to: "/settings", label: "Ajustes", icon: "settings" },
];
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

<style>
.app-shell {
  display: grid;
  grid-template-columns: var(--sidebar-w) 1fr;
  min-height: 100dvh;
}

.sidebar {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
  position: sticky;
  top: 0;
  height: 100dvh;
  padding: 1.25rem 0.75rem;
  border-right: 1px solid var(--border);
  background: var(--bg-sunken);
}

.brand {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  padding: 0 0.5rem;
}

.brand-logo {
  width: 28px;
  height: 28px;
}

.brand-name {
  font-weight: 700;
  font-size: 0.95rem;
  color: var(--text);
  white-space: nowrap;
}

.nav {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.7rem;
  padding: 0.55rem 0.75rem;
  border-radius: var(--radius-sm);
  color: var(--text-secondary);
  font-weight: 500;
  transition:
    background var(--speed) var(--ease),
    color var(--speed) var(--ease);
}

.nav-item:hover {
  background: var(--surface);
  color: var(--text);
}

.nav-item-active {
  background: var(--surface);
  color: var(--accent);
  font-weight: 600;
}

.sidebar-footer {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0 0.5rem;
  color: var(--text-tertiary);
  font-size: 0.75rem;
}

.status-dot {
  width: 8px;
  height: 8px;
  flex: none;
  border-radius: 50%;
  background: var(--success);
}

.content {
  min-width: 0;
}

@media (max-width: 768px) {
  .app-shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    position: relative;
    height: auto;
    flex-direction: row;
    align-items: center;
    gap: 0.75rem;
    border-right: none;
    border-bottom: 1px solid var(--border);
  }

  .nav {
    flex-direction: row;
    overflow-x: auto;
  }

  .sidebar-footer {
    display: none;
  }
}
</style>
