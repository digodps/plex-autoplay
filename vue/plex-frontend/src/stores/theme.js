import { defineStore } from "pinia";
import { ref } from "vue";
import { apiFetch } from "../api";

export const useThemeStore = defineStore("theme", () => {
  const accent = ref("#e5a00d");
  const mode = ref("system");

  async function loadTheme() {
    try {
      const res = await apiFetch("/api/theme");
      if (res.ok) {
        const data = await res.json();
        accent.value = data.accentColor || data.accent || "#e5a00d";
        mode.value = data.mode || "system";
      }
    } catch (e) {
      console.warn("Could not load theme from API:", e);
    }
  }

  async function saveTheme() {
    await apiFetch("/api/theme", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        accentColor: accent.value,
        mode: mode.value,
      }),
    });
  }

  return { accent, mode, loadTheme, saveTheme };
});