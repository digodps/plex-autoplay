<script setup>
import { computed, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useSidebarStore } from "@/stores/sidebar";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import AppIcon from "@/components/AppIcon.vue";
import SettingsModal from "@/components/SettingsModal.vue";

const route = useRoute();
const router = useRouter();
const sidebar = useSidebarStore();
const theme = useThemeStore();
const auth = useAuthStore();

const showProfile = ref(false);
const showSettings = ref(false);

const pageTitle = computed(() => route.meta.title || "");
const isDark = computed(() => theme.mode === "dark");

const iconButton =
  "flex items-center gap-2 rounded-lg p-2 text-gray-700 hover:text-primary hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary";

function logout() {
  showProfile.value = false;
  auth.logout();
  router.push({ name: "login" });
}
</script>

<template>
  <header
    class="sticky top-0 z-30 flex items-center gap-2 border-b border-gray-300 bg-white px-4 py-2 text-sm dark:border-gray-700 dark:bg-gray-900"
  >
    <button
      :class="iconButton"
      type="button"
      :aria-label="sidebar.isCollapsed ? 'Expandir menu' : 'Recolher menu'"
      :aria-expanded="!sidebar.isCollapsed"
      @click="sidebar.toggleCollapsed"
    >
      <AppIcon name="menu" size="sm" />
    </button>

    <span class="h-5 w-px bg-gray-300 dark:bg-gray-700" aria-hidden="true"></span>

    <nav aria-label="Breadcrumb" class="truncate">
      <ol class="flex items-center gap-1 text-gray-700 dark:text-bodydark">
        <li><router-link to="/dashboard" class="hover:text-primary">Início</router-link></li>
        <li aria-hidden="true">/</li>
        <li class="font-medium text-black dark:text-white" aria-current="page">{{ pageTitle }}</li>
      </ol>
    </nav>

    <div class="flex-1"></div>

    <button
      :class="iconButton"
      type="button"
      :aria-label="isDark ? 'Tema claro' : 'Tema escuro'"
      :title="isDark ? 'Tema claro' : 'Tema escuro'"
      @click="theme.toggle"
    >
      <AppIcon :name="isDark ? 'sun' : 'moon'" size="sm" />
    </button>

    <button
      :class="iconButton"
      type="button"
      aria-label="Configurações"
      title="Configurações"
      @click="showSettings = true"
    >
      <AppIcon name="settings" size="sm" />
    </button>

    <div class="relative">
      <button
        :class="iconButton"
        type="button"
        aria-label="Menu do usuário"
        :aria-expanded="showProfile"
        @click="showProfile = !showProfile"
      >
        <AppIcon name="user" size="sm" />
      </button>

      <div
        v-if="showProfile"
        class="absolute right-0 mt-2 w-44 rounded-lg border border-gray-300 bg-white p-1 shadow-lg dark:border-gray-700 dark:bg-gray-800"
      >
        <router-link
          to="/settings"
          class="block rounded px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-700"
          @click="showProfile = false"
        >
          Configurações
        </router-link>
        <button
          type="button"
          class="block w-full rounded px-3 py-2 text-left text-red hover:bg-gray-100 dark:hover:bg-gray-700"
          @click="logout"
        >
          Sair
        </button>
      </div>
    </div>

    <SettingsModal :is-open="showSettings" @close="showSettings = false" />
  </header>
</template>
