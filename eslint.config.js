import js from "@eslint/js";
import pluginVue from "eslint-plugin-vue";

export default [
  js.configs.recommended,
  ...pluginVue.configs["flat/recommended"],
  {
    languageOptions: {
      ecmaVersion: "latest",
      sourceType: "module",
      globals: {
        window: "readonly",
        document: "readonly",
        fetch: "readonly",
        localStorage: "readonly",
        console: "readonly",
        URLSearchParams: "readonly",
        setTimeout: "readonly",
        clearTimeout: "readonly",
        setInterval: "readonly",
        clearInterval: "readonly",
        EventSource: "readonly",
        process: "readonly",
        Buffer: "readonly",
        location: "readonly",
        requestAnimationFrame: "readonly",
        navigator: "readonly",
      },
    },
    rules: {
      "no-unused-vars": ["error", { argsIgnorePattern: "^_" }],
      // Regras de formatação do vue/* conflitam com Prettier (fonte de verdade
      // de formatação). Desligamos as puramente estéticas para não brigarem.
      "vue/max-attributes-per-line": "off",
      "vue/singleline-html-element-content-newline": "off",
      "vue/multiline-html-element-content-newline": "off",
      "vue/html-self-closing": "off",
      "vue/html-indent": "off",
      "vue/attributes-order": "warn",
      // Permitir componentes de UI genéricos com nome de uma palavra (ex: Breadcrumb, Button, Icon)
      "vue/multi-word-component-names": "off",
    },
  },
  {
    ignores: [
      "src/PlexAutoplay.Web/wwwroot/**",
      "vue/plex-frontend/dist/**",
      "**/node_modules/**",
      "test-results/**",
      "playwright-report/**",
      "test-artifacts/**",
    ],
  },
  {
    // Scripts CommonJS (Playwright E2E e mock do Plex)
    files: ["tests/**/*.js", "tests/e2e/**/*.js", "scripts/**/*.js"],
    languageOptions: {
      sourceType: "commonjs",
      globals: {
        require: "readonly",
        module: "writable",
        exports: "writable",
        __dirname: "readonly",
        process: "readonly",
        Buffer: "readonly",
        URL: "readonly",
      },
    },
  },
];
