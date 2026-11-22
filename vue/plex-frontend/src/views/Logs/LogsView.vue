<script setup>
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";
import LogFilterBar from "@/components/Logs/LogFilterBar.vue";
import LogCarouselView from "@/components/Logs/LogCarouselView.vue";
import { useLogsStore } from "@/stores/logs";
import { apiFetch } from "@/api";
import AppIcon from "@/components/AppIcon.vue";

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
  <div class="mx-auto max-w-7xl px-4 py-6">
    <!-- Header -->
    <header class="mb-6 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
      <div>
        <h1 class="text-2xl font-semibold text-black dark:text-white">{{ t("logs.title") }}</h1>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">{{ t("logs.subtitle") }}</p>
      </div>

      <div class="flex flex-wrap items-center gap-2" role="group" aria-label="Ações de logs">
        <button
          type="button"
          @click="handleStartScan"
          class="btn btn-primary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
          data-testid="log-btn-scan"
        >
          <AppIcon name="refresh-cw" size="sm" class="mr-1" aria-hidden="true" />
          {{ t("logs.syncNow") }}
        </button>

        <button
          type="button"
          @click="handleClearLogs"
          class="btn btn-secondary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
          data-testid="log-btn-clear"
        >
          <AppIcon name="trash-2" size="sm" class="mr-1" aria-hidden="true" />
          {{ t("logs.clearLogs") }}
        </button>

        <button
          type="button"
          @click="handleCleanup"
          class="btn btn-secondary focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2 dark:focus-visible:ring-offset-gray-900"
          data-testid="log-btn-cleanup"
        >
          <AppIcon name="archive" size="sm" class="mr-1" aria-hidden="true" />
          {{ t("logs.cleanupLogs") }}
        </button>
      </div>
    </header>

    <!-- Filter Bar & Carousel -->
    <main>
      <LogFilterBar />
      <LogCarouselView
        :items="logsStore.logs"
        :group-by="logsStore.groupBy"
        :loading="logsStore.loadingLogs"
      />
    </main>
  </div>
</template>