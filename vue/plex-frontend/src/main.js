import { createApp } from "vue";
import { createPinia } from "pinia";
import "./style.css";
import { getToken, setCookie } from "./api";
import App from "./App.vue";
import router from "./router";
import { useThemeStore } from "./stores/theme";

// Migração de token: sessões antigas guardavam o token em localStorage; move
// para o cookie (persistente, pois o usuário já evitou login anteriormente) e
// limpa o localStorage.
(function migrateTokenStorage() {
  const legacyToken = localStorage.getItem("auth_token");
  if (legacyToken && !getToken()) {
    setCookie("auth_token", legacyToken, 30);
  }
  if (legacyToken) {
    localStorage.removeItem("auth_token");
  }
})();

const app = createApp(App);

app.use(createPinia());
app.use(router);

// Load theme preferences on startup (pre-mount) so UI renders with correct theme
const themeStore = useThemeStore();
themeStore.loadTheme().catch((e) => console.warn("Theme load error:", e));

// Wait for router to be ready (initial navigation complete) before mounting
// This prevents the "Redirecionando..." fallback from showing during redirect
router.isReady().then(() => {
  app.mount("#app");
});
