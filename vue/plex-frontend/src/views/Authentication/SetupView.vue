<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useI18n } from "vue-i18n";
import DefaultAuthCard from "@/components/Auths/DefaultAuthCard.vue";
import AppIcon from "@/components/AppIcon.vue";
import MediaRootsField from "@/components/MediaRootsField.vue";
import { apiFetch, extractErrors, submitSetup } from "@/api";
import { useAuthStore } from "@/stores/auth";

const { t } = useI18n();
const router = useRouter();
const authStore = useAuthStore();

const step = ref(1);
const token = ref("");
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

const showSetupPassword = ref(false);
const showSetupConfirm = ref(false);
const showSetupPlexToken = ref(false);
const setupPasswordRef = ref(null);
const setupConfirmRef = ref(null);
const setupPlexTokenRef = ref(null);

const steps = [
  { id: 1, label: t("auth.stepPassword") },
  { id: 2, label: t("auth.stepPlex") },
  { id: 3, label: t("auth.stepMedia") },
];

function clearMessages() {
  error.value = "";
  errors.value = [];
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
    if (!token.value.trim()) {
      error.value = t("auth.loginError");
      return false;
    }
    if (newPassword.value.length < 8) {
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
  try {
    const response = await submitSetup(
      token.value.trim(),
      newPassword.value,
      plexUrl.value.trim(),
      plexToken.value.trim(),
      mediaRoots.value,
    );
    if (response.status === 204) {
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
  <div class="setup-view" style="min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: var(--s-4); background: var(--ground-bg);">
    <div style="width: 100%; max-width: 640px;">
      <DefaultAuthCard :title="t('app.name')" :subtitle="t('auth.setupTitle')">
        <!-- Progress Steps -->
        <ol class="setup-steps" data-testid="setup-steps" style="display: flex; align-items: center; gap: var(--s-2); margin-bottom: var(--s-5);">
          <li
            v-for="item in steps"
            :key="item.id"
            style="flex: 1; display: flex; align-items: center; gap: var(--s-2);"
            :class="{ 'is-active': item.id === step }"
          >
            <span
              class="step-indicator"
              style="display: inline-flex; align-items: center; justify-content: center; width: 32px; height: 32px; flex-shrink: 0; border-radius: 50%; border: 2px solid; font-weight: 600; font-size: var(--t-small);"
              :style="item.id === step ? 'border-color: var(--action); background: var(--action); color: var(--action-ink);' : 'border-color: var(--border); color: var(--muted);'"
            >
              {{ item.id }}
            </span>
            <span :data-testid="`setup-step-${item.id}`" class="t-small" :style="item.id === step ? 'font-weight: 600; color: var(--ink);' : 'color: var(--muted);'">{{ item.label }}</span>
          </li>
        </ol>

        <!-- Error Alert -->
        <div v-if="error" class="alert alert--error" role="alert" data-testid="error" style="margin-bottom: var(--s-4); display: flex; align-items: flex-start; gap: var(--s-3);">
          <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon" width="20" height="20" aria-hidden="true" style="flex-shrink: 0; margin-top: 2px;" />
          <span>{{ error }}</span>
        </div>

        <!-- Multiple Errors -->
        <ul v-if="errors.length" class="alert alert--error" role="alert" style="margin-bottom: var(--s-4); padding: var(--s-3) var(--s-4); display: flex; flex-direction: column; gap: var(--s-1);">
          <li v-for="(item, index) in errors" :key="index" data-testid="setup-error">{{ item }}</li>
        </ul>

        <!-- Step 1: Password -->
        <form v-if="step === 1" data-testid="setup-panel-password" style="display: flex; flex-direction: column; gap: var(--s-4);" @submit.prevent="next">
          <div class="alert alert--info" data-testid="setup-token-hint" style="display: flex; align-items: flex-start; gap: var(--s-3); background: color-mix(in oklab, var(--action) 8%, transparent); border-color: color-mix(in oklab, var(--action) 25%, transparent);">
            <img src="/assets/icons/ui-info.svg" data-icon="ui-info" class="icon" width="20" height="20" aria-hidden="true" style="flex-shrink: 0; margin-top: 2px;" />
            <div>
              <p style="margin: 0; font-size: var(--t-small);">O token de setup é de <strong>uso único</strong> e está no log do container.</p>
              <p style="margin: var(--s-1) 0 0; font-size: var(--t-small);">Execute <code class="mono" style="background: color-mix(in oklab, var(--ink) 8%, transparent); padding: 2px 6px; border-radius: var(--r-sm);">docker logs plex-autoplay</code> para obtê-lo.</p>
            </div>
          </div>

          <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
            <label for="setup-token" class="t-label">Token de Setup</label>
            <input
              id="setup-token"
              v-model="token"
              type="text"
              autocomplete="one-time-code"
              :placeholder="t('auth.tokenPlaceholder')"
              class="field__input"
              data-testid="setup-token"
              required
            />
          </div>

          <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
            <label for="setup-password" class="t-label">Nova Senha</label>
            <div class="field__wrapper" style="position: relative; display: flex;">
              <input
                id="setup-password"
                ref="setupPasswordRef"
                v-model="newPassword"
                :type="showSetupPassword ? 'text' : 'password'"
                autocomplete="new-password"
                minlength="8"
                maxlength="128"
                class="field__input"
                style="flex: 1; padding-right: 48px;"
                data-testid="setup-password"
                required
              />
              <button
                type="button"
                class="field__toggle"
                style="position: absolute; right: var(--s-3); top: 50%; transform: translateY(-50%);"
                :aria-label="showSetupPassword ? t('aria.collapse') : t('aria.expand')"
                :aria-pressed="showSetupPassword"
                data-testid="toggle-password-setup-new"
                @click="toggleSetupPassword"
              >
                <AppIcon :name="showSetupPassword ? 'eye-off' : 'eye'" size="sm" />
              </button>
            </div>
          </div>

          <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
            <label for="setup-confirm" class="t-label">Confirmar Senha</label>
            <div class="field__wrapper" style="position: relative; display: flex;">
              <input
                id="setup-confirm"
                ref="setupConfirmRef"
                v-model="confirmPassword"
                :type="showSetupConfirm ? 'text' : 'password'"
                autocomplete="new-password"
                minlength="8"
                maxlength="128"
                class="field__input"
                style="flex: 1; padding-right: 48px;"
                data-testid="setup-confirm"
                required
              />
              <button
                type="button"
                class="field__toggle"
                style="position: absolute; right: var(--s-3); top: 50%; transform: translateY(-50%);"
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
            class="btn btn--primary btn-lg"
            style="width: 100%; margin-top: var(--s-2);"
            data-testid="setup-next"
          >
            <span>{{ t("common.next") }}</span>
            <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="18" height="18" aria-hidden="true" />
          </button>
        </form>

        <!-- Step 2: Plex -->
        <form v-else-if="step === 2" data-testid="setup-panel-plex" style="display: flex; flex-direction: column; gap: var(--s-4);" @submit.prevent="next">
          <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
            <label for="setup-plex-url" class="t-label">URL do Plex</label>
            <input
              id="setup-plex-url"
              v-model="plexUrl"
              type="url"
              placeholder="http://192.168.1.5:32400"
              class="field__input"
              data-testid="setup-plex-url"
              required
            />
          </div>

          <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
            <label for="setup-plex-token" class="t-label">Token do Plex</label>
            <div class="field__wrapper" style="position: relative; display: flex;">
              <input
                id="setup-plex-token"
                ref="setupPlexTokenRef"
                v-model="plexToken"
                :type="showSetupPlexToken ? 'text' : 'password'"
                autocomplete="off"
                class="field__input"
                style="flex: 1; padding-right: 48px;"
                data-testid="setup-plex-token"
                required
              />
              <button
                type="button"
                class="field__toggle"
                style="position: absolute; right: var(--s-3); top: 50%; transform: translateY(-50%);"
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
            class="btn btn--secondary btn-lg"
            style="width: 100%;"
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
            style="margin: 0;"
          >
            {{ plexCheck.message }}
          </p>

          <div class="btn-row" style="display: flex; gap: var(--s-3); margin-top: var(--s-2);">
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
              class="btn btn--primary btn-lg"
              style="flex: 1;"
              data-testid="setup-next"
            >
              <span>{{ t("common.next") }}</span>
              <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="18" height="18" aria-hidden="true" />
            </button>
          </div>
        </form>

        <!-- Step 3: Media -->
        <form v-else data-testid="setup-panel-media" style="display: flex; flex-direction: column; gap: var(--s-4);" @submit.prevent="finish">
          <div>
            <MediaRootsField v-model="mediaRoots" />
          </div>

          <div class="btn-row" style="display: flex; gap: var(--s-3);">
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
              class="btn btn--primary btn-lg"
              style="flex: 1;"
              data-testid="setup-submit"
            >
              <AppIcon :name="loading ? 'spinner' : 'check'" size="sm" :class="{ 'animate-spin': loading }" />
              {{ loading ? t("common.loading") : t("common.save") }}
            </button>
          </div>
        </form>
      </DefaultAuthCard>
    </div>
  </div>
</template>

<style scoped>
.setup-view {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--s-4);
  background: var(--ground-bg);
}

.setup-steps {
  display: flex;
  align-items: center;
  gap: var(--s-2);
  margin-bottom: var(--s-5);
}

.setup-steps li {
  flex: 1;
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
  border: 2px solid;
  font-weight: 600;
  font-size: var(--t-small);
  transition: all var(--m-fast) var(--m-ease);
}

.setup-steps li.is-active .step-indicator {
  border-color: var(--action);
  background: var(--action);
  color: var(--action-ink);
}

.setup-steps li:not(.is-active) .step-indicator {
  border-color: var(--border);
  color: var(--muted);
}

.field {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
}

.field__input {
  width: 100%;
  padding: var(--s-3) var(--s-4);
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--r-md);
  transition: border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

.field__input:focus {
  outline: none;
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), 0 0 0 4px color-mix(in oklab, var(--action) 20%, transparent);
}

.field__input.mono {
  font-family: var(--font-mono);
}

.field__wrapper {
  position: relative;
  display: flex;
}

.field__toggle {
  position: absolute;
  right: var(--s-3);
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--s-1);
  background: transparent;
  border: none;
  color: var(--muted);
  cursor: pointer;
  transition: color var(--m-fast) var(--m-ease);
}

.field__toggle:hover {
  color: var(--ink);
}

.field__toggle:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
  border-radius: var(--r-sm);
}

.btn-row {
  display: flex;
  gap: var(--s-3);
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
    background: var(--card);
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

.alert .icon {
  flex-shrink: 0;
  margin-top: 2px;
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

.mono {
  font-family: var(--font-mono);
}
</style>