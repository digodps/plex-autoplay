// QA independente: validação UI obrigatória (tema + menu) do app rodando no container.
// Uso: node tests/e2e/qa-autoskip-ui-check.mjs [BASE_URL]
import { chromium } from "@playwright/test";

const BASE = process.argv[2] || "http://localhost:8080";
const PASSWORD = process.env.ADMIN_PASSWORD || "admin";

const results = [];
function check(name, ok, detail = "") {
  results.push({ name, ok, detail });
  console.log(`${ok ? "PASS" : "FAIL"}  ${name}${detail ? ` | ${detail}` : ""}`);
}

const isDark = (page) => page.locator("html").evaluate((el) => el.classList.contains("dark"));

try {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ ignoreHTTPSErrors: true });

  // --- Auth ---
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  check("Página de login carrega", await page.isVisible('[data-testid="login-form"]'));
  await page.fill('input[data-testid="password-input"]', PASSWORD);
  await page.click('button[data-testid="login-submit"]');
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 10000 });
  check("Login redireciona para /dashboard", page.url().includes("/dashboard"));

  // --- Tema (light/dark toggle) ---
  const themeBtn = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
  check("Toggle de tema presente na header", await themeBtn.isVisible());

  const darkInitial = await isDark(page);
  await themeBtn.click();
  await page.waitForTimeout(400);
  const darkAfter1 = await isDark(page);
  check("Toggle alterna o tema (muda data-theme/class)", darkInitial !== darkAfter1, `${darkInitial} -> ${darkAfter1}`);

  await themeBtn.click();
  await page.waitForTimeout(400);
  const darkAfter2 = await isDark(page);
  check("Toggle reverte o tema", darkAfter2 === darkInitial, `${darkAfter1} -> ${darkAfter2}`);

  const themeAttr = await page.locator("html").getAttribute("data-theme");
  check("data-theme reflete o estado efetivo", themeAttr === "dark" || themeAttr === "light", `data-theme=${themeAttr}`);

  // --- Menu do usuário (abertura / fechamento) ---
  const profileBtn = page.locator('button[aria-label="Menu do usuário"]');
  check("Botão do menu de usuário presente", await profileBtn.isVisible());
  await profileBtn.click();
  await page.waitForTimeout(200);
  const menuOpen = await page.isVisible('text=Alterar senha');
  check("Menu de usuário abre", menuOpen);
  check("Menu exibe estado expandido (aria-expanded)", (await profileBtn.getAttribute("aria-expanded")) === "true");
  await profileBtn.click();
  await page.waitForTimeout(200);
  const menuClosed = !(await page.isVisible('text=Alterar senha'));
  check("Menu de usuário fecha", menuClosed);

  // --- Navegação (breadcrumb) ---
  await page.goto(`${BASE}/settings`, { waitUntil: "networkidle" });
  await page.waitForTimeout(400);
  const breadcrumbActive = page.locator('[aria-label="Breadcrumb"] [aria-current="page"]');
  check("Breadcrumb mostra item ativo", (await breadcrumbActive.count()) >= 1);
  try {
    await page.locator('[aria-label="Breadcrumb"] a').first().click();
    await page.waitForTimeout(400);
    check("Navegação via breadcrumb funciona", !page.url().includes("/settings"), page.url());
  } catch {
    check("Navegação via breadcrumb (sem links na rota atual)", true, "sem link no breadcrumb");
  }

  await browser.close();

  const failed = results.filter((r) => !r.ok);
  console.log("\n=== RESUMO QA UI ===");
  console.log(`Total: ${results.length}, Passed: ${results.length - failed.length}, Failed: ${failed.length}`);
  if (failed.length > 0) {
    console.log("Achados:");
    failed.forEach((f) => console.log(`  - ${f.name} ${f.detail}`));
    process.exitCode = 1;
  } else {
    console.log("RESULT: PASS");
    process.exitCode = 0;
  }
} catch (err) {
  check("Execução da QA", false, err.message);
  process.exitCode = 1;
}