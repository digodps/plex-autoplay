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
const newPassword = ref("123456");
const confirmPassword = ref("123456");
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
  { id: 1, label: t("auth.newPassword") },
  { id: 2, label: t("auth.plexUrl") },
  { id: 3, label: t("libraries.libraryName") },
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
    if (newPassword.value.length < 8) {
      error.value = "A senha deve ter pelo menos 8 caracteres";
      return false;
    }
    if (newPassword.value !== confirmPassword.value) {
      error.value = "As senhas não conferem";
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
  <div class="flex h-screen items-center justify-center bg-whiten px-4 dark:bg-boxdark-2">
    <div class="w-full max-w-2xl">
      <DefaultAuthCard :title="t('app.name')" :subtitle="t('auth.setupTitle')">
        <ol class="mb-5 flex items-center gap-2 text-xs" data-testid="setup-steps">
          <li
            v-for="item in steps"
            :key="item.id"
            class="flex flex-1 items-center gap-2"
            :class="item.id === step ? 'font-semibold text-primary' : 'text-body dark:text-bodydark'"
          >
            <span
              class="flex h-6 w-6 shrink-0 items-center justify-center rounded-full border"
              :class="
                item.id === step
                  ? 'border-primary bg-primary text-white'
                  : 'border-stroke dark:border-strokedark'
              "
            >
              {{ item.id }}
            </span>
            <span :data-testid="`setup-step-${item.id}`">{{ item.label }}</span>
          </li>
        </ol>

        <div
          v-if="error"
          class="error alert alert-error mb-4 flex items-center gap-2 rounded-sm border border-meta-1/30 bg-meta-1/10 px-4 py-3 text-sm font-medium text-meta-1"
          role="alert"
          data-testid="error"
        >
          <AppIcon name="warning" size="sm" />
          {{ error }}
        </div>

        <ul
          v-if="errors.length"
          class="error alert alert-error mb-4 space-y-1 rounded-sm border border-meta-1/30 bg-meta-1/10 px-4 py-3 text-sm font-medium text-meta-1"
          role="alert"
        >
          <li v-for="(item, index) in errors" :key="index" data-testid="setup-error">
            {{ item }}
          </li>
        </ul>

        <!-- Passo 1: senha -->
        <form v-if="step === 1" data-testid="setup-panel-password" @submit.prevent="next">
          <div class="mb-4">
            <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-password">
              {{ t("auth.newPassword") }}
            </label>
            <div class="relative">
              <input
                id="setup-password"
                ref="setupPasswordRef"
                v-model="newPassword"
                :type="showSetupPassword ? 'text' : 'password'"
                autocomplete="new-password"
                minlength="8"
                maxlength="128"
                class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-12 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
                data-testid="setup-password"
                required
              />
              <button
                type="button"
                class="absolute right-4 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
                :aria-label="showSetupPassword ? t('aria.collapse') : t('aria.expand')"
                :aria-pressed="showSetupPassword"
                data-testid="toggle-password-setup-new"
                @click="toggleSetupPassword"
              >
                <AppIcon :name="showSetupPassword ? 'eye-off' : 'eye'" size="sm" />
              </button>
            </div>
          </div>

          <div class="mb-5">
            <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-confirm">
              {{ t("auth.confirmPassword") }}
            </label>
            <div class="relative">
              <input
                id="setup-confirm"
                ref="setupConfirmRef"
                v-model="confirmPassword"
                :type="showSetupConfirm ? 'text' : 'password'"
                autocomplete="new-password"
                minlength="8"
                maxlength="128"
                class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-12 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
                data-testid="setup-confirm"
                required
              />
              <button
                type="button"
                class="absolute right-4 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
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
            class="flex w-full cursor-pointer items-center justify-center gap-2 rounded-lg bg-primary p-4 font-medium text-white transition hover:bg-opacity-90"
            data-testid="setup-next"
          >
            {{ t("common.next") }}
            <AppIcon name="chevron-right" size="sm" />
          </button>
        </form>

        <!-- Passo 2: Plex -->
        <form v-else-if="step === 2" data-testid="setup-panel-plex" @submit.prevent="next">
          <div class="mb-4">
            <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-plex-url">
              {{ t("auth.plexUrl") }}
            </label>
            <input
              id="setup-plex-url"
              v-model="plexUrl"
              type="url"
              placeholder="http://192.168.1.5:32400"
              class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-5 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
              data-testid="setup-plex-url"
              required
            />
          </div>

          <div class="mb-4">
            <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-plex-token">
              {{ t("auth.plexToken") }}
            </label>
            <div class="relative">
              <input
                id="setup-plex-token"
                ref="setupPlexTokenRef"
                v-model="plexToken"
                :type="showSetupPlexToken ? 'text' : 'password'"
                autocomplete="off"
                class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-12 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
                data-testid="setup-plex-token"
                required
              />
              <button
                type="button"
                class="absolute right-4 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
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
            class="mb-4 w-full cursor-pointer rounded-lg border border-stroke p-3 font-medium text-black transition hover:bg-gray-2 disabled:opacity-60 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
            data-testid="setup-test-plex"
            :disabled="testing"
            @click="testPlex"
          >
            {{ testing ? t("common.loading") : t("auth.testConnection") }}
          </button>

          <p
            v-if="plexCheck"
            class="mb-4 text-sm"
            :class="plexCheck.ok ? 'text-meta-3' : 'text-meta-1'"
            data-testid="plex-test-result"
          >
            {{ plexCheck.message }}
          </p>

          <div class="flex gap-2">
            <button
              type="button"
              class="cursor-pointer rounded-lg border border-stroke px-4 py-4 font-medium text-black transition hover:bg-gray-2 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
              data-testid="setup-back"
              @click="back"
            >
              {{ t("common.back") }}
            </button>
            <button
              type="submit"
              class="flex flex-1 cursor-pointer items-center justify-center gap-2 rounded-lg bg-primary p-4 font-medium text-white transition hover:bg-opacity-90"
              data-testid="setup-next"
            >
              {{ t("common.next") }}
              <AppIcon name="chevron-right" size="sm" />
            </button>
          </div>
        </form>

        <!-- Passo 3: mídia -->
        <form v-else data-testid="setup-panel-media" @submit.prevent="finish">
          <div class="mb-5">
            <MediaRootsField v-model="mediaRoots" />
          </div>

          <div class="flex gap-2">
            <button
              type="button"
              class="cursor-pointer rounded-lg border border-stroke px-4 py-4 font-medium text-black transition hover:bg-gray-2 dark:border-strokedark dark:text-white dark:hover:bg-meta-4"
              data-testid="setup-back"
              :disabled="loading"
              @click="back"
            >
              {{ t("common.back") }}
            </button>
            <button
              :disabled="loading"
              type="submit"
              class="flex flex-1 cursor-pointer items-center justify-center gap-2 rounded-lg bg-primary p-4 font-medium text-white transition hover:bg-opacity-90 disabled:opacity-60"
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