<script setup>
import { onMounted, onUnmounted, ref, nextTick } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import AppIcon from "@/components/AppIcon.vue";
import MediaRootsField from "@/components/MediaRootsField.vue";
import { apiFetch, extractErrors, submitSetup } from "@/api";
import { useAuthStore } from "@/stores/auth";

const { t } = useI18n();
const router = useRouter();
const authStore = useAuthStore();

onMounted(async () => {
  document.body.classList.add("ground-inverse");
  document.documentElement.dataset.ground = "painting";
  // Ensure moments.js runs after Teleport content is in DOM
  await nextTick();
  const { mountAll } = await import("/assets/moments/moments.js");
  await mountAll(document);
});
onUnmounted(() => {
  document.body.classList.remove("ground-inverse");
  document.documentElement.dataset.ground = "sand";
});

const step = ref(1);
const newPassword = ref("");
const confirmPassword = ref("");
const plexUrl = ref("");
const plexToken = ref("");
const mediaRoots = ref([]);
const error = ref("");
const errors = ref([]);
const loading = ref(false);
const testing = ref(false);
const plexCheck = ref(null);
const formStatus = ref("");

const showSetupPassword = ref(false);
const showSetupConfirm = ref(false);
const showSetupPlexToken = ref(false);
const setupPasswordRef = ref(null);
const setupConfirmRef = ref(null);
const setupPlexTokenRef = ref(null);

const steps = [
  { id: 1, label: t("auth.stepPassword") },
  { id: 2, label: t("auth.plexUrl") },
  { id: 3, label: t("libraries.libraryName") },
];

function clearMessages() {
  error.value = "";
  errors.value = [];
  formStatus.value = "";
}

function makeTogglePassword(showRef, inputRef) {
  return () => {
    showRef.value = !showRef.value;
    inputRef.value?.focus();
  };
}
const toggleSetupPassword = makeTogglePassword(showSetupPassword, setupPasswordRef);
const toggleSetupConfirm = makeTogglePassword(showSetupConfirm, setupConfirmRef);
const toggleSetupPlexToken = makeTogglePassword(showSetupPlexToken, setupPlexTokenRef);

function validateStep() {
  clearMessages();
  if (step.value === 1) {
    if (newPassword.value.length < 6) {
      error.value = t("auth.passwordMinLength");
      return false;
    }
    if (newPassword.value !== confirmPassword.value) {
      error.value = t("auth.passwordMismatch");
      return false;
    }
  }
  if (step.value === 2 && (!plexUrl.value.trim() || !plexToken.value.trim())) {
    error.value = t("auth.connectionFailed");
    return false;
  }
  if (step.value === 3 && mediaRoots.value.length === 0) {
    error.value = t("libraries.noLibraries");
    return false;
  }
  return true;
}

function next() {
  if (validateStep()) {
    step.value = Math.min(3, step.value + 1);
  }
}

function back() {
  clearMessages();
  step.value = Math.max(1, step.value - 1);
}

async function testPlex() {
  clearMessages();
  plexCheck.value = null;
  if (!plexUrl.value.trim() || !plexToken.value.trim()) {
    error.value = t("auth.connectionFailed");
    return;
  }
  testing.value = true;
  try {
    const response = await apiFetch("/api/plex/identity", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        plexUrl: plexUrl.value.trim(),
        plexToken: plexToken.value.trim(),
      }),
    });
    if (!response.ok) {
      error.value = t("auth.connectionFailed");
      return;
    }
    const data = await response.json();
    plexCheck.value = {
      ok: !!data.success,
      message: data.success
        ? `${t("auth.connectionSuccess")} ${data.friendlyName || "Plex"} (${data.version || "?"})`
        : data.message || t("auth.connectionFailed"),
    };
  } catch {
    error.value = t("errors.network");
  } finally {
    testing.value = false;
  }
}

async function finish() {
  if (!validateStep()) return;
  loading.value = true;
  formStatus.value = t("auth.settingUp");
  try {
    const response = await submitSetup(
      newPassword.value,
      plexUrl.value.trim(),
      plexToken.value.trim(),
      mediaRoots.value,
    );
    if (response.status === 204) {
      formStatus.value = t("auth.setupComplete");
      await loginAfterSetup();
      return;
    }
    if (response.status === 409) {
      await router.push({ name: "login" });
      return;
    }
    if (response.status === 401) {
      step.value = 1;
      error.value = t("auth.loginError");
      return;
    }
    if (response.status === 400) {
      const messages = await extractErrors(response);
      if (messages.length) {
        errors.value = messages;
        return;
      }
    }
    const data = await response.json().catch(() => ({}));
    error.value = data.message || t("errors.generic");
  } catch {
    error.value = t("errors.network");
  } finally {
    loading.value = false;
    formStatus.value = "";
  }
}

async function loginAfterSetup() {
  const response = await fetch("/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ password: newPassword.value }),
  });
  if (!response.ok) {
    await router.push({ name: "login" });
    return;
  }
  const data = await response.json();
  authStore.login(data.token);
  await router.push({ name: "dashboard" });
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
      <!-- Hero Section: Intro/Credit Marker Setup - EXACTLY one screen (SR-1) -->
      <section class="hero setup-hero hero--split" data-enter data-moment="liquid-type" :data-moment-opts="{ strength: 0.8 }">
        <div class="wrap">
          <!-- Hero text with proper 12-column grid -->
          <div class="hero-text hero__grid">
            <div class="hero__head">
              <!-- Content spans first 7 columns -->
              <div>
                <!-- Progress Steps -->
                <ol class="setup-steps" data-testid="setup-steps">
                  <li
                    v-for="item in steps"
                    :key="item.id"
                    :class="{ 'is-active': item.id === step }"
                  >
                    <span class="step-indicator" :class="{ active: item.id === step }">
                      {{ item.id }}
                    </span>
                    <span :data-testid="`setup-step-${item.id}`" class="step-label">{{ item.label }}</span>
                  </li>
                </ol>

                <!-- Headline with liquid-type moment -->
                <h1 class="t-h2 mo-lt__type">Intro/Credit Marker Setup</h1>
                <p class="t-body">Create your admin account and connect to Plex</p>
                <p class="t-label" style="color: var(--accent); margin-top: var(--s-3);">6 min setup · 4 platforms · Linux, Windows, macOS, Docker</p>

                <!-- Step 1: Create Account -->
                <form v-if="step === 1" data-testid="setup-panel-password" @submit.prevent="next">
                  <!-- Form Status - always present for screen readers -->
                  <div role="status" aria-live="polite" class="form-status" :data-visible="!!formStatus">{{ formStatus }}</div>

                  <div v-if="error" class="alert alert--error" role="alert" data-testid="error">
                    <AppIcon name="triangle-alert" size="sm" style="color: var(--danger);" />
                    <span>{{ error }}</span>
                  </div>

                  <ul v-if="errors.length" class="alert alert--error" role="alert">
                    <li v-for="(item, index) in errors" :key="index" data-testid="setup-error">{{ item }}</li>
                  </ul>

                  <div class="field">
                    <label for="setup-password" class="t-label">{{ t("auth.password") }}</label>
                    <div class="field__wrapper">
                      <input
                        id="setup-password"
                        ref="setupPasswordRef"
                        v-model="newPassword"
                        :type="showSetupPassword ? 'text' : 'password'"
                        autocomplete="new-password"
                        minlength="6"
                        maxlength="128"
                        class="field__input field__input--full"
                        data-testid="setup-password"
                        required
                      />
                      <button
                        type="button"
                        class="field__toggle"
                        :aria-label="showSetupPassword ? t('aria.collapse') : t('aria.expand')"
                        :aria-pressed="showSetupPassword"
                        data-testid="toggle-password-setup-new"
                        @click="toggleSetupPassword"
                      >
                        <AppIcon :name="showSetupPassword ? 'eye-off' : 'eye'" size="sm" />
                      </button>
                    </div>
                  </div>

                  <div class="field">
                    <label for="setup-confirm" class="t-label">{{ t("auth.confirmPassword") }}</label>
                    <div class="field__wrapper">
                      <input
                        id="setup-confirm"
                        ref="setupConfirmRef"
                        v-model="confirmPassword"
                        :type="showSetupConfirm ? 'text' : 'password'"
                        autocomplete="new-password"
                        minlength="6"
                        maxlength="128"
                        class="field__input field__input--full"
                        data-testid="setup-confirm"
                        required
                      />
                      <button
                        type="button"
                        class="field__toggle"
                        :aria-label="showSetupConfirm ? t('aria.collapse') : t('aria.expand')"
                        :aria-pressed="showSetupConfirm"
                        data-testid="toggle-password-setup-confirm"
                        @click="toggleSetupConfirm"
                      >
                        <AppIcon :name="showSetupConfirm ? 'eye-off' : 'eye'" size="sm" />
                      </button>
                    </div>
                  </div>

                  <button
                    type="submit"
                    class="btn btn--primary btn-lg btn--no-chevron btn--full"
                    data-testid="setup-next"
                  >
                    <span>Create account</span>
                  </button>
                </form>

                <!-- Step 2: Connect Plex -->
                <form v-else-if="step === 2" data-testid="setup-panel-plex" @submit.prevent="next">
                  <div role="status" aria-live="polite" class="form-status" :data-visible="!!formStatus">{{ formStatus }}</div>

                  <div v-if="error" class="alert alert--error" role="alert" data-testid="error">
                    <AppIcon name="triangle-alert" size="sm" style="color: var(--danger);" />
                    <span>{{ error }}</span>
                  </div>

                  <ul v-if="errors.length" class="alert alert--error" role="alert">
                    <li v-for="(item, index) in errors" :key="index" data-testid="setup-error">{{ item }}</li>
                  </ul>

                  <div class="field">
                    <label for="setup-plex-url" class="t-label">{{ t("auth.plexUrlLabel") }}</label>
                    <input
                      id="setup-plex-url"
                      v-model="plexUrl"
                      type="url"
                      placeholder="http://192.168.1.5:32400"
                      class="field__input field__input--full"
                      data-testid="setup-plex-url"
                      required
                    />
                  </div>

                  <div class="field">
                    <label for="setup-plex-token" class="t-label">{{ t("auth.plexTokenLabel") }}</label>
                    <div class="field__wrapper">
                      <input
                        id="setup-plex-token"
                        ref="setupPlexTokenRef"
                        v-model="plexToken"
                        :type="showSetupPlexToken ? 'text' : 'password'"
                        autocomplete="off"
                        class="field__input field__input--full"
                        data-testid="setup-plex-token"
                        required
                      />
                      <button
                        type="button"
                        class="field__toggle"
                        :aria-label="showSetupPlexToken ? t('aria.collapse') : t('aria.expand')"
                        :aria-pressed="showSetupPlexToken"
                        data-testid="toggle-password-setup-plex-token"
                        @click="toggleSetupPlexToken"
                      >
                        <AppIcon :name="showSetupPlexToken ? 'eye-off' : 'eye'" size="sm" />
                      </button>
                    </div>
                  </div>

                  <button
                    type="button"
                    class="btn btn--secondary btn-lg btn--full"
                    data-testid="setup-test-plex"
                    :disabled="testing"
                    @click="testPlex"
                  >
                    {{ testing ? t("common.loading") : t("auth.testConnection") }}
                  </button>

                  <p
                    v-if="plexCheck"
                    :class="['t-body', plexCheck.ok ? 'text-success' : 'text-danger']"
                    data-testid="plex-test-result"
                  >
                    {{ plexCheck.message }}
                  </p>

                  <div class="btn-row" style="margin-top: var(--s-2);">
                    <button
                      type="button"
                      class="btn btn--secondary"
                      data-testid="setup-back"
                      @click="back"
                    >
                      {{ t("common.back") }}
                    </button>
                    <button
                      type="submit"
                      class="btn btn--primary btn-lg btn--no-chevron btn--full"
                      data-testid="setup-next"
                    >
                      <span>Connect Plex</span>
                    </button>
                  </div>
                </form>

                <!-- Step 3: Select Media -->
                <form v-else data-testid="setup-panel-media" @submit.prevent="finish">
                  <div role="status" aria-live="polite" class="form-status" :data-visible="!!formStatus">{{ formStatus }}</div>

                  <div v-if="error" class="alert alert--error" role="alert" data-testid="error">
                    <AppIcon name="triangle-alert" size="sm" style="color: var(--danger);" />
                    <span>{{ error }}</span>
                  </div>

                  <ul v-if="errors.length" class="alert alert--error" role="alert">
                    <li v-for="(item, index) in errors" :key="index" data-testid="setup-error">{{ item }}</li>
                  </ul>

                  <div>
                    <MediaRootsField v-model="mediaRoots" />
                  </div>

                  <div class="btn-row">
                    <button
                      type="button"
                      class="btn btn--secondary"
                      data-testid="setup-back"
                      :disabled="loading"
                      @click="back"
                    >
                      {{ t("common.back") }}
                    </button>
                    <button
                      :disabled="loading"
                      type="submit"
                      class="btn btn--primary btn-lg btn--no-chevron btn--full"
                      data-testid="setup-submit"
                    >
                      <AppIcon :name="loading ? 'spinner' : 'check'" size="sm" :class="{ 'animate-spin': loading }" />
                      {{ loading ? t("common.saving") : t("auth.finishSetup") }}
                    </button>
                  </div>
                </form>
              </div>

              <!-- Button row in columns 9/-1 -->
              <div class="btn-row hero__btn-row" aria-hidden="true">
                <!-- Empty - buttons are in the forms above -->
              </div>
            </div>
          </div>

          <!-- Hero Visual/Media - max 96px tall strip -->
          <div class="hero-visual hero__media" aria-hidden="true">
            <svg viewBox="0 0 400 96" fill="none" xmlns="http://www.w3.org/2000/svg" style="width: 100%; height: 96px; max-height: 96px;">
              <rect x="20" y="8" width="360" height="80" rx="8" stroke="#5ab9fc" stroke-width="2" />
              <circle cx="80" cy="48" r="20" stroke="#5ab9fc" stroke-width="2" />
              <path d="M80 68 L80 100" stroke="#5ab9fc" stroke-width="2" stroke-linecap="round" />
              <circle cx="80" cy="108" r="5" fill="#5ab9fc" />
              <rect x="200" y="28" width="160" height="6" rx="3" fill="#5ab9fc" opacity="0.3" />
              <rect x="200" y="43" width="120" height="6" rx="3" fill="#5ab9fc" opacity="0.3" />
              <rect x="200" y="58" width="80" height="6" rx="3" fill="#5ab9fc" opacity="0.3" />
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
          <p class="t-body">Plex Autoplay — Intro/Credit Marker Manager for Plex</p>
          <p class="t-body">Set up your Plex server to automatically skip intros and credits</p>
          <p class="t-body">Built with Vue 3, TypeScript, and .NET 9</p>
          <p class="t-body">Automatically detects and marks intros, recaps, and credits in your media library</p>
          <p class="t-body">No cloud dependencies — your media, your server, your rules</p>
          <p class="t-body">Open source under MIT license with active community support</p>
          <p class="t-body">Supports multiple Plex libraries with granular marker control</p>
          <p class="t-body">Real-time marker preview with waveform visualization</p>
          <p class="t-body">Frame-perfect intro/credit detection with ML-powered analysis</p>
          <p class="t-body">Cross-platform: Linux, Windows, macOS, Docker</p>
          <p class="t-body">Automatic library scanning with configurable schedules</p>
          <p class="t-body">Privacy-first: no telemetry, no external connections</p>
          <p class="t-body">Free forever — no subscriptions, no limits</p>
          <p class="t-body">Community-driven development with transparent roadmap</p>
          <p class="t-body">Works with any Plex server version 1.20+</p>
          <p class="t-body">No external API keys required — fully self-contained</p>
          <p class="t-body">Configurable marker types: intro, recap, credits, custom</p>
          <p class="t-body">Batch processing with progress tracking and logs</p>
          <p class="t-body">Export/import marker configurations between servers</p>
        </div>
        <div class="closing-side">
          <nav class="closing-legal" aria-label="Footer navigation">
            <p class="t-label closing-nav__title">Open source project</p>
            <a href="https://github.com/digodps/plex-autoplay" class="closing-nav__link">GitHub Repository</a>
            <a href="https://github.com/digodps/plex-autoplay/issues" class="closing-nav__link">Report an Issue</a>
            <a href="https://github.com/digodps/plex-autoplay/discussions" class="closing-nav__link">Discussions</a>
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
.setup-hero {
  height: 100svh;
  min-height: 600px;
  display: flex;
  flex-direction: column;
  position: relative;
  box-sizing: border-box;
}

/* Hero split: two direct children of wrap */
.setup-hero .wrap {
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

.setup-steps {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  margin-bottom: var(--s-6);
  padding-left: 0;
  list-style: none;
}

.setup-steps li {
  display: flex;
  align-items: center;
  gap: var(--s-2);
}

.step-indicator {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  flex-shrink: 0;
  border-radius: 50%;
  border: 2px solid var(--ink);
  color: var(--ink);
  font-weight: 600;
  font-size: var(--t-small);
  opacity: 0.6;
  transition: all var(--m-fast) var(--m-ease);
}

.step-indicator.active {
  border-color: var(--action);
  background: var(--action);
  color: var(--action-ink);
  opacity: 1;
}

.step-label {
  color: var(--ink);
  font-weight: 500;
  text-wrap: pretty;
}

/* Hero visual - max 96px tall strip */
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
.btn-row {
  display: flex;
  gap: var(--s-3);
  width: 100%;
}

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

.btn--secondary {
  background: var(--surface);
  color: var(--ink);
  border-color: var(--border);
}

@media (hover: hover) and (pointer: fine) {
  .btn--secondary:hover:not(:disabled) {
    background: var(--bg);
    border-color: color-mix(in oklab, var(--ink) 16%, transparent);
    transform: translateY(-2px);
  }
  .btn--secondary:active:not(:disabled) {
    transform: scale(0.98);
  }
}

.btn--secondary:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn-lg {
  padding: var(--s-3) var(--s-6);
  font-size: var(--t-body);
}

.btn-sm {
  padding: var(--s-1) var(--s-3);
  font-size: var(--t-small);
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

.alert--info {
  background: color-mix(in oklab, var(--action) 8%, transparent);
  border: 1px solid color-mix(in oklab, var(--action) 25%, transparent);
  color: var(--ink);
}

.text-success {
  color: var(--success, #38a169);
}

.text-danger {
  color: var(--danger, #e03e3e);
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
  .setup-hero .wrap {
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

/* Orphan prevention */
.step-label {
  text-wrap: pretty;
}
</style>