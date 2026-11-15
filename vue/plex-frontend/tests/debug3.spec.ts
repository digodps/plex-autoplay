import { test, expect } from '@playwright/test';

test('debug simple login flow', async ({ page }) => {
  // Go to root - redirects to setup
  await page.goto('/');
  await page.waitForLoadState('networkidle');
  
  console.log('Root page URL:', page.url());
  console.log('Root page title:', await page.title());
  
  // Check if we're on setup page
  if (page.url().includes('/setup')) {
    console.log('On setup page, going to login');
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    console.log('Login page URL:', page.url());
  }
  
  // Try to fill and submit
  const passwordInput = page.locator('input[type="password"]');
  await expect(passwordInput).toBeVisible({ timeout: 5000 });
  console.log('Password input found');
  
  await passwordInput.fill('admin123');
  console.log('Password filled');
  
  const submitBtn = page.locator('button[type="submit"]');
  await expect(submitBtn).toBeVisible({ timeout: 5000 });
  console.log('Submit button found');
  
  await submitBtn.click();
  console.log('Submit clicked');
  
  await page.waitForTimeout(2000);
  console.log('After click URL:', page.url());
  
  // Check for error message
  const error = page.locator('[data-testid="error"]');
  if (await error.count() > 0) {
    console.log('Error:', await error.textContent());
  }
  
  const error2 = page.locator('.alert-error, .error');
  if (await error2.count() > 0) {
    console.log('Error2:', await error2.textContent());
  }
  
  // Check console
  const token = await page.evaluate(() => document.cookie);
  console.log('document.cookie:', token);
  
  console.log('SUCCESS');
});
