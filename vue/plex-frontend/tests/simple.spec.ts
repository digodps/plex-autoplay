import { test, expect } from '@playwright/test';

test('simple page load', async ({ page }) => {
  await page.goto('/');
  await page.waitForLoadState('networkidle');
  console.log('URL:', page.url());
  console.log('Title:', await page.title());
});
