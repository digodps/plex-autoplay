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
      <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">{{ label }}</label>
      <p class="mt-1.5 text-xs text-gray-500 dark:text-gray-400">
        Caminhos existentes dentro do container. Use quantos precisar.
      </p>
    </div>

    <p v-if="!modelValue.length" class="text-sm text-gray-500 dark:text-gray-400">
      Nenhum diretório adicionado.
    </p>

    <ul class="space-y-2">
      <li
        v-for="path in modelValue"
        :key="path"
        class="flex items-center justify-between gap-3 rounded-lg border border-stroke bg-white dark:bg-gray-800 px-4 py-2.5 text-sm dark:border-gray-700"
      >
        <span class="truncate font-mono text-gray-700 dark:text-gray-300">{{ path }}</span>
        <button
          type="button"
          class="shrink-0 text-red-600 dark:text-red-400 transition hover:text-red-700 dark:hover:text-red-300"
          :aria-label="`Remover ${path}`"
          data-testid="media-root-remove"
          @click="remove(path)"
        >
          Remove
        </button>
      </li>
    </ul>

    <div class="flex flex-wrap items-center gap-2">
      <input
        v-model="draft"
        type="text"
        placeholder="/media"
        class="min-w-0 flex-1 rounded-lg border border-stroke bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 placeholder-gray-400 px-4 py-2.5 focus:border-accent focus:outline-none focus:ring-1 focus:ring-accent/30 dark:border-gray-600"
        @keydown.enter.prevent="addDraft"
      />
      <button
        type="button"
        class="rounded-lg border border-stroke px-4 py-2.5 text-sm font-medium text-gray-700 dark:text-gray-300 transition hover:border-accent hover:text-accent dark:border-strokedark dark:hover:border-accent"
        data-testid="media-root-add"
        @click="addDraft"
      >
        Adicionar
      </button>
      <button
        type="button"
        class="rounded-lg border border-stroke px-4 py-2.5 text-sm font-medium text-gray-700 dark:text-gray-300 transition hover:border-accent hover:text-accent dark:border-strokedark dark:hover:border-accent"
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
