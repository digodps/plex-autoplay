// Teste de contraste dos tokens (WCAG 1.4.3 texto / 1.4.11 não-texto).
// Roda sem dependências: node --test tests/
import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const cssPath = join(dirname(fileURLToPath(import.meta.url)), "..", "src", "styles", "tokens.css");
const css = readFileSync(cssPath, "utf8");

function tokensFrom(selector) {
  const start = css.indexOf(selector);
  assert.notEqual(start, -1, `bloco ${selector} ausente em tokens.css`);
  const open = css.indexOf("{", start);
  const close = css.indexOf("}", open);
  const body = css.slice(open + 1, close);
  const map = {};
  for (const line of body.split("\n")) {
    const match = line.match(/^\s*(--[a-z-]+)\s*:\s*(#[0-9a-fA-F]{3,8})\s*;/);
    if (match) map[match[1]] = match[2];
  }
  return map;
}

const themes = {
  light: tokensFrom(":root"),
  dark: tokensFrom('[data-theme="dark"]'),
};

function luminance(hex) {
  const clean = hex.replace("#", "");
  const full =
    clean.length === 3
      ? clean
          .split("")
          .map((c) => c + c)
          .join("")
      : clean.slice(0, 6);
  const [r, g, b] = [0, 2, 4].map((i) => parseInt(full.slice(i, i + 2), 16) / 255);
  const channel = (c) => (c <= 0.03928 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4);
  return 0.2126 * channel(r) + 0.7152 * channel(g) + 0.0722 * channel(b);
}

function contrast(a, b) {
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
  return (hi + 0.05) / (lo + 0.05);
}

// [foreground, background, mínimo, descrição]
const textPairs = [
  ["--text", "--bg", 4.5],
  ["--text", "--surface", 4.5],
  ["--text-secondary", "--bg", 4.5],
  ["--text-tertiary", "--bg", 4.5],
  ["--text-tertiary", "--surface-raised", 4.5],
  ["--brand-strong", "--rail-bg", 4.5],
  ["--brand-strong", "--brand-soft", 4.5],
  ["--danger", "--surface", 4.5],
  ["--success", "--surface", 4.5],
  ["--warning", "--surface", 4.5],
];

const nonTextPairs = [
  ["--brand", "--rail-bg", 3],
  ["--focus-ring", "--bg", 3],
  ["--border-input", "--surface", 3],
];

for (const [themeName, tokens] of Object.entries(themes)) {
  test(`tokens de marca declarados no tema ${themeName}`, () => {
    for (const token of ["--brand", "--brand-strong", "--brand-soft", "--rail-bg", "--rail-border"]) {
      assert.ok(tokens[token], `${token} ausente no tema ${themeName}`);
    }
  });

  for (const [fg, bg, min] of textPairs) {
    test(`${themeName}: ${fg} sobre ${bg} ≥ ${min}:1`, () => {
      const ratio = contrast(tokens[fg], tokens[bg]);
      assert.ok(
        ratio >= min,
        `${themeName}: ${fg} (${tokens[fg]}) sobre ${bg} (${tokens[bg]}) = ${ratio.toFixed(2)}:1, mínimo ${min}:1`,
      );
    });
  }

  for (const [fg, bg, min] of nonTextPairs) {
    test(`${themeName}: ${fg} sobre ${bg} ≥ ${min}:1 (não-texto)`, () => {
      const ratio = contrast(tokens[fg], tokens[bg]);
      assert.ok(
        ratio >= min,
        `${themeName}: ${fg} (${tokens[fg]}) sobre ${bg} (${tokens[bg]}) = ${ratio.toFixed(2)}:1, mínimo ${min}:1`,
      );
    });
  }
}
