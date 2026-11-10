import { createRouter, createWebHistory } from "vue-router";
import SigninView from "../views/Authentication/SigninView.vue";

const routes = [
  { path: "/", redirect: "/dashboard" },
  {
    path: "/login",
    name: "login",
    component: SigninView,
    meta: { title: "Entrar" },
  },
  {
    path: "/setup",
    name: "setup",
    component: () => import("../views/Authentication/SetupView.vue"),
    meta: { title: "Configuração inicial" },
  },
  {
    path: "/dashboard",
    name: "dashboard",
    component: () => import("../views/Dashboard/DashboardView.vue"),
    meta: { title: "Dashboard" },
  },
  {
    path: "/libraries",
    name: "libraries",
    component: () => import("../views/Libraries/LibrariesView.vue"),
    meta: { title: "Bibliotecas" },
  },
  {
    path: "/libraries/:key/series",
    name: "series",
    component: () => import("../views/Series/SeriesView.vue"),
    props: true,
    meta: { title: "Biblioteca" },
  },
  {
    path: "/series/:seriesKey/markers",
    name: "series-markers",
    component: () => import("../views/Markers/MarkerEditorView.vue"),
    meta: { title: "Marcadores" },
  },
  {
    path: "/sync",
    name: "sync",
    component: () => import("../views/Sync/SyncView.vue"),
    meta: { title: "Sincronizar" },
  },
  {
    path: "/logs",
    name: "logs",
    component: () => import("../views/Logs/LogsView.vue"),
    meta: { title: "Logs de processamento" },
  },
  {
    path: "/settings",
    name: "settings",
    component: () => import("../views/Pages/SettingsView.vue"),
    meta: { title: "Ajustes" },
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
  if (to.name !== "login" && to.name !== "setup") {
    document.title = `${to.meta.title ?? "Plex Autoplay"} | Plex Autoplay`;
  }

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