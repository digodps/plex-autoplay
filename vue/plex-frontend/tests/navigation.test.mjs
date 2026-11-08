import { test } from "node:test";
import assert from "node:assert/strict";
import { isPathActive } from "../src/lib/navigation.js";

test("Bibliotecas ativo em rotas filhas", () => {
  assert.ok(isPathActive("/libraries/1/series", "/libraries"));
  assert.ok(isPathActive("/libraries", "/libraries"));
  assert.ok(isPathActive("/series/42/markers", "/libraries", ["/series"]));
  assert.ok(!isPathActive("/libraries-other", "/libraries"));
  assert.ok(!isPathActive("/dashboard", "/libraries", ["/series"]));
});
