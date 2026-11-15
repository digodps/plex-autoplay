import { createApp } from "vue";
import { createPinia } from "pinia";
import i18n from "./i18n";
import "./styles/app.css";
import "./styles/style.css";
import App from "./App.vue";
import router from "./router";
import { useThemeStore } from "./stores/theme";
import { useLocaleStore } from "./stores/locale";

const app = createApp(App);
app.use(createPinia());
app.use(i18n);
app.use(router);

// Initialize locale store (detects/saves locale, loads messages)
const localeStore = useLocaleStore();
localeStore.init();

// Apply theme before mount to avoid FOUC
const themeStore = useThemeStore();
themeStore.applyTheme();

router.isReady().then(() => {
  app.mount("#app");
});