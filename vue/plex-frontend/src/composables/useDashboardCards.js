import { computed } from "vue";
import { useDashboardStore } from "../stores/dashboard";

/**
 * Composable para configurar os cards do dashboard.
 * Extrai a lógica de mapeamento store -> props do StatusCard.
 */
export function useDashboardCards() {
  const store = useDashboardStore();

  const statusVariant = computed(() => (status) => {
    if (status === "failed") return "danger";
    if (status === "running") return "warning";
    return "success";
  });

  /** Card: Serviço (uptime, CPU, RAM) */
  const serviceCard = computed(() => {
    const h = store.serviceHealth;
    if (!h) return { loading: true };
    const variant = statusVariant.value(h.lastJob?.status);
    return {
      icon: "library",
      label: "Serviço",
      value: h.uptime,
      breakdown: [
        { label: "CPU", value: h.cpuPercent },
        { label: "RAM", value: `${h.memMB} MB` },
      ],
      badge: h.lastJob
        ? { text: h.lastJob.status, variant }
        : { text: "Online", variant: "success" },
      href: "/settings",
      loading: store.loading && !h,
      error: store.partialErrors.status,
    };
  });

  /** Card: Última Sincronização */
  const syncCard = computed(() => {
    const h = store.serviceHealth;
    if (!h) return { loading: true };
    const job = h.lastJob;
    if (!job) {
      return {
        icon: "sync",
        label: "Última Sincronização",
        value: "Nunca executado",
        badge: { text: "Pendente", variant: "neutral" },
        href: "/sync",
        loading: store.loading && !h,
        error: store.partialErrors.status,
      };
    }
    const updatedAt = job.updatedAt
      ? new Date(job.updatedAt).toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" })
      : "—";
    const variant = statusVariant.value(job.status);
    return {
      icon: "sync",
      label: "Última Sincronização",
      value: updatedAt,
      breakdown: [
        {
          label: "Processados",
          value: job.processed.toLocaleString("pt-BR"),
          class: "text-success",
        },
        { label: "Pulados", value: job.skipped.toLocaleString("pt-BR") },
        {
          label: "Erros",
          value: job.errors.toLocaleString("pt-BR"),
          class: job.errors > 0 ? "text-danger" : "",
        },
      ],
      badge: { text: job.status, variant },
      href: "/sync",
      loading: store.loading && !h,
      error: store.partialErrors.status,
    };
  });

  /** Card: Bibliotecas */
  const librariesCard = computed(() => {
    const s = store.librariesSummary;
    return {
      icon: "library",
      label: "Bibliotecas",
      value: s.count.toLocaleString("pt-BR"),
      breakdown: [{ label: "Bibliotecas", value: s.count }],
      badge: s.count
        ? { text: `${s.count} libs`, variant: "neutral" }
        : { text: "Vazio", variant: "warning" },
      href: "/libraries",
      loading: store.loading && !store.librariesData.length,
      error: store.partialErrors.libraries,
    };
  });

  /** Card: Marcadores */
  const markersCard = computed(() => {
    const m = store.markersSummary;
    return {
      icon: "bookmark",
      label: "Marcadores",
      value: m.total.toLocaleString("pt-BR"),
      breakdown: [
        { label: "Intro", value: m.byKind.intro.toLocaleString("pt-BR") },
        { label: "Créditos", value: m.byKind.credits.toLocaleString("pt-BR") },
        { label: "Detectados", value: m.byOrigin.detected.toLocaleString("pt-BR") },
        { label: "Manuais", value: m.byOrigin.manual.toLocaleString("pt-BR") },
      ],
      badge: m.total
        ? { text: "Cobertura", variant: "neutral" }
        : { text: "Sem dados", variant: "warning" },
      href: "/series",
      loading: store.loading && !store.markersData.length,
      error: store.partialErrors.markers,
    };
  });

  return {
    serviceCard,
    syncCard,
    librariesCard,
    markersCard,
  };
}
