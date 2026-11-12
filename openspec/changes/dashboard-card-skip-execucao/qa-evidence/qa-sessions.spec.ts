import { test, expect } from '@playwright/test';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const BASE_URL = 'http://localhost:8080';
const PASSWORDS = ['PlexAdmin2026', 'testmaster123'];
const EVIDENCE = join(__dirname);

test.describe('Dashboard & Sessions Skip Visibility QA', () => {
  test.beforeEach(async ({ page, context, request }) => {
    await context.clearCookies();
    
    // Ensure setup
    const status = await request.get(`${BASE_URL}/api/auth/setup/status`);
    const body = await status.json();
    if (body.setupRequired) {
      await request.post(`${BASE_URL}/api/auth/setup`, {
        data: {
          token: 'testmaster123',
          newPassword: 'PlexAdmin2026',
          plexUrl: 'http://127.0.0.1:32400',
          plexToken: 'mocktoken',
          mediaRoots: ['/tmp'],
        },
      });
    }

    // Login
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    let loggedIn = false;
    for (const pwd of PASSWORDS) {
      try {
        await page.waitForSelector('[data-testid="password-input"]', { state: 'visible', timeout: 3000 });
        await page.fill('[data-testid="password-input"]', pwd);
        await page.click('[data-testid="login-submit"]');
        await page.waitForURL(/\/dashboard/, { timeout: 5000 });
        loggedIn = true;
        break;
      } catch {
        // try next
      }
    }
    expect(loggedIn, 'Login failed').toBe(true);
  });

  test('1. Theme toggle validation', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'networkidle' });
    const themeBtn = page.locator('button[aria-label*="Tema"]').first();
    await expect(themeBtn).toBeVisible();
    await themeBtn.click();
    await page.screenshot({ path: join(EVIDENCE, '01-theme-toggled.png'), fullPage: true });
  });

  test('2. Dashboard card Sessões & Skips & Sessions View navigation', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'networkidle' });
    const card = page.locator('a[href="/sessions"]').first();
    await expect(card).toBeVisible();
    await card.click();
    await expect(page).toHaveURL(/\/sessions/);
    await expect(page.locator('h1:has-text("Sessões & Skips")')).toBeVisible();
    await page.screenshot({ path: join(EVIDENCE, '02-sessions-page.png'), fullPage: true });
  });
});
