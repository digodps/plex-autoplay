import { test, expect } from '@playwright/test';

test.describe('Series Detail Page', () => {
  test.beforeEach(async ({ page }) => {
    // Mock Plex API to return fake libraries
    await page.route('**/api/plex/libraries', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { key: '1', title: 'TV Shows', type: 'show' },
          { key: '2', title: 'Anime', type: 'show' }
        ])
      });
    });

    // Mock series list API (for series view)
    await page.route('**/api/series/**', async (route) => {
      const url = route.request().url();
      if (url.includes('/episodes')) {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify([
            { 
              rating_key: 'ep1', 
              parent_key: '1', 
              title: 'Pilot', 
              season_number: 1, 
              episode_number: 1, 
              watched: false,
              progress: 0,
              duration: 2700000,
              media_url: '/api/stream/ep1'
            },
            { 
              rating_key: 'ep2', 
              parent_key: '1', 
              title: 'Episode 2', 
              season_number: 1, 
              episode_number: 2, 
              watched: true,
              progress: 2700000,
              duration: 2700000,
              media_url: '/api/stream/ep2'
            }
          ])
        });
      } else {
        // Series list for a library - return format with items and totalSize
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              { ratingKey: 'series1', title: 'Test Series', libraryKey: '1', thumb: '/thumb1.jpg' },
              { ratingKey: 'series2', title: 'Another Series', libraryKey: '1', thumb: '/thumb2.jpg' }
            ],
            totalSize: 2
          })
        });
      }
    });

    // Mock auto-play preference
    await page.route('**/api/user/preferences/autoplay', async (route) => {
      if (route.request().method() === 'GET') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ enabled: true })
        });
      } else if (route.request().method() === 'PUT') {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({ success: true })
        });
      }
    });

    // Login
    await page.goto('/');
    if (page.url().includes('/setup')) {
      await page.goto('/login');
      await page.fill('input[type="password"]', 'admin123');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL(/\/dashboard/);
    } else if (!page.url().includes('/dashboard')) {
      await page.goto('/login');
      await page.fill('input[type="password"]', 'admin123');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL(/\/dashboard/);
    }
  });

  test('should navigate to series detail page and show video player', async ({ page }) => {
    // Navigate to libraries
    await page.goto('/libraries');
    await expect(page.locator('h1:has-text("Bibliotecas")')).toBeVisible({ timeout: 10000 });
    
    // Click on first library that has series
    const libraryCard = page.locator('[data-testid="library-card"]').first();
    await libraryCard.waitFor({ state: 'visible', timeout: 10000 });
    await libraryCard.click();
    
    // Should be on series list page
    await expect(page.locator('.series-view')).toBeVisible({ timeout: 10000 });
    
    // Click on first series
    const firstSeries = page.locator('.series-card').first();
    await firstSeries.waitFor({ state: 'visible', timeout: 10000 });
    await firstSeries.click();
    
    // Should be on series detail page
    await expect(page).toHaveURL(/\/series\//, { timeout: 10000 });
    await expect(page.locator('.series-page')).toBeVisible({ timeout: 10000 });
    
    // Check video player is visible
    await expect(page.locator('.video-player')).toBeVisible({ timeout: 10000 });
    
    // Check episode list is visible (desktop layout)
    await expect(page.locator('.episode-list')).toBeVisible({ timeout: 10000 });
  });

  test('should have search at top above tabs', async ({ page }) => {
    await page.goto('/libraries');
    const libraryCard = page.locator('[data-testid="library-card"]').first();
    await libraryCard.waitFor({ state: 'visible', timeout: 10000 });
    await libraryCard.click();
    
    // Search should be in toolbar at top
    await expect(page.locator('.search-box')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.search-box input[type="search"]')).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Theme Toggle', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    if (page.url().includes('/setup')) {
      await page.goto('/login');
      await page.fill('input[type="password"]', 'admin123');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL(/\/dashboard/);
    } else if (!page.url().includes('/dashboard')) {
      await page.goto('/login');
      await page.fill('input[type="password"]', 'admin123');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL(/\/dashboard/);
    }
  });

  test('should toggle theme between light and dark', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Wait for theme to be applied
    await page.waitForTimeout(500);
    
    // Find theme toggle button (aria-label contains "Modo de tema" or "theme")
    const themeToggle = page.locator('button[aria-label*="tema" i], button[aria-label*="theme" i]').first();
    await expect(themeToggle).toBeVisible({ timeout: 10000 });
    
    // Get initial theme - check for dark class
    const initialTheme = await page.evaluate(() => 
      document.documentElement.classList.contains('dark') ? 'dark' : 'light'
    );
    
    await themeToggle.click();
    await page.waitForTimeout(500);
    
    const newTheme = await page.evaluate(() => 
      document.documentElement.classList.contains('dark') ? 'dark' : 'light'
    );
    
    expect(newTheme).not.toBe(initialTheme);
  });
});