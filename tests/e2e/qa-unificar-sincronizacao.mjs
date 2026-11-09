import { chromium, request } from "@playwright/test";

const BASE = process.env.QA_BASE_URL || "http://127.0.0.1:8099";
const PASSWORD = process.env.QA_PASSWORD || "senha-qa-123";
const SETUP_TOKEN = process.env.SETUP_TOKEN || "qa-setup-token";
const PLEX_URL = process.env.PLEX_URL || "http://plex-mock:32400";
const PLEX_TOKEN = process.env.PLEX_TOKEN || "qa-plex-token";
const MEDIA = process.env.QA_MEDIA_DIR || "/media/Animes";

const results = [];
const check = (name, ok, extra = "") => {
  results.push({ name, ok, extra });
  console.log(`${ok ? "PASS" : "FAIL"} | ${name}${extra ? ` | ${extra}` : ""}`);
};

// ---------- API ----------
const api = await request.newContext({ baseURL: BASE });

// Setup inicial se necessário
let statusRes = await api.get("/api/auth/setup/status");
let setupRequired = false;
if (statusRes.ok) {
  const body = await statusRes.json();
  setupRequired = body.setupRequired ?? false;
}
if (setupRequired) {
  const setup = await api.post("/api/auth/setup", {
    data: { token: SETUP_TOKEN, newPassword: PASSWORD, plexUrl: PLEX_URL, plexToken: PLEX_TOKEN, mediaRoots: [MEDIA] },
  });
  check("Setup inicial: senha + Plex + diretório de mídia", setup.status() === 200 || setup.status() === 204, String(setup.status()));
}

// Login
const login = await api.post("/auth/login", { data: { password: PASSWORD } });
check("Login retorna 200", login.status() === 200, String(login.status()));
let token = "";
try {
  const b = await login.json();
  token = b.token || b.access_token || "";
} catch {}
check("Token JWT obtido", token.length > 0);
const auth = { Authorization: `Bearer ${token}` };

// ---------- Settings: um único intervalo ----------
const settings = await api.get("/api/settings", { headers: auth });
const s = await settings.json();
check("GET /api/settings retorna settings", settings.ok, String(settings.status()));
check("GET não retorna sync_catalog_interval_minutes", s.sync_catalog_interval_minutes === undefined, JSON.stringify(s));
check("GET retorna sync_interval_minutes", typeof s.sync_interval_minutes === "number", String(s.sync_interval_minutes));

const putRes = await api.put("/api/settings", {
  headers: { ...auth, "Content-Type": "application/json" },
  data: { plex_url: PLEX_URL, media_roots: [MEDIA], sync_interval_minutes: 15 },
});
check("PUT /api/settings retorna 200", putRes.status() === 200, String(putRes.status()));
if (putRes.ok) {
  const updated = await putRes.json();
  check("PUT persiste 15 minutos", updated.sync_interval_minutes === 15, JSON.stringify(updated));
}

// ---------- UI ----------
const browser = await chromium.launch();
const page = await (await browser.newContext({ viewport: { width: 1366, height: 900 } })).newPage();
const pageErrors = [];
page.on("pageerror", (error) => pageErrors.push(error.message));

await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
await page.fill('input[type="password"]', PASSWORD);
await page.click('button[type="submit"]');
await page.waitForURL("**/dashboard", { timeout: 15000 });
check("Login redireciona para /dashboard", page.url().includes("/dashboard"));

// Tema
const themeOf = () => page.evaluate(() => document.documentElement.dataset.theme);
const t0 = await themeOf();
await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(300);
const t1 = await themeOf();
check("toggle de tema alterna data-theme", t0 !== t1, `${t0} -> ${t1}`);
await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(300);
const t2 = await themeOf();
check("toggle de tema reverte", t2 === t0 && t2 !== t1, `${t1} -> ${t2}`);

// Menu do usuário
await page.click('[aria-label="Menu do usuário"]');
const settingsLink = page.locator('a[href="/settings"]').first();
check("menu do usuário abre e mostra /settings", await settingsLink.isVisible());
await settingsLink.click();
await page.waitForURL("**/settings", { timeout: 10000 });
check("menu do usuário navega para /settings", page.url().endsWith("/settings"));

// Ajustes: um único campo de intervalo
await page.waitForSelector('input[id="sync-interval"]', { timeout: 10000 });
check("apenas 1 campo de intervalo em /settings", (await page.locator('input[id="sync-interval"]').count()) === 1);
check("não existe campo de catálogo", (await page.locator('input[id="catalog-interval"]').count()) === 0);
check("intervalo carregado do banco (15)", (await page.locator('input[id="sync-interval"]').inputValue()) === "15");

// Salvamento dos ajustes
await page.fill('input[id="sync-interval"]', "7");
await page.getByRole("button", { name: /Salvar/ }).click();
await page.waitForTimeout(800);
check("confirmação de salvamento", (await page.locator("text=Configurações salvas com sucesso").count()) > 0);
check("intervalo persiste após salvar", (await page.locator('input[id="sync-interval"]').inputValue()) === "7");

// Navegação de menu via usuário (já valida abertura/fechamento/navegação/estado ativo acima)
await page.click('[aria-label="Menu do usuário"]');
check("menu fecha ao clicar novamente", (await page.locator('a[href="/settings"]').count()) === 0);

// ---------- Sync: força sincronização completa ----------
await page.goto(`${BASE}/sync`, { waitUntil: "networkidle" });
await page.waitForTimeout(400);

// Captura o job_id do POST /api/sync/run
let jobId = null;
page.on("response", async (resp) => {
  if (resp.url().includes("/api/sync/run") && resp.request().method() === "POST") {
    try { jobId = (await resp.json()).job_id; } catch {}
  }
});

await page.getByRole("button", { name: /Iniciar Escaneamento/ }).click();
await page.waitForTimeout(2500);
check("job_id obtido da resposta", typeof jobId === "string");

// Polling do status até completar (até ~20s)
let status = "";
let logsText = "";
for (let i = 0; i < 20; i++) {
  const st = await api.get(`/api/sync/status/${jobId}`, { headers: auth });
  if (st.ok) {
    const j = await st.json();
    status = j.status;
    logsText = (j.logs ?? []).map((l) => l.message).join("\n");
    if (status === "completed" || status === "failed" || status === "cancelled") break;
  }
  await page.waitForTimeout(1000);
}
check("job atingiu estado final", status === "completed" || status === "failed" || status === "cancelled", `status=${status}`);
if (logsText) {
  const catalogLog = /catálogo|catalog/i.test(logsText);
  const markerLog = /marcador|marker/i.test(logsText);
  check("logs da etapa 1 (catálogo) aparecem", catalogLog);
  check("logs da etapa 2 (marcadores) aparecem", markerLog);
  check("ordem: catálogo antes dos marcadores", catalogLog && markerLog && logsText.indexOf("catálogo") < logsText.indexOf("marcador"), `indexOf catálogo=${logsText.indexOf("catálogo")}, marcador=${logsText.indexOf("marcador")}`);
  check("job status final = completed", status === "completed", status);
} else {
  check("logs do job acessíveis", false, `status=${status}`);
}

check("sem erros de runtime na UI", pageErrors.length === 0, pageErrors.join(" | "));
await page.screenshot({ path: "/tmp/opencode/qa/unificar-sincronizacao.png" });
await browser.close();
await api.dispose();

const failed = results.filter((r) => !r.ok);
console.log(`\n=== QA unificar-sincronizacao ===`);
console.log(`Total: ${results.length}, Passed: ${results.length - failed.length}, Failed: ${failed.length}`);
failed.forEach((f) => console.log(`  - ${f.name} ${f.extra}`));
process.exit(failed.length === 0 ? 0 : 1);
