import { chromium } from "@playwright/test";

const BASE = process.env.QA_BASE_URL || "http://127.0.0.1:8091";
const PASSWORD = process.env.QA_PASSWORD || "senha-qa-123";
const MEDIA = process.env.QA_MEDIA_DIR || "/tmp/opencode/qa/media";
const results = [];
const check = (name, ok, extra = "") => {
  results.push({ name, ok, extra });
  console.log(`${ok ? "PASS" : "FAIL"} | ${name}${extra ? ` | ${extra}` : ""}`);
};

const browser = await chromium.launch();
const page = await (await browser.newContext({ viewport: { width: 1366, height: 900 } })).newPage();
const pageErrors = [];
page.on("pageerror", (error) => pageErrors.push(error.message));

async function navigatePickerTo(target) {
  await page.fill('[data-testid="directory-picker-goto"]', target);
  await page.locator('[data-testid="directory-picker-goto"]').press("Enter");
  await page.waitForTimeout(400);
}

// ---- Setup inicial ----
await page.goto(`${BASE}/setup`, { waitUntil: "networkidle" });
check("guard redireciona para /setup", page.url().endsWith("/setup"), page.url());

await page.fill('[data-testid="setup-token"]', "qa-setup-token");
await page.fill('[data-testid="setup-password"]', PASSWORD);
await page.fill('[data-testid="setup-confirm"]', PASSWORD);
await page.fill('[data-testid="setup-plex-url"]', "http://127.0.0.1:32411");
await page.fill('[data-testid="setup-plex-token"]', "qa-plex-token");

// Picker: abre, navega por diretórios reais do servidor e seleciona.
await page.click('[data-testid="media-root-open-picker"]');
await page.waitForSelector('[data-testid="directory-picker"]');
check("picker de diretórios abre no setup", await page.isVisible('[data-testid="directory-picker"]'));
await navigatePickerTo(`${MEDIA}/Animes`);
await page.click('[data-testid="directory-picker-select"]');
await page.waitForSelector('[data-testid="directory-picker"]', { state: "detached" });
check(
  "picker adiciona diretório selecionado",
  (await page.locator("li", { hasText: `${MEDIA}/Animes` }).count()) > 0,
);
await page.screenshot({ path: "/tmp/opencode/qa/01-setup-com-diretorio.png" });

// Segundo diretório digitado (lista múltipla).
await page.locator('input[placeholder="/media"]').fill(`${MEDIA}/Filmes`);
await page.click('[data-testid="media-root-add"]');
check(
  "adicionar segundo diretório manualmente",
  (await page.locator('[data-testid="media-root-remove"]').count()) === 2,
);

await page.click('[data-testid="setup-submit"]');
await page.waitForURL("**/login", { timeout: 15000 });
check("setup conclui e redireciona para /login", page.url().endsWith("/login"));

// ---- Login ----
await page.fill("#password", PASSWORD);
await page.click('[data-testid="login-form"] button[type="submit"]');
await page.waitForURL("**/dashboard", { timeout: 15000 });
check("login autentica e abre dashboard", page.url().endsWith("/dashboard"));

// ---- Tema ----
const themeOf = () => page.evaluate(() => document.documentElement.dataset.theme);
const t0 = await themeOf();
await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(300);
const t1 = await themeOf();
check("toggle de tema alterna data-theme", t0 !== t1, `${t0} -> ${t1}`);
const darkApplied = await page.evaluate(() => document.documentElement.classList.contains("dark"));
check("html.dark coerente com data-theme", darkApplied === (t1 === "dark"));
check(
  "tema aplicado no body (cor de fundo muda)",
  (await page.evaluate(() => getComputedStyle(document.body).backgroundColor)) !==
    "rgba(0, 0, 0, 0)",
);
await page.screenshot({ path: "/tmp/opencode/qa/02-tema-escuro.png" });
await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(300);
const t2 = await themeOf();
check("toggle de tema reverte", t2 === t0 && t2 !== t1, `${t1} -> ${t2}`);

// ---- Menus ----
const menuLink = page.locator('a[href="/libraries"]').first();
check("menu principal presente", (await menuLink.count()) > 0);
await menuLink.click();
await page.waitForURL("**/libraries", { timeout: 10000 });
await page.waitForTimeout(300);
check("navegação do menu abre /libraries", page.url().endsWith("/libraries"));
const activeCrumb = page.locator('[aria-current="page"]');
check(
  "item ativo do menu (breadcrumb) marcado",
  (await activeCrumb.count()) === 1 && /Bibliotecas/.test(await activeCrumb.innerText()),
  await activeCrumb.innerText().catch(() => ""),
);
await page.click('[aria-label="Menu do usuário"]');
const settingsLink = page.locator('a[href="/settings"]').first();
check("menu do usuário abre", await settingsLink.isVisible());
await settingsLink.click();
await page.waitForURL("**/settings", { timeout: 10000 });
check("navegação do menu abre /settings", page.url().endsWith("/settings"));
await page.click('[aria-label="Menu do usuário"]');
check("menu do usuário abre novamente", (await page.locator('a[href="/settings"]').count()) === 1);
await page.click('[aria-label="Menu do usuário"]');
check("menu do usuário fecha ao clicar novamente", (await page.locator('a[href="/settings"]').count()) === 0);
await page.screenshot({ path: "/tmp/opencode/qa/03-ajustes.png" });

// ---- Ajustes: lista múltipla + picker ----
await page.waitForSelector('[data-testid="media-root-remove"]');
const rootCount = await page.locator('[data-testid="media-root-remove"]').count();
check("ajustes mostra múltiplos diretórios persistidos", rootCount === 2, `${rootCount} itens`);

await page.click('[data-testid="media-root-open-picker"]');
await page.waitForSelector('[data-testid="directory-picker"]');
const modalBg = await page.evaluate(
  () => getComputedStyle(document.querySelector('[data-testid="directory-picker"] > div')).backgroundColor,
);
check("picker renderiza com cor de tema", modalBg !== "rgba(0, 0, 0, 0)", modalBg);
await page.keyboard.press("Escape");
await page.waitForSelector('[data-testid="directory-picker"]', { state: "detached" });
check("picker fecha com Esc", true);

await page.locator('[data-testid="media-root-remove"]').first().click();
check(
  "remover diretório reduz lista",
  (await page.locator('[data-testid="media-root-remove"]').count()) === 1,
);
await page.locator('input[placeholder="/media"]').fill(`${MEDIA}/Series`);
await page.click('[data-testid="media-root-add"]');
await page.getByRole("button", { name: /Salvar/ }).click();
await page.waitForTimeout(800);
const saved = await page.locator("text=Configurações salvas com sucesso").count();
check("salvar ajustes com múltiplos diretórios", saved > 0);

check("sem erros de runtime na página", pageErrors.length === 0, pageErrors.join(" | "));
await page.screenshot({ path: "/tmp/opencode/qa/04-ajustes-salvo.png" });

await browser.close();

const failed = results.filter((result) => !result.ok);
console.log(`\n${results.length - failed.length}/${results.length} checks OK`);
process.exit(failed.length === 0 ? 0 : 1);
