import { test } from "node:test";
import assert from "node:assert/strict";
import { createPinia, setActivePinia } from "pinia";

const store = new Map();
globalThis.localStorage = {
  getItem: (k) => (store.has(k) ? store.get(k) : null),
  setItem: (k, v) => store.set(k, String(v)),
};

const { useSidebarStore } = await import("../src/stores/sidebar.js");

test("compact mode toggles and persists to localStorage", () => {
  setActivePinia(createPinia());
  const sidebar = useSidebarStore();
  assert.equal(sidebar.isCollapsed, false);

  sidebar.toggleCollapsed();
  assert.equal(sidebar.isCollapsed, true);
  assert.equal(store.get("sidebar.collapsed"), "true");

  setActivePinia(createPinia());
  assert.equal(useSidebarStore().isCollapsed, true, "restores on load");
});
