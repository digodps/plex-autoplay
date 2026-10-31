import vue from "@vitejs/plugin-vue";
import { defineConfig } from "vite";

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  esbuild: {
    drop: ["debugger"],
    // Keep console.log in production for debugging
    pure: [],
  },
  build: {
    minify: "esbuild",
    terserOptions: {
      compress: {
        drop_console: false,
        drop_debugger: true,
      },
    },
  },
});
