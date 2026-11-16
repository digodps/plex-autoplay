<script setup>
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";
import LogFilterBar from "@/components/Logs/LogFilterBar.vue";
import LogCarouselView from "@/components/Logs/LogCarouselView.vue";
import { useLogsStore } from "@/stores/logs";
import { apiFetch } from "@/api";

const { t } = useI18n();
const logsStore = useLogsStore();

onMounted(() => {
  logsStore.fetchLogs();
  logsStore.fetchJobs();
});

async function handleStartScan() {
  try {
    const res = await apiFetch("/api/plex/scan", { method: "POST" });
    if (res.ok) {
      logsStore.fetchJobs();
      logsStore.fetchLogs();
      // Polling para atualizar logs de forma assíncrona enquanto roda
      const interval = setInterval(() => {
        logsStore.fetchJobs();
        logsStore.fetchLogs();
      }, 2000);
      setTimeout(() => clearInterval(interval), 30000); // Para após 30 segundos
    } else {
      window.alert(t("logs.syncFailed"));
    }
  } catch (e) {
    window.alert(t("logs.syncFailed") + ": " + e.message);
  }
}

async function handleClearLogs() {
  if (!window.confirm(t("logs.confirmClearLogs"))) return;
  try {
    await logsStore.clearLogs();
    window.alert(t("logs.logsCleared"));
  } catch (e) {
    window.alert(t("errors.generic") + ": " + e.message);
  }
}

async function handleCleanup() {
  const days = window.prompt(t("logs.cleanupPrompt"), "30");
  if (days === null) return;
  if (!days.match(/^\d+$/)) {
    window.alert(t("errors.validation"));
    return;
  }
  try {
    await logsStore.cleanupLogs(parseInt(days));
    window.alert(t("logs.cleanupComplete"));
  } catch (e) {
    window.alert(t("errors.generic") + ": " + e.message);
  }
}
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-3">
    <div class="mx-auto max-w-6xl px-4 py-6">
      <div class="flex items-center justify-between gap-3 flex-wrap">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-gray-100">{{ t("logs.title") }}</h1>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            {{ t("logs.subtitle") }}
          </p>
        </div>
        <div class="flex items-center gap-2 flex-wrap">
          <button
            data-testid="log-btn-scan"
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark"
            @click="handleStartScan"
          >
            {{ t("sync.syncNow") }}
          </button>
          <button
            data-testid="log-btn-clear"
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark"
            @click="handleClearLogs"
          >
            {{ t("logs.clearLogs") }}
          </button>
          <button
            data-testid="log-btn-cleanup"
            class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark"
            @click="handleCleanup"
          >
            {{ t("logs.cleanupLogs") }}
          </button>
        </div>
      </div>

      <div class="mt-6">
        <LogFilterBar />
        <LogCarouselView
          :items="logsStore.logs"
          :group-by="logsStore.groupBy"
          :loading="logsStore.loadingLogs"
        />
      </div>
    </div>
  </div>
</template>