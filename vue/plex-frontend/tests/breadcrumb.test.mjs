import { test } from "node:test";
import assert from "node:assert/strict";
import { buildBreadcrumb } from "../src/lib/breadcrumb.js";

const libraries = [
  { key: "1", title: "Animes" },
  { key: "2", title: "Filmes" },
];

const home = { label: "Início", to: { name: "dashboard" }, isCurrent: false };
const librariesItem = { label: "Bibliotecas", to: { name: "libraries" }, isCurrent: false };

function contracts(items) {
  return items.map(({ label, to, isCurrent }) => ({ label, to, isCurrent }));
}

test("dashboard mostra Início, com destino e marcado como atual", () => {
  const items = buildBreadcrumb({ name: "dashboard" }, libraries);
  assert.deepEqual(contracts(items), [
    { label: "Início", to: { name: "dashboard" }, isCurrent: true },
  ]);
});

test("libraries mostra Início > Bibliotecas", () => {
  const items = buildBreadcrumb({ name: "libraries" }, libraries);
  assert.deepEqual(contracts(items), [
    home,
    { label: "Bibliotecas", to: { name: "libraries" }, isCurrent: true },
  ]);
});

test("séries da biblioteca: Início > Bibliotecas > título", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "1" } }, libraries);
  assert.deepEqual(contracts(items), [
    home,
    { ...librariesItem, isCurrent: false },
    { label: "Animes", to: undefined, isCurrent: true },
  ]);
});

test("fallback para query quando biblioteca não carregou", () => {
  const items = buildBreadcrumb(
    { name: "series", params: { key: "9" }, query: { libraryTitle: "Animes" } },
    [],
  );
  assert.deepEqual(contracts(items), [
    home,
    librariesItem,
    { label: "Animes", to: undefined, isCurrent: true },
  ]);
});

test("biblioteca desconhecida após loading", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, []);
  assert.deepEqual(contracts(items), [
    home,
    librariesItem,
    { label: "Biblioteca indisponível", to: undefined, isCurrent: true },
  ]);
});

test("loading mostra Início > Bibliotecas sem inventar título", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, [], { loading: true });
  assert.deepEqual(contracts(items), [
    home,
    { label: "Bibliotecas", to: { name: "libraries" }, isCurrent: true },
  ]);
});

test("marcadores: Início > Bibliotecas > biblioteca > série", () => {
  const items = buildBreadcrumb(
    {
      name: "series-markers",
      params: { seriesKey: "42" },
      query: { libraryKey: "1", libraryTitle: "Animes", seriesTitle: "Aosh" },
    },
    libraries,
  );
  assert.deepEqual(contracts(items), [
    home,
    librariesItem,
    { label: "Animes", to: { name: "series", params: { key: "1" } }, isCurrent: false },
    { label: "Aosh", to: undefined, isCurrent: true },
  ]);
});

test("deep link de marcadores sem metadados: Início > Bibliotecas > Série", () => {
  const items = buildBreadcrumb(
    { name: "series-markers", params: { seriesKey: "42" } },
    libraries,
  );
  assert.deepEqual(contracts(items), [
    home,
    librariesItem,
    { label: "Série", to: undefined, isCurrent: true },
  ]);
});

test("rota desconhecida usa meta.title", () => {
  const items = buildBreadcrumb({ name: "settings", meta: { title: "Configurações" } }, libraries);
  assert.deepEqual(contracts(items), [
    home,
    { label: "Configurações", to: undefined, isCurrent: true },
  ]);
});

test("rota desconhecida sem título usa Página", () => {
  const items = buildBreadcrumb({ name: "unknown" }, libraries);
  assert.deepEqual(contracts(items), [
    home,
    { label: "Página", to: undefined, isCurrent: true },
  ]);
});

test("nenhum item vazio e último sempre atual", () => {
  const roads = [
    { name: "dashboard" },
    { name: "libraries" },
    { name: "series", params: { key: "1" } },
    { name: "series", params: { key: "9" } },
    { name: "series-markers", params: { seriesKey: "42" } },
    { name: "unknown" },
  ];
  for (const route of roads) {
    const items = buildBreadcrumb(route, libraries);
    assert.ok(items.every((item) => item.label));
    assert.equal(items.filter((item) => item.isCurrent).length, 1);
    assert.equal(items.at(-1).isCurrent, true);
  }
});
