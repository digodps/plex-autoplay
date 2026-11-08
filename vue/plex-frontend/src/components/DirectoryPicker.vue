<script setup>
import { onUnmounted, ref, watch } from "vue";
import { extractErrors, fetchDirectories } from "@/api";

const props = defineProps({
  open: { type: Boolean, default: false },
  startPath: { type: String, default: "/media" },
});
const emit = defineEmits(["select", "close"]);

const currentPath = ref("");
const parent = ref(null);
const entries = ref([]);
const loading = ref(false);
const error = ref("");

async function load(path, allowFallback = true) {
  loading.value = true;
  error.value = "";
  try {
    const res = await fetchDirectories(path);
    if (!res.ok) {
      const messages = await extractErrors(res);
      if (allowFallback && path !== "/") {
        return load("/", false);
      }
      error.value = messages.join(" ") || "Não foi possível listar o diretório";
      return;
    }
    const data = await res.json();
    currentPath.value = data.path;
    parent.value = data.parent ?? null;
    entries.value = data.entries ?? [];
  } catch {
    error.value = "Erro ao listar diretórios";
  } finally {
    loading.value = false;
  }
}

function handleKeydown(event) {
  if (event.key === "Escape") emit("close");
}

const gotoPath = ref("");
function goTo() {
  const value = gotoPath.value.trim();
  if (value) load(value);
}

watch(
  () => props.open,
  (open) => {
    if (open) {
      window.addEventListener("keydown", handleKeydown);
      currentPath.value = props.startPath;
      entries.value = [];
      parent.value = null;
      load(props.startPath);
    } else {
      window.removeEventListener("keydown", handleKeydown);
    }
  },
  { immediate: true },
);

onUnmounted(() => window.removeEventListener("keydown", handleKeydown));
</script>

<template>
  <div
    v-if="open"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
    data-testid="directory-picker"
    @click.self="emit('close')"
  >
    <div
      class="w-full max-w-lg rounded-lg border border-gray-200 bg-white p-4 shadow-lg dark:border-gray-600 dark:bg-gray-800"
    >
      <div class="mb-3 flex items-center justify-between gap-2">
        <p
          class="truncate font-mono text-sm text-gray-700 dark:text-gray-300"
          data-testid="directory-picker-path"
        >
          {{ currentPath || startPath }}
        </p>
        <button
          type="button"
          class="rounded border border-gray-200 px-2 py-1 text-sm text-gray-700 transition hover:bg-gray-100 disabled:opacity-50 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          :disabled="parent === null || loading"
          data-testid="directory-picker-up"
          @click="parent !== null && load(parent)"
        >
          ↑ Pai
        </button>
      </div>

      <form class="mb-3 flex items-center gap-2" @submit.prevent="goTo">
        <input
          v-model="gotoPath"
          type="text"
          placeholder="/caminho/para/abrir"
          class="min-w-0 flex-1 rounded border border-gray-300 bg-gray-100 px-3 py-2 font-mono text-sm text-black focus:border-primary focus:bg-white focus:text-black dark:border-gray-600 dark:bg-gray-700 dark:text-white"
          data-testid="directory-picker-goto"
        />
        <button
          type="submit"
          class="rounded border border-gray-200 px-3 py-2 text-sm text-gray-700 transition hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
        >
          Ir
        </button>
      </form>

      <p
        v-if="error"
        class="mb-3 text-sm text-red-600 dark:text-red-400"
        role="alert"
      >
        {{ error }}
      </p>

      <div
        class="mb-4 h-64 overflow-y-auto rounded border border-gray-200 dark:border-gray-600"
      >
        <p
          v-if="loading"
          class="p-3 text-sm text-gray-500 dark:text-gray-400"
        >
          Carregando...
        </p>
        <p
          v-else-if="entries.length === 0"
          class="p-3 text-sm text-gray-500 dark:text-gray-400"
        >
          Nenhum diretório encontrado.
        </p>
        <template v-else>
          <button
            v-for="entry in entries"
            :key="entry.path"
            type="button"
            class="block w-full truncate px-3 py-2 text-left text-sm text-gray-700 transition hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-700"
            data-testid="directory-picker-entry"
            @click="load(entry.path)"
          >
            {{ entry.name }}
          </button>
        </template>
      </div>

      <div class="flex items-center justify-end gap-2">
        <button
          type="button"
          class="rounded-lg border border-gray-200 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
          data-testid="directory-picker-cancel"
          @click="emit('close')"
        >
          Cancelar
        </button>
        <button
          type="button"
          class="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition hover:opacity-90 disabled:opacity-50"
          :disabled="loading || !currentPath"
          data-testid="directory-picker-select"
          @click="emit('select', currentPath)"
        >
          Selecionar este diretório
        </button>
      </div>
    </div>
  </div>
</template>
