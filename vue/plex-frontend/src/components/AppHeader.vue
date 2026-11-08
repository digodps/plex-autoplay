<script setup>
import { computed, ref } from "vue";
import { useRouter } from "vue-router";
import { useSidebarStore } from "@/stores/sidebar";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import AppIcon from "@/components/AppIcon.vue";
import AppLogo from "@/components/AppLogo.vue";
import PasswordModal from "@/components/PasswordModal.vue";

defineProps({
  items: { type: Array, default: () => [] },
});

const router = useRouter();
const sidebar = useSidebarStore();
const theme = useThemeStore();
const auth = useAuthStore();

const showProfile = ref(false);
const showPassword = ref(false);

const isDark = computed(() => theme.mode === "dark");

const iconButton =
  "flex items-center gap-2 rounded-lg p-2 text-gray-700 hover:text-primary hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary";

function logout() {
  showProfile.value = false;
  auth.logout();
  router.push({ name: "login" });
}

function openPassword() {
  showProfile.value = false;
  showPassword.value = true;
}
</script>

<template>
  <header
    class="sticky top-0 z-30 flex items-center gap-2 border-b border-gray-300 bg-white px-4 py-2 text-sm dark:border-gray-700 dark:bg-gray-900"
  >
    <RouterLink
      to="/"
      class="flex shrink-0 items-center gap-2 rounded-lg focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
      aria-label="Plex Autoplay — início"
    >
      <AppLogo :size="28" />
      <span class="hidden font-semibold text-black sm:inline dark:text-white">
        Plex Autoplay
      </span>
    </RouterLink>

    <span class="h-5 w-px bg-gray-300 dark:bg-gray-700" aria-hidden="true"></span>

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

    <nav aria-label="Breadcrumb" class="min-w-0 truncate">
      <ol class="flex items-center gap-1 text-gray-700 dark:text-bodydark">
        <li
          v-for="(item, index) in items"
          :key="index"
          class="flex min-w-0 items-center gap-1"
        >
          <RouterLink
            v-if="item.to && !item.isCurrent"
            :to="item.to"
            class="truncate hover:text-primary"
          >
            {{ item.label }}
          </RouterLink>
          <span
            v-else
            class="truncate"
            :class="item.isCurrent ? 'font-medium text-black dark:text-white' : ''"
            :aria-current="item.isCurrent ? 'page' : undefined"
          >
            {{ item.label }}
          </span>
          <span v-if="index < items.length - 1" aria-hidden="true">/</span>
        </li>
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
          class="flex items-center gap-2 rounded px-3 py-2 text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
          @click="showProfile = false"
        >
          <AppIcon name="settings" size="sm" />
          Configurações
        </router-link>
        <button
          type="button"
          class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
          data-testid="change-password-menu"
          @click="openPassword"
        >
          <AppIcon name="edit" size="sm" />
          Alterar senha
        </button>
        <button
          type="button"
          class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-red hover:bg-gray-100 dark:hover:bg-gray-700"
          @click="logout"
        >
          <AppIcon name="logout" size="sm" />
          Sair
        </button>
      </div>
    </div>

    <PasswordModal :is-open="showPassword" @close="showPassword = false" />
  </header>
</template>
