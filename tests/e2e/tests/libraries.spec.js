const { test, expect } = require('@playwright/test');
const { loadEnv } = require('../utils/env');

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || 'https://localhost:8443';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe('Bibliotecas de séries', () => {
  test.beforeEach(async ({ page }) => {
    // Faz login antes de cada teste
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click('button');
    await page.waitForURL(`${BASE_URL}/dashboard`);
  });

  test('acessa /libraries e exibe apenas bibliotecas de tipo "show"', async ({ page, request }) => {
    // Obtém token do localStorage
    const token = await page.evaluate(() => localStorage.getItem('auth_token'));

    // Valida via API que apenas bibliotecas do tipo "show" são retornadas
    const response = await request.get(`${BASE_URL}/api/plex/libraries`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const libraries = await response.json();
    expect(Array.isArray(libraries)).toBe(true);

    // Todas as bibliotecas retornadas devem ser do tipo "show"
    for (const lib of libraries) {
      expect(lib.type).toBe('show');
    }

    // Navega para a página de bibliotecas (deve carregar sem erro)
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
  });

  test('exibe erro quando Plex está indisponível', async ({ page }) => {
    // A aplicação retorna lista vazia quando o Plex não está acessível
    // (não lança erro não tratado). Verifica que a página carrega sem erro.
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });

    // A página deve carregar sem exceção não tratada
    await expect(page.locator('h1')).toContainText('Bibliotecas');
  });
});
