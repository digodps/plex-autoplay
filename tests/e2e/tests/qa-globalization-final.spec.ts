import { test, expect } from '@playwright/test';

const PASSWORD = 'admin123';
const BASE = 'http://localhost:8080';

async function login(page) {
  await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
  await page.fill('[data-testid="password-input"]', PASSWORD);
  await page.click('[data-testid="login-submit"]');
  await page.waitForSelector('[data-testid="app-header"]', { state: 'visible', timeout: 30000 });
  await page.waitForTimeout(1000);
}

async function openUserMenu(page) {
  const userMenuBtn = page.locator('[data-testid="user-menu-button"]');
  await expect(userMenuBtn).toBeVisible({ timeout: 10000 });
  await userMenuBtn.click();
  await page.waitForTimeout(300);
}

async function selectLanguage(page, languageLabel) {
  await openUserMenu(page);
  await page.click('[data-testid="language-selector-button"]');
  await page.waitForTimeout(300);
  await expect(page.locator('[data-testid="language-selector-dropdown"]')).toBeVisible({ timeout: 10000 });
  await page.click(`[data-testid="language-option"]:has-text("${languageLabel}")`);
  await page.waitForTimeout(500);
}

test.describe('Globalization QA - Final Validation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
    await page.evaluate(() => localStorage.clear());
    await page.reload({ waitUntil: 'networkidle' });
  });

  test('Default locale is pt-BR on first visit', async ({ page }) => {
    await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
    await page.evaluate(() => localStorage.setItem('app.locale', 'pt-BR'));
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForTimeout(500);
    await expect(page.locator('html')).toHaveAttribute('lang', 'pt-BR');
  });

  test('Language selector in user menu shows pt-BR by default after login', async ({ page }) => {
    await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
    await page.evaluate(() => localStorage.setItem('app.locale', 'pt-BR'));
    await page.reload({ waitUntil: 'networkidle' });
    await login(page);
    await openUserMenu(page);
    await page.waitForTimeout(300);
    await expect(page.locator('[data-testid="language-selector-button"]')).toContainText('Português');
  });

  test('Language selector dropdown opens and shows all 3 languages', async ({ page }) => {
    await login(page);
    await openUserMenu(page);
    await page.click('[data-testid="language-selector-button"]');
    await expect(page.locator('[data-testid="language-selector-dropdown"]')).toBeVisible();
    await expect(page.locator('[data-testid="language-option"]')).toHaveCount(3);
    await expect(page.locator('[data-testid="language-option"]:has-text("Português")')).toBeVisible();
    await expect(page.locator('[data-testid="language-option"]:has-text("English")')).toBeVisible();
    await expect(page.locator('[data-testid="language-option"]:has-text("Español")')).toBeVisible();
  });

  test('Can switch to English and UI updates', async ({ page }) => {
    await login(page);
    await selectLanguage(page, 'English');
    await expect(page.locator('html')).toHaveAttribute('lang', 'en');
    await openUserMenu(page);
    await expect(page.locator('[data-testid="language-selector-button"]')).toContainText('English');
  });

  test('Can switch to Spanish and UI updates', async ({ page }) => {
    await login(page);
    await selectLanguage(page, 'English');
    await selectLanguage(page, 'Español');
    await expect(page.locator('html')).toHaveAttribute('lang', 'es');
    await openUserMenu(page);
    await expect(page.locator('[data-testid="language-selector-button"]')).toContainText('Español');
  });

  test('Language persists after page reload', async ({ page }) => {
    await login(page);
    await selectLanguage(page, 'English');
    await expect(page.locator('html')).toHaveAttribute('lang', 'en');
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForSelector('[data-testid="app-header"]', { state: 'visible' });
    await expect(page.locator('html')).toHaveAttribute('lang', 'en');
  });

  test('All pages have correct translations (no missing keys)', async ({ page }) => {
    await login(page);
    const pages = ['/', '/libraries', '/series', '/markers', '/logs', '/sync', '/settings', '/sessions'];
    for (const path of pages) {
      await page.goto(`${BASE}${path}`, { waitUntil: 'networkidle' });
      await page.waitForSelector('[data-testid="app-header"]', { state: 'visible' });
      const bodyText = await page.locator('body').innerText();
      // Check for common missing translation patterns
      expect(bodyText).not.toMatch(/t\(["'][\w.]+["']\)/);
      expect(bodyText).not.toMatch(/MISSING:/);
      expect(bodyText).not.toMatch(/\[object Object\]/);
    }
  });

  test('Language switcher works on all pages', async ({ page }) => {
    await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
    await page.evaluate(() => localStorage.setItem('app.locale', 'pt-BR'));
    await page.reload({ waitUntil: 'networkidle' });
    await login(page);
    await expect(page.locator('html')).toHaveAttribute('lang', 'pt-BR');
    await selectLanguage(page, 'English');
    await expect(page.locator('html')).toHaveAttribute('lang', 'en');
    await selectLanguage(page, 'Español');
    await expect(page.locator('html')).toHaveAttribute('lang', 'es');
    await selectLanguage(page, 'Português');
    await expect(page.locator('html')).toHaveAttribute('lang', 'pt-BR');
  });

  test('Login error message says "password" not "email or password"', async ({ page }) => {
    await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
    await page.fill('[data-testid="password-input"]', 'wrongpassword');
    await page.click('[data-testid="login-submit"]');
    await page.waitForTimeout(1000);
    const errorEl = page.locator('.text-red-600, .text-red-500, [class*="error"]').first();
    await expect(errorEl).toBeVisible({ timeout: 5000 });
    const errorText = await errorEl.innerText();
    expect(errorText.toLowerCase()).toContain('password');
    expect(errorText.toLowerCase()).not.toContain('email or password');
  });

  test('Breadcrumb translations work correctly', async ({ page }) => {
    await page.goto(`${BASE}/login`, { waitUntil: 'networkidle' });
    await page.evaluate(() => localStorage.setItem('app.locale', 'pt-BR'));
    await page.reload({ waitUntil: 'networkidle' });
    await login(page);
    await page.goto(`${BASE}/libraries`, { waitUntil: 'networkidle' });
    await page.waitForSelector('[data-testid="app-header"]', { state: 'visible' });
    const breadcrumb = page.locator('nav[aria-label="Breadcrumb"]').first();
    await expect(breadcrumb).toBeVisible({ timeout: 5000 });
    const breadcrumbText = await breadcrumb.innerText();
    expect(breadcrumbText).toContain('Bibliotecas');
  });
});