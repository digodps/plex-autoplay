import { chromium, request } from "@playwright/test";

const BASE = process.env.QA_BASE_URL || "http://127.0.0.1:8091";
const PASSWORD = process.env.QA_PASSWORD || "senha-qa-123";
const asList = (body) => (Array.isArray(body) ? body : (body.markers ?? []));
const results = [];
const check = (name, ok, extra = "") => {
  results.push({ name, ok, extra });
  console.log(`${ok ? "PASS" : "FAIL"} | ${name}${extra ? ` | ${extra}` : ""}`);
};

// ---- CRUD via API ----
const api = await request.newContext({ baseURL: BASE });
const login = await api.post("/auth/login", { data: { password: PASSWORD } });
const { token } = await login.json();
const auth = { Authorization: `Bearer ${token}` };

await api.delete("/api/markers/5001/intro", { headers: auth });
const listBefore = asList(await (await api.get("/api/markers/5001", { headers: auth })).json());
check("CRUD read inicial vazio", listBefore.length === 0, JSON.stringify(listBefore).slice(0, 120));

const created = await api.post("/api/markers", {
  headers: auth,
  data: { ratingKey: "5001", kind: "intro", startMs: 1000, endMs: 5000 },
});
check("CRUD create retorna 201", created.status() === 201, String(created.status()));

const afterCreate = asList(await (await api.get("/api/markers/5001", { headers: auth })).json());
const createdMarker = afterCreate.find((m) => m.kind === "intro");
check(
  "CRUD create persistiu intro 1000-5000",
  createdMarker?.startMs === 1000 && createdMarker?.endMs === 5000,
  JSON.stringify(createdMarker),
);

const updated = await api.put("/api/markers/5001/intro", {
  headers: auth,
  data: { startMs: 2000, endMs: 6000 },
});
check("CRUD update retorna 200", updated.status() === 200, String(updated.status()));

const afterUpdate = asList(await (await api.get("/api/markers/5001", { headers: auth })).json());
const updatedMarker = afterUpdate.find((m) => m.kind === "intro");
check(
  "CRUD update persistiu intro 2000-6000",
  updatedMarker?.startMs === 2000 && updatedMarker?.endMs === 6000,
  JSON.stringify(updatedMarker),
);

const removed = await api.delete("/api/markers/5001/intro", { headers: auth });
check("CRUD delete retorna 200", removed.status() === 200, String(removed.status()));

const afterDelete = asList(await (await api.get("/api/markers/5001", { headers: auth })).json());
const deletedMarker = afterDelete.find((m) => m.kind === "intro");
check("CRUD delete removeu marcador", deletedMarker == null, JSON.stringify(deletedMarker));

// ---- CRUD via UI ----
const browser = await chromium.launch();
const page = await (await browser.newContext({ viewport: { width: 1366, height: 1000 } })).newPage();
const pageErrors = [];
page.on("pageerror", (error) => pageErrors.push(error.message));

await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
await page.fill('input[type="password"]', PASSWORD);
await page.click('button[type="submit"]');
await page.waitForURL("**/dashboard", { timeout: 15000 });

// Marcador via UI na série 101 (2 episódios no mock): criar credits no episódio 5002.
await page.goto(`${BASE}/series/101/markers`, { waitUntil: "networkidle" });
const row = page.locator('[data-testid="marker-row-5002-credits"]');
await row.waitFor({ timeout: 15000 });
check("UI lista episódios da série", (await page.locator('[data-testid^="marker-row-"]').count()) >= 4);

await row.locator('[data-testid="marker-add"]').click();
await row.locator('[data-testid="marker-start"]').fill("3000");
await row.locator('[data-testid="marker-end"]').fill("9000");
await row.locator('[data-testid="marker-save"]').click();
await page.waitForTimeout(700);
const rowText = await row.innerText();
check("UI create mostra marcador manual", /00:03\.00/.test(rowText) && /00:09\.00/.test(rowText), rowText.replace(/\n/g, " "));

await row.locator('[data-testid="marker-edit"]').click();
await row.locator('[data-testid="marker-start"]').fill("4000");
await row.locator('[data-testid="marker-end"]').fill("10000");
await row.locator('[data-testid="marker-save"]').click();
await page.waitForTimeout(700);
check("UI update reflete novos tempos", /00:04\.00/.test(await row.innerText()));

const persisted = asList(await (await api.get("/api/markers/5002", { headers: auth })).json());
const uiMarker = persisted.find((m) => m.kind === "credits");
check(
  "UI update persistiu no banco",
  uiMarker?.startMs === 4000 && uiMarker?.endMs === 10000,
  JSON.stringify(uiMarker),
);

await row.locator('[data-testid="marker-remove"]').click();
await page.waitForTimeout(700);
check("UI delete remove marcador da tela", (await row.locator("text=--:--").count()) > 0);
const afterUiDelete = asList(await (await api.get("/api/markers/5002", { headers: auth })).json());
const uiDeleted = afterUiDelete.find((m) => m.kind === "credits");
check("UI delete removeu do banco", uiDeleted == null, JSON.stringify(uiDeleted));

check("sem erros de runtime na página de marcadores", pageErrors.length === 0, pageErrors.join(" | "));
await page.screenshot({ path: "/tmp/opencode/qa/05-crud-marcadores.png" });

await browser.close();
await api.dispose();

const failed = results.filter((result) => !result.ok);
console.log(`\n${results.length - failed.length}/${results.length} checks OK`);
process.exit(failed.length === 0 ? 0 : 1);
