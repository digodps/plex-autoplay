import { test, expect } from "@playwright/test";

const BASE = process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080";
const PASSWORD = process.env.OPERATOR_PASSWORD || process.env.ADMIN_PASSWORD || "admin123";

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: "networkidle" });
  await page.waitForSelector("[data-testid=login-form]", { state: "visible", timeout: 15000 });
  await page.fill("[data-testid=password-input]", PASSWORD);
  await page.click("[data-testid=login-submit]");
  await page.waitForURL(`${BASE}/dashboard`, { timeout: 15000 });
}

test("página de séries não exibe título duplicado da biblioteca, apenas migalha do header", async ({ page }) => {
  await login(page);
  await page.goto(`${BASE}/libraries/1/series`, { waitUntil: "networkidle" });

  const body = await page.textContent("body");

  // Não deve exibir o título duplicado "Biblioteca → ..." nem o subtítulo
  expect(body).not.toContain("Biblioteca →");
  expect(body).not.toContain("Biblioteca selecionada.");

  // A migalha de pão do header deve estar presente ("Bibliotecas > Animes")
  expect(body).toContain("Bibliotecas");
});
