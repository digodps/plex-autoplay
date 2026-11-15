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
  <div class="page">
    <div class="mb-6">
      <div>
        <h1 class="text-2xl font-bold text-black dark:text-white">{{ t("dashboard.title") }}</h1>
        <p class="mt-2 text-sm font-medium text-gray-600 dark:text-bodydark">
          {{ t("dashboard.subtitle") }}
        </p>
      </div>
    </div>

    <DataStatsOne />

    <div
      v-if="store.isTotalFailure"
      class="mt-8 rounded-2xl border border-meta-1/30 bg-meta-1/10 px-6 py-4 text-sm text-meta-1"
      role="alert"
    >
      <AppIcon name="warning" size="lg" class="mr-3" />
      <p class="flex-1">{{ store.error }}</p>
      <button class="btn btn-md btn-danger" :disabled="store.isLoading" @click="store.retry">
        {{ t("common.retry") }}
      </button>
    </div>

    <div
      v-else-if="store.libraryError"
      class="mt-8 rounded-2xl rounded-tl-0 rounded-tr-0 border border-meta-6/30 bg-meta-6/10 px-6 py-4 text-sm text-body dark:text-bodydark"
      role="alert"
    >
      <div class="flex flex-wrap items-center gap-3">
        <AppIcon name="warning" size="lg" class="text-meta-6" />
        <p class="flex-1 font-medium text-meta-6">{{ store.libraryError }}</p>
        <button class="btn btn-md btn-warning" :disabled="store.isLoading" @click="store.retry">
          {{ t("common.retry") }}
        </button>
      </div>
      <p class="mt-3 text-xs text-body dark:text-bodydark">
        {{ t("dashboard.plexConnectionHelp") }}
      </p>
    </div>
  </div>
</template>