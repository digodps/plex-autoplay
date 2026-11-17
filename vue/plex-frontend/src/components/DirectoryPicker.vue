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
    class="modal-overlay"
    data-testid="directory-picker"
    @click.self="emit('close')"
  >
    <div class="modal panel ground-inverse" style="max-width: 560px; width: 100%;">
      <header class="modal__header">
        <p class="t-body mono truncate" data-testid="directory-picker-path">
          {{ currentPath || startPath }}
        </p>
        <button
          type="button"
          class="btn btn--secondary btn-sm"
          :disabled="parent === null || loading"
          data-testid="directory-picker-up"
          @click="parent !== null && load(parent)"
        >
          <img src="/assets/icons/ui-chevron-up.svg" data-icon="ui-chevron-up" class="icon" width="16" height="16" aria-hidden="true" />
          <span>Pai</span>
        </button>
      </header>

      <form class="modal__form" @submit.prevent="goTo">
        <div class="field-row">
          <input
            v-model="gotoPath"
            type="text"
            placeholder="/caminho/para/abrir"
            class="field__input mono"
            data-testid="directory-picker-goto"
          />
          <button
            type="submit"
            class="btn btn--secondary btn-sm"
          >
            Ir
          </button>
        </div>
      </form>

      <p v-if="error" class="alert alert--error" role="alert">
        <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon" width="16" height="16" aria-hidden="true" />
        {{ error }}
      </p>

      <div class="modal__list" role="listbox" aria-label="Diretórios">
        <p v-if="loading" class="muted text-center" style="padding: var(--s-4);">Carregando...</p>
        <p v-else-if="entries.length === 0" class="muted text-center" style="padding: var(--s-4);">Nenhum diretório encontrado.</p>
        <template v-else>
          <button
            v-for="entry in entries"
            :key="entry.path"
            type="button"
            class="modal__item"
            role="option"
            data-testid="directory-picker-entry"
            @click="load(entry.path)"
          >
            {{ entry.name }}
          </button>
        </template>
      </div>

      <footer class="modal__footer">
        <button
          type="button"
          class="btn btn--secondary"
          data-testid="directory-picker-cancel"
          @click="emit('close')"
        >
          Cancelar
        </button>
        <button
          type="button"
          class="btn btn--primary"
          :disabled="loading || !currentPath"
          data-testid="directory-picker-select"
          @click="emit('select', currentPath)"
        >
          Selecionar este diretório
        </button>
      </footer>
    </div>
  </div>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--s-4);
  background: color-mix(in oklab, #111111 60%, transparent);
}

.modal {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-lg);
  box-shadow: var(--shadow-xl);
  overflow: hidden;
}

html[data-tone="dark"] .modal-overlay {
  background: color-mix(in oklab, #000000 70%, transparent);
}

.modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--s-2);
  padding: var(--s-4);
  border-bottom: 1px solid var(--border);
}

.modal__header .t-body {
  margin: 0;
}

.modal__form {
  padding: var(--s-4);
  border-bottom: 1px solid var(--border);
}

.field-row {
  display: flex;
  gap: var(--s-2);
}

.field-row .field__input {
  flex: 1;
  min-width: 0;
}

.modal__list {
  max-height: 256px;
  overflow-y: auto;
  border-bottom: 1px solid var(--border);
}

.modal__item {
  display: block;
  width: 100%;
  padding: var(--s-3) var(--s-4);
  text-align: left;
  border: none;
  background: transparent;
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  transition: background var(--m-fast) var(--m-ease);
}

@media (hover: hover) and (pointer: fine) {
  .modal__item:hover {
    background: var(--surface);
  }
}

.modal__item:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: -2px;
}

.modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--s-3);
  padding: var(--s-4);
}
</style>