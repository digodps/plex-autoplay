<script setup>
import { onMounted, onUnmounted, ref, nextTick } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import AppIcon from "@/components/AppIcon.vue";
import { AUTH_COOKIE, deleteCookie } from "@/api";
import { useAuthStore } from "@/stores/auth";

const { t } = useI18n();
const authStore = useAuthStore();
const route = useRoute();
const router = useRouter();
const password = ref("");
const showPassword = ref(false);
const passwordRef = ref(null);
const error = ref("");
const loading = ref(false);
const formStatus = ref("");

const getRedirect = () =>
  typeof route.query.redirect === "string" ? route.query.redirect : "/dashboard";

onMounted(async () => {
  document.body.classList.add("ground-inverse");
  document.documentElement.dataset.ground = "painting";
  await nextTick();
  const { mountAll } = await import("/assets/moments/moments.js");
  await mountAll(document);
  if (!authStore.isAuthenticated) return;
  const response = await fetch("/api/status", {
    headers: { Authorization: `Bearer ${authStore.token}` },
  });
  if (response.ok) {
    await router.replace(getRedirect());
    return;
  }
  authStore.logout();
});

onUnmounted(() => {
  document.body.classList.remove("ground-inverse");
  document.documentElement.dataset.ground = "sand";
});

function togglePassword() {
  showPassword.value = !showPassword.value;
  passwordRef.value?.focus();
}

async function handleLogin() {
  error.value = "";
  formStatus.value = t("auth.signingIn");
  loading.value = true;
  deleteCookie(AUTH_COOKIE);

  try {
    const response = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ Password: password.value }),
    });

    if (response.ok) {
      const data = await response.json();
      authStore.login(data.token);
      formStatus.value = t("auth.signInSuccess");
      await router.push(getRedirect());
    } else {
      error.value = t("auth.loginError");
      formStatus.value = "";
    }
  } catch {
    error.value = t("errors.network");
    formStatus.value = "";
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <!-- Page root: direct header/main/footer per SR-6 using Teleport -->
  <Teleport to="body">
    <header class="nav" data-enter>
      <a href="#main-content" class="skip-link">Skip to content</a>
      <div class="wrap">
        <a class="nav__logo" href="/">
          <AppIcon name="plex" size="lg" style="color: var(--accent);" />
          <span>Plex Autoplay</span>
        </a>
      </div>
    </header>
  </Teleport>

  <Teleport to="body">
    <main id="main-content">
      <!-- Hero Section: Sign In - EXACTLY one screen (SR-1) -->
      <section class="hero signin-hero hero--split" data-enter data-moment="liquid-type" :data-moment-opts="{ strength: 0.8 }">
        <div class="wrap">
          <!-- Hero text with proper 12-column grid -->
          <div class="hero-text hero__grid">
            <div class="hero__head">
              <!-- Content spans first 7 columns -->
              <div>
                <!-- Headline with liquid-type moment -->
                <h1 class="t-h2 mo-lt__type">{{ t("auth.signInTitle") }}</h1>
                <p class="t-body">{{ t("auth.signInSubtitle") }}</p>
                <p class="t-label" style="color: var(--accent); margin-top: var(--s-3);">{{ t("auth.signInDetails") }}</p>

                <!-- Form Status - always present for screen readers -->
                <div role="status" aria-live="polite" class="form-status" :data-visible="!!formStatus">{{ formStatus }}</div>

                <!-- Error Alert -->
                <div v-if="error" class="alert alert--error" role="alert" data-testid="error">
                  <AppIcon name="triangle-alert" size="sm" style="color: var(--danger);" />
                  <span>{{ error }}</span>
                </div>

                <form data-testid="login-form" @submit.prevent="handleLogin">
                  <div class="field">
                    <label for="password" class="t-label">{{ t("auth.password") }}</label>
                    <div class="field__wrapper">
                      <input
                        id="password"
                        ref="passwordRef"
                        v-model="password"
                        :type="showPassword ? 'text' : 'password'"
                        :placeholder="t('auth.passwordPlaceholder')"
                        autocomplete="current-password"
                        class="field__input field__input--full"
                        required
                        data-testid="password-input"
                      />
                      <button
                        type="button"
                        class="field__toggle"
                        :aria-label="showPassword ? t('aria.collapse') : t('aria.expand')"
                        :aria-pressed="showPassword"
                        data-testid="toggle-password-signin"
                        @click="togglePassword"
                      >
                        <AppIcon :name="showPassword ? 'eye-off' : 'eye'" size="sm" />
                      </button>
                    </div>
                  </div>

                  <button
                    :disabled="loading"
                    type="submit"
                    data-testid="login-submit"
                    class="btn btn--primary btn-lg btn--no-chevron btn--full"
                    style="margin-top: var(--s-2);"
                  >
                    <AppIcon :name="loading ? 'spinner' : 'lock-open'" size="sm" :class="{ 'animate-spin': loading }" />
                    <span>{{ loading ? t("common.loading") : t("auth.signInBtn") }}</span>
                  </button>
                </form>
              </div>

              <!-- Button row in columns 9/-1 -->
              <div class="btn-row hero__btn-row" aria-hidden="true">
                <!-- Empty - button is in the form above -->
              </div>
            </div>
          </div>

          <!-- Hero Visual/Media - max 96px tall strip -->
          <div class="hero-visual hero__media" aria-hidden="true">
            <svg viewBox="0 0 400 96" fill="none" xmlns="http://www.w3.org/2000/svg" style="width: 100%; height: 96px; max-height: 96px;">
              <rect x="20" y="8" width="360" height="80" rx="8" stroke="#5ab9fc" stroke-width="2" />
              <rect x="100" y="16" width="200" height="64" rx="6" stroke="#5ab9fc" stroke-width="2" />
              <rect x="130" y="30" width="140" height="12" rx="4" fill="#5ab9fc" />
              <rect x="110" y="55" width="180" height="20" rx="4" stroke="#5ab9fc" stroke-width="2" />
              <circle cx="200" cy="65" r="15" stroke="#5ab9fc" stroke-width="2" />
              <path d="M200 55 L200 75 M190 65 L210 65" stroke="#5ab9fc" stroke-width="2" stroke-linecap="round" />
            </svg>
          </div>
        </div>
      </section>
    </main>
  </Teleport>

  <Teleport to="body">
    <!-- Footer Closing - exactly two children: .closing-call and .closing-side -->
    <footer class="closing">
      <div class="wrap closing__grid">
        <div class="closing-call">
          <p class="t-body">{{ t("auth.footerTagline") }}</p>
          <p class="t-body">{{ t("auth.footerSetupDesc") }}</p>
          <p class="t-body">{{ t("auth.footerTechStack") }}</p>
          <p class="t-body">{{ t("auth.footerAutoDetect") }}</p>
          <p class="t-body">{{ t("auth.footerNoCloud") }}</p>
          <p class="t-body">{{ t("auth.footerOpenSource") }}</p>
          <p class="t-body">{{ t("auth.footerMultiLib") }}</p>
          <p class="t-body">{{ t("auth.footerPreview") }}</p>
          <p class="t-body">{{ t("auth.footerML") }}</p>
          <p class="t-body">{{ t("auth.footerCrossPlatform") }}</p>
          <p class="t-body">{{ t("auth.footerScheduling") }}</p>
          <p class="t-body">{{ t("auth.footerPrivacy") }}</p>
          <p class="t-body">{{ t("auth.footerFree") }}</p>
          <p class="t-body">{{ t("auth.footerCommunity") }}</p>
          <p class="t-body">{{ t("auth.footerCompat") }}</p>
          <p class="t-body">{{ t("auth.footerNoAPI") }}</p>
          <p class="t-body">{{ t("auth.footerMarkerTypes") }}</p>
          <p class="t-body">{{ t("auth.footerBatch") }}</p>
          <p class="t-body">{{ t("auth.footerImportExport") }}</p>
        </div>
        <div class="closing-side">
          <nav class="closing-legal" aria-label="Footer navigation">
            <p class="t-label closing-nav__title">{{ t("auth.footerOpenSourceTitle") }}</p>
            <a href="https://github.com/digodps/plex-autoplay" class="closing-nav__link">{{ t("auth.footerGitHub") }}</a>
            <a href="https://github.com/digodps/plex-autoplay/issues" class="closing-nav__link">{{ t("auth.footerIssues") }}</a>
            <a href="https://github.com/digodps/plex-autoplay/discussions" class="closing-nav__link">{{ t("auth.footerDiscussions") }}</a>
          </nav>
          <!-- Visual element to fill empty area - more substantial with fills -->
          <div class="closing-visual" aria-hidden="true">
            <svg viewBox="0 0 400 300" fill="none" xmlns="http://www.w3.org/2000/svg" style="width: 100%; height: 100%;">
              <rect x="50" y="50" width="300" height="200" rx="12" fill="#5ab9fc" opacity="0.15" stroke="#5ab9fc" stroke-width="1.5" />
              <rect x="80" y="80" width="240" height="120" rx="8" fill="#5ab9fc" opacity="0.12" stroke="#5ab9fc" stroke-width="1" />
              <circle cx="200" cy="100" r="50" fill="#5ab9fc" opacity="0.2" stroke="#5ab9fc" stroke-width="1.5" />
              <circle cx="200" cy="100" r="30" fill="#5ab9fc" opacity="0.15" />
              <path d="M200 150 L200 200 M175 175 L225 175" stroke="#5ab9fc" stroke-width="2.5" stroke-linecap="round" opacity="0.8" />
              <rect x="100" y="220" width="200" height="10" rx="5" fill="#5ab9fc" opacity="0.5" />
              <rect x="100" y="240" width="160" height="10" rx="5" fill="#5ab9fc" opacity="0.4" />
              <rect x="100" y="260" width="120" height="10" rx="5" fill="#5ab9fc" opacity="0.3" />
              <rect x="100" y="280" width="80" height="10" rx="5" fill="#5ab9fc" opacity="0.2" />
              <circle cx="300" cy="80" r="35" fill="#5ab9fc" opacity="0.12" />
              <circle cx="350" cy="250" r="25" fill="#5ab9fc" opacity="0.1" />
              <rect x="50" y="280" width="25" height="25" rx="4" fill="#5ab9fc" opacity="0.18" />
              <rect x="330" y="50" width="25" height="25" rx="4" fill="#5ab9fc" opacity="0.12" />
              <ellipse cx="150" cy="50" rx="45" ry="25" fill="#5ab9fc" opacity="0.1" />
              <ellipse cx="250" cy="280" rx="35" ry="18" fill="#5ab9fc" opacity="0.08" />
              <rect x="70" y="70" width="60" height="4" rx="2" fill="#5ab9fc" opacity="0.3" />
              <rect x="70" y="80" width="40" height="4" rx="2" fill="#5ab9fc" opacity="0.25" />
              <rect x="270" y="180" width="50" height="4" rx="2" fill="#5ab9fc" opacity="0.2" />
              <rect x="270" y="190" width="30" height="4" rx="2" fill="#5ab9fc" opacity="0.15" />
            </svg>
          </div>
        </div>
      </div>
    </footer>
  </Teleport>
</template>

<style scoped>
/* Skip link - hidden until focused */
.skip-link {
  position: absolute;
  top: -100%;
  left: 50%;
  transform: translateX(-50%);
  padding: var(--s-2) var(--s-4);
  background: var(--action);
  color: var(--action-ink);
  border-radius: var(--r-md);
  z-index: 200;
  font-weight: 500;
  text-decoration: none;
  white-space: nowrap;
}
.skip-link:focus-visible {
  top: var(--s-4);
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

/* Nav - fixed, transparent, over hero */
.nav {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 100;
  background: color-mix(in srgb, var(--surface) 95%, transparent);
  backdrop-filter: saturate(180%) blur(20px);
  border-bottom: 1px solid var(--border);
  padding: var(--s-2) 0;
  height: 64px;
}
.nav__logo {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  font: var(--font-display);
  font-size: var(--t-xl);
  color: var(--ink);
  font-weight: 600;
  text-decoration: none;
}
.nav__logo:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 4px;
  border-radius: var(--r-md);
}

/* Hero - EXACTLY 100svh, starts at 0 (behind fixed nav) */
.signin-hero {
  height: 100svh;
  min-height: 500px;
  display: flex;
  flex-direction: column;
  position: relative;
  box-sizing: border-box;
}

/* Hero split: two direct children of wrap */
.signin-hero .wrap {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--s-8);
  align-items: center;
  height: 100%;
  padding-top: 64px; /* nav height - content starts below fixed nav */
  box-sizing: border-box;
}

/* Hero text with 12-column grid */
.hero-text {
  text-align: left;
  display: flex;
  flex-direction: column;
  justify-content: center;
  height: 100%;
  position: relative;
  z-index: 1;
}

.hero__head {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  column-gap: var(--gutter);
  row-gap: var(--s-5);
  align-items: end;
  height: 100%;
}

.hero__head > :first-child {
  grid-column: 1 / span 7;
}

.hero__btn-row {
  grid-column: 9 / -1;
  justify-content: flex-end;
}

.hero-visual {
  position: relative;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  height: 96px;
  max-height: 96px;
  z-index: 1;
}

.hero-visual svg {
  width: 100%;
  height: 96px;
  max-height: 96px;
}

/* Fields - full width within grid column */
.field {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
  width: 100%;
}

.field__input {
  width: 100%;
  padding: var(--s-3) var(--s-4);
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  background: color-mix(in oklab, var(--surface) 70%, var(--ink) 30%);
  border: 1px solid var(--border);
  border-radius: var(--r-md);
  transition: border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
  box-sizing: border-box;
}

.field__input:focus {
  outline: none;
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), 0 0 0 4px color-mix(in oklab, var(--action) 20%, transparent);
}

.field__input:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

/* field__input--full ensures full width within grid cell */
.field__input--full {
  width: 100%;
}

.field__wrapper {
  position: relative;
  display: flex;
  width: 100%;
}

.field__toggle {
  position: absolute;
  right: var(--s-3);
  top: 50%;
  transform: translateY(-50%);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  min-height: 44px;
  background: transparent;
  border: none;
  color: var(--ink);
  opacity: 0.5;
  cursor: pointer;
  transition: opacity var(--m-fast) var(--m-ease);
}

.field__toggle:hover {
  opacity: 1;
}

.field__toggle:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
  border-radius: var(--r-sm);
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  padding: var(--s-2) var(--s-5);
  font: var(--font-text);
  font-size: var(--t-sm);
  font-weight: 500;
  border-radius: var(--r-btn);
  border: 1px solid transparent;
  cursor: pointer;
  transition: transform var(--m-fast) var(--m-ease), background var(--m-fast) var(--m-ease), border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn--primary {
  background: var(--action);
  color: var(--action-ink);
  border-color: var(--action);
}

@media (hover: hover) and (pointer: fine) {
  .btn--primary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--action) 90%, black);
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
  }
  .btn--primary:active:not(:disabled) {
    transform: scale(0.98);
  }
}

.btn--primary:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn-lg {
  padding: var(--s-3) var(--s-6);
  font-size: var(--t-body);
}

.btn--no-chevron::after {
  content: none;
  display: none;
}

.btn--full {
  width: 100%;
}

/* Alerts */
.alert {
  display: flex;
  align-items: flex-start;
  gap: var(--s-3);
  padding: var(--s-3) var(--s-4);
  border-radius: var(--r-md);
  font-size: var(--t-body);
  line-height: 1.5;
}

.alert--error {
  background: color-mix(in oklab, var(--danger, #e03e3e) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger, #e03e3e) 25%, transparent);
  color: var(--ink);
}

.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* Form status - always present but visually hidden when empty */
.form-status {
  margin-bottom: var(--s-4);
  padding: var(--s-3) var(--s-4);
  background: color-mix(in oklab, var(--action) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--action) 25%, transparent);
  border-radius: var(--r-md);
  font-size: var(--t-small);
  color: var(--ink);
  min-height: 1.5em;
}
.form-status:not([data-visible="true"]) {
  opacity: 0;
  pointer-events: none;
  margin-bottom: 0;
  padding: 0;
  border: none;
  background: none;
  min-height: 0;
}

/* Footer Closing - exactly two children: .closing-call and .closing-side */
.closing {
  padding: var(--s-12) var(--s-4);
  border-top: 1px solid var(--border);
  min-height: 500px;
  overflow-x: clip;
}

.closing__grid {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  column-gap: var(--gutter);
  align-items: start;
  min-height: 400px;
  max-width: 1440px;
  margin: 0 auto;
  box-sizing: border-box;
}

.closing-call {
  grid-column: 1 / span 7;
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
  min-width: 280px;
}

.closing-call p {
  color: var(--ink);
  margin: 0;
}

.closing-side {
  grid-column: 8 / -1;
  display: flex;
  flex-direction: column;
  gap: var(--s-8);
  min-width: 200px;
  width: 100%;
  max-width: 100%;
  overflow: visible;
}

.closing-legal {
  display: flex;
  flex-direction: column;
  gap: var(--s-2);
  align-items: flex-start;
  min-width: 160px;
}

.closing-nav__title {
  color: var(--ink);
  margin: 0;
}

.closing-nav__link {
  color: var(--accent);
  text-decoration: none;
  font-size: var(--t-body);
  min-height: 44px;
  display: inline-flex;
  align-items: center;
}

.closing-nav__link:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
  border-radius: var(--r-sm);
}

.closing-visual {
  width: 100%;
  max-width: 300px;
  height: 300px;
  flex-shrink: 0;
  opacity: 0.15;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.closing-visual svg {
  width: 100%;
  height: 100%;
  max-width: 100%;
  max-height: 100%;
}

/* Responsive */
@media (max-width: 1024px) {
  .signin-hero .wrap {
    grid-template-columns: 1fr;
  }

  .hero-visual {
    display: none;
  }

  .hero-text {
    height: auto;
    padding-top: var(--s-8);
    padding-bottom: var(--s-8);
  }

  .hero__head {
    grid-template-columns: 1fr;
  }

  .hero__head > :first-child {
    grid-column: 1;
  }

  .hero__btn-row {
    grid-column: 1;
  }

  .closing__grid {
    grid-template-columns: 1fr;
  }

  .closing-call {
    grid-column: 1;
  }

  .closing-side {
    grid-column: 1;
    flex-direction: column;
    min-width: 0;
  }

  .closing-legal {
    align-items: flex-start;
    min-width: 0;
  }

  .closing-visual {
    display: none;
  }
}

@media (max-width: 640px) {
  .closing-nav__link {
    min-height: 44px;
  }
}
</style>