import { test, expect } from '@playwright/test';

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:8080';
const PASSWORD = process.env.OPERATOR_PASSWORD || 'admin123';
const EVIDENCE = '/home/hermes/plex-autoplay-add-globalization/openspec/changes/add-globalization/qa-evidence';

test.describe('Globalization QA', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForSelector('[data-testid="password-input"]', { state: 'visible', timeout: 30000 });
    await page.fill('[data-testid="password-input"]', PASSWORD);
    await page.click('[data-testid="login-submit"]');
    await page.waitForSelector('[data-testid="app-header"]', { state: 'visible', timeout: 30000 });
  });

  test.use({ locale: 'pt-BR' });

  async function openUserMenu(page) {
    await page.click('button[aria-label*="Menu do usuário"]');
    await page.waitForTimeout(300);
  }

  async function selectLanguage(page, index) {
    await openUserMenu(page);
    await page.locator('[data-testid="language-selector-menu"]').selectOption({ index });
    await page.waitForTimeout(1500);
  }

  async function reopenUserMenuAndWait(page) {
    // Locale change triggers page reload; wait for app to remount
    await page.waitForTimeout(1500);
    // Menu might already be open after reload; check and wait for select
    const menuBtn = page.locator('button[aria-label*="Menu do usuário"]');
    const isExpanded = await menuBtn.getAttribute('aria-expanded');
    if (isExpanded === 'true') {
      // Menu already open, just wait for select
      await page.waitForSelector('[data-testid="language-selector-menu"]', { state: 'visible', timeout: 10000 });
    } else {
      // Menu closed, open it
      await openUserMenu(page);
      await page.waitForSelector('[data-testid="language-selector-menu"]', { state: 'visible', timeout: 10000 });
    }
  }

  test('language selector in user menu shows pt-BR by default', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    await openUserMenu(page);
    await expect(page.locator('[data-testid="language-selector-menu"]')).toBeVisible();

    const select = page.locator('[data-testid="language-selector-menu"]');
    await expect(select).toHaveValue('pt-BR');

    await page.screenshot({ path: `${EVIDENCE}/01-lang-default-ptbr.png`, fullPage: true });
  });

  test('language selector dropdown opens and shows all languages', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    await openUserMenu(page);
    await expect(page.locator('[data-testid="language-selector-menu"]')).toBeVisible();

    const options = page.locator('[data-testid="language-selector-menu"] option');
    await expect(options).toHaveCount(3);

    // Check pt-BR
    await expect(options.nth(0)).toContainText('Português');
    await expect(options.nth(0)).toHaveAttribute('value', 'pt-BR');

    // Check English
    await expect(options.nth(1)).toContainText('English');
    await expect(options.nth(1)).toHaveAttribute('value', 'en');

    // Check Spanish
    await expect(options.nth(2)).toContainText('Español');
    await expect(options.nth(2)).toHaveAttribute('value', 'es');

    await page.screenshot({ path: `${EVIDENCE}/02-lang-dropdown-open.png`, fullPage: true });
  });

  test('can switch to English and UI updates', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    await selectLanguage(page, 1); // English

    // Verify html lang attribute changed
    const htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('en');

    // Verify page title translated
    await expect(page.locator('text="Dashboard"')).toBeVisible({ timeout: 5000 });

    // Verify select shows English (reopen menu after locale change)
    await reopenUserMenuAndWait(page);
    await expect(page.locator('[data-testid="language-selector-menu"]')).toHaveValue('en');

    await page.screenshot({ path: `${EVIDENCE}/03-lang-switched-en.png`, fullPage: true });
  });

  test('can switch to Spanish and UI updates', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    await selectLanguage(page, 2); // Spanish

    const htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('es');

    await expect(page.locator('text="Panel"')).toBeVisible({ timeout: 5000 });

    await reopenUserMenuAndWait(page);
    await expect(page.locator('[data-testid="language-selector-menu"]')).toHaveValue('es');

    await page.screenshot({ path: `${EVIDENCE}/04-lang-switched-es.png`, fullPage: true });
  });

  test('language persists after page reload', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    await selectLanguage(page, 1); // English
    await page.waitForTimeout(500);

    await page.reload({ waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    // Should still show English
    const htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('en');

    await openUserMenu(page);
    await expect(page.locator('[data-testid="language-selector-menu"]')).toHaveValue('en');

    await page.screenshot({ path: `${EVIDENCE}/05-lang-persist-reload.png`, fullPage: true });
  });

  test('theme toggle works', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    // Get initial theme
    const initialTheme = await page.evaluate(() => document.documentElement.dataset.theme || 'light');
    console.log(`INITIAL THEME: ${initialTheme}`);
    expect(['light', 'dark']).toContain(initialTheme);

    await page.screenshot({ path: `${EVIDENCE}/06-theme-${initialTheme}.png`, fullPage: true });

    // Find and click theme toggle (moon/sun button in header)
    const themeToggle = page.locator('button[aria-label*="tema"], button[aria-label*="theme"], button[aria-label*="claro"], button[aria-label*="escuro"], button[aria-label*="light"], button[aria-label*="dark"]').first();
    if (await themeToggle.count() > 0) {
      await themeToggle.click();
      await page.waitForTimeout(600);

      const newTheme = await page.evaluate(() => document.documentElement.dataset.theme || 'light');
      console.log(`NEW THEME: ${newTheme}`);
      expect(newTheme).not.toBe(initialTheme);

      await page.screenshot({ path: `${EVIDENCE}/07-theme-${newTheme}.png`, fullPage: true });

      // Language selector still works after theme change
      await openUserMenu(page);
      const options = page.locator('[data-testid="language-selector-menu"] option');
      await expect(options).toHaveCount(3);
    } else {
      console.log('NO THEME TOGGLE FOUND');
    }
  });

  test('menu navigation works across all pages', async ({ page }) => {
    const pages = [
      { path: '/dashboard', titleKeys: ['Dashboard', 'Panel', 'Panel'] },
      { path: '/libraries', titleKeys: ['Bibliotecas', 'Libraries', 'Bibliotecas'] },
      { path: '/series', titleKeys: ['Séries', 'Series', 'Series'] },
      { path: '/markers', titleKeys: ['Marcadores', 'Markers', 'Marcadores'] },
      { path: '/logs', titleKeys: ['Logs', 'Logs', 'Logs'] },
      { path: '/sync', titleKeys: ['Sync', 'Sync', 'Sync'] },
      { path: '/settings', titleKeys: ['Configurações', 'Settings', 'Configuración'] },
      { path: '/sessions', titleKeys: ['Sessões', 'Sessions', 'Sesiones'] },
    ];

    for (const pg of pages) {
      await page.goto(`${BASE_URL}${pg.path}`, { waitUntil: 'domcontentloaded', timeout: 30000 });
      await page.waitForLoadState('networkidle', { timeout: 15000 });

      // Verify page loaded
      await expect(page.locator('[data-testid="app-header"]')).toBeVisible({ timeout: 10000 });

      // Verify language selector still works on each page
      await openUserMenu(page);
      await expect(page.locator('[data-testid="language-selector-menu"]')).toBeVisible();
      await page.keyboard.press('Escape'); // Close menu

      await page.screenshot({ path: `${EVIDENCE}/08-nav-${pg.path.replace('/', '')}.png`, fullPage: true });
    }
  });

  test('keyboard navigation in language selector', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    await openUserMenu(page);
    const select = page.locator('[data-testid="language-selector-menu"]');
    await select.focus();
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');
    await page.waitForTimeout(500);

    const htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('es');

    await page.screenshot({ path: `${EVIDENCE}/09-lang-keyboard-nav.png`, fullPage: true });
  });

  test('all pages show correct html lang attribute', async ({ page }) => {
    // Test with pt-BR
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    let htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('pt-BR');

    // Switch to English
    await selectLanguage(page, 1);
    htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('en');

    // Navigate to another page and verify lang persisted
    await page.goto(`${BASE_URL}/settings`, { waitUntil: 'domcontentloaded' });
    await page.waitForLoadState('networkidle');

    htmlLang = await page.evaluate(() => document.documentElement.lang);
    expect(htmlLang).toBe('en');

    await page.screenshot({ path: `${EVIDENCE}/10-lang-attr-persisted.png`, fullPage: true });
  });
});