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
  <div class="mb-6 rounded-lg border border-gray-800 bg-gray-900 p-4">
    <div class="grid grid-cols-1 gap-3 md:grid-cols-2 lg:grid-cols-5">
      <label class="flex flex-col gap-1 lg:col-span-2">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-400">Busca textual</span>
        <input
          v-model="textInput"
          type="search"
          placeholder="Filtrar mensagens..."
          data-testid="log-filter-text"
          class="log-filter-input w-full max-w-full h-10 box-border rounded-md border border-gray-700 bg-gray-800 px-3 py-2 text-sm text-white focus:outline-none focus:ring-2 focus:ring-primary"
          @input="onTextInput"
        />
      </label>
      <label class="flex flex-col gap-1">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-400">Nível</span>
        <select
          :value="logsStore.filters.level"
          data-testid="log-filter-level"
          class="log-filter-input w-full max-w-full h-10 box-border appearance-none rounded-md border border-gray-700 bg-gray-800 px-3 py-2 text-sm text-white"
          @change="onLevelChange"
        >
          <option value="">Todos</option>
          <option v-for="level in levels" :key="level" :value="level">{{ level }}</option>
        </select>
      </label>
      <label class="flex flex-col gap-1">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-400">De</span>
        <input
          v-model="fromInput"
          type="date"
          class="log-filter-input w-full max-w-full h-10 box-border appearance-none rounded-md border border-gray-700 bg-gray-800 px-3 py-2 text-sm text-white"
          @change="onDateChange"
        />
      </label>
      <label class="flex flex-col gap-1">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-400">Até</span>
        <input
          v-model="toInput"
          type="date"
          class="log-filter-input w-full max-w-full h-10 box-border appearance-none rounded-md border border-gray-700 bg-gray-800 px-3 py-2 text-sm text-white"
          @change="onDateChange"
        />
      </label>
    </div>
    <div class="mt-3 flex flex-wrap items-center gap-2">
      <span class="text-xs font-medium uppercase tracking-wide text-gray-400">Agrupar por</span>
      <button
        v-for="option in groupingOptions"
        :key="option.value"
        type="button"
        data-testid="log-groupby"
        class="rounded-md border px-3 py-1.5 text-sm font-medium transition"
        :class="
          logsStore.groupBy === option.value
            ? 'border-primary bg-primary text-white'
            : 'border-gray-700 text-gray-300 hover:border-primary hover:text-white'
        "
        @click="onGroupingChange({ target: { value: option.value } })"
      >
        {{ option.label }}
      </button>
    </div>
  </div>
</template>

<style scoped>
/* Normaliza inputs de data e selects no iOS/Safari, que renderizam com altura/largura nativas diferentes */
.log-filter-input {
  width: 100% !important;
  max-width: 100% !important;
  box-sizing: border-box;
  -webkit-appearance: none;
  appearance: none;
}
.log-filter-input::-webkit-datetime-edit,
.log-filter-input::-webkit-date-and-time-value {
  width: 100% !important;
  min-width: 100%;
  text-align: left;
}
</style>