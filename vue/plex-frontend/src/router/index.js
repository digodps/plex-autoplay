import { createRouter, createWebHistory } from "vue-router";
import { useI18n } from "vue-i18n";
import SigninView from "../views/Authentication/SigninView.vue";
import SeriesView from "../views/Series/SeriesView.vue";

const routes = [
  { path: "/", redirect: "/dashboard" },
  {
    path: "/login",
    name: "login",
    component: SigninView,
    meta: { titleKey: "auth.login" },
  },
  {
    path: "/setup",
    name: "setup",
    component: () => import("../views/Authentication/SetupView.vue"),
    meta: { titleKey: "auth.setupTitle" },
  },
  {
    path: "/dashboard",
    name: "dashboard",
    component: () => import("../views/Dashboard/DashboardView.vue"),
    meta: { titleKey: "nav.dashboard" },
  },
  {
    path: "/libraries",
    name: "libraries",
    component: () => import("../views/Libraries/LibrariesView.vue"),
    meta: { titleKey: "nav.libraries" },
  },
  {
    path: "/libraries/:key/series",
    name: "series",
    component: SeriesView,
    props: true,
    meta: { titleKey: "nav.series" },
  },
  {
    path: "/series/:seriesKey",
    name: "series-detail",
    component: () => import("../views/SeriesDetail/SeriesPage.vue"),
    props: true,
    meta: { titleKey: "nav.series" },
  },
  {
    path: "/series/:seriesKey/markers",
    name: "series-markers",
    component: () => import("../views/Markers/MarkerEditorView.vue"),
    meta: { titleKey: "nav.markers" },
  },

  {
    path: "/logs",
    name: "logs",
    component: () => import("../views/Logs/LogsView.vue"),
    meta: { titleKey: "nav.logs" },
  },
  {
    path: "/sync",
    name: "sync",
    component: () => import("../views/SyncView.vue"),
    meta: { titleKey: "nav.sync" },
  },
  {
    path: "/settings",
    name: "settings",
    component: () => import("../views/Pages/SettingsView.vue"),
    meta: { titleKey: "nav.settings" },
  },
  {
    path: "/sessions",
    name: "sessions",
    component: () => import("../views/Sessions/SessionsView.vue"),
    meta: { titleKey: "nav.sessions" },
  },
  { path: "/ajustes", redirect: "/settings" },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior() {
    return { top: 0 };
  },
});

router.beforeEach(async (to) => {
  const { t } = useI18n();

  // Update document title and lang attribute
  if (to.name !== "login" && to.name !== "setup") {
    const title = to.meta.titleKey ? t(to.meta.titleKey) : "Plex Autoplay";
    document.title = `${title} | Plex Autoplay`;
  }

  // Update html lang attribute based on current locale
  const locale = localStorage.getItem("app.locale") || "pt-BR";
  document.documentElement.lang = locale;

  // Setup inicial é público e tem prioridade sobre o guard de sessão.
  const setupStatus = await fetch("/api/auth/setup/status")
    .then((res) => (res.ok ? res.json() : null))
    .catch(() => null);

  if (setupStatus?.setupRequired && to.name !== "setup") {
    return { name: "setup" };
  }
  if (setupStatus && !setupStatus.setupRequired && to.name === "setup") {
    return { name: "login" };
  }

  if (to.name === "login" || to.name === "setup") return true;

  // Session cookie is HttpOnly on the backend; ask the API whether we're logged in.
  const res = await fetch("/api/status", { credentials: "include" });
  if (!res.ok) {
    return { name: "login", query: { redirect: to.fullPath } };
  }
  return true;
});

export default router;
