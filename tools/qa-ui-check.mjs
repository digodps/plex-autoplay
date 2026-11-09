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

// user menu open/close
const profileBtn = page.getByRole("button", { name: "Menu do usuário" });
await profileBtn.click();
const alterar = page.getByTestId("change-password-menu");
check("menu do usuário abre", await alterar.isVisible());
check("menu lista Configurações", await page.getByRole("link", { name: "Configurações" }).isVisible());
check("menu lista Sair", await page.getByRole("button", { name: "Sair" }).isVisible());
await profileBtn.click();
await page.waitForTimeout(150);
check("menu do usuário fecha", (await alterar.count()) === 0 || !(await alterar.isVisible()));

// password modal via menu
await profileBtn.click();
await alterar.click();
await page.waitForTimeout(300);
const dialog = page.getByTestId("password-modal");
check("modal de alteração de senha abre", (await dialog.count()) > 0 && (await dialog.isVisible()));
const closeBtn = page.getByTestId("password-cancel");
await closeBtn.click();
await page.waitForTimeout(300);
check("modal de alteração de senha fecha", (await dialog.count()) === 0);

// settings route via menu
await profileBtn.click();
await page.getByRole("link", { name: "Configurações" }).click();
await page.waitForURL("**/settings", { timeout: 15000 });
await page.waitForLoadState("networkidle");
check("menu navega para /settings", page.url().endsWith("/settings"));
const heading = (await page.locator("h1").first().textContent())?.trim();
check("settings renderiza título", Boolean(heading), heading || "");

// breadcrumb present on a deep route
await page.goto(`${BASE}/libraries`, { waitUntil: "networkidle" });
check(
  "breadcrumb marca página atual",
  (await page.locator('nav[aria-label="Breadcrumb"] [aria-current="page"]').count()) > 0,
);

// mobile: no horizontal overflow
await page.setViewportSize({ width: 390, height: 844 });
await page.goto(`${BASE}/dashboard`, { waitUntil: "networkidle" });
const overflow = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
check("mobile: sem overflow horizontal", overflow <= 1, `${overflow}px`);

check("sem erros JS de página", errors.length === 0, errors.join("; "));
await page.screenshot({ path: "/tmp/opencode/qa-dashboard-mobile.png", fullPage: true });
await page.setViewportSize({ width: 1366, height: 900 });
await page.screenshot({ path: "/tmp/opencode/qa-dashboard-desktop.png", fullPage: true });
await browser.close();

const failed = results.filter((r) => !r.ok);
console.log(`\n${results.length - failed.length}/${results.length} checks passed`);
process.exit(failed.length ? 1 : 0);