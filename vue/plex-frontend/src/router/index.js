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
  },
  {
    path: "/series",
    name: "series-index",
    component: () => import("../views/SeriesView.vue"),
  },
  {
    path: "/series/:seriesKey/markers",
    name: "series-markers",
    component: () => import("../views/MarkerEditorView.vue"),
  },
  {
    path: "/markers",
    name: "markers",
    redirect: "/series",
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
// para retorno após o login.
router.beforeEach((to) => {
  if (to.name === "login") return true;

  if (!getToken()) {
    return { name: "login", query: { redirect: to.fullPath } };
  }
  return true;
});

export default router;
