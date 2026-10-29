import { createApp } from "vue";
import { createPinia } from "pinia";
import "./style.css";
import { getToken, setCookie } from "./api";
import App from "./App.vue";
import router from "./router";

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

// Wait for router to be ready (initial navigation complete) before mounting
// This prevents the "Redirecionando..." fallback from showing during redirect
router.isReady().then(() => {
  app.mount("#app");
});
