import { test, expect } from '@playwright/test';

test('debug libraries page content', async ({ page }) => {
  const errors = [];
  page.on('console', msg => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });
  page.on('pageerror', error => {
    errors.push(error.message);
  });
  
  // Login first
  await page.goto('/login');
  await page.waitForLoadState('networkidle');
  
  await page.fill('input[type="password"]', 'admin123');
  await page.click('button[type="submit"]');
  await page.waitForURL('**/dashboard', { timeout: 10000 });
  
  // Navigate to libraries via link
  await page.goto('/dashboard');
  await page.waitForLoadState('networkidle');
  
  const librariesLink = page.locator('a[href="/libraries"]').first();
  await librariesLink.click();
  await page.waitForTimeout(3000);
  
  console.log('Final URL:', page.url());
  console.log('Final title:', await page.title());
  console.log('Console errors:', errors);
  
  // Check for library cards
  const libraryCards = page.locator('.library-card');
  const count = await libraryCards.count();
  console.log('Library cards count:', count);
});
