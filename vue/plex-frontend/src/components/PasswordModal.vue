<script setup>
import { onMounted, onUnmounted, ref } from "vue";
import { changePassword } from "@/api";

const props = defineProps({
  isOpen: { type: Boolean, default: false },
});
const emit = defineEmits(["close"]);

const currentPassword = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const error = ref("");
const loading = ref(false);

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
    error.value = "A senha deve ter pelo menos 8 caracteres";
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    error.value = "As senhas não conferem";
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
      error.value = "Senha atual incorreta";
      return;
    }
    const data = await response.json().catch(() => ({}));
    error.value = data.message || "Não foi possível alterar a senha";
  } catch {
    error.value = "Erro ao conectar com o servidor";
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
        Alterar senha
      </h2>

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="password-current">
        Senha atual
      </label>
      <input
        id="password-current"
        v-model="currentPassword"
        type="password"
        autocomplete="current-password"
        class="mb-3 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white"
        data-testid="password-current"
        required
      />

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="password-new">
        Nova senha
      </label>
      <input
        id="password-new"
        v-model="newPassword"
        type="password"
        autocomplete="new-password"
        minlength="8"
        maxlength="128"
        class="mb-3 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white"
        data-testid="password-new"
        required
      />

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="password-confirm">
        Confirmar nova senha
      </label>
      <input
        id="password-confirm"
        v-model="confirmPassword"
        type="password"
        autocomplete="new-password"
        minlength="8"
        maxlength="128"
        class="mb-2 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white"
        data-testid="password-confirm"
        required
      />

      <p v-if="error" class="mb-2 text-sm text-red" data-testid="password-error">{{ error }}</p>

      <div class="mt-4 flex justify-end gap-2">
        <button
          type="button"
          class="rounded-md px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800"
          data-testid="password-cancel"
          @click="close"
        >
          Cancelar
        </button>
        <button
          type="button"
          class="rounded-md bg-primary px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-60"
          :disabled="loading"
          data-testid="password-submit"
          @click="apply"
        >
          {{ loading ? "Salvando..." : "Alterar senha" }}
        </button>
      </div>
    </div>
  </div>
</template>
