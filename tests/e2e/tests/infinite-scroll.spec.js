const { test, expect } = require("@playwright/test");
const { loadEnv } = require("../utils/env");

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || "https://plex-autoplay.dns.podman:443";
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe("Infinite Scroll Regression", () => {
  test.beforeEach(async ({ page, context }) => {
    // Login
    await page.goto(`${BASE_URL}/login`, { waitUntil: "networkidle" });
    await page.waitForSelector('input[type="password"]', { state: "visible", timeout: 10000 });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click("button");
    await page.waitForURL(`${BASE_URL}/dashboard`);

    // Obtém token do cookie
    const cookies = await context.cookies();
    const token = cookies.find((c) => c.name === "auth_token")?.value ?? "";
    test.token = token;
  });

  test("infinite scroll carrega múltiplas páginas ao rolar", async ({ page }) => {
    // Navega para a página de séries da biblioteca 1
    await page.goto(`${BASE_URL}/libraries/1/series`, { waitUntil: "networkidle" });

    // Aguarda o grid de séries carregar
    await page.waitForSelector(".series-grid", { state: "visible", timeout: 10000 });
    await page.waitForTimeout(1000); // Pequena pausa para estabilizar

    // Conta cards iniciais (página 1 = 24)
    let cards = await page.locator(".series-card:not(.skeleton)").count();
    console.log(`Cards iniciais: ${cards}`);
    expect(cards).toBeGreaterThanOrEqual(20); // Pelo menos 20 (pode ser menos se mock tiver menos)

    // Scroll até a sentinela (fim do grid) - scroll o container diretamente
    await page.evaluate(() => {
      const wrapper = document.querySelector(".series-grid-wrapper");
      if (wrapper) {
        wrapper.scrollTop = wrapper.scrollHeight;
      } else {
        window.scrollTo(0, document.body.scrollHeight);
      }
    });

    // Aguarda carregamento da próxima página (espera por nova resposta da API)
    await page.waitForResponse(
      (resp) => resp.url().includes("/api/series/1") && resp.url().includes("page=2"),
      { timeout: 15000 },
    );

    // Aguarda renderização dos novos cards
    await page.waitForTimeout(500);

    // Verifica que mais cards foram carregados
    cards = await page.locator(".series-card:not(.skeleton)").count();
    console.log(`Cards após scroll 1: ${cards}`);
    expect(cards).toBeGreaterThan(24); // Deve ter mais que a primeira página

    // Scroll novamente para página 3 - scroll o container diretamente
    await page.evaluate(() => {
      const wrapper = document.querySelector(".series-grid-wrapper");
      if (wrapper) {
        wrapper.scrollTop = wrapper.scrollHeight;
      } else {
        window.scrollTo(0, document.body.scrollHeight);
      }
    });

    // Aguarda carregamento da página 3
    await page.waitForResponse(
      (resp) => resp.url().includes("/api/series/1") && resp.url().includes("page=3"),
      { timeout: 15000 },
    );

    await page.waitForTimeout(500);

    cards = await page.locator(".series-card:not(.skeleton)").count();
    console.log(`Cards após scroll 2: ${cards}`);
    expect(cards).toBeGreaterThan(48); // Deve ter mais que 2 páginas

    // Verifica que indicador "Fim do catálogo" aparece eventualmente
    // Como temos 60 séries, após 3 páginas (72) deve aparecer "Fim do catálogo"
    const endOfCatalog = page.locator(".end-of-catalog");
    await expect(endOfCatalog).toBeVisible({ timeout: 10000 });
  });

  test("busca combina com infinite scroll", async ({ page }) => {
    await page.goto(`${BASE_URL}/libraries/1/series`, { waitUntil: "networkidle" });
    await page.waitForSelector(".series-grid", { state: "visible", timeout: 10000 });
    await page.waitForTimeout(500);

    // Digita termo de busca
    const searchInput = page.locator('.search-box input[type="search"]');
    await searchInput.fill("Action");

    // Aguarda debounce (300ms) e carregamento da página 1 filtrada
    await page.waitForResponse(
      (resp) => resp.url().includes("/api/series/1") && resp.url().includes("query=Action"),
      { timeout: 10000 },
    );

    await page.waitForTimeout(500);

    // Verifica resultados filtrados
    let cards = await page.locator(".series-card:not(.skeleton)").count();
    console.log(`Cards com busca 'Action': ${cards}`);
    expect(cards).toBeGreaterThan(0);

    // Verifica que os títulos contêm "Action"
    const titles = await page.locator(".series-card h4").allTextContents();
    for (const title of titles) {
      expect(title.toLowerCase()).toContain("action");
    }

    // Scroll para carregar página 2 da busca - scroll o container diretamente
    await page.evaluate(() => {
      const wrapper = document.querySelector(".series-grid-wrapper");
      if (wrapper) {
        wrapper.scrollTop = wrapper.scrollHeight;
      } else {
        window.scrollTo(0, document.body.scrollHeight);
      }
    });

    // Aguarda página 2 com mesmo query
    await page.waitForResponse(
      (resp) =>
        resp.url().includes("/api/series/1") &&
        resp.url().includes("page=2") &&
        resp.url().includes("query=Action"),
      { timeout: 15000 },
    );

    await page.waitForTimeout(500);

    // Verifica mais resultados
    cards = await page.locator(".series-card:not(.skeleton)").count();
    console.log(`Cards após scroll com busca: ${cards}`);

    // Limpa busca
    await searchInput.fill("");
    await page.waitForResponse(
      (resp) => resp.url().includes("/api/series/1") && !resp.url().includes("query="),
      { timeout: 10000 },
    );

    await page.waitForTimeout(500);

    // Verifica catálogo completo restaurado
    cards = await page.locator(".series-card:not(.skeleton)").count();
    console.log(`Cards após limpar busca: ${cards}`);
    expect(cards).toBeGreaterThan(24);
  });

  test("breadcrumb navega de volta para bibliotecas", async ({ page }) => {
    await page.goto(`${BASE_URL}/libraries/1/series`, { waitUntil: "networkidle" });
    await page.waitForSelector(".breadcrumb", { state: "visible", timeout: 10000 });

    // Clica no link "Bibliotecas" no breadcrumb
    await page.click(".breadcrumb-link:has-text('Bibliotecas')");

    // Verifica navegação para /libraries
    await page.waitForURL(`${BASE_URL}/libraries`, { timeout: 10000 });
    await expect(page.locator("h1")).toContainText("Bibliotecas");
  });
});
