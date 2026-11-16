<script setup>
import { computed, ref, onMounted, onUnmounted, watch } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import { useLocaleStore } from "@/stores/locale";
import { useUIStore } from "@/stores/ui";
import AppIcon from "@/components/AppIcon.vue";
import AppLogo from "@/components/AppLogo.vue";
import PasswordModal from "@/components/PasswordModal.vue";

const { t } = useI18n();
const localeStore = useLocaleStore();
const uiStore = useUIStore();

defineProps({
  items: { type: Array, default: () => [] },
});

const router = useRouter();
const theme = useThemeStore();
const auth = useAuthStore();

const showPassword = ref(false);
const languageMenuRef = ref(null);

function handleClickOutside(event) {
  if (languageMenuRef.value && !languageMenuRef.value.contains(event.target)) {
    uiStore.closeLanguageMenu();
  }
}

onMounted(() => {
  document.addEventListener("click", handleClickOutside);
});

onUnmounted(() => {
  document.removeEventListener("click", handleClickOutside);
});

const isDark = computed(() => theme.effective === "dark");

const iconButton =
  "flex items-center gap-2 rounded-lg p-2 text-gray-700 hover:text-primary hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary";

const menuItem = "flex w-full items-center gap-2 rounded px-3 py-2 text-left text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700";

function logout() {
  uiStore.closeProfile();
  auth.logout();
  router.push({ name: "login" });
}

function openPassword() {
  uiStore.closeProfile();
  showPassword.value = true;
}

function selectLocale(code) {
  localeStore.setLocale(code);
  uiStore.closeProfile();
}
</script>

<template>
  <header
    data-testid="app-header"
    class="relative z-30 flex shrink-0 items-center gap-2 px-4 md:px-6 py-2 text-sm"
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

    <nav
      v-if="items.length"
      aria-label="Breadcrumb"
      class="min-w-0 flex-1 overflow-x-auto"
    >
      <ol class="flex w-max items-center gap-1 whitespace-nowrap text-gray-700 dark:text-bodydark">
        <li v-for="(item, index) in items" :key="index" class="flex items-center gap-1">
          <AppIcon name="chevron-right" size="sm" class="shrink-0 text-gray-400" />
          <RouterLink
            v-if="item.to && !item.isCurrent"
            :to="item.to"
            class="hover:text-primary"
          >
            {{ $t(item.label) }}
          </RouterLink>
          <span
            v-else
            :class="item.isCurrent ? 'font-medium text-black dark:text-white' : ''"
            :aria-current="item.isCurrent ? 'page' : undefined"
          >
            {{ $t(item.label) }}
          </span>
        </li>
      </ol>
    </nav>

    <button
      :class="[iconButton, 'ml-auto']"
      type="button"
      :aria-label="isDark ? $t('theme.light') : $t('theme.dark')"
      :title="isDark ? $t('theme.light') : $t('theme.dark')"
      @click="theme.cycleMode"
    >
      <AppIcon :name="isDark ? 'sun' : 'moon'" size="sm" />
    </button>

    <div class="relative">
      <button
        :class="iconButton"
        type="button"
        aria-label="Menu do usuário"
        :aria-expanded="uiStore.showProfile"
        @click="uiStore.toggleProfile"
        data-testid="user-menu-button"
      >
        <AppIcon name="user" size="sm" />
      </button>

      <div
        v-if="uiStore.showProfile"
        class="absolute right-0 mt-2 w-44 rounded-lg border border-gray-300 bg-white p-1 shadow-lg dark:border-gray-700 dark:bg-gray-800"
      >
        <router-link
          to="/settings"
          class="flex items-center gap-2 rounded px-3 py-2 text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
          @click="uiStore.closeProfile"
        >
          <AppIcon name="settings" size="sm" />
          {{ t("userMenu.settings") }}
        </router-link>
        <div class="border-t border-gray-200 dark:border-gray-700 my-1" />
        <div class="relative" ref="languageMenuRef">
          <button
            type="button"
            :class="menuItem"
            :aria-expanded="uiStore.showLanguageMenu"
            aria-haspopup="menu"
            aria-label="Selecionar idioma"
            @click="uiStore.toggleLanguageMenu"
            data-testid="language-selector-button"
          >
            <AppIcon :name="localeStore.currentLocaleInfo.icon" size="sm" class="w-6 h-4" aria-hidden="true" />
            <span>{{ localeStore.currentLocaleInfo.label }}</span>
            <AppIcon name="chevron-down" size="sm" class="ml-1 text-gray-500" />
          </button>
          <div
            v-if="uiStore.showLanguageMenu"
            class="absolute right-0 mt-1 w-40 rounded-lg border border-gray-300 bg-white shadow-lg py-1 dark:border-gray-700 dark:bg-gray-800 z-50"
            data-testid="language-selector-dropdown"
          >
            <button
              v-for="locale in localeStore.availableLocales"
              :key="locale.code"
              type="button"
              :class="['flex w-full items-center gap-2 px-3 py-2 text-left text-sm', locale.code === localeStore.locale ? 'bg-gray-100 dark:bg-gray-700' : 'text-gray-700 hover:bg-gray-100', 'dark:text-gray-200 dark:hover:bg-gray-700']"
              @click="selectLocale(locale.code)"
              data-testid="language-option"
            >
              <AppIcon :name="locale.icon" size="sm" class="w-6 h-4" aria-hidden="true" />
              <span class="font-medium">{{ locale.label }}</span>
            </button>
          </div>
        </div>
        <button
          type="button"
          class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-gray-700 hover:bg-gray-100 dark:text-gray-200 dark:hover:bg-gray-700"
          data-testid="change-password-menu"
          @click="openPassword"
        >
          <AppIcon name="edit" size="sm" />
          {{ t("userMenu.changePassword") }}
        </button>
        <button
          type="button"
          class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-red hover:bg-gray-100 dark:hover:bg-gray-700"
          @click="logout"
        >
          <AppIcon name="logout" size="sm" />
          {{ t("userMenu.logout") }}
        </button>
      </div>
    </div>

    <PasswordModal :is-open="showPassword" @close="showPassword = false" />
  </header>
</template>
