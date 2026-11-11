<<<<<<< HEAD
import { fileURLToPath, URL } from "node:url";
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
<<<<<<< HEAD
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("./src", import.meta.url)),
    },
  },
  esbuild: {
    drop: ["debugger"],
=======
  esbuild: {
    drop: ["debugger"],
    // Keep console.log in production for debugging
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
    pure: [],
  },
  build: {
    minify: "esbuild",
<<<<<<< HEAD
  },
});
=======
    terserOptions: {
      compress: {
        drop_console: false,
        drop_debugger: true,
      },
    },
  },
});
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
