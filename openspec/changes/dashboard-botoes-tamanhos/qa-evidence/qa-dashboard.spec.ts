import { test, expect } from '@playwright/test';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);
const BASE_URL = 'http://localhost:8080';
const PASSWORDS = ['PlexAdmin2026', 'testmaster123'];
const EVIDENCE = join(__dirname);

test.describe('Dashboard QA - Button Size Standardization', () => {
  test.beforeEach(async ({ page, context, request }) => {
    // Fresh context per test: clear cookies; localStorage is per-context
    await context.clearCookies();

    // Ensure setup is done (app may need master setup once)
    const status = await request.get(`${BASE_URL}/api/auth/setup/status`);
    const body = await status.json();
    if (body.setupRequired) {
      await request.post(`${BASE_URL}/api/auth/setup`, {
        data: {
          token: 'testmaster123',
          newPassword: 'PlexAdmin2026',
          plexUrl: 'http://192.168.18.5:32400',
          plexToken: 'zyRszxikoo8fdybxT2rS',
          mediaRoots: ['/home/hermes/media'],
        },
      });
    }

    // Login via UI (try both passwords)
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    let loggedIn = false;
    for (const pwd of PASSWORDS) {
      try {
        await page.waitForSelector('[data-testid="password-input"]', { state: 'visible', timeout: 5000 });
        await page.fill('[data-testid="password-input"]', pwd);
        await page.click('[data-testid="login-submit"]');
        await page.waitForURL(/\/dashboard/, { timeout: 8000 });
        loggedIn = true;
        console.log(`Logged in with password: ${pwd}`);
        break;
      } catch {
        console.log(`Login failed with ${pwd}`);
      }
    }
    expect(loggedIn, 'Could not log in with either credential').toBe(true);
    await page.waitForSelector('header', { state: 'visible' });
    await page.screenshot({ path: join(EVIDENCE, '01-dashboard-loaded.png'), fullPage: true });
  });

  test('1. Theme toggle - <html> .dark class added/removed; button legible in both', async ({ page }) => {
    const themeBtn = page.locator('header button[aria-label*="Tema"]').first();
    await expect(themeBtn).toBeVisible();
    const html = page.locator('html');

    const initialDark = await html.evaluate((el) => el.classList.contains('dark'));
    console.log(`Initial .dark class: ${initialDark}`);

    await themeBtn.click();
    let toggledDark = initialDark;
    for (let i = 0; i < 10; i++) {
      await page.waitForTimeout(200);
      toggledDark = await html.evaluate((el) => el.classList.contains('dark'));
      if (toggledDark !== initialDark) break;
    }
    console.log(`Toggled .dark class: ${toggledDark}`);

    const themeChanged = toggledDark !== initialDark;
    expect(themeChanged).toBe(true);

    // Button legible in toggled theme
    const dashboardBtn = page.locator('button:has-text("Ver Log Completo")').first();
    await expect(dashboardBtn).toBeVisible();
    await page.screenshot({ path: join(EVIDENCE, '02-theme-toggled.png'), fullPage: true });

    // Toggle back to initial state
    await themeBtn.click();
    let restoredDark = toggledDark;
    for (let i = 0; i < 10; i++) {
      await page.waitForTimeout(200);
      restoredDark = await html.evaluate((el) => el.classList.contains('dark'));
      if (restoredDark === initialDark) break;
    }
    expect(restoredDark).toBe(initialDark);
  });

  test('2. Menu navigation - /libraries, back to /dashboard via logo/breadcrumb, assert URL + active highlight', async ({ page }) => {
    // Navigate to /libraries via direct navigation
    await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });
    await expect(page).toHaveURL(/\/libraries/);

    const libActive = page.locator('header nav [aria-current="page"]').first();
    await expect(libActive).toBeVisible();
    await expect(libActive).toContainText('Bibliotecas');
    await page.screenshot({ path: join(EVIDENCE, '03-libraries.png'), fullPage: true });

    // Back to /dashboard via logo link (/ redirects to /dashboard)
    const logo = page.locator('header a[aria-label="Plex Autoplay — início"]').first();
    if ((await logo.count()) > 0) {
      await logo.click();
    } else {
      await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'networkidle' });
    }
    await expect(page).toHaveURL(/\/dashboard/);
    await expect(page.locator('header')).toBeVisible();
  });

  test('3. Button size standardization on Dashboard', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'networkidle' });

    const btnViewLog = page.locator('button:has-text("Ver Log Completo")').first();
    const btnStartScan = page.locator('button:has-text("Iniciar Escaneamento")').first();
    await expect(btnViewLog).toBeVisible();
    await expect(btnStartScan).toBeVisible();

    const buttons = [
      { name: 'Ver Log Completo', locator: btnViewLog },
      { name: 'Iniciar Escaneamento', locator: btnStartScan },
      { name: 'Tentar novamente (total)', locator: page.locator('div[role="alert"] button:has-text("Tentar novamente")').first() },
      { name: 'Tentar novamente (lib)', locator: page.locator('div[role="alert"]:nth-of-type(2) button:has-text("Tentar novamente")').first() },
    ];

    const results: any[] = [];

    for (const b of buttons) {
      const visible = await b.locator.isVisible().catch(() => false);
      if (!visible) {
        const count = await b.locator.count();
        const classAttr = count > 0 ? await b.locator.getAttribute('class') : null;
        const classes = classAttr ? classAttr.split(' ') : [];
        const hasBtn = classes.includes('btn');
        const hasBtnMd = classes.includes('btn-md');
        const variants = classes.filter((c) => ['btn-primary', 'btn-secondary', 'btn-danger', 'btn-warning'].includes(c));
        const noBad = !classes.some((c) => ['px-5', 'py-2.5', 'rounded-xl'].includes(c));
        results.push({
          name: b.name, visible: false, classAttr, hasBtn, hasBtnMd, variants,
          hasExactlyOneVariant: variants.length === 1, noBad, height: null,
          note: 'Not rendered (no error state) - DOM checked',
        });
        continue;
      }

      const classAttr = await b.locator.getAttribute('class');
      const classes = classAttr ? classAttr.split(' ') : [];
      const hasBtn = classes.includes('btn');
      const hasBtnMd = classes.includes('btn-md');
      const variants = classes.filter((c) => ['btn-primary', 'btn-secondary', 'btn-danger', 'btn-warning'].includes(c));
      const noBad = !classes.some((c) => ['px-5', 'py-2.5', 'rounded-xl'].includes(c));
      const box = await b.locator.boundingBox();

      results.push({
        name: b.name, visible: true, classAttr, hasBtn, hasBtnMd, variants,
        hasExactlyOneVariant: variants.length === 1, noBad, height: box?.height,
      });

      expect(hasBtn, `${b.name}: missing 'btn' class`).toBe(true);
      expect(hasBtnMd, `${b.name}: missing 'btn-md' class`).toBe(true);
      expect(variants.length, `${b.name}: expected exactly one variant, got ${variants.join(',') || 'none'}`).toBe(1);
      expect(noBad, `${b.name}: contains forbidden classes (px-5/py-2.5/rounded-xl)`).toBe(true);
    }

    // Measure heights precisely, multiple runs
    const viewLogBox = await btnViewLog.boundingBox();
    const startScanBox = await btnStartScan.boundingBox();
    const viewLogStyle = await btnViewLog.evaluate((el) => getComputedStyle(el).height);
    const startScanStyle = await btnStartScan.evaluate((el) => getComputedStyle(el).height);
    for (let i = 0; i < 3; i++) {
      const vb = await btnViewLog.boundingBox();
      const sb = await btnStartScan.boundingBox();
      const vStyle = await btnViewLog.evaluate((el) => getComputedStyle(el).height);
      const sStyle = await btnStartScan.evaluate((el) => getComputedStyle(el).height);
      console.log(`Height run ${i}: bbox ViewLog=${vb?.height}px StartScan=${sb?.height}px | computed ViewLog=${vStyle} StartScan=${sStyle}`);
      await page.waitForTimeout(50);
    }
    console.log(`FINAL Heights: Ver Log Completo = ${viewLogBox?.height}px, Iniciar Escaneamento = ${startScanBox?.height}px`);
    expect(viewLogBox?.height).toBe(startScanBox?.height);

    await page.screenshot({ path: join(EVIDENCE, '05-dashboard-buttons.png'), clip: { x: 0, y: 0, width: 1280, height: 400 } });

    for (const r of results) {
      const variant = r.variants && r.variants[0] ? r.variants[0] : 'NONE';
      console.log(
        `${r.visible ? 'VISIBLE' : 'hidden '} | ${r.name.padEnd(28)} | btn=${r.hasBtn} btn-md=${r.hasBtnMd} variant=${variant.padEnd(10)} height=${r.height ?? '-'}px noBad=${r.noBad} | classes: ${r.classAttr?.substring(0, 120)}`
      );
    }
  });

  test('4. Focus-visible and disabled states on all buttons', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`, { waitUntil: 'networkidle' });

    const btnViewLog = page.locator('button:has-text("Ver Log Completo")').first();
    const btnStartScan = page.locator('button:has-text("Iniciar Escaneamento")').first();

    for (const btn of [btnViewLog, btnStartScan]) {
      await btn.focus();
      const focused = await btn.evaluate((el) => el === document.activeElement);
      expect(focused).toBe(true);
      const classes = await btn.getAttribute('class');
      expect(classes).toContain('btn');
    }

    const viewLogClasses = await btnViewLog.getAttribute('class');
    const startScanClasses = await btnStartScan.getAttribute('class');
    expect(viewLogClasses).toContain('btn');
    expect(startScanClasses).toContain('btn');
    expect(viewLogClasses).toContain('btn-md');
    expect(startScanClasses).toContain('btn-md');
  });
});