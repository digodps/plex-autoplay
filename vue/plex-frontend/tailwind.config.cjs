module.exports = {
  content: ["./index.html", "./src/**/*.{vue,js}"],
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
};
