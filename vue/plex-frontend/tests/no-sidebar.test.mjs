import { test } from "node:test";
import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";

const files = ["../src/components/layouts/DefaultLayout.vue", "../src/components/AppHeader.vue"];

test("shell não renderiza menu lateral", async () => {
  for (const file of files) {
    const source = await readFile(new URL(file, import.meta.url), "utf8");
    assert.doesNotMatch(source, /SidebarArea|useSidebarStore|<aside/);
  }
});
