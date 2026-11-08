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
  <div class="space-y-3">
    <div>
      <label class="mb-2.5 block text-sm font-medium text-gray-700 dark:text-gray-300">
        {{ label }}
      </label>
      <p class="text-xs text-gray-500 dark:text-gray-400">
        Caminhos existentes dentro do container. Use quantos precisar.
      </p>
    </div>

    <ul v-if="modelValue.length" class="space-y-2">
      <li
        v-for="path in modelValue"
        :key="path"
        class="flex items-center justify-between gap-2 rounded border border-gray-200 bg-gray-100 px-3 py-2 text-sm dark:border-gray-600 dark:bg-gray-700"
      >
        <span class="truncate font-mono text-gray-700 dark:text-gray-300">{{ path }}</span>
        <button
          type="button"
          class="text-red-600 transition hover:opacity-80 dark:text-red-400"
          :aria-label="`Remover ${path}`"
          data-testid="media-root-remove"
          @click="remove(path)"
        >
          Remover
        </button>
      </li>
    </ul>
    <p v-else class="text-sm text-gray-500 dark:text-gray-400">
      Nenhum diretório adicionado.
    </p>

    <div class="flex flex-wrap items-center gap-2">
      <input
        v-model="draft"
        type="text"
        placeholder="/media"
        class="min-w-0 flex-1 rounded border border-gray-300 bg-gray-100 px-3 py-2 text-black focus:border-primary focus:bg-white focus:text-black dark:border-gray-600 dark:bg-gray-700 dark:text-white"
        @keydown.enter.prevent="addDraft"
      />
      <button
        type="button"
        class="rounded-lg border border-gray-200 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
        data-testid="media-root-add"
        @click="addDraft"
      >
        Adicionar
      </button>
      <button
        type="button"
        class="rounded-lg border border-gray-200 px-4 py-2 text-sm font-medium text-gray-700 transition hover:bg-gray-100 dark:border-gray-600 dark:text-gray-300 dark:hover:bg-gray-700"
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
