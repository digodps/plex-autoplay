<script setup>
import { onMounted, onUnmounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { changePassword } from "@/api";
import AppIcon from "@/components/AppIcon.vue";

const { t } = useI18n();

const props = defineProps({
  isOpen: { type: Boolean, default: false },
});
const emit = defineEmits(["close"]);

const currentPassword = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const error = ref("");
const loading = ref(false);

const showCurrentPassword = ref(false);
const showNewPassword = ref(false);
const showConfirmPassword = ref(false);
const currentPasswordRef = ref(null);
const newPasswordRef = ref(null);
const confirmPasswordRef = ref(null);

function toggleCurrentPassword() {
  showCurrentPassword.value = !showCurrentPassword.value;
  currentPasswordRef.value?.focus();
}

function toggleNewPassword() {
  showNewPassword.value = !showNewPassword.value;
  newPasswordRef.value?.focus();
}

function toggleConfirmPassword() {
  showConfirmPassword.value = !showConfirmPassword.value;
  confirmPasswordRef.value?.focus();
}

function reset() {
  currentPassword.value = "";
  newPassword.value = "";
  confirmPassword.value = "";
  error.value = "";
}

function close() {
  reset();
  emit("close");
}

async function apply() {
  error.value = "";

  if (newPassword.value.length < 8) {
    error.value = t("auth.passwordMinLength");
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    error.value = t("auth.passwordsMismatch");
    return;
  }

  loading.value = true;
  try {
    const response = await changePassword(currentPassword.value, newPassword.value);
    if (response.status === 204) {
      close();
      return;
    }
    if (response.status === 403) {
      error.value = t("auth.currentPasswordIncorrect");
      return;
    }
    const data = await response.json().catch(() => ({}));
    error.value = data.message || t("errors.generic");
  } catch {
    error.value = t("errors.network");
  } finally {
    loading.value = false;
  }
}

function onKeydown(e) {
  if (e.key === "Escape" && props.isOpen) close();
}

onMounted(() => document.addEventListener("keydown", onKeydown));
onUnmounted(() => document.removeEventListener("keydown", onKeydown));
</script>

<template>
  <div
    v-if="props.isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    data-testid="password-modal"
    @click.self="close"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="password-title"
      class="w-full max-w-md rounded-lg border border-gray-300 bg-white p-6 shadow-lg dark:border-gray-700 dark:bg-gray-900"
    >
      <h2 id="password-title" class="mb-4 text-lg font-semibold text-black dark:text-white">
        {{ t("auth.changePassword") }}
      </h2>

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="password-current">
        {{ t("auth.currentPassword") }}
      </label>
      <div class="mb-3 relative">
        <input
          id="password-current"
          ref="currentPasswordRef"
          v-model="currentPassword"
          :type="showCurrentPassword ? 'text' : 'password'"
          autocomplete="current-password"
          class="mb-3 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white pr-12"
          data-testid="password-current"
          required
        />
        <button
          type="button"
          class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
          :aria-label="showCurrentPassword ? t('aria.collapse') : t('aria.expand')"
          :aria-pressed="showCurrentPassword"
          data-testid="toggle-password-current"
          @click="toggleCurrentPassword"
        >
          <AppIcon :name="showCurrentPassword ? 'eye-off' : 'eye'" size="sm" />
        </button>
      </div>

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="password-new">
        {{ t("auth.newPassword") }}
      </label>
      <div class="mb-3 relative">
        <input
          id="password-new"
          ref="newPasswordRef"
          v-model="newPassword"
          :type="showNewPassword ? 'text' : 'password'"
          autocomplete="new-password"
          minlength="8"
          maxlength="128"
          class="mb-3 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white pr-12"
          data-testid="password-new"
          required
        />
        <button
          type="button"
          class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
          :aria-label="showNewPassword ? t('aria.collapse') : t('aria.expand')"
          :aria-pressed="showNewPassword"
          data-testid="toggle-password-new"
          @click="toggleNewPassword"
        >
          <AppIcon :name="showNewPassword ? 'eye-off' : 'eye'" size="sm" />
        </button>
      </div>

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="password-confirm">
        {{ t("auth.confirmPassword") }}
      </label>
      <div class="mb-2 relative">
        <input
          id="password-confirm"
          ref="confirmPasswordRef"
          v-model="confirmPassword"
          :type="showConfirmPassword ? 'text' : 'password'"
          autocomplete="new-password"
          minlength="8"
          maxlength="128"
          class="mb-2 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white pr-12"
          data-testid="password-confirm"
          required
        />
        <button
          type="button"
          class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
          :aria-label="showConfirmPassword ? t('aria.collapse') : t('aria.expand')"
          :aria-pressed="showConfirmPassword"
          data-testid="toggle-password-confirm"
          @click="toggleConfirmPassword"
        >
          <AppIcon :name="showConfirmPassword ? 'eye-off' : 'eye'" size="sm" />
        </button>
      </div>

      <p v-if="error" class="mb-2 text-sm text-red" data-testid="password-error">{{ error }}</p>

      <div class="mt-4 flex justify-end gap-2">
        <button
          type="button"
          class="rounded-md px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800"
          data-testid="password-cancel"
          @click="close"
        >
          {{ t("common.cancel") }}
        </button>
        <button
          type="button"
          class="rounded-md bg-primary px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-60"
          :disabled="loading"
          data-testid="password-submit"
          @click="apply"
        >
          {{ loading ? t("common.loading") : t("auth.changePassword") }}
        </button>
      </div>
    </div>
  </div>
</template>