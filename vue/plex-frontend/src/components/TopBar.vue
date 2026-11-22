<script setup>
import { computed, ref, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import { useThemeStore } from "@/stores/theme";
import { useAuthStore } from "@/stores/auth";
import { useLocaleStore } from "@/stores/locale";
import { useUIStore } from "@/stores/ui";
import AppIcon from "@/components/AppIcon.vue";
import PasswordModal from "@/components/PasswordModal.vue";

const { t } = useI18n();
const localeStore = useLocaleStore();
const uiStore = useUIStore();
const router = useRouter();
const theme = useThemeStore();
const auth = useAuthStore();

const props = defineProps({
  /** Emite evento para sidebar toggler (mobile) */
  onToggleSidebar: { type: Function, default: null },
  /** Breadcrumb items passados pelo layout pai */
  breadcrumbs: { type: Array, default: () => [] },
});

const showPassword = ref(false);
const showSearch = ref(false);
const searchQuery = ref("");
const profileMenuRef = ref(null);
const languageMenuRef = ref(null);
const searchInputRef = ref(null);

// Fechar menus ao clicar fora
function handleClickOutside(event) {
  if (profileMenuRef.value && !profileMenuRef.value.contains(event.target)) {
    uiStore.closeProfile();
  }
  if (languageMenuRef.value && !languageMenuRef.value.contains(event.target)) {
    uiStore.closeLanguageMenu();
  }
}

// Fechar busca ao pressionar Escape
function handleKeydown(event) {
  if (event.key === "Escape") {
    if (showSearch.value) closeSearch();
    if (uiStore.showProfile) uiStore.closeProfile();
  }
}

onMounted(() => {
  document.addEventListener("click", handleClickOutside);
  document.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
  document.removeEventListener("click", handleClickOutside);
  document.removeEventListener("keydown", handleKeydown);
});

const isDark = computed(() => theme.effective === "dark");

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

function openSearch() {
  showSearch.value = true;
  // Aguarda DOM atualizar antes de focar
  setTimeout(() => searchInputRef.value?.focus(), 0);
}

function closeSearch() {
  showSearch.value = false;
  searchQuery.value = "";
}

function submitSearch() {
  if (!searchQuery.value.trim()) return;
  // ponytail: busca real, integrar com router/api quando houver endpoint
  closeSearch();
}

const iconBtn =
  "flex items-center justify-center rounded-lg p-2 text-[color:var(--text-secondary)] hover:text-[color:var(--text)] hover:bg-[color:var(--bg-sunken)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[color:var(--accent)] transition-colors";

const menuItemCls =
  "flex w-full items-center gap-2 rounded px-3 py-2 text-left text-sm text-[color:var(--text-secondary)] hover:bg-[color:var(--bg-sunken)] hover:text-[color:var(--text)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[color:var(--accent)]";
</script>

<template>
  <header
    data-testid="top-bar"
    class="relative z-30 flex shrink-0 items-center gap-2 px-3 md:px-4 py-2 border-b text-sm bg-[color:var(--surface)] border-[color:var(--border)]"
    role="banner"
  >
    <!-- Botão hamburger (mobile) -->
    <button
      v-if="onToggleSidebar"
      type="button"
      :class="[iconBtn, 'md:hidden']"
      aria-label="Abrir menu lateral"
      @click="onToggleSidebar"
    >
      <AppIcon name="list" size="sm" />
    </button>

    <!-- Breadcrumbs -->
    <nav
      v-if="breadcrumbs.length"
      aria-label="Localização atual"
      class="min-w-0 flex-1 overflow-x-auto"
    >
      <ol class="flex w-max items-center gap-1 whitespace-nowrap text-[color:var(--text-secondary)]">
        <li
          v-for="(item, index) in breadcrumbs"
          :key="index"
          class="flex items-center gap-1"
        >
          <AppIcon name="chevron-right" size="sm" class="shrink-0 text-[color:var(--text-tertiary)]" />
          <RouterLink
            v-if="item.to && !item.isCurrent"
            :to="item.to"
            class="hover:text-[color:var(--accent)] focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[color:var(--accent)] rounded"
          >
            {{ $t(item.label) }}
          </RouterLink>
          <span
            v-else
            :class="item.isCurrent ? 'font-medium text-[color:var(--text)]' : ''"
            :aria-current="item.isCurrent ? 'page' : undefined"
          >
            {{ $t(item.label) }}
          </span>
        </li>
      </ol>
    </nav>

    <!-- Spacer quando sem breadcrumb -->
    <div v-else class="flex-1" aria-hidden="true" />

    <!-- Busca inline (mobile: modal; desktop: expansível) -->
    <div class="relative flex items-center">
      <!-- Campo de busca expandido -->
      <div
        v-if="showSearch"
        class="flex items-center gap-1 rounded-lg border border-[color:var(--border)] bg-[color:var(--bg-sunken)] px-2 py-1 focus-within:border-[color:var(--accent)] focus-within:ring-2 focus-within:ring-[color:var(--accent)]"
        role="search"
      >
        <AppIcon name="search" size="sm" class="shrink-0 text-[color:var(--text-tertiary)]" />
        <input
          ref="searchInputRef"
          v-model="searchQuery"
          type="search"
          :placeholder="t('topBar.searchPlaceholder', 'Buscar…')"
          class="w-40 sm:w-56 bg-transparent text-sm text-[color:var(--text)] outline-none placeholder:text-[color:var(--text-tertiary)]"
          aria-label="Buscar no sistema"
          @keydown.enter="submitSearch"
          @keydown.escape="closeSearch"
        />
        <button
          type="button"
          :class="iconBtn"
          :aria-label="t('topBar.closeSearch', 'Fechar busca')"
          @click="closeSearch"
        >
          <AppIcon name="close" size="sm" />
        </button>
      </div>

      <!-- Botão de abrir busca -->
      <button
        v-else
        type="button"
        :class="iconBtn"
        :aria-label="t('topBar.search', 'Buscar')"
        :title="t('topBar.search', 'Buscar')"
        @click="openSearch"
      >
        <AppIcon name="search" size="sm" />
      </button>
    </div>

    <!-- Alternar tema -->
    <button
      type="button"
      :class="iconBtn"
      :aria-label="isDark ? t('theme.light', 'Tema claro') : t('theme.dark', 'Tema escuro')"
      :title="isDark ? t('theme.light', 'Tema claro') : t('theme.dark', 'Tema escuro')"
      @click="theme.cycleMode"
    >
      <AppIcon :name="isDark ? 'sun' : 'moon'" size="sm" />
    </button>

    <!-- Perfil do usuário -->
    <div ref="profileMenuRef" class="relative">
      <button
        type="button"
        :class="iconBtn"
        aria-label="Menu do usuário"
        :aria-expanded="uiStore.showProfile"
        aria-haspopup="menu"
        @click="uiStore.toggleProfile"
        data-testid="user-menu-button"
      >
        <AppIcon name="user" size="sm" />
      </button>

      <!-- Dropdown de perfil -->
      <div
        v-if="uiStore.showProfile"
        role="menu"
        aria-label="Opções do usuário"
        class="absolute right-0 mt-2 w-48 rounded-lg border border-[color:var(--border)] bg-[color:var(--surface)] p-1 shadow-[var(--shadow-2)] z-50"
      >
        <!-- Configurações -->
        <RouterLink
          to="/settings"
          role="menuitem"
          :class="menuItemCls"
          @click="uiStore.closeProfile"
        >
          <AppIcon name="settings" size="sm" aria-hidden="true" />
          {{ t("userMenu.settings") }}
        </RouterLink>

        <div class="my-1 border-t border-[color:var(--border)]" role="separator" />

        <!-- Seletor de idioma -->
        <div ref="languageMenuRef" class="relative">
          <button
            type="button"
            role="menuitem"
            :class="menuItemCls"
            :aria-expanded="uiStore.showLanguageMenu"
            aria-haspopup="menu"
            aria-label="Selecionar idioma"
            @click="uiStore.toggleLanguageMenu"
            data-testid="language-selector-button"
          >
            <AppIcon
              :name="localeStore.currentLocaleInfo.icon"
              size="sm"
              class="w-5 h-4 shrink-0"
              aria-hidden="true"
            />
            <span class="flex-1">{{ localeStore.currentLocaleInfo.label }}</span>
            <AppIcon name="chevron-down" size="sm" class="text-[color:var(--text-tertiary)]" aria-hidden="true" />
          </button>

          <div
            v-if="uiStore.showLanguageMenu"
            role="menu"
            aria-label="Idiomas disponíveis"
            class="absolute right-0 mt-1 w-44 rounded-lg border border-[color:var(--border)] bg-[color:var(--surface)] shadow-[var(--shadow-2)] py-1 z-50"
            data-testid="language-selector-dropdown"
          >
            <button
              v-for="locale in localeStore.availableLocales"
              :key="locale.code"
              type="button"
              role="menuitemradio"
              :aria-checked="locale.code === localeStore.locale"
              :class="[
                menuItemCls,
                locale.code === localeStore.locale
                  ? 'bg-[color:var(--bg-sunken)] font-semibold'
                  : '',
              ]"
              @click="selectLocale(locale.code)"
              data-testid="language-option"
            >
              <AppIcon :name="locale.icon" size="sm" class="w-5 h-4 shrink-0" aria-hidden="true" />
              <span>{{ locale.label }}</span>
            </button>
          </div>
        </div>

        <div class="my-1 border-t border-[color:var(--border)]" role="separator" />

        <!-- Alterar senha -->
        <button
          type="button"
          role="menuitem"
          :class="menuItemCls"
          data-testid="change-password-menu"
          @click="openPassword"
        >
          <AppIcon name="edit" size="sm" aria-hidden="true" />
          {{ t("userMenu.changePassword") }}
        </button>

        <!-- Logout -->
        <button
          type="button"
          role="menuitem"
          :class="[menuItemCls, 'text-[color:var(--danger)] hover:bg-[color:color-mix(in_srgb,var(--danger)_10%,transparent)]']"
          @click="logout"
        >
          <AppIcon name="logout" size="sm" aria-hidden="true" />
          {{ t("userMenu.logout") }}
        </button>
      </div>
    </div>

    <PasswordModal :is-open="showPassword" @close="showPassword = false" />
  </header>
</template>
