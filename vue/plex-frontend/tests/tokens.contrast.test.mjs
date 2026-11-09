// Teste de contraste dos tokens de tema (WCAG 1.4.3 / 1.4.11).
// Fonte única: src/styles/tokens.css (:root e [data-theme="dark"]).
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const css = readFileSync(
  join(dirname(fileURLToPath(import.meta.url)), "..", "src", "styles", "tokens.css"),
  "utf8",
);

function tokensFrom(selector) {
  const start = css.indexOf(selector);
  assert.ok(start >= 0, `${selector} ausente em style.css`);
  const open = css.indexOf("{", start);
  const close = css.indexOf("}", open);
  const tokens = {};
  for (const m of css.slice(open + 1, close).matchAll(/(--[\w-]+):\s*(#[0-9A-Fa-f]{6})/g)) {
    tokens[m[1]] = m[2];
  }
  return tokens;
}

const themes = { light: tokensFrom(":root"), dark: tokensFrom('[data-theme="dark"]') };

function luminance(hex) {
  const full = hex.replace("#", "");
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(full.slice(i, i + 2), 16) / 255);
  const channel = (c) => (c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4);
  return 0.2126 * channel(r) + 0.7152 * channel(g) + 0.0722 * channel(b);
}

function contrast(a, b) {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
  return (hi + 0.05) / (lo + 0.05);
}

const required = ["--surface", "--border", "--text", "--text-secondary", "--accent"];
const pairs = [
  ["--text", "--surface", 4.5],
  ["--text-secondary", "--surface", 4.5],
  ["--accent", "--surface", 3],
];

for (const [name, tokens] of Object.entries(themes)) {
  test(`tokens declarados no tema ${name}`, () => {
    for (const token of required) assert.ok(tokens[token], `${token} ausente no tema ${name}`);
  });

  for (const [fg, bg, min] of pairs) {
    test(`${name}: ${fg} sobre ${bg} >= ${min}:1`, () => {
      const ratio = contrast(tokens[fg], tokens[bg]);
      assert.ok(ratio >= min, `${fg}/${bg} = ${ratio.toFixed(2)}:1`);
    });
  }
}
