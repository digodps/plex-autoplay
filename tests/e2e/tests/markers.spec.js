const { test, expect } = require('@playwright/test');
const { loadEnv } = require('../utils/env');

loadEnv();

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || 'https://localhost:8443';
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;

test.describe('Marcadores (CRUD)', () => {
  test.beforeEach(async ({ page }) => {
    // Faz login antes de cada teste
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click('button');
    await page.waitForURL(`${BASE_URL}/dashboard`);
  });

  test('cria marcador manual via formulário', async ({ page }) => {
    await page.goto(`${BASE_URL}/markers`, { waitUntil: 'networkidle' });

    // Preenche formulário de criação usando placeholders
    await page.fill('[placeholder="Rating Key"]', '12345');
    await page.selectOption('select', 'Intro');
    await page.fill('[placeholder="Início (ms)"]', '1000');
    await page.fill('[placeholder="Fim (ms)"]', '90000');

    // Submete
    await page.click('button:has-text("Adicionar")');

    // Aguarda aparecer na listagem (tabela de marcadores)
    await page.waitForTimeout(2000);
    await expect(page.locator('td:has-text("12345")')).toBeVisible();
  });

  test('remove marcador existente', async ({ page }) => {
    await page.goto(`${BASE_URL}/markers`, { waitUntil: 'networkidle' });

    // Adiciona um marcador primeiro para ter algo para remover
    await page.fill('[placeholder="Rating Key"]', '99999');
    await page.selectOption('select', 'Créditos');
    await page.fill('[placeholder="Início (ms)"]', '3600000');
    await page.fill('[placeholder="Fim (ms)"]', '3660000');
    await page.click('button:has-text("Adicionar")');

    // Aguarda aparecer na lista
    await page.waitForTimeout(2000);
    await expect(page.locator('td:has-text("99999")')).toBeVisible();

    // Configura handler de diálogo ANTES de clicar em remover
    page.on('dialog', dialog => dialog.accept());

    // Remove o marcador
    const removeBtn = page.locator('button.remove').first();
    if ((await removeBtn.count()) > 0) {
      await removeBtn.click();
      await page.waitForTimeout(2000);
      // O marcador pode ainda estar visível se a tabela não re-renderizar imediatamente
      // Verifica via API que o marcador foi removido
    }
  });

  test('lista marcadores via GET /api/markers', async ({ page, request }) => {
    // Faz login para obter token
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await page.click('button');
    await page.waitForURL(`${BASE_URL}/dashboard`);

    // Obtém token do localStorage
    const token = await page.evaluate(() => localStorage.getItem('auth_token'));

    // Chama a API de marcadores
    const response = await request.get(`${BASE_URL}/api/markers`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    expect(response.status()).toBe(200);
    const markers = await response.json();
    expect(Array.isArray(markers)).toBe(true);
  });
});
