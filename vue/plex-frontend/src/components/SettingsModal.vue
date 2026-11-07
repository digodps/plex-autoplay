<script setup>
import { computed, onMounted, onUnmounted, ref } from "vue";
import { useThemeStore } from "@/stores/theme";

const props = defineProps({
  isOpen: { type: Boolean, default: false },
});
const emit = defineEmits(["close"]);

const theme = useThemeStore();
const newPassword = ref("");
const confirmPassword = ref("");
const error = ref("");

const dark = computed({
  get: () => theme.mode === "dark",
  set: (val) => (val ? theme.enterDark() : theme.enterLight()),
});

// ponytail: UI-only password form, no API endpoint exists yet;
// wire emit("apply", password) to /api/auth/password when backend adds it.
function apply() {
  error.value = "";
  if (newPassword.value !== confirmPassword.value) {
    error.value = "As senhas não conferem";
    return;
  }
  newPassword.value = "";
  confirmPassword.value = "";
  emit("close");
}

function onKeydown(e) {
  if (e.key === "Escape" && props.isOpen) emit("close");
}

onMounted(() => document.addEventListener("keydown", onKeydown));
onUnmounted(() => document.removeEventListener("keydown", onKeydown));
</script>

<template>
  <div
    v-if="props.isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4"
    @click.self="emit('close')"
  >
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="settings-title"
      class="w-full max-w-md rounded-lg border border-gray-300 bg-white p-6 shadow-lg dark:border-gray-700 dark:bg-gray-900"
    >
      <h2 id="settings-title" class="mb-4 text-lg font-semibold text-black dark:text-white">
        Configurações
      </h2>

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark">Tema</label>
      <select
        v-model="dark"
        class="mb-4 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white"
      >
        <option :value="false">Claro</option>
        <option :value="true">Escuro</option>
      </select>

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark">Nova senha</label>
      <input
        v-model="newPassword"
        type="password"
        autocomplete="new-password"
        class="mb-3 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white"
      />

      <label class="mb-1 block text-sm text-gray-700 dark:text-bodydark">Confirmar senha</label>
      <input
        v-model="confirmPassword"
        type="password"
        autocomplete="new-password"
        class="mb-2 w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-black dark:border-gray-700 dark:bg-gray-800 dark:text-white"
      />

      <p v-if="error" class="mb-2 text-sm text-red">{{ error }}</p>

      <div class="mt-4 flex justify-end gap-2">
        <button
          type="button"
          class="rounded-md px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 dark:text-bodydark dark:hover:bg-gray-800"
          @click="emit('close')"
        >
          Cancelar
        </button>
        <button
          type="button"
          class="rounded-md bg-primary px-4 py-2 text-sm font-medium text-white hover:opacity-90"
          @click="apply"
        >
          Aplicar
        </button>
      </div>
    </div>
  </div>
</template>
