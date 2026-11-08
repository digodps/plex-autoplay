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

test("bibliotecas: Início > Bibliotecas atual", () => {
  const items = buildBreadcrumb({ name: "libraries" }, libraries);
  assert.deepEqual(items.map((i) => i.label), ["Início", "Bibliotecas"]);
  assert.equal(items[1].isCurrent, true);
});

test("séries: Início > Bibliotecas > biblioteca atual", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "1" } }, libraries);
  assert.deepEqual(items.map((i) => i.label), ["Início", "Bibliotecas", "Animes"]);
  assert.deepEqual(items[0].to, { name: "dashboard" });
  assert.deepEqual(items[1].to, { name: "libraries" });
  assert.equal(items[2].isCurrent, true);
  assert.equal(items[2].to, undefined);
});

test("fallback para query quando biblioteca não carregou", () => {
  const items = buildBreadcrumb(
    { name: "series", params: { key: "9" }, query: { libraryTitle: "Animes" } },
    [],
  );
  assert.deepEqual(items.map((i) => i.label), ["Início", "Bibliotecas", "Animes"]);
});

test("biblioteca desconhecida mantém Bibliotecas e rótulo genérico", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, []);
  assert.deepEqual(items.map((i) => i.label), ["Início", "Bibliotecas", "Biblioteca"]);
  assert.deepEqual(items[1].to, { name: "libraries" });
});

test("marcadores: Início > Bibliotecas > biblioteca linkável > série atual", () => {
  const items = buildBreadcrumb(
    {
      name: "series-markers",
      params: { seriesKey: "42" },
      query: { libraryKey: "1", seriesTitle: "One Piece" },
    },
    libraries,
  );
  assert.deepEqual(items.map((i) => i.label), ["Início", "Bibliotecas", "Animes", "One Piece"]);
  assert.deepEqual(items[1].to, { name: "libraries" });
  assert.deepEqual(items[2].to, { name: "series", params: { key: "1" } });
  assert.equal(items[3].isCurrent, true);
});

test("marcadores sem biblioteca ainda mostram Bibliotecas", () => {
  const items = buildBreadcrumb(
    { name: "series-markers", params: { seriesKey: "42" } },
    [],
  );
  assert.deepEqual(items.map((i) => i.label), ["Início", "Bibliotecas", "Série"]);
  assert.deepEqual(items[1].to, { name: "libraries" });
});

test("rotas genéricas usam o título da rota", () => {
  const items = buildBreadcrumb({ name: "sync", meta: { title: "Sincronizar" } }, libraries);
  assert.deepEqual(items.map((i) => i.label), ["Início", "Sincronizar"]);
});
