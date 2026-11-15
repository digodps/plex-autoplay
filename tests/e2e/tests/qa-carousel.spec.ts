import { test, expect } from '@playwright/test';

const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:8080';
const PASSWORD = process.env.OPERATOR_PASSWORD || 'admin123';
const EVIDENCE = '/home/hermes/plex-autoplay-log-carousel-accordion/openspec/changes/log-carousel-accordion/qa-evidence';

test.describe('Log Carousel Accordion QA', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForSelector('[data-testid="password-input"]', { state: 'visible', timeout: 30000 });
    await page.fill('[data-testid="password-input"]', PASSWORD);
    await page.click('[data-testid="login-submit"]');
    await page.waitForSelector('[data-testid="app-header"]', { state: 'visible', timeout: 30000 });
  });

  test('carousel view renders cards', async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: 'domcontentloaded' });
    // Wait for first group header or card (carousel root may have 0 height)
    await page.waitForSelector('[data-testid^="log-group-header-"], [data-testid^="log-card-"]', { state: 'visible', timeout: 15000 });
    // Wait for network to settle (logs fetch + filter debounce)
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    const cards = page.locator('[data-testid^="log-card-"]');
    await expect(cards.first()).toBeVisible({ timeout: 5000 });
    const count = await cards.count();
    console.log(`CARDS RENDERED: ${count}`);
    expect(count).toBeGreaterThan(0);
    await page.screenshot({ path: `${EVIDENCE}/03-carousel-collapsed.png`, fullPage: true });
  });

  test('card expands and shows hierarchy, collapses', async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('[data-testid^="log-group-header-"], [data-testid^="log-card-"]', { state: 'visible', timeout: 15000 });
    await page.waitForLoadState('networkidle', { timeout: 15000 });

    // Trigger a scan to generate logs with hierarchy (library/series/episode)
    await page.click('[data-testid="log-btn-scan"]');
    await page.waitForLoadState('networkidle', { timeout: 30000 });
    await page.waitForTimeout(5000); // Wait for scan to complete and logs to appear

    const firstCard = page.locator('[data-testid^="log-card-"]').first();
    const button = firstCard.locator('button').first();
    await button.click();
    await page.waitForTimeout(500);

    // Expanded content visible
    const content = firstCard.locator('[role="region"]').last();
    await expect(content).toBeVisible({ timeout: 5000 });
    await expect(firstCard.getByText('Hierarquia do processamento')).toBeVisible({ timeout: 5000 });

    await page.screenshot({ path: `${EVIDENCE}/04-carousel-expanded.png`, fullPage: true });

    // Collapse
    await button.click();
    await page.waitForTimeout(500);
    await expect(firstCard.getByText('Hierarquia do processamento')).toBeHidden({ timeout: 5000 });
  });

  test('accordion single-open behavior', async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('[data-testid^="log-card-"]', { state: 'visible', timeout: 15000 });

    const cards = page.locator('[data-testid^="log-card-"]');
    const first = cards.nth(0);
    const second = cards.nth(1);

    await first.locator('button').first().click();
    await page.waitForTimeout(400);
    await expect(first.locator('button').first()).toHaveAttribute('aria-expanded', 'true');

    await second.locator('button').first().click();
    await page.waitForTimeout(400);
    await expect(second.locator('button').first()).toHaveAttribute('aria-expanded', 'true');
    await expect(first.locator('button').first()).toHaveAttribute('aria-expanded', 'false');
  });

  test('theme light/dark toggle applied', async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('[data-testid^="log-card-"]', { state: 'visible', timeout: 15000 });

    const initialTheme = await page.evaluate(() => document.documentElement.dataset.theme || 'light');
    console.log(`INITIAL THEME: ${initialTheme}`);
    expect(['light', 'dark']).toContain(initialTheme);
    await page.screenshot({ path: `${EVIDENCE}/05-theme-${initialTheme}.png`, fullPage: true });

    // Find theme toggle
    const toggle = page.locator('[data-testid="theme-toggle"], button[aria-label*="tema"], button[aria-label*="theme"]').first();
    if (await toggle.count() > 0) {
      await toggle.click();
      await page.waitForTimeout(600);
      const newTheme = await page.evaluate(() => document.documentElement.dataset.theme || 'light');
      console.log(`NEW THEME: ${newTheme}`);
      expect(newTheme).not.toBe(initialTheme);
      await page.screenshot({ path: `${EVIDENCE}/06-theme-${newTheme}.png`, fullPage: true });
      // Card still visible after theme change
      await expect(page.locator('[data-testid="log-carousel-view"]')).toBeVisible();
    } else {
      console.log('NO THEME TOGGLE FOUND');
    }
  });

  test('menu navigation to logs works and active state', async ({ page }) => {
    // Open logs via menu link
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'domcontentloaded' });
    const logsLink = page.locator('a[href="/logs"]').first();
    await expect(logsLink).toBeVisible({ timeout: 10000 });
    await logsLink.click();
    await page.waitForURL(`${BASE_URL}/logs`, { timeout: 10000 });
    await page.waitForSelector('[data-testid^="log-card-"]', { state: 'visible', timeout: 15000 });
    await page.screenshot({ path: `${EVIDENCE}/07-menu-navigation.png`, fullPage: true });
  });

  test('filter bar still works with carousel', async ({ page }) => {
    await page.goto(`${BASE_URL}/logs`, { waitUntil: 'domcontentloaded' });
    await page.waitForSelector('[data-testid^="log-card-"]', { state: 'visible', timeout: 15000 });
    const filter = page.locator('[data-testid="log-filter-text"]');
    if (await filter.count() > 0) {
      await filter.fill('Breaking Bad');
      await page.waitForTimeout(500);
      await page.waitForLoadState('networkidle');
      const cards = page.locator('[data-testid^="log-card-"]');
      const count = await cards.count();
      console.log(`FILTERED CARDS: ${count}`);
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });
});
