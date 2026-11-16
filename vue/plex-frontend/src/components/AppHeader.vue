<script setup>
import { computed, ref, onMounted, onUnmounted } from "vue";
import { useRouter, useRoute } from "vue-router";
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
const router = useRouter();
const theme = useThemeStore();
const auth = useAuthStore();
const route = useRoute();

const showPassword = ref(false);
const languageMenuRef = ref(null);
const userMenuRef = ref(null);

function handleClickOutside(event) {
  if (languageMenuRef.value && !languageMenuRef.value.contains(event.target)) {
    uiStore.closeLanguageMenu();
  }
  if (userMenuRef.value && !userMenuRef.value.contains(event.target)) {
    uiStore.closeProfile();
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

function handleKeydown(event) {
  if (event.key === "Escape") {
    uiStore.closeLanguageMenu();
    uiStore.closeProfile();
  }
}

const isDark = computed(() => theme.effective === "dark");

const navItems = [
  { to: "/", label: "nav.dashboard", icon: "home", exact: true },
  { to: "/libraries", label: "nav.libraries", icon: "server" },
  { to: "/sync", label: "nav.sync", icon: "refresh-cw" },
  { to: "/settings", label: "nav.settings", icon: "settings" },
  { to: "/logs", label: "nav.logs", icon: "file-text" },
];

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
  uiStore.closeLanguageMenu();
}
</script>

<template>
<header class="nav" data-testid="app-header">
  <div class="wrap nav__bar">
    <!-- Skip link for accessibility -->
    <a class="skip-link" href="#main-content" :aria-label="$t('a11y.skipToContent')">{{ $t('a11y.skipToContent') }}</a>

    <!-- Logo / Wordmark -->
    <RouterLink to="/" class="nav__logo" aria-label="Plex Autoplay — início">
      <AppLogo :size="28" />
      <span class="hidden sm:inline font-medium text-[var(--p-ink)]">Plex Autoplay</span>
    </RouterLink>

    <!-- Main Navigation (nav-cards pattern) -->
    <nav class="nav__links" aria-label="Navegação principal">
      <div class="nav__item" v-for="item in navItems" :key="item.to">
        <RouterLink
          :to="item.to"
          class="nav__link"
          :aria-current="route.path === item.to && item.exact ? 'page' : undefined"
          data-testid="nav-link"
        >
          <AppIcon :name="item.icon" size="sm" class="nav__icon" aria-hidden="true" />
          {{ $t(item.label) }}
          <svg class="chev" viewBox="0 0 12 12" aria-hidden="true">
            <path d="M3 4.5l3 3 3-3" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
          </svg>
        </RouterLink>
        <!-- Dropdown menu for nav-cards (could be expanded with mega-menu later) -->
        <div class="nav__menu" v-if="item.to === '/libraries' || item.to === '/settings'">
          <div class="wrap grid">
            <div class="menu-col span-3" v-if="item.to === '/libraries'">
              <p class="t-label">{{ $t('nav.menu.libraries') }}</p>
              <RouterLink to="/libraries" class="menu-link" @click="uiStore.closeProfile">
                <span>{{ $t('nav.menu.allLibraries') }}</span>
                <span class="menu-desc">{{ $t('nav.menu.allLibrariesDesc') }}</span>
              </RouterLink>
              <RouterLink to="/sync" class="menu-link" @click="uiStore.closeProfile">
                <span>{{ $t('nav.menu.sync') }}</span>
                <span class="menu-desc">{{ $t('nav.menu.syncDesc') }}</span>
              </RouterLink>
            </div>
            <div class="menu-col span-3" v-if="item.to === '/settings'">
              <p class="t-label">{{ $t('nav.menu.settings') }}</p>
              <RouterLink to="/settings" class="menu-link" @click="uiStore.closeProfile">
                <span>{{ $t('nav.menu.general') }}</span>
                <span class="menu-desc">{{ $t('nav.menu.generalDesc') }}</span>
              </RouterLink>
              <RouterLink to="/settings" class="menu-link" @click="uiStore.closeProfile">
                <span>{{ $t('nav.menu.plex') }}</span>
                <span class="menu-desc">{{ $t('nav.menu.plexDesc') }}</span>
              </RouterLink>
              <RouterLink to="/settings" class="menu-link" @click="uiStore.closeProfile">
                <span>{{ $t('nav.menu.advanced') }}</span>
                <span class="menu-desc">{{ $t('nav.menu.advancedDesc') }}</span>
              </RouterLink>
            </div>
            <a class="menu-card span-3 start-10" :href="item.to" @click="uiStore.closeProfile">
              <div class="panel">
                <AppIcon :name="item.icon" size="lg" class="menu-card-icon" aria-hidden="true" />
              </div>
              <span class="t-card">{{ $t('nav.menu.new') }}</span>
              <span class="t-small muted">{{ $t('nav.menu.newDesc') }}</span>
            </a>
          </div>
        </div>
      </div>
    </nav>

    <!-- Actions: Theme toggle + User menu -->
    <div class="nav__actions">
      <!-- Theme Toggle -->
      <button
        class="btn btn--ghost nav__theme-toggle"
        type="button"
        :aria-label="isDark ? $t('theme.light') : $t('theme.dark')"
        :title="isDark ? $t('theme.light') : $t('theme.dark')"
        @click="theme.cycleMode"
        data-testid="theme-toggle"
      >
        <AppIcon :name="isDark ? 'sun' : 'moon'" size="sm" />
      </button>

      <!-- User Menu -->
      <div class="relative" ref="userMenuRef">
        <button
          class="btn btn--ghost nav__user-toggle"
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
          class="nav__dropdown absolute right-0 mt-2 w-56 rounded-lg border border-[var(--border)] bg-[var(--surface)] p-1 shadow-lg z-50"
          data-testid="user-dropdown"
        >
          <RouterLink
            to="/settings"
            class="flex items-center gap-2 rounded px-3 py-2 text-[var(--p-ink)] hover:bg-[var(--p-muted)]/10 dark:hover:bg-[var(--p-inv-muted)]/10"
            @click="uiStore.closeProfile"
            data-testid="settings-link"
          >
            <AppIcon name="settings" size="sm" />
            {{ t("userMenu.settings") }}
          </RouterLink>

          <div class="border-t border-[var(--border)] my-1" />

          <!-- Language Selector -->
          <div class="relative" ref="languageMenuRef">
            <button
              type="button"
              class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-[var(--p-ink)] hover:bg-[var(--p-muted)]/10"
              :aria-expanded="uiStore.showLanguageMenu"
              aria-haspopup="menu"
              aria-label="Selecionar idioma"
              @click="uiStore.toggleLanguageMenu"
              data-testid="language-selector-button"
            >
              <AppIcon :name="localeStore.currentLocaleInfo.icon" size="sm" class="w-6 h-4" aria-hidden="true" />
              <span>{{ localeStore.currentLocaleInfo.label }}</span>
              <AppIcon name="chevron-down" size="sm" class="ml-1 text-[var(--p-muted)]" />
            </button>
            <div
              v-if="uiStore.showLanguageMenu"
              class="absolute right-0 mt-1 w-44 rounded-lg border border-[var(--border)] bg-[var(--surface)] shadow-lg py-1 z-50"
              data-testid="language-selector-dropdown"
            >
              <button
                v-for="locale in localeStore.availableLocales"
                :key="locale.code"
                type="button"
                class="flex w-full items-center gap-2 px-3 py-2 text-left text-sm"
                :class="[
                  locale.code === localeStore.locale ? 'bg-[var(--p-muted)]/10' : 'hover:bg-[var(--p-muted)]/10',
                  'text-[var(--p-ink)]'
                ]"
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
            class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-[var(--p-ink)] hover:bg-[var(--p-muted)]/10"
            data-testid="change-password-menu"
            @click="openPassword"
          >
            <AppIcon name="pencil" size="sm" />
            {{ t("userMenu.changePassword") }}
          </button>
          <button
            type="button"
            class="flex w-full items-center gap-2 rounded px-3 py-2 text-left text-[var(--p-action)] hover:bg-[var(--p-action)]/10"
            @click="logout"
          >
            <AppIcon name="logout" size="sm" />
            {{ t("userMenu.logout") }}
          </button>
        </div>
      </div>

      <!-- Mobile menu toggle -->
      <button class="nav__toggle" type="button" aria-expanded="false" aria-label="Menu" aria-controls="main-nav">
        <svg width="18" height="18" viewBox="0 0 18 18" aria-hidden="true"><path d="M2 5h14M2 13h14" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"/></svg>
      </button>
    </div>
  </div>
</header>

<PasswordModal :is-open="showPassword" @close="showPassword = false" />
</template>

<style scoped>
.nav {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
  background: color-mix(in srgb, var(--p-surface) 95%, transparent);
  backdrop-filter: saturate(180%) blur(20px);
  border-bottom: 1px solid var(--border);
}

.nav__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--s-4);
  height: var(--nav-h, 64px);
  max-width: var(--wrap-max, 1440px);
  margin: 0 auto;
  padding: 0 var(--s-4);
}

.skip-link {
  position: absolute;
  top: -100%;
  left: 50%;
  transform: translateX(-50%);
  padding: var(--s-2) var(--s-4);
  background: var(--p-accent);
  color: var(--p-accent-ink);
  border-radius: var(--r-md);
  z-index: 200;
  font-weight: 500;
}
.skip-link:focus {
  top: var(--s-4);
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

.nav__logo {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  font: var(--font-display);
  font-size: var(--t-xl);
  font-weight: 600;
  color: var(--p-ink);
  text-decoration: none;
}
.nav__logo:hover { opacity: 0.8; }
.nav__logo:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 4px;
  border-radius: var(--r-md);
}

.nav__links {
  display: flex;
  align-items: center;
  gap: var(--s-1);
}

.nav__item {
  position: relative;
}

.nav__link {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  padding: var(--s-2) var(--s-3);
  font-size: var(--t-sm);
  font-weight: 500;
  color: var(--p-ink);
  text-decoration: none;
  border-radius: var(--r-md);
  transition: background var(--speed) var(--ease), color var(--speed) var(--ease);
}
.nav__link:hover {
  background: var(--p-muted);
  color: var(--p-accent);
}
.nav__link[aria-current="page"] {
  color: var(--p-accent);
  background: color-mix(in srgb, var(--p-accent) 10%, transparent);
}
.nav__link:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

.nav__icon {
  flex-shrink: 0;
  color: var(--p-muted);
}
.nav__link:hover .nav__icon,
.nav__link[aria-current="page"] .nav__icon {
  color: var(--p-accent);
}

.chev {
  width: 14px;
  height: 14px;
  flex-shrink: 0;
  color: var(--p-muted);
  transition: transform var(--speed) var(--ease);
}
.nav__item:hover .chev {
  transform: translateY(2px);
}

.nav__menu {
  position: absolute;
  top: calc(100% + var(--s-2));
  left: 0;
  min-width: 480px;
  padding: var(--s-4);
  background: var(--p-surface);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-3);
  opacity: 0;
  visibility: hidden;
  transform: translateY(-8px);
  transition: opacity var(--speed) var(--ease), transform var(--speed) var(--ease), visibility var(--speed);
  z-index: 100;
}
.nav__item:hover .nav__menu,
.nav__item:focus-within .nav__menu {
  opacity: 1;
  visibility: visible;
  transform: translateY(0);
}

.menu-col {
  display: flex;
  flex-direction: column;
  gap: var(--s-2);
}
.menu-col .t-label {
  font: var(--font-text);
  font-size: var(--t-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--p-muted);
  margin: 0;
}
.menu-link {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--s-2) 0;
  color: var(--p-ink);
  text-decoration: none;
  border-radius: var(--r-sm);
  transition: background var(--speed) var(--ease);
}
.menu-link:hover {
  background: var(--p-muted);
}
.menu-desc {
  font-size: var(--t-xs);
  color: var(--p-muted);
}

.menu-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  aspect-ratio: 1;
  padding: var(--s-6);
  text-decoration: none;
  border-radius: var(--r-lg);
  border: 1px solid var(--border);
  background: var(--p-surface);
  transition: transform var(--speed) var(--ease), box-shadow var(--speed) var(--ease), border-color var(--speed) var(--ease);
}
.menu-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-2);
  border-color: var(--p-accent);
}
.menu-card:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}
.menu-card-icon {
  color: var(--p-accent);
}
.t-card {
  font: var(--font-display);
  font-size: var(--t-sm);
  font-weight: 600;
  color: var(--p-ink);
  text-align: center;
}
.t-small.muted {
  font-size: var(--t-xs);
  color: var(--p-muted);
  text-align: center;
}

.nav__actions {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  margin-left: auto;
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  padding: var(--s-2) var(--s-4);
  font: var(--font-text);
  font-size: var(--t-sm);
  font-weight: 500;
  border-radius: var(--r-full);
  border: 1px solid transparent;
  cursor: pointer;
  transition: background var(--speed) var(--ease), border-color var(--speed) var(--ease), color var(--speed) var(--ease), transform var(--speed) var(--ease), box-shadow var(--speed) var(--ease);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

.btn--primary {
  background: var(--p-action);
  color: var(--p-action-ink);
  border-color: var(--p-action);
}
.btn--primary:hover:not(:disabled) {
  background: color-mix(in srgb, var(--p-action) 90%, black);
  border-color: color-mix(in srgb, var(--p-action) 90%, black);
}
.btn--primary:active:not(:disabled) {
  transform: scale(0.98);
}

.btn--ghost {
  background: transparent;
  color: var(--p-ink);
  border-color: transparent;
  padding: var(--s-2);
}
.btn--ghost:hover:not(:disabled) {
  background: var(--p-muted);
}
.btn--ghost:active:not(:disabled) {
  transform: scale(0.98);
}

.nav__theme-toggle,
.nav__user-toggle {
  width: 40px;
  height: 40px;
  border-radius: var(--r-full);
}
.nav__theme-toggle .icon,
.nav__user-toggle .icon {
  color: var(--p-ink);
}
.nav__theme-toggle:hover .icon,
.nav__user-toggle:hover .icon {
  color: var(--p-accent);
}

.nav__dropdown {
  min-width: 220px;
}
.nav__dropdown .menu-link {
  padding: var(--s-2) var(--s-3);
}
.nav__dropdown .border-t {
  border-color: var(--border);
}

.nav__toggle {
  display: none;
  width: 44px;
  height: 44px;
  align-items: center;
  justify-content: center;
  background: transparent;
  border: none;
  border-radius: var(--r-md);
  color: var(--p-ink);
  cursor: pointer;
}
.nav__toggle:hover {
  background: var(--p-muted);
}
.nav__toggle:focus-visible {
  outline: 2px solid var(--p-accent);
  outline-offset: 2px;
}

/* Mobile nav sheet (handled by system.js) */
@media (max-width: 1023px) {
  .nav__toggle { display: flex; }
  .nav__links { display: none; }
  .nav.is-open .nav__links {
    display: flex;
    flex-direction: column;
    position: fixed;
    top: var(--nav-h, 64px);
    left: 0;
    right: 0;
    bottom: 0;
    padding: var(--s-6);
    background: var(--p-surface);
    border-top: 1px solid var(--border);
    gap: var(--s-1);
    overflow-y: auto;
  }
  .nav__item { width: 100%; }
  .nav__link { width: 100%; padding: var(--s-3) var(--s-4); }
  .nav__menu {
    position: static;
    opacity: 1;
    visibility: visible;
    transform: none;
    box-shadow: none;
    border: none;
    background: transparent;
    padding: var(--s-2) 0 0 var(--s-4);
    margin-top: var(--s-2);
    border-top: 1px solid var(--border);
  }
  .nav__actions { width: 100%; justify-content: flex-end; padding-top: var(--s-4); border-top: 1px solid var(--border); }
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .nav__link,
  .btn,
  .menu-card,
  .nav__menu,
  .chev {
    transition: none !important;
  }
}
</style>