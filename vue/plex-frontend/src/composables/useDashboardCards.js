import { computed } from "vue";
import { useDashboardStore } from "../stores/dashboard";

<<<<<<< HEAD
=======
/**
 * Composable para configurar os cards do dashboard.
 * Extrai a lógica de mapeamento store -> props do StatusCard.
 */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export function useDashboardCards() {
  const store = useDashboardStore();

  const statusVariant = computed(() => (status) => {
    if (status === "failed") return "danger";
    if (status === "running") return "warning";
    return "success";
  });

<<<<<<< HEAD
=======
  /** Card: Serviço (uptime, CPU, RAM) */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
<<<<<<< HEAD
      badge: h.lastJob ? { text: h.lastJob.status, variant } : { text: "Online", variant: "success" },
=======
      badge: h.lastJob
        ? { text: h.lastJob.status, variant }
        : { text: "Online", variant: "success" },
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
      href: "/settings",
      loading: store.loading && !h,
      error: store.partialErrors.status,
    };
  });

<<<<<<< HEAD
=======
  /** Card: Última Sincronização */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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
<<<<<<< HEAD
        { label: "Processados", value: job.processed.toLocaleString("pt-BR"), class: "text-success" },
        { label: "Pulados", value: job.skipped.toLocaleString("pt-BR") },
        { label: "Erros", value: job.errors.toLocaleString("pt-BR"), class: job.errors > 0 ? "text-danger" : "" },
=======
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
      ],
      badge: { text: job.status, variant },
      href: "/sync",
      loading: store.loading && !h,
      error: store.partialErrors.status,
    };
  });

<<<<<<< HEAD
  const librariesCard = computed(() => {
    const s = store.librariesSummary;
    const m = store.markersSummary;
=======
  /** Card: Bibliotecas */
  const librariesCard = computed(() => {
    const s = store.librariesSummary;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    return {
      icon: "library",
      label: "Bibliotecas",
      value: s.count.toLocaleString("pt-BR"),
<<<<<<< HEAD
=======
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
      breakdown: [
        { label: "Intro", value: m.byKind.intro.toLocaleString("pt-BR") },
        { label: "Créditos", value: m.byKind.credits.toLocaleString("pt-BR") },
        { label: "Detectados", value: m.byOrigin.detected.toLocaleString("pt-BR") },
        { label: "Manuais", value: m.byOrigin.manual.toLocaleString("pt-BR") },
      ],
<<<<<<< HEAD
      badge: s.count ? { text: `${s.count} libs`, variant: "neutral" } : { text: "Vazio", variant: "warning" },
      href: "/libraries",
      loading: store.loading && (!store.librariesData.length || !store.markersData.length),
      error: store.partialErrors.libraries || store.partialErrors.markers,
=======
      badge: m.total
        ? { text: "Cobertura", variant: "neutral" }
        : { text: "Sem dados", variant: "warning" },
      href: "/series",
      loading: store.loading && !store.markersData.length,
      error: store.partialErrors.markers,
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    };
  });

  return {
    serviceCard,
    syncCard,
    librariesCard,
<<<<<<< HEAD
  };
}
=======
    markersCard,
  };
}
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
