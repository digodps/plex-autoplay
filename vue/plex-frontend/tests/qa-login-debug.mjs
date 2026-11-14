import { chromium } from 'playwright';
(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 390, height: 844 } });
  page.on('console', m => console.log('CONSOLE:', m.text()));
  page.on('pageerror', e => console.log('PAGEERROR:', e.message));
  await page.goto('http://localhost:8080/login');
  await page.waitForLoadState('domcontentloaded');
  await page.waitForTimeout(2000);
  console.log('URL:', page.url());
  const bodyText = await page.textContent('body');
  console.log('BODY:', bodyText.substring(0, 500));
  await page.screenshot({ path: '/tmp/opencode/login-debug.png', fullPage: true });
  await browser.close();
})().catch(e => { console.error(e); process.exit(1); });