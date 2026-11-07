import { test } from "node:test";
import assert from "node:assert/strict";
import { buildBreadcrumb } from "../src/lib/breadcrumb.js";

const libraries = [
  { key: "1", title: "Animes" },
  { key: "2", title: "Filmes" },
];

test("dashboard mostra só Início", () => {
  const items = buildBreadcrumb({ name: "dashboard" }, libraries);
  assert.deepEqual(items.map((i) => [i.label, i.isCurrent]), [["Início", true]]);
});

test("séries da biblioteca: Início > título dinâmico", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "1" } }, libraries);
  assert.deepEqual(items.map((i) => i.label), ["Início", "Animes"]);
  assert.deepEqual(items[0].to, { name: "dashboard" });
  assert.equal(items[1].isCurrent, true);
});

test("fallback para query quando biblioteca não carregou", () => {
  const items = buildBreadcrumb(
    { name: "series", params: { key: "9" }, query: { libraryTitle: "Animes" } },
    [],
  );
  assert.deepEqual(items.map((i) => i.label), ["Início", "Animes"]);
});

test("biblioteca desconhecida após loading", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, []);
  assert.equal(items.at(-1).label, "Biblioteca indisponível");
});

test("loading não inventa título", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, [], { loading: true });
  assert.deepEqual(items.map((i) => i.label), ["Início"]);
});

test("marcadores: Início > biblioteca > série, biblioteca linkável", () => {
  const items = buildBreadcrumb(
    {
      name: "series-markers",
      params: { seriesKey: "42" },
      query: { libraryKey: "1", libraryTitle: "Animes", seriesTitle: "One Piece" },
    },
    libraries,
  );
  assert.deepEqual(items.map((i) => i.label), ["Início", "Animes", "One Piece"]);
  assert.deepEqual(items[1].to, { name: "series", params: { key: "1" } });
  assert.equal(items.at(-1).isCurrent, true);
});

test("sem separador pendurado: nenhum item vazio", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, [], { loading: true });
  assert.ok(items.every((i) => i.label));
});
