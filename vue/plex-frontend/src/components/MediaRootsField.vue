<script setup>
import { ref } from "vue";
import DirectoryPicker from "@/components/DirectoryPicker.vue";

const props = defineProps({
  modelValue: { type: Array, default: () => [] },
  label: { type: String, default: "Diretórios de Mídia" },
});
const emit = defineEmits(["update:modelValue"]);

const draft = ref("");
const pickerOpen = ref(false);

function add(value) {
  const path = String(value ?? "").trim();
  if (!path || props.modelValue.includes(path)) return;
  emit("update:modelValue", [...props.modelValue, path]);
}

function addDraft() {
  add(draft.value);
  draft.value = "";
}

function remove(path) {
  emit(
    "update:modelValue",
    props.modelValue.filter((item) => item !== path),
  );
}

function handleSelect(path) {
  add(path);
  pickerOpen.value = false;
}
</script>

<template>
  <div class="media-roots-field" style="display: flex; flex-direction: column; gap: var(--s-3);">
    <div>
      <label class="t-label" style="display: block; margin-bottom: var(--s-2);">{{ label }}</label>
      <p class="t-small muted" style="margin: 0;">Caminhos existentes dentro do container. Use quantos precisar.</p>
    </div>

    <p v-if="!modelValue.length" class="t-body muted" style="margin: 0;">Nenhum diretório adicionado.</p>

    <ul style="list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: var(--s-2);">
      <li
        v-for="path in modelValue"
        :key="path"
        class="media-root-item"
        style="display: flex; align-items: center; justify-content: space-between; gap: var(--s-3); padding: var(--s-3) var(--s-4); background: var(--card); border: 1px solid var(--border); border-radius: var(--r-md);"
      >
        <span class="mono truncate" style="flex: 1; min-width: 0; font-size: var(--t-body);">{{ path }}</span>
        <button
          type="button"
          class="btn btn--secondary btn-sm"
          :aria-label="`Remover ${path}`"
          data-testid="media-root-remove"
          @click="remove(path)"
        >
          Remover
        </button>
      </li>
    </ul>

    <div style="display: flex; flex-wrap: wrap; gap: var(--s-2);">
      <input
        v-model="draft"
        type="text"
        placeholder="/media"
        class="field__input mono"
        style="flex: 1; min-width: 200px;"
        @keydown.enter.prevent="addDraft"
      />
      <button
        type="button"
        class="btn btn--secondary"
        data-testid="media-root-add"
        @click="addDraft"
      >
        Adicionar
      </button>
      <button
        type="button"
        class="btn btn--secondary"
        data-testid="media-root-open-picker"
        @click="pickerOpen = true"
      >
        Procurar…
      </button>
    </div>

    <DirectoryPicker
      :open="pickerOpen"
      @select="handleSelect"
      @close="pickerOpen = false"
    />
  </div>
</template>

<style scoped>
.media-roots-field {
  display: flex;
  flex-direction: column;
  gap: var(--s-3);
}

.media-root-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--s-3);
  padding: var(--s-3) var(--s-4);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--r-md);
}
</style>