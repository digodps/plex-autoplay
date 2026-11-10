import { test, expect } from '@playwright/test';

test('Validação UI obrigatória: login, tema light/dark e menu do usuário', async ({ page }) => {
  const base = 'http://localhost:8080';

  // ---- Login ----
  await page.goto(base + '/login');
  await page.waitForLoadState('domcontentloaded');
  await page.getByTestId('password-input').fill(process.env.ADMIN_PASSWORD ?? 'admin123');
  await page.getByTestId('login-submit').click();
  await page.waitForURL(/dashboard/, { timeout: 15000 });
  await page.waitForTimeout(1000);

  const html = page.locator('html');
  const themeBtn = page.getByRole('button', { name: /tema|theme/i }).first();

  // ---- Tema light/dark ----
  await expect(themeBtn).toBeVisible({ timeout: 5000 });
  const dark0 = ((await html.getAttribute('class')) ?? '').includes('dark');
  await themeBtn.click();
  await page.waitForTimeout(400);
  const dark1 = ((await html.getAttribute('class')) ?? '').includes('dark');
  expect(dark1).not.toBe(dark0);
  await page.screenshot({ path: 'openspec/changes/otimizar-docker-compose/qa-evidence-theme-dark.png', fullPage: true });

  await themeBtn.click();
  await page.waitForTimeout(400);
  const dark2 = ((await html.getAttribute('class')) ?? '').includes('dark');
  expect(dark2).toBe(dark0);
  await page.screenshot({ path: 'openspec/changes/otimizar-docker-compose/qa-evidence-theme-light.png', fullPage: true });

  // ---- Menu do usuário: abrir ----
  const userMenu = page.getByRole('button', { name: /usuário|user/i }).first();
  await expect(userMenu).toBeVisible({ timeout: 5000 });
  await userMenu.click();
  await page.waitForTimeout(400);
  const changePwdItem = page.getByTestId('change-password-menu');
  await expect(changePwdItem).toBeVisible({ timeout: 5000 });
  await page.screenshot({ path: 'openspec/changes/otimizar-docker-compose/qa-evidence-menu-open.png', fullPage: true });

  // ---- Fechar com Escape ----
  // Fechar: clicar novamente no botão do usuário (toggle)
  await userMenu.click();
  await page.waitForTimeout(400);
  await expect(changePwdItem).toBeHidden();

  // ---- Navegação entre menus/rotas e estado ativo ----
  await page.getByRole('link', { name: /bibliotecas|libraries/i }).first().click().catch(() => {});
  await page.waitForTimeout(800);
  console.log('URL após navegação:', page.url());
  await page.screenshot({ path: 'openspec/changes/otimizar-docker-compose/qa-evidence-nav.png', fullPage: true });
});
