import { fileURLToPath, URL } from "node:url";
import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  esbuild: {
    drop: ["debugger"],
    pure: [],
  },
  build: {
    outDir: "../../src/PlexAutoplay.Web/wwwroot",
    emptyOutDir: true,
    minify: "esbuild",
  },
});
