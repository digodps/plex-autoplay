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
  <!-- Hero entrance with liquid-type moment -->
  <section class="hero hero--split" data-enter data-moment="liquid-type" style="margin-bottom: var(--s-8);">
    <div class="wrap hero__grid">
      <div class="hero-text hero__head">
        <div class="hero__brand">
          <h1 class="t-h2 mo-lt__type" style="font: var(--font-display); font-weight: var(--display-weight); letter-spacing: var(--display-tracking); margin-bottom: var(--s-2);">{{ t("dashboard.title") }}</h1>
          <p class="t-body muted" style="margin: 0;">{{ t("dashboard.subtitle") }}</p>
        </div>
      </div>
      <div class="hero-visual" style="grid-column: 9 / -1; align-self: center;">
        <div class="hero-visual__strip" aria-hidden="true">
          <svg viewBox="0 0 400 200" fill="none" xmlns="http://www.w3.org/2000/svg" preserveAspectRatio="none" style="width: 100%; height: 100%;">
            <defs>
              <linearGradient id="dashGrad1" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" style="stop-color:var(--p-accent);stop-opacity:0.25" />
                <stop offset="100%" style="stop-color:#7c3aed;stop-opacity:0.15" />
              </linearGradient>
            </defs>
            <rect x="20" y="20" width="360" height="160" rx="12" fill="url(#dashGrad1)" />
            <circle cx="200" cy="100" r="50" fill="var(--p-accent)" opacity="0.15" />
            <path d="M150 100 Q200 50 250 100 Q200 150 150 100" stroke="var(--p-accent)" stroke-width="2" fill="none" opacity="0.4" />
            <circle cx="150" cy="100" r="8" fill="var(--p-accent)" opacity="0.6" />
            <circle cx="250" cy="100" r="8" fill="#7c3aed" opacity="0.5" />
          </svg>
        </div>
      </div>
    </div>
  </section>

  <!-- Stats section - dealt moment: picture-develops -->
  <section class="section" data-moment="picture-develops" style="margin-bottom: var(--s-8);">
    <div class="wrap">
      <DataStatsOne />
    </div>
  </section>

  <!-- Status alerts -->
  <div class="wrap">
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

.hero-visual__strip {
  max-height: 96px;
  width: 100%;
}

@media (max-width: 1024px) {
  .hero--split .hero__grid {
    grid-template-columns: 1fr !important;
  }
  .hero-visual {
    display: none;
  }
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