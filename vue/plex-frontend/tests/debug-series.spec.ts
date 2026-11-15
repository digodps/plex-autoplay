import { test, expect } from '@playwright/test';

test('debug series page full', async ({ page }) => {
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
    console.log('API request:', url);
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
  
  // Wait a bit for navigation
  await page.waitForTimeout(3000);
  
  console.log('URL after click:', page.url());
  console.log('Title:', await page.title());
  
  // Check page content - get full HTML
  const bodyHtml = await page.locator('body').innerHTML();
  console.log('Body HTML length:', bodyHtml.length);
  
  // Search for series-page
  const idx = bodyHtml.indexOf('series-page');
  if (idx >= 0) {
    console.log('Found series-page at index:', idx);
    console.log('Context:', bodyHtml.substring(Math.max(0, idx-200), idx+200));
  } else {
    console.log('series-page NOT found in body');
  }
  
  // Check for video-player
  const idx2 = bodyHtml.indexOf('video-player');
  if (idx2 >= 0) {
    console.log('Found video-player at index:', idx2);
    console.log('Context:', bodyHtml.substring(Math.max(0, idx2-200), idx2+200));
  } else {
    console.log('video-player NOT found in body');
  }
  
  // Check for episode-list
  const idx3 = bodyHtml.indexOf('episode-list');
  if (idx3 >= 0) {
    console.log('Found episode-list at index:', idx3);
    console.log('Context:', bodyHtml.substring(Math.max(0, idx3-200), idx3+200));
  } else {
    console.log('episode-list NOT found in body');
  }
  
  // Check for loading/error states
  const idx4 = bodyHtml.indexOf('series-page__loading');
  if (idx4 >= 0) {
    console.log('Found series-page__loading at index:', idx4);
  }
  const idx5 = bodyHtml.indexOf('series-page__error');
  if (idx5 >= 0) {
    console.log('Found series-page__error at index:', idx5);
  }
});
