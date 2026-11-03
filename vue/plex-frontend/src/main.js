import { createApp } from "vue";
import { createPinia } from "pinia";
import "./styles/tokens.css";
import "./styles/base.css";
import "./styles/shell.css";
import "./styles/components/button.css";
import "./styles/components/card.css";
import "./styles/components/field.css";
import "./styles/components/table.css";
import "./styles/components/state.css";
import { getToken, setCookie } from "./lib/auth";
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

// Aplica o tema resolvido (localStorage/system) antes do mount e então
// reconcilia com a preferência persistida no backend.
const themeStore = useThemeStore();
themeStore.applyTheme();
themeStore.loadTheme().catch((e) => console.warn("Theme load error:", e));

router.isReady().then(() => {
  app.mount("#app");
});
