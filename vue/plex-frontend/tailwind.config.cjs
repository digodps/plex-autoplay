const withMT = require("@material-tailwind/html/utils/withMT");

module.exports = withMT({
  content: ["./index.html", "./src/**/*.{vue,js}"],
  // Preflight desligado: o reset do Tailwind quebraria as telas existentes,
  // que ainda usam CSS próprio (ver docs/designs/sidebar-canva-material-tailwind.md).
  corePlugins: { preflight: false },
  theme: {
    extend: {
      colors: {
        brand: "var(--brand)",
        "brand-strong": "var(--brand-strong)",
        "brand-soft": "var(--brand-soft)",
        rail: "var(--rail-bg)",
        "rail-border": "var(--rail-border)",
        surface: "var(--surface)",
        ink: "var(--text)",
        "ink-soft": "var(--text-secondary)",
      },
    },
  },
});
