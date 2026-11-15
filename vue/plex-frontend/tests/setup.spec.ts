import { test, expect } from '@playwright/test';

test('complete setup via UI with mocked Plex', async ({ page }) => {
  // Mock the Plex identity API call
  await page.route('**/api/plex/identity', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        success: true,
        friendlyName: 'Test Plex',
        version: '1.0.0'
      })
    });
  });
  
  // Go to setup page
  await page.goto('/setup');
  await page.waitForLoadState('networkidle');
  
  console.log('Setup page URL:', page.url());
  console.log('Setup page title:', await page.title());
  
  // Check step 1 elements
  const tokenInput = page.locator('[data-testid="setup-token"]');
  await expect(tokenInput).toBeVisible({ timeout: 10000 });
  console.log('Step 1 elements found');
  
  // Step 1: Password
  const setupToken = 'test_master_password_123';
  await page.fill('[data-testid="setup-token"]', setupToken);
  await page.fill('[data-testid="setup-password"]', 'admin123');
  await page.fill('[data-testid="setup-confirm"]', 'admin123');
  await page.click('[data-testid="setup-next"]');
  await page.waitForTimeout(2000);
  
  console.log('After step 1 URL:', page.url());
  
  // Check step 2 elements
  const plexUrlInput = page.locator('[data-testid="setup-plex-url"]');
  await expect(plexUrlInput).toBeVisible({ timeout: 10000 });
  console.log('Step 2 elements found');
  
  // Step 2: Plex - skip test, just continue
  await page.fill('[data-testid="setup-plex-url"]', 'http://host.docker.internal:32400');
  await page.fill('[data-testid="setup-plex-token"]', 'test_token');
  await page.click('[data-testid="setup-next"]');
  await page.waitForTimeout(2000);
  
  console.log('After step 2 URL:', page.url());
  
  // Check step 3 elements
  const submitBtn = page.locator('[data-testid="setup-submit"]');
  await expect(submitBtn).toBeVisible({ timeout: 10000 });
  console.log('Step 3 elements found');
  
  // Step 3: Media - add a media root first
  const mediaRootInput = page.locator('[data-testid="media-root-add"]').locator('..').locator('input');
  if (await mediaRootInput.count() > 0) {
    await mediaRootInput.fill('/home/hermes/media');
    await page.click('[data-testid="media-root-add"]');
    await page.waitForTimeout(1000);
  }
  
  console.log('Clicking submit...');
  await page.click('[data-testid="setup-submit"]');
  await page.waitForTimeout(10000);
  
  console.log('After setup URL:', page.url());
  console.log('After setup title:', await page.title());
  
  // Should redirect to dashboard
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 10000 });
  console.log('SUCCESS: Redirected to dashboard');
});
