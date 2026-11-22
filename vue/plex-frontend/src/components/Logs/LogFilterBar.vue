<script setup>
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useLogsStore } from "@/stores/logs";
import AppIcon from "@/components/AppIcon.vue";

const { t } = useI18n();
const logsStore = useLogsStore();

const levels = ["info", "success", "warning", "error", "skip"];

const groupingOptions = computed(() => [
  { value: "date", label: t("logs.groupBy.date") },
  { value: "status", label: t("logs.groupBy.status") },
  { value: "level", label: t("logs.groupBy.level") },
  { value: "library", label: t("logs.groupBy.library") },
]);

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

function clearFilters() {
  textInput.value = "";
  fromInput.value = "";
  toInput.value = "";
  logsStore.setTextFilter("");
  logsStore.setDateRange("", "");
  logsStore.setLevelFilter("all");
}
</script>

<template>
  <div class="mb-6 rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
    <div class="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-5">
      <!-- Text search -->
      <label class="flex flex-col gap-1.5 lg:col-span-2" for="log-filter-text">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">{{ t("logs.filter.text") }}</span>
        <div class="relative">
          <input
            id="log-filter-text"
            v-model="textInput"
            type="search"
            :placeholder="t('logs.filter.placeholder')"
            @input="onTextInput"
            class="input w-full pl-10 pr-4 py-2.5"
            aria-label="Buscar logs"
            data-testid="log-filter-text"
          />
          <AppIcon name="search" size="sm" class="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" aria-hidden="true" />
        </div>
      </label>

      <!-- Level filter -->
      <label class="flex flex-col gap-1.5" for="log-filter-level">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">{{ t("logs.filter.level") }}</span>
        <select
          id="log-filter-level"
          v-model="logsStore.filters.level"
          @change="onLevelChange"
          class="input w-full py-2.5 pr-8"
          aria-label="Filtrar por nível"
          data-testid="log-filter-level"
        >
          <option value="all">{{ t("logs.filter.allLevels") }}</option>
          <option v-for="level in levels" :key="level" :value="level">
            {{ t(`logs.levels.${level}`) }}
          </option>
        </select>
      </label>

      <!-- Date range -->
      <div class="flex flex-col gap-1.5">
        <span class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400">{{ t("logs.filter.dateRange") }}</span>
        <div class="flex items-center gap-2">
          <label for="log-filter-from" class="sr-only">{{ t("logs.filter.from") }}</label>
          <input
            id="log-filter-from"
            v-model="fromInput"
            type="date"
            @change="onDateChange"
            class="input log-filter-input w-full py-2.5"
            aria-label="Data inicial"
            data-testid="log-filter-from"
          />
          <span class="text-gray-500 dark:text-gray-400" aria-hidden="true">–</span>
          <label for="log-filter-to" class="sr-only">{{ t("logs.filter.to") }}</label>
          <input
            id="log-filter-to"
            v-model="toInput"
            type="date"
            @change="onDateChange"
            class="input log-filter-input w-full py-2.5"
            aria-label="Data final"
            data-testid="log-filter-to"
          />
        </div>
      </div>

      <!-- Group by & Clear -->
      <div class="flex flex-col gap-1.5">
        <div class="flex items-center justify-between">
          <label for="log-filter-group" class="text-xs font-medium uppercase tracking-wide text-gray-500 dark:text-gray-400 mb-0">
            {{ t("logs.groupBy.label") }}
          </label>
          <button
            type="button"
            @click="clearFilters"
            class="text-xs text-primary hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900 rounded"
            :aria-label="t('logs.clearFilters')"
            data-testid="log-filter-clear"
          >
            {{ t("logs.clearFilters") }}
          </button>
        </div>
        <select
          id="log-filter-group"
          v-model="logsStore.groupBy"
          @change="onGroupingChange"
          class="input w-full py-2.5 pr-8"
          aria-label="Agrupar logs por"
          data-testid="log-filter-group"
        >
          <option v-for="opt in groupingOptions" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </option>
        </select>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Corrige largura de inputs de data e selects no iOS/Safari, que renderizam com altura/largura nativas diferentes */
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