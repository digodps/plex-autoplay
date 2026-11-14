import { test, expect } from '@playwright/test';

const BASE = 'http://localhost:4173';

test.describe('Sessions Page - Mobile Layout Validation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE}/sessions`, { waitUntil: 'networkidle' });
    // Page heading rendered
    await page.waitForSelector('text=Sessões ativas', { timeout: 10000 });
  });

  test('Theme toggle works - light/dark mode applied correctly', async ({ page }) => {
    const themeMode = () => page.evaluate(() => localStorage.getItem('theme.mode'));

    // Start explicitly in light mode
    await page.evaluate(() => localStorage.setItem('theme.mode', 'light'));
    await page.reload({ waitUntil: 'networkidle' });
    await expect(themeMode()).resolves.toBe('light');
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'light');

    const toggle = page.locator('button[aria-label="Tema claro"], button[aria-label="Tema escuro"]').first();
    await expect(toggle).toBeVisible({ timeout: 5000 });

    // light -> dark
    await toggle.click();
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await expect(themeMode()).resolves.toBe('dark');

    // Persistence across reload
    await page.reload({ waitUntil: 'networkidle' });
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
    await expect(themeMode()).resolves.toBe('dark');

    // dark -> system (resolves to light under default color scheme)
    await toggle.click();
    await expect(themeMode()).resolves.toBe('system');
  });

  test('Sessions page renders active sessions as cards on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForSelector('text=Sessões ativas', { timeout: 10000 });

    // Mobile card container (space-y-3 md:hidden) is visible
    const mobileCards = page.locator('div.space-y-3.md\\:hidden').first();
    await expect(mobileCards).toBeVisible();
    await expect(mobileCards.locator('> div')).toHaveCount(2);

    // Desktop table is hidden at this viewport
    const desktopTable = page.locator('table.hidden.md\\:table').first();
    await expect(desktopTable).toBeHidden();

    // No horizontal overflow
    const bodyScrollWidth = await page.evaluate(() => document.body.scrollWidth);
    const viewportWidth = await page.evaluate(() => window.innerWidth);
    expect(bodyScrollWidth).toBeLessThanOrEqual(viewportWidth + 1);

    await page.screenshot({ path: 'openspec/changes/refactor-session-mobile-layout/qa-evidence/sessions-mobile-light.png', fullPage: true });

    // Dark mode mobile
    await page.evaluate(() => localStorage.setItem('theme.mode', 'dark'));
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForSelector('text=Sessões ativas', { timeout: 10000 });
    await page.screenshot({ path: 'openspec/changes/refactor-session-mobile-layout/qa-evidence/sessions-mobile-dark.png', fullPage: true });
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  });

  test('Sessions page renders as table on desktop viewport', async ({ page }) => {
    await page.setViewportSize({ width: 1280, height: 800 });
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForSelector('text=Sessões ativas', { timeout: 10000 });

    const desktopTable = page.locator('table.hidden.md\\:table').first();
    await expect(desktopTable).toBeVisible();
    await expect(desktopTable.locator('tbody tr')).toHaveCount(2);

    // Mobile cards hidden at desktop viewport
    const mobileCards = page.locator('div.space-y-3.md\\:hidden').first();
    await expect(mobileCards).toBeHidden();

    await page.screenshot({ path: 'openspec/changes/refactor-session-mobile-layout/qa-evidence/sessions-desktop-light.png', fullPage: true });
  });

  test('Skip history section is readable on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForSelector('text=Histórico de skips', { timeout: 10000 });

    const skipHeading = page.locator('text=Histórico de skips').first();
    await expect(skipHeading).toBeVisible();

    const bodyScrollWidth = await page.evaluate(() => document.body.scrollWidth);
    const viewportWidth = await page.evaluate(() => window.innerWidth);
    expect(bodyScrollWidth).toBeLessThanOrEqual(viewportWidth + 1);
  });

  test('Navigation menus work correctly (user menu)', async ({ page }) => {
    const menuBtn = page.locator('button[aria-label="Menu do usuário"]').first();
    await expect(menuBtn).toBeVisible();
    await expect(menuBtn).toHaveAttribute('aria-expanded', 'false');

    await menuBtn.click();
    await expect(menuBtn).toHaveAttribute('aria-expanded', 'true');
    await expect(page.locator('[data-testid="change-password-menu"]')).toBeVisible();

    // Navigate via menu item, menu closes
    await page.click('a[href="/settings"]');
    await expect(page).toHaveURL(`${BASE}/settings`);
    await expect(menuBtn).toHaveAttribute('aria-expanded', 'false');
  });

  test('Breadcrumb navigation works', async ({ page }) => {
    await page.goto(`${BASE}/libraries`, { waitUntil: 'networkidle' });

    const breadcrumb = page.locator('nav[aria-label=Breadcrumb]');
    await expect(breadcrumb).toBeVisible();

    const current = breadcrumb.locator('[aria-current="page"]');
    await expect(current.first()).toBeVisible();
  });

  test('No horizontal overflow on mobile viewport', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto(`${BASE}/sessions`, { waitUntil: 'networkidle' });
    await page.waitForSelector('text=Sessões ativas', { timeout: 10000 });

    const bodyScrollWidth = await page.evaluate(() => document.body.scrollWidth);
    const viewportWidth = await page.evaluate(() => window.innerWidth);
    expect(bodyScrollWidth).toBeLessThanOrEqual(viewportWidth + 1);
  });
});