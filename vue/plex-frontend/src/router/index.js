import { createRouter, createWebHistory } from "vue-router";
<<<<<<< HEAD
import SigninView from "../views/Authentication/SigninView.vue";
=======
import { getToken } from "../api";
import LoginView from "../views/LoginView.vue";
import SyncView from "../views/SyncView.vue";
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

const routes = [
  { path: "/", redirect: "/dashboard" },
  {
<<<<<<< HEAD
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
=======
    path: "/",
    redirect: "/dashboard",
  },
  {
    path: "/login",
    name: "login",
    component: LoginView,
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  },
  {
    path: "/dashboard",
    name: "dashboard",
<<<<<<< HEAD
    component: () => import("../views/Dashboard/DashboardView.vue"),
    meta: { title: "Dashboard" },
=======
    component: () => import("../views/DashboardView.vue"),
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  },
  {
    path: "/libraries",
    name: "libraries",
<<<<<<< HEAD
    component: () => import("../views/Libraries/LibrariesView.vue"),
    meta: { title: "Bibliotecas" },
=======
    component: () => import("../views/LibrariesView.vue"),
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  },
  {
    path: "/libraries/:key/series",
    name: "series",
<<<<<<< HEAD
    component: () => import("../views/Series/SeriesView.vue"),
    props: true,
    meta: { title: "Biblioteca" },
=======
    component: () => import("../views/SeriesView.vue"),
    props: true,
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  },
  {
    path: "/series/:seriesKey/markers",
    name: "series-markers",
<<<<<<< HEAD
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
=======
    component: () => import("../views/MarkerEditorView.vue"),
  },
  {
    name: "sync",
    path: "/sync",
    component: SyncView,
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
  },
  {
    path: "/settings",
    name: "settings",
<<<<<<< HEAD
    component: () => import("../views/Pages/SettingsView.vue"),
    meta: { title: "Ajustes" },
  },
  { path: "/ajustes", redirect: "/settings" },
=======
    component: () => import("../views/SettingsView.vue"),
  },
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
<<<<<<< HEAD
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
=======
});

// Guarda: todas as páginas exigem autenticação; o destino original é preservado
// para retorno após o login.
router.beforeEach((to) => {
  if (to.name === "login") return true;

  if (!getToken()) {
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    return { name: "login", query: { redirect: to.fullPath } };
  }
  return true;
});

<<<<<<< HEAD
export default router;
=======
export default router;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
