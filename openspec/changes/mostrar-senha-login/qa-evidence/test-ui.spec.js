import { test, expect } from '@playwright/test';

const SETUP_TOKEN = process.env.SETUP_TOKEN || 'testmaster123';
const PASSWORD = process.env.E2E_PASSWORD || 'E2eValidSenha123';

test.describe('Validação UI — mostrar-senha-login', () => {
  test('Toggle senha no login, tema light/dark e menus', async ({ page }) => {
    // --- 1. Login e toggle de senha na tela de login ---
    await page.goto('http://localhost:8080/login');
    await page.waitForLoadState('networkidle');

    await expect(page.getByTestId('password-input')).toBeVisible();
    await page.fill('[data-testid="password-input"]', PASSWORD);
    await expect(page.getByTestId('password-input')).toHaveAttribute('type', 'password');

    // Toggle mostra/oculta senha no login
    const loginToggle = page.locator('[data-testid="toggle-password-signin"]');
    await expect(loginToggle).toBeVisible();
    await loginToggle.click();
    await expect(page.getByTestId('password-input')).toHaveAttribute('type', 'text');
    await loginToggle.click();
    await expect(page.getByTestId('password-input')).toHaveAttribute('type', 'password');

    // Login
    await page.click('[data-testid="login-submit"]');
    await page.waitForURL('**/dashboard');
    console.log('Login OK:', page.url());

    // --- 2. Tema light/dark ---
    const docTheme = () => page.evaluate(() => document.documentElement.dataset.theme);
    await expect(docTheme()).resolves.toBe('light');
    const darkBtn = page.locator('[aria-label="Tema escuro"]');
    await expect(darkBtn).toBeVisible();
    await darkBtn.click();
    await expect(docTheme()).resolves.toBe('dark');
    console.log('Tema escuro aplicado.');
    const lightBtn = page.locator('[aria-label="Tema claro"]');
    await expect(lightBtn).toBeVisible();
    await lightBtn.click();
    await expect(docTheme()).resolves.toBe('light');
    console.log('Tema claro restaurado.');

    // --- 3. Menus do usuário ---
    const menuBtn = page.locator('[aria-label="Menu do usuário"]');
    await expect(menuBtn).toBeVisible();
    await menuBtn.click();
    await expect(page.locator('[aria-label="Menu do usuário"]')).toHaveAttribute('aria-expanded', 'true');

    // Alterar senha via menu
    await page.click('[data-testid="change-password-menu"]');
    const modal = page.locator('[role="dialog"][aria-modal="true"], .modal-open').first();
    await expect(modal).toBeVisible();
    console.log('Menu alterar senha aberto.');

    // --- 4. Toggle senha nos campos do modal de troca ---
    const current = modal.locator('[data-testid="password-current"]');
    const newPw = modal.locator('[data-testid="password-new"]');
    const confirm = modal.locator('[data-testid="password-confirm"]');

    await expect(current).toHaveAttribute('type', 'password');
    await modal.locator('[data-testid="toggle-password-current"]').click();
    await expect(current).toHaveAttribute('type', 'text');
    await modal.locator('[data-testid="toggle-password-current"]').click();
    await expect(current).toHaveAttribute('type', 'password');

    await expect(newPw).toHaveAttribute('type', 'password');
    await modal.locator('[data-testid="toggle-password-new"]').click();
    await expect(newPw).toHaveAttribute('type', 'text');

    await expect(confirm).toHaveAttribute('type', 'password');
    const toggleConfirm = modal.locator('[data-testid="toggle-password-confirm"]');
    await toggleConfirm.click();
    await expect(confirm).toHaveAttribute('type', 'text');

    // Fecha modal
    await page.keyboard.press('Escape');

    // --- 5. Ajustes: token do Plex tem toggle ---
    await menuBtn.click();
    const settingsLink = page.getByRole('link', { name: 'Configurações' });
    await expect(settingsLink).toBeVisible();
    await settingsLink.click();
    await page.waitForURL('**/settings');
    await page.click('[data-testid="toggle-password-plex-token"]');
    await expect(page.getByTestId('plex-token')).toHaveAttribute('type', 'text');
    await page.click('[data-testid="toggle-password-plex-token"]');
    await expect(page.getByTestId('plex-token')).toHaveAttribute('type', 'password');
    console.log('Toggle do token do Plex em Ajustes OK.');

    console.log(`URL final acessada: ${page.url()}`);
    console.log('Validação UI concluída com PASS.');
  });
});
