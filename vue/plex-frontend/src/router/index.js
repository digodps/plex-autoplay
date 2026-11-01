import { createRouter, createWebHistory } from "vue-router";
import { getToken } from "../api";
import LoginView from "../views/LoginView.vue";
import SyncView from "../views/SyncView.vue";

const routes = [
  {
    path: "/",
    redirect: "/dashboard",
  },
  {
    path: "/login",
    name: "login",
    component: LoginView,
  },
  {
    path: "/dashboard",
    name: "dashboard",
    component: () => import("../views/DashboardView.vue"),
  },
  {
    path: "/libraries",
    name: "libraries",
    component: () => import("../views/LibrariesView.vue"),
  },
  {
    path: "/libraries/:key/series",
    name: "series",
    component: () => import("../views/SeriesView.vue"),
    props: true,
  },
  {
    path: "/series/:seriesKey/markers",
    name: "series-markers",
    component: () => import("../views/MarkerEditorView.vue"),
  },
  {
    name: "sync",
    path: "/sync",
    component: SyncView,
  },
  {
    path: "/settings",
    name: "settings",
    component: () => import("../views/SettingsView.vue"),
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

// Guarda: todas as páginas exigem autenticação; o destino original é preservado
// para retorno após o login. Como o cookie auth_token é HttpOnly, o frontend
// usa `credentials: 'include'` para que o navegador envie o cookie automaticamente.
router.beforeEach(async (to) => {
  if (to.name === "login") return true;

  const res = await fetch("/api/status", { credentials: "include" });
  if (!res.ok) {
    return { name: "login", query: { redirect: to.fullPath } };
  }
  return true;
});

export default router;
