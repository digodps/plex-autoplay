import js from "@eslint/js";
import pluginVue from "eslint-plugin-vue";
import boundaries from "eslint-plugin-boundaries";

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
  {
    // Fronteiras da matriz §4 do design (Clean Architecture):
    // views -> composables | stores; components -> lib apenas;
    // composables -> stores; stores -> api.js; lib -> puro.
    files: ["**/src/**/*.{js,vue}"],
    plugins: { boundaries },
    settings: {
      "boundaries/elements": [
        { type: "views", pattern: "src/views/**/*" },
        { type: "components", pattern: "src/components/**/*" },
        { type: "composables", pattern: "src/composables/**/*" },
        { type: "stores", pattern: "src/stores/**/*" },
        { type: "lib", pattern: "src/lib/**/*" },
        { type: "router", pattern: "src/router/**/*" },
      ],
    },
    rules: {
      "boundaries/dependencies": [
        "error",
        {
          default: "allow",
          policies: [
            { from: "views", disallow: ["api"] },
            {
              from: "components",
              disallow: ["stores", "composables", "api", "router"],
            },
            { from: "composables", disallow: ["api", "views", "components"] },
            { from: "stores", disallow: ["composables", "views", "components"] },
            {
              from: "lib",
              disallow: ["views", "components", "composables", "stores", "api", "router"],
            },
          ],
        },
      ],
    },
  },
];
