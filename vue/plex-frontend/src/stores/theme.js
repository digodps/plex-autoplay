import { defineStore } from "pinia";
import { ref } from "vue";
import { apiFetch } from "../api";

const MODES = ["light", "dark", "system"];
const STORAGE_KEY = "theme.mode";
const SYSTEM_ACCENT = "#171717";

function resolveTheme(mode) {
  if (mode === "light" || mode === "dark") return mode;
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

export const useThemeStore = defineStore("theme", () => {
  const mode = ref(localStorage.getItem(STORAGE_KEY) || "system");

  function applyTheme() {
    const effective = resolveTheme(mode.value);
    document.documentElement.dataset.theme = effective;
    document.documentElement.classList.toggle("dark", effective === "dark");
    document.documentElement.style.colorScheme = effective;
  }

  function setMode(next) {
    if (!MODES.includes(next)) return;
    mode.value = next;
    localStorage.setItem(STORAGE_KEY, next);
    applyTheme();
    saveTheme().catch((e) => console.warn("Theme save error:", e));
  }

  function cycleMode() {
    const next = MODES[(MODES.indexOf(mode.value) + 1) % MODES.length];
    setMode(next);
  }

  function enterDark() {
    setMode("dark");
  }

  function enterLight() {
    setMode("light");
  }

  async function loadTheme() {
    try {
      const res = await apiFetch("/api/theme");
      if (res.ok) {
        const data = await res.json();
        if (MODES.includes(data.mode)) {
          mode.value = data.mode;
          localStorage.setItem(STORAGE_KEY, data.mode);
        }
      }
    } catch (e) {
      console.warn("Could not load theme from API:", e);
    }
    applyTheme();
  }

  async function saveTheme() {
    try {
      await apiFetch("/api/theme", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ mode: mode.value, accentColor: SYSTEM_ACCENT }),
      });
    } catch (e) {
      console.warn("Could not save theme:", e);
    }
  }

  const media = window.matchMedia("(prefers-color-scheme: dark)");
  if (media.addEventListener) {
    media.addEventListener("change", () => {
      if (mode.value === "system") applyTheme();
    });
  }

  return {
    mode,
    applyTheme,
    loadTheme,
    saveTheme,
    setMode,
    cycleMode,
    enterDark,
    enterLight,
  };
});