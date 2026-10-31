import { createApp } from "vue";
import { createPinia } from "pinia";
import "./style.css";
import App from "./App.vue";
import router from "./router";
import { useThemeStore } from "./stores/theme";

const app = createApp(App);

app.use(createPinia());
app.use(router);

const themeStore = useThemeStore();
themeStore.loadTheme().catch((e) => console.warn("Theme load error:", e));

// Wait for router to be ready (initial navigation complete) before mounting
// This prevents the "Redirecionando..." fallback from showing during redirect
router.isReady().then(() => {
  app.mount("#app");
});
