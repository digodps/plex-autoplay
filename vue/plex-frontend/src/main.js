import { createApp } from "vue";
import { createPinia } from "pinia";
import "./styles/app.css";
import App from "./App.vue";
import router from "./router";
import { useThemeStore } from "./stores/theme";

const app = createApp(App);
app.use(createPinia());
app.use(router);

// Apply theme before mount to avoid FOUC
const themeStore = useThemeStore();
themeStore.applyTheme();

router.isReady().then(() => {
  app.mount("#app");
});