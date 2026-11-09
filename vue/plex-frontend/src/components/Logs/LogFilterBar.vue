<script setup>
import { ref } from "vue";
import { useLogsStore } from "@/stores/logs";

const logsStore = useLogsStore();

const levels = ["info", "success", "warning", "error", "skip"];
const groupingOptions = [
  { value: "date", label: "Data" },
  { value: "status", label: "Status" },
  { value: "level", label: "Tipo" },
];

const textInput = ref(logsStore.filters.text);
const fromInput = ref(logsStore.filters.from);
const toInput = ref(logsStore.filters.to);

function onTextInput() {
  logsStore.setTextFilter(textInput.value);
}

function onLevelChange(event) {
  logsStore.setLevelFilter(event.target.value);
}

function onDateChange() {
  logsStore.setDateRange(fromInput.value, toInput.value);
}

function onGroupingChange(event) {
  logsStore.setGroupBy(event.target.value);
}
</script>

<template>
  <div class="mb-6 rounded-lg border border-gray-200 bg-white p-4 dark:border-gray-600 dark:bg-gray-800">
    <div class="grid grid-cols-1 gap-3 md:grid-cols-2 lg:grid-cols-5">
      <label class="flex flex-col gap-1 lg:col-span-2">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Busca textual</span>
        <input
          v-model="textInput"
          type="search"
          placeholder="Filtrar mensagens..."
          data-testid="log-filter-text"
          class="w-full h-10 rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100"
          @input="onTextInput"
        />
      </label>
      <label class="flex flex-col gap-1">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Nível</span>
        <select
          :value="logsStore.filters.level"
          data-testid="log-filter-level"
          class="w-full h-10 rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100"
          @change="onLevelChange"
        >
          <option value="">Todos</option>
          <option v-for="level in levels" :key="level" :value="level">{{ level }}</option>
        </select>
      </label>
      <label class="flex flex-col gap-1">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">De</span>
        <input
          v-model="fromInput"
          type="date"
          class="w-full h-10 rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100"
          @change="onDateChange"
        />
      </label>
      <label class="flex flex-col gap-1">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Até</span>
        <input
          v-model="toInput"
          type="date"
          class="w-full h-10 rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-sm dark:border-gray-600 dark:bg-gray-700 dark:text-gray-100"
          @change="onDateChange"
        />
      </label>
    </div>
    <div class="mt-3 flex flex-wrap items-center gap-2">
      <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">Agrupar por</span>
      <button
        v-for="option in groupingOptions"
        :key="option.value"
        type="button"
        data-testid="log-groupby"
        class="rounded-md border px-3 py-1.5 text-sm font-medium transition"
        :class="
          logsStore.groupBy === option.value
            ? 'border-primary bg-primary text-white'
            : 'border-gray-300 text-gray-600 hover:border-primary hover:text-primary dark:border-gray-600 dark:text-gray-300'
        "
        @click="onGroupingChange({ target: { value: option.value } })"
      >
        {{ option.label }}
      </button>
    </div>
  </div>
</template>