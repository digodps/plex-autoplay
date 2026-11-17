<script setup>
import { onMounted, onUnmounted } from "vue";
import { useI18n } from "vue-i18n";
import DataStatsOne from "@/components/DataStats/DataStatsOne.vue";
import AppIcon from "@/components/AppIcon.vue";
import { useDashboardStore } from "@/stores/dashboard";

const { t } = useI18n();
const store = useDashboardStore();

onMounted(() => {
  store.fetchAll();
  store.startPolling();
});

onUnmounted(() => {
  store.stopPolling();
});
</script>

<template>
  <div class="dashboard-view" data-enter>
    <header class="page-header" style="margin-bottom: var(--s-6);">
      <h1 class="t-h2" style="font: var(--font-display); font-weight: var(--display-weight); letter-spacing: var(--display-tracking); margin-bottom: var(--s-2);">{{ t("dashboard.title") }}</h1>
      <p class="t-body muted" style="margin: 0;">{{ t("dashboard.subtitle") }}</p>
    </header>

    <DataStatsOne />

    <div
      v-if="store.isTotalFailure"
      class="alert alert--error"
      role="alert"
      style="margin-top: var(--s-6); display: flex; align-items: center; gap: var(--s-3);"
    >
      <AppIcon name="triangle-alert" size="lg" />
      <p class="flex-1">{{ store.error }}</p>
      <button class="btn btn--secondary btn-sm" :disabled="store.isLoading" @click="store.retry">
        {{ t("common.retry") }}
      </button>
    </div>

    <div
      v-else-if="store.libraryError"
      class="alert alert--warning"
      role="alert"
      style="margin-top: var(--s-6); display: flex; flex-direction: column; gap: var(--s-3);"
    >
      <div style="display: flex; align-items: center; gap: var(--s-3); flex-wrap: wrap;">
        <AppIcon name="triangle-alert" size="lg" style="color: var(--warning, #e0a800);" />
        <p class="flex-1" style="margin: 0;">{{ store.libraryError }}</p>
        <button class="btn btn--secondary btn-sm" :disabled="store.isLoading" @click="store.retry">
          {{ t("common.retry") }}
        </button>
      </div>
      <p class="t-small muted" style="margin: 0;">{{ t("dashboard.plexConnectionHelp") }}</p>
    </div>
  </div>
</template>

<style scoped>
.dashboard-view {
  width: 100%;
}

.page-header {
  margin-bottom: var(--s-6);
}

.alert {
  display: flex;
  align-items: flex-start;
  gap: var(--s-3);
  padding: var(--s-4);
  border-radius: var(--r-lg);
  font-size: var(--t-body);
  line-height: 1.55;
}

.alert--error {
  background: color-mix(in oklab, var(--danger, #e03e3e) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger, #e03e3e) 25%, transparent);
  color: var(--ink);
}

.alert--warning {
  background: color-mix(in oklab, var(--warning, #e0a800) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--warning, #e0a800) 25%, transparent);
  color: var(--ink);
}

.alert--warning > div:first-child {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  flex-wrap: wrap;
  width: 100%;
}

@media (max-width: 639px) {
  .alert--warning > div:first-child {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>