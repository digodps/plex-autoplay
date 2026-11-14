import { chromium } from 'playwright';

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 390, height: 844 } });

  // Mock session API
  await page.route('**/api/sessions/skip-status', async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        activeSessions: [
          {
            machineIdentifier: 'client-123',
            product: 'Plex for Samsung',
            platform: 'Tizen',
            state: 'playing',
            positionMs: 1292000,
            durationMs: 1375000,
            nextMarker: { kind: 'credits', msUntilSkip: 80000 }
          }
        ],
        recentSkips: [
          {
            timestamp: new Date().toISOString(),
            product: 'Plex for Samsung',
            ratingKey: '12345',
            markerKind: 'intro',
            skippedFromMs: 149000,
            skippedToMs: 195000
          }
        ]
      })
    });
  });

  // Login
  await page.goto('http://localhost:8080/login');
  await page.waitForLoadState('domcontentloaded');
  await page.getByTestId('password-input').fill(process.env.ADMIN_PASSWORD ?? 'admin123');
  await page.getByTestId('login-submit').click();
  await page.waitForURL(/dashboard/, { timeout: 15000 });

  // Navigate to sessions
  await page.goto('http://localhost:8080/sessions');
  await page.waitForLoadState('domcontentloaded');
  await page.waitForTimeout(1000);

  const bodyText = await page.textContent('body');
  if (bodyText.includes('Carregando') || bodyText.includes('Falha ao carregar')) {
    throw new Error('Sessions page did not load data: ' + bodyText.substring(0, 300));
  }

  const mobileCards = page.locator('.space-y-3.md\\:hidden');
  const cardCount = await mobileCards.count();
  if (cardCount === 0) throw new Error('No mobile cards rendered on sessions page');
  console.log('Mobile cards rendered:', cardCount);

  await page.waitForTimeout(500);
  const tableElements = await page.locator('table').count();
  console.log('Desktop table elements (should be 0 on mobile):', tableElements);

  if (!bodyText.includes('Plex for Samsung')) {
    throw new Error('Expected session product not found in mobile view');
  }

  // Theme toggle
  const html = page.locator('html');
  const dark0 = ((await html.getAttribute('class')) || '').includes('dark');
  const themeBtn = page.locator('button[aria-label*="Tema"]').first();
  if (await themeBtn.count() > 0) {
    await themeBtn.click();
    await page.waitForTimeout(400);
    const dark1 = ((await html.getAttribute('class')) || '').includes('dark');
    if (dark1 === dark0) throw new Error('Theme toggle did not change theme class');
    console.log('Theme toggle works correctly');
  }

  // User menu
  const menuBtn = page.locator('button[aria-label="Menu do usuário"]');
  if (await menuBtn.count() > 0) {
    await menuBtn.click();
    await page.waitForTimeout(300);
    const settingsLink = page.locator('text=Configurações');
    if (await settingsLink.count() > 0) {
      await settingsLink.click();
      await page.waitForTimeout(500);
      if (!page.url().includes('/settings')) throw new Error('Menu navigation failed');
      console.log('User menu navigation works correctly');
    }
  }

  await page.screenshot({ path: 'openspec/changes/refactor-session-mobile-layout/qa-evidence-mobile-sessions.png', fullPage: true });
  console.log('MOBILE SESSIONS LAYOUT QA PASS');
  await browser.close();
})().catch(e => {
  console.error('MOBILE SESSIONS LAYOUT QA FAIL:', e.message);
  process.exit(1);
});