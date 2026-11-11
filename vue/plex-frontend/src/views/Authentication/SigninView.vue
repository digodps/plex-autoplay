<script setup>
import { onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import DefaultAuthCard from "@/components/Auths/DefaultAuthCard.vue";
import AppIcon from "@/components/AppIcon.vue";
import { AUTH_COOKIE, deleteCookie } from "@/api";
import { useAuthStore } from "@/stores/auth";

const authStore = useAuthStore();
const route = useRoute();
const router = useRouter();
const password = ref("");
const showPassword = ref(false);
const passwordRef = ref(null);
const error = ref("");
const loading = ref(false);

const getRedirect = () =>
  typeof route.query.redirect === "string" ? route.query.redirect : "/dashboard";

onMounted(async () => {
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

function togglePassword() {
  showPassword.value = !showPassword.value;
  passwordRef.value?.focus();
}

async function handleLogin() {
  error.value = "";
  loading.value = true;
  deleteCookie(AUTH_COOKIE);

  try {
    const response = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ password: password.value }),
    });

    if (response.ok) {
      const data = await response.json();
      authStore.login(data.token);
      await router.push(getRedirect());
    } else {
      error.value = "Senha inválida";
    }
  } catch {
    error.value = "Erro ao conectar com o servidor";
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="flex h-screen items-center justify-center bg-whiten px-4 dark:bg-boxdark-2">
    <DefaultAuthCard title="Plex Autoplay" subtitle="Painel administrativo">
      <div
        v-if="error"
        class="error alert alert-error mb-4 flex items-center gap-2 rounded-sm border border-meta-1/30 bg-meta-1/10 px-4 py-3 text-sm font-medium text-meta-1"
        role="alert"
        data-testid="error"
      >
        <AppIcon name="warning" size="sm" />
        {{ error }}
      </div>

      <form data-testid="login-form" @submit.prevent="handleLogin">
        <div class="mb-5">
          <label class="mb-2.5 block font-medium text-black dark:text-white" for="password">
            Senha
          </label>
          <div class="relative">
            <input
              id="password"
              ref="passwordRef"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Digite a senha"
              autocomplete="current-password"
              class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-12 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
              required
              data-testid="password-input"
            />
            <button
              type="button"
              class="absolute right-4 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
              :aria-label="showPassword ? 'Ocultar senha' : 'Mostrar senha'"
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
          class="flex w-full cursor-pointer items-center justify-center gap-2 rounded-lg bg-primary p-4 font-medium text-white transition hover:bg-opacity-90 disabled:opacity-60"
        >
          <AppIcon :name="loading ? 'spinner' : 'check'" size="sm" :class="{ 'animate-spin': loading }" />
          {{ loading ? "Entrando..." : "Entrar" }}
        </button>
      </form>
    </DefaultAuthCard>
  </div>
</template>