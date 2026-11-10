<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import AppIcon from "@/components/AppIcon.vue";

const router = useRouter();
const error = ref("");
const oldPassword = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const saving = ref(false);

const showOldPassword = ref(false);
const showNewPassword = ref(false);
const showConfirmPassword = ref(false);
const oldPasswordRef = ref(null);
const newPasswordRef = ref(null);
const confirmPasswordRef = ref(null);

function togglePassword(showRef, inputRef) {
  showRef.value = !showRef.value;
  inputRef.value?.focus();
}

function apply() {
  error.value = "";
  if (!oldPassword.value || !newPassword.value || !confirmPassword.value) {
    error.value = "Preencha todos os campos";
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    error.value = "As senhas não conferem";
    return;
  }
  saving.value = true;
  /* TODO: chamar API /api/auth/password quando disponível */
  setTimeout(() => {
    saving.value = false;
    router.push("/");
  }, 1500);
}
</script>
<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-gray-800 p-4">
    <div role="dialog" aria-modal="true" aria-labelledby="change-password-title" class="w-full max-w-md rounded-lg border p-6 shadow-lg dark:border-gray-700 dark:bg-gray-900">
      <h2 id="change-password-title" class="mb-4 text-lg font-semibold text-black dark:text-white"> Alterar senha </h2>
      <div class="mb-1 relative">
        <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark" for="cp-old-password"> Senha atual </label>
        <input
          id="cp-old-password"
          ref="oldPasswordRef"
          v-model="oldPassword"
          :type="showOldPassword ? 'text' : 'password'"
          placeholder="Digite sua senha atual"
          autocomplete="current-password"
          class="mt-1 block w-full rounded border border-stroke bg-white text-black px-3 py-2 pr-12 focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          required
        />
        <button
          type="button"
          class="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
          :aria-label="showOldPassword ? 'Ocultar senha' : 'Mostrar senha'"
          :aria-pressed="showOldPassword"
          data-testid="toggle-password-cp-current"
          @click="togglePassword(showOldPassword, oldPasswordRef)"
        >
          <AppIcon :name="showOldPassword ? 'eye-off' : 'eye'" size="sm" />
        </button>
      </div>
      <label class="mt-4 mb-1 block text-sm text-gray-700 dark:text-bodydark" for="cp-new-password"> Nova senha </label>
      <div class="relative">
        <input
          id="cp-new-password"
          ref="newPasswordRef"
          v-model="newPassword"
          :type="showNewPassword ? 'text' : 'password'"
          placeholder="Digite uma nova senha"
          autocomplete="new-password"
          class="mt-1 block w-full rounded border border-stroke bg-white text-black px-3 py-2 pr-12 focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          required
        />
        <button
          type="button"
          class="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
          :aria-label="showNewPassword ? 'Ocultar senha' : 'Mostrar senha'"
          :aria-pressed="showNewPassword"
          data-testid="toggle-password-cp-new"
          @click="togglePassword(showNewPassword, newPasswordRef)"
        >
          <AppIcon :name="showNewPassword ? 'eye-off' : 'eye'" size="sm" />
        </button>
      </div>
      <label class="mt-4 mb-1 block text-sm text-gray-700 dark:text-bodydark" for="cp-confirm-password"> Confirmar nova senha </label>
      <div class="relative">
        <input
          id="cp-confirm-password"
          ref="confirmPasswordRef"
          v-model="confirmPassword"
          :type="showConfirmPassword ? 'text' : 'password'"
          placeholder="Confirme a nova senha"
          autocomplete="new-password"
          class="mt-1 block w-full rounded border border-stroke bg-white text-black px-3 py-2 pr-12 focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          required
        />
        <button
          type="button"
          class="absolute right-2 top-1/2 -translate-y-1/2 text-gray-500 hover:text-black dark:text-bodydark dark:hover:text-white"
          :aria-label="showConfirmPassword ? 'Ocultar senha' : 'Mostrar senha'"
          :aria-pressed="showConfirmPassword"
          data-testid="toggle-password-cp-confirm"
          @click="togglePassword(showConfirmPassword, confirmPasswordRef)"
        >
          <AppIcon :name="showConfirmPassword ? 'eye-off' : 'eye'" size="sm" />
        </button>
      </div>
      <p v-if="error" class="mt-2 mb-2 text-sm text-red">{{ error }}</p>
      <div class="mt-4 flex justify-end gap-2">
        <button type="button" class="rounded-md px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800"> Cancelar </button>
        <button :disabled="saving" type="button" class="rounded-md bg-primary px-4 py-2 text-sm text-white hover:opacity-90 disabled:opacity-50" @click="apply"> {{ saving ? "Salvando..." : "Salvar" }} </button>
      </div>
    </div>
  </div>
</template>
