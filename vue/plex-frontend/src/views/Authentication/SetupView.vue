<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import DefaultAuthCard from "@/components/Auths/DefaultAuthCard.vue";
import AppIcon from "@/components/AppIcon.vue";
import { submitSetup } from "@/api";

const router = useRouter();
const token = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const error = ref("");
const loading = ref(false);

async function handleSetup() {
  error.value = "";

  if (newPassword.value.length < 8) {
    error.value = "A senha deve ter pelo menos 8 caracteres";
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    error.value = "As senhas não conferem";
    return;
  }

  loading.value = true;
  try {
    const response = await submitSetup(token.value, newPassword.value);
    if (response.status === 204 || response.status === 409) {
      await router.push({ name: "login" });
      return;
    }
    if (response.status === 401) {
      error.value = "Token de setup inválido ou já utilizado";
      return;
    }
    const data = await response.json().catch(() => ({}));
    error.value = data.message || "Não foi possível concluir a configuração";
  } catch {
    error.value = "Erro ao conectar com o servidor";
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="flex h-screen items-center justify-center bg-whiten px-4 dark:bg-boxdark-2">
    <DefaultAuthCard title="Plex Autoplay" subtitle="Configuração inicial">
      <div
        class="mb-5 rounded-sm border border-primary/30 bg-primary/5 px-4 py-3 text-sm text-body dark:text-bodydark"
        data-testid="setup-token-hint"
      >
        O token de setup é de <strong>uso único</strong> e está no log do
        container. Execute <code>docker logs plex-autoplay</code> para obtê-lo.
      </div>

      <div
        v-if="error"
        class="error alert alert-error mb-4 flex items-center gap-2 rounded-sm border border-meta-1/30 bg-meta-1/10 px-4 py-3 text-sm font-medium text-meta-1"
        role="alert"
        data-testid="error"
      >
        <AppIcon name="warning" size="sm" />
        {{ error }}
      </div>

      <form @submit.prevent="handleSetup">
        <div class="mb-4">
          <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-token">
            Token de setup
          </label>
          <input
            id="setup-token"
            v-model="token"
            type="text"
            autocomplete="one-time-code"
            placeholder="SETUP_TOKEN do log do container"
            class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-5 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
            data-testid="setup-token"
            required
          />
        </div>

        <div class="mb-4">
          <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-password">
            Nova senha
          </label>
          <input
            id="setup-password"
            v-model="newPassword"
            type="password"
            autocomplete="new-password"
            minlength="8"
            maxlength="128"
            class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-5 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
            data-testid="setup-password"
            required
          />
        </div>

        <div class="mb-5">
          <label class="mb-2.5 block font-medium text-black dark:text-white" for="setup-confirm">
            Confirmar senha
          </label>
          <input
            id="setup-confirm"
            v-model="confirmPassword"
            type="password"
            autocomplete="new-password"
            minlength="8"
            maxlength="128"
            class="w-full rounded-lg border border-stroke bg-transparent py-4 pl-5 pr-5 text-black outline-none focus:border-primary focus-visible:shadow-none dark:border-form-strokedark dark:bg-form-input dark:text-white"
            data-testid="setup-confirm"
            required
          />
        </div>

        <button
          :disabled="loading"
          type="submit"
          class="flex w-full cursor-pointer items-center justify-center gap-2 rounded-lg bg-primary p-4 font-medium text-white transition hover:bg-opacity-90 disabled:opacity-60"
          data-testid="setup-submit"
        >
          <AppIcon :name="loading ? 'spinner' : 'check'" size="sm" :class="{ 'animate-spin': loading }" />
          {{ loading ? "Configurando..." : "Criar senha" }}
        </button>
      </form>
    </DefaultAuthCard>
  </div>
</template>
