<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "../stores/auth";
import AppIcon from "@/components/AppIcon.vue";
import DefaultAuthCard from "@/components/Auths/DefaultAuthCard.vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();
const authStore = useAuthStore();
const route = useRoute();
const router = useRouter();

const password = ref("");
const showPassword = ref(false);
const error = ref("");
const loading = ref(false);

const getRedirect = () =>
  typeof route.query.redirect === "string" ? route.query.redirect : "/dashboard";

onMounted(async () => {
  if (!authStore.isAuthenticated) return;

  const response = await fetch("/api/status", { credentials: "include" });
  if (response.ok) {
    await router.replace(getRedirect());
    return;
  }
  authStore.logout();
});

const handleLogin = async () => {
  error.value = "";
  loading.value = true;

  try {
    const response = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify({ password: password.value }),
    });

    if (response.ok) {
      const data = await response.json();
      authStore.login(data.token);
      await router.push(getRedirect());
    } else {
      error.value = t("auth.invalidPassword");
    }
  } catch {
    error.value = t("auth.serverError");
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4 bg-surface dark:bg-surfacedark">
    <DefaultAuthCard :title="t('app.name')" :subtitle="t('auth.adminPanel')">
      <template #default>
        <div v-if="error" class="mb-4 rounded-lg bg-danger/10 border border-danger/30 p-3 text-sm text-danger flex items-center gap-2" role="alert" aria-live="assertive">
          <AppIcon name="warning" size="sm" class="flex-shrink-0" />
          <span>{{ error }}</span>
        </div>

        <form @submit.prevent="handleLogin" class="space-y-4" novalidate>
          <div>
            <label for="password" class="block mb-1.5 text-sm font-medium text-body dark:text-bodydark">
              {{ t("auth.password") }}
            </label>
            <div class="relative">
              <input
                id="password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                :placeholder="t('auth.passwordPlaceholder')"
                class="input w-full"
                autocomplete="current-password"
                aria-describedby="password-toggle"
                data-testid="password-input"
              />
              <button
                type="button"
                id="password-toggle"
                @click="showPassword = !showPassword"
                class="absolute inset-y-0 right-0 pr-3 flex items-center"
                :aria-label="showPassword ? t('auth.hidePassword') : t('auth.showPassword')"
                :aria-pressed="showPassword"
                data-testid="password-toggle"
              >
                <AppIcon :name="showPassword ? 'eye-off' : 'eye'" size="sm" class="text-bodydark dark:text-body" />
              </button>
            </div>
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="btn btn-primary w-full py-2.5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
            data-testid="login-submit"
          >
            <AppIcon v-if="loading" name="spinner" size="sm" class="spin" aria-hidden="true" />
            <AppIcon v-else name="check" size="sm" aria-hidden="true" />
            <span>{{ loading ? t("auth.signingIn") : t("auth.signIn") }}</span>
          </button>
        </form>
      </template>
    </DefaultAuthCard>
  </div>
</template>

<style scoped>
.spin {
  animation: rotate 0.9s linear infinite;
}
@keyframes rotate {
  to { transform: rotate(360deg); }
}
</style>