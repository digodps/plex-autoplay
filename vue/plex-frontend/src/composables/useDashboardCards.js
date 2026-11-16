import { ref, computed, onMounted, onUnmounted } from "vue";
import { useI18n } from "vue-i18n";
import { useDashboardStore } from "../stores/dashboard";
import { apiFetch } from "../api";

export function useDashboardCards() {
  const { t } = useI18n();
  const store = useDashboardStore();

  const statusVariant = computed(() => (status) => {
    if (status === "failed") return "danger";
    if (status === "running") return "warning";
    return "success";
  });

  // Skip sessions & skips card state
  const skipSessionsCount = ref(0);
  const lastSkipAt = ref(null);
  let skipPollTimer = null;
  const SKIP_POLL_INTERVAL = 5000;

  async function fetchSkipStatus() {
    try {
      const res = await apiFetch("/api/sessions/skip-status");
      if (res.ok) {
        const data = await res.json();
        skipSessionsCount.value = data.activeSessions?.length ?? 0;
        if (data.recentSkips?.length > 0) {
          const last = data.recentSkips[0];
          const locale = localStorage.getItem("app.locale") || "pt-BR";
          lastSkipAt.value = new Date(last.timestamp).toLocaleString(
            locale === "pt-BR" ? "pt-BR" : "en-US",
            { dateStyle: "short", timeStyle: "short" }
          );
        }
      }
    } catch (e) {
      console.warn("[Dashboard] fetchSkipStatus failed:", e);
    }
  }

  function startSkipPolling() {
    if (skipPollTimer) return;
    fetchSkipStatus();
    skipPollTimer = setInterval(fetchSkipStatus, SKIP_POLL_INTERVAL);
  }

  function stopSkipPolling() {
    if (skipPollTimer) {
      clearInterval(skipPollTimer);
      skipPollTimer = null;
    }
  }

  onMounted(startSkipPolling);
  onUnmounted(stopSkipPolling);

  const serviceCard = computed(() => {
    const h = store.serviceHealth;
    if (!h) return { loading: true };
    const variant = statusVariant.value(h.lastJob?.status);
    return {
      icon: "ui-server",
      label: t("dashboard.serviceHealth"),
      value: h.uptime,
      breakdown: [
        { label: t("dashboard.cpu"), value: h.cpuPercent },
        { label: t("dashboard.ram"), value: `${h.memMB} MB` },
      ],
      badge: h.lastJob ? { text: h.lastJob.status, variant } : { text: t("dashboard.online"), variant: "success" },
      href: "/settings",
      loading: store.loading && !h,
      error: store.partialErrors.status,
    };
  });

  const syncCard = computed(() => {
    const h = store.serviceHealth;
    if (!h) return { loading: true };
    const job = h.lastJob;
    if (!job) {
      return {
        icon: "ui-refresh-cw",
        label: t("dashboard.lastSync"),
        value: t("dashboard.neverRun"),
        badge: { text: t("dashboard.pending"), variant: "neutral" },
        href: "/logs",
        loading: store.loading && !h,
        error: store.partialErrors.status,
      };
    }
    const locale = localStorage.getItem("app.locale") || "pt-BR";
    const updatedAt = job.updatedAt
      ? new Date(job.updatedAt).toLocaleString(locale === "pt-BR" ? "pt-BR" : "en-US", { dateStyle: "short", timeStyle: "short" })
      : "—";
    const variant = statusVariant.value(job.status);
    return {
      icon: "ui-refresh-cw",
      label: t("dashboard.lastSync"),
      value: updatedAt,
      breakdown: [
        { label: t("dashboard.processed"), value: job.processed.toLocaleString(locale), class: "text-success" },
        { label: t("dashboard.skipped"), value: job.skipped.toLocaleString(locale) },
        { label: t("dashboard.errors"), value: job.errors.toLocaleString(locale), class: job.errors > 0 ? "text-danger" : "" },
      ],
      badge: { text: job.status, variant },
      href: "/logs",
      loading: store.loading && !h,
      error: store.partialErrors.status,
    };
  });

  const librariesCard = computed(() => {
    const s = store.librariesSummary;
    const m = store.markersSummary;
    const locale = localStorage.getItem("app.locale") || "pt-BR";
    return {
      icon: "ui-database",
      label: t("nav.libraries"),
      value: s.count.toLocaleString(locale),
      breakdown: [
        { label: t("markers.intro"), value: m.byKind.intro.toLocaleString(locale) },
        { label: t("markers.credits"), value: m.byKind.credits.toLocaleString(locale) },
        { label: t("markers.detected"), value: m.byOrigin.detected.toLocaleString(locale) },
        { label: t("markers.manual"), value: m.byOrigin.manual.toLocaleString(locale) },
      ],
      badge: s.count ? { text: `${s.count} ${t("common.libs")}`, variant: "neutral" } : { text: t("dashboard.empty"), variant: "warning" },
      href: "/libraries",
      loading: store.loading && (!store.librariesData.length || !store.markersData.length),
      error: store.partialErrors.libraries || store.partialErrors.markers,
    };
  });

  const skipSessionsCard = computed(() => ({
    icon: "ui-tv",
    label: t("nav.sessions"),
    value: skipSessionsCount.value === 0 ? "—" : skipSessionsCount.value.toString(),
    breakdown: [
      { label: t("sessions.activeSessions"), value: skipSessionsCount.value.toString() },
      { label: t("sessions.lastSkip"), value: lastSkipAt.value ?? "—" },
    ],
    badge: skipSessionsCount.value > 0 ? { text: `${skipSessionsCount.value} ${t("sessions.active")}`, variant: "warning" } : { text: t("sessions.noSessions"), variant: "neutral" },
    href: "/sessions",
    loading: store.loading,
    error: false,
  }));

  return {
    serviceCard,
    syncCard,
    librariesCard,
    skipSessionsCard,
  };
}