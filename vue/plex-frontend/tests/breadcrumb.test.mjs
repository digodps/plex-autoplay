import { test } from "node:test";
import assert from "node:assert/strict";
import { buildBreadcrumb } from "../src/lib/breadcrumb.js";

const libraries = [
  { key: "1", title: "Animes" },
  { key: "2", title: "Filmes" },
];

const librariesItem = { label: "Bibliotecas", to: { name: "libraries" }, isCurrent: false };

function contracts(items) {
  return items.map(({ label, to, isCurrent }) => ({ label, to, isCurrent }));
}

test("dashboard não exibe trilha", () => {
  assert.deepEqual(buildBreadcrumb({ name: "dashboard" }, libraries), []);
});

test("libraries mostra Bibliotecas", () => {
  const items = buildBreadcrumb({ name: "libraries" }, libraries);
  assert.deepEqual(contracts(items), [
    { label: "Bibliotecas", to: { name: "libraries" }, isCurrent: true },
  ]);
});

test("séries da biblioteca: Bibliotecas > título", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "1" } }, libraries);
  assert.deepEqual(contracts(items), [
    librariesItem,
    { label: "Animes", to: undefined, isCurrent: true },
  ]);
});

test("fallback para query quando biblioteca não carregou", () => {
  const items = buildBreadcrumb(
    { name: "series", params: { key: "9" }, query: { libraryTitle: "Animes" } },
    [],
  );
  assert.deepEqual(contracts(items), [
    librariesItem,
    { label: "Animes", to: undefined, isCurrent: true },
  ]);
});

test("biblioteca desconhecida após loading", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, []);
  assert.deepEqual(contracts(items), [
    librariesItem,
    { label: "Biblioteca indisponível", to: undefined, isCurrent: true },
  ]);
});

test("loading mostra Bibliotecas sem inventar título", () => {
  const items = buildBreadcrumb({ name: "series", params: { key: "9" } }, [], { loading: true });
  assert.deepEqual(contracts(items), [
    { label: "Bibliotecas", to: { name: "libraries" }, isCurrent: true },
  ]);
});

test("marcadores: Bibliotecas > biblioteca > série", () => {
  const items = buildBreadcrumb(
    {
      name: "series-markers",
      params: { seriesKey: "42" },
      query: { libraryKey: "1", libraryTitle: "Animes", seriesTitle: "Aosh" },
    },
    libraries,
  );
  assert.deepEqual(contracts(items), [
    librariesItem,
    { label: "Animes", to: { name: "series", params: { key: "1" } }, isCurrent: false },
    { label: "Aosh", to: undefined, isCurrent: true },
  ]);
});

test("deep link de marcadores sem metadados: Bibliotecas > Biblioteca", () => {
  const items = buildBreadcrumb(
    { name: "series-markers", params: { seriesKey: "42" } },
    libraries,
  );
  assert.deepEqual(contracts(items), [
    librariesItem,
    { label: "Biblioteca", to: undefined, isCurrent: true },
  ]);
});

test("deep link de marcadores com bibliotecas ainda carregando: Bibliotecas > Biblioteca", () => {
  const items = buildBreadcrumb(
    { name: "series-markers", params: { seriesKey: "42" } },
    [],
    { loading: true },
  );
  assert.deepEqual(contracts(items), [
    librariesItem,
    { label: "Biblioteca", to: undefined, isCurrent: true },
  ]);
});

test("rota conhecida usa meta.title sem Início", () => {
  const items = buildBreadcrumb({ name: "settings", meta: { title: "Configurações" } }, libraries);
  assert.deepEqual(contracts(items), [
    { label: "Configurações", to: undefined, isCurrent: true },
  ]);
});

test("rota sem título usa Página", () => {
  const items = buildBreadcrumb({ name: "unknown" }, libraries);
  assert.deepEqual(contracts(items), [
    { label: "Página", to: undefined, isCurrent: true },
  ]);
});

test("trilha não vazia: nenhum item vazio, um único atual no fim", () => {
  const roads = [
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
