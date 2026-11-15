import { test, expect } from '@playwright/test';

test('simple page load login', async ({ page }) => {
  await page.goto('/login');
  await page.waitForLoadState('networkidle');
  console.log('URL:', page.url());
  console.log('Title:', await page.title());
});
