import { chromium } from "@playwright/test";

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://192.168.18.5:8080";
const PASS = process.env.ADMIN_PASSWORD || "admin";
const results = [];
const check = (name, ok, extra = "") => {
  results.push({ name, ok, extra });
  console.log(`${ok ? "PASS" : "FAIL"} | ${name}${extra ? ` | ${extra}` : ""}`);
};

const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 1366, height: 900 } });
const page = await ctx.newPage();
const errors = [];
page.on("pageerror", (e) => errors.push(e.message));

// login
await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
await page.fill('[data-testid="password-input"]', PASS);
await page.click('[data-testid="login-submit"]');
await page.waitForURL("**/dashboard", { timeout: 15000 });
check("login redireciona para /dashboard", page.url().endsWith("/dashboard"));

// theme toggle
const themeOf = () => page.evaluate(() => document.documentElement.dataset.theme);
const t0 = await themeOf();
const mode0 = await page.evaluate(() => localStorage.getItem("theme.mode"));
await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(250);
const t1 = await themeOf();
const mode1 = await page.evaluate(() => localStorage.getItem("theme.mode"));
check("toggle de tema alterna data-theme", t0 !== t1, `${t0} -> ${t1}`);
check("toggle de tema persiste em localStorage", mode1 !== mode0, `${mode0} -> ${mode1}`);
check("html.dark coerente com data-theme", (await page.evaluate(() => document.documentElement.classList.contains("dark"))) === (t1 === "dark"));
await page.getByRole("button", { name: /Tema (claro|escuro)/ }).click();
await page.waitForTimeout(250);
check("toggle de tema reverte", (await themeOf()) === t0);

// sidebar collapse
const aside = page.locator("aside.sidebar");
const wBefore = (await aside.boundingBox()).width;
const collapseBtn = page.getByRole("button", { name: /(Recolher|Expandir) menu/ });
const expBefore = await collapseBtn.getAttribute("aria-expanded");
await collapseBtn.click();
await page.waitForTimeout(250);
const wAfter = (await aside.boundingBox()).width;
const expAfter = await collapseBtn.getAttribute("aria-expanded");
check("botão de menu alterna aria-expanded", expBefore !== expAfter, `${expBefore} -> ${expAfter}`);
check("botão de menu recolhe a sidebar", wBefore !== wAfter, `${Math.round(wBefore)}px -> ${Math.round(wAfter)}px`);
await collapseBtn.click();
await page.waitForTimeout(250);
check("botão de menu restaura a sidebar", Math.abs((await aside.boundingBox()).width - wBefore) < 2);

// sidebar navigation
const firstLib = aside.locator("a[href^='/libraries/']").first();
const libCount = await aside.locator("a[href^='/libraries/']").count();
check("sidebar lista bibliotecas", libCount > 0, `${libCount} itens`);
if (libCount > 0) {
  const href = await firstLib.getAttribute("href");
  await firstLib.click();
  await page.waitForURL(`**${href}*`, { timeout: 15000 });
  await page.waitForLoadState("networkidle");
  check("navegação da sidebar abre séries", page.url().includes(href), page.url());
  check("item ativo da sidebar marcado", (await firstLib.getAttribute("aria-current")) === "page");
  const crumb = page.locator('nav[aria-label="Breadcrumb"] [aria-current="page"]');
  check("breadcrumb marca página atual", (await crumb.count()) > 0, await crumb.first().textContent().catch(() => ""));

  const cardImgs = page.locator("main img");
  const cardCount = await cardImgs.count();
  let cardLoaded = 0;
  for (let i = 0; i < cardCount; i++) {
    if (await cardImgs.nth(i).evaluate((el) => el.complete && el.naturalWidth > 0).catch(() => false)) cardLoaded++;
  }
  check("miniatura das séries carrega", cardLoaded > 0, `${cardLoaded}/${cardCount}`);

  await page.locator("main button:has(img)").first().click();
  await page.waitForURL("**/markers*", { timeout: 15000 });
  await page.waitForLoadState("networkidle");
  await page
    .locator('button[title="Adicionar marcador"], button[title="Editar"]')
    .first()
    .waitFor({ state: "visible", timeout: 15000 })
    .catch(() => {});
  const epImgs = page.locator("main img");
  const epCount = await epImgs.count();
  let epLoaded = 0;
  for (let i = 0; i < epCount; i++) {
    if (await epImgs.nth(i).evaluate((el) => el.complete && el.naturalWidth > 0).catch(() => false)) epLoaded++;
  }
  check("miniatura do episódio carrega", epLoaded > 0, `${epLoaded}/${epCount}`);
  check("linhas de marcador editáveis presentes", (await page.locator('button[title="Adicionar marcador"], button[title="Editar"]').count()) >= 2);
}

// back to dashboard for menu tests
await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

// profile menu open/close
const profileBtn = page.getByRole("button", { name: "Menu do usuário" });
await profileBtn.click();
const alterar = page.getByRole("button", { name: "Alterar senha" });
check("menu do usuário abre", await alterar.isVisible());
check("menu do usuário lista Configurações", await page.getByRole("link", { name: "Configurações" }).isVisible());
await profileBtn.click();
await page.waitForTimeout(150);
check("menu do usuário fecha", (await alterar.count()) === 0 || !(await alterar.isVisible()));

// password modal via menu
await profileBtn.click();
await alterar.click();
await page.waitForTimeout(300);
const dialog = page.locator('[role="dialog"]').first();
check("modal de alteração de senha abre", (await dialog.count()) > 0 && (await dialog.isVisible()));
const closeBtn = page.getByRole("button", { name: /fechar|cancelar/i }).first();
if (await closeBtn.count()) {
  await closeBtn.click();
  await page.waitForTimeout(300);
  check("modal de alteração de senha fecha", (await page.locator('[role="dialog"]:visible').count()) === 0);
}

// settings route via menu
await profileBtn.click();
await page.getByRole("link", { name: "Configurações" }).click();
await page.waitForURL("**/settings", { timeout: 15000 });
await page.waitForLoadState("networkidle");
check("menu navega para /settings", page.url().endsWith("/settings"));
check("settings renderiza título", (await page.locator("h1").first().textContent())?.trim().length > 0, await page.locator("h1").first().textContent());
await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });

// mobile: sidebar auto-collapsed, no horizontal overflow
await page.setViewportSize({ width: 390, height: 844 });
await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });
const mw = (await page.locator("aside.sidebar").boundingBox()).width;
check("mobile: sidebar compacta", mw <= 80, `${Math.round(mw)}px`);
const overflow = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
check("mobile: sem overflow horizontal", overflow <= 1, `${overflow}px`);

check("sem erros JS de página", errors.length === 0, errors.join("; "));
await page.screenshot({ path: "/tmp/qa-dashboard-mobile.png", fullPage: true });
await page.setViewportSize({ width: 1366, height: 900 });
await page.screenshot({ path: "/tmp/qa-dashboard-desktop.png", fullPage: true });
await browser.close();

const failed = results.filter((r) => !r.ok);
console.log(`\n${results.length - failed.length}/${results.length} checks passed`);
process.exit(failed.length ? 1 : 0);
