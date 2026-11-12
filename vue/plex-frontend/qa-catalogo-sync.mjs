import { chromium } from 'playwright';

const base = 'http://localhost:8080';
const evidenceDir = 'openspec/changes/catalogo-mensagem-botao-forcar';
const results = [];
function check(name, ok, detail = '') {
  results.push({ name, ok, detail });
  console.log(`${ok ? 'PASS' : 'FAIL'} | ${name}${detail ? ' | ' + detail : ''}`);
}

const browser = await chromium.launch({ headless: true });
const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });

try {
  // 1. Login
  await page.goto(base + '/login');
  await page.waitForLoadState('domcontentloaded');
  await page.getByTestId('password-input').fill('admin123');
  await page.getByTestId('login-submit').click();
  await page.waitForURL(/dashboard/, { timeout: 15000 });
  await page.waitForTimeout(1200);
  check('login + redirect dashboard', /dashboard/.test(page.url()), page.url());

  // 2. Navegar para bibliotecas
  await page.getByRole('link', { name: /bibliotecas|libraries/i }).first().click();
  await page.waitForURL(/libraries/, { timeout: 10000 });
  await page.waitForTimeout(1200);
  check('navegação para bibliotecas', /libraries/.test(page.url()), page.url());
  await page.screenshot({ path: `${evidenceDir}/qa-evidence-libraries.png`, fullPage: true });

  // 3. Tema light/dark
  const html = page.locator('html');
  const themeBtn = page.getByRole('button', { name: /tema|theme/i }).first();
  const themeVisible = await themeBtn.isVisible().catch(() => false);
  if (themeVisible) {
    const dark0 = ((await html.getAttribute('class')) ?? '').includes('dark');
    await themeBtn.click();
    await page.waitForTimeout(400);
    const dark1 = ((await html.getAttribute('class')) ?? '').includes('dark');
    check('tema toggle alterna', dark1 !== dark0, `dark0=${dark0} dark1=${dark1}`);
    await page.screenshot({ path: `${evidenceDir}/qa-evidence-theme-dark.png`, fullPage: true });
    await themeBtn.click();
    await page.waitForTimeout(400);
    const dark2 = ((await html.getAttribute('class')) ?? '').includes('dark');
    check('tema volta ao original', dark2 === dark0, `dark2=${dark2}`);
    await page.screenshot({ path: `${evidenceDir}/qa-evidence-theme-light.png`, fullPage: true });
  } else {
    check('botão de tema visível', false, 'não encontrado');
  }

  // 4. Menu do usuário abre/fecha
  const userMenu = page.getByRole('button', { name: /usuário|user/i }).first();
  const menuVisible = await userMenu.isVisible().catch(() => false);
  if (menuVisible) {
    await userMenu.click();
    await page.waitForTimeout(400);
    const changePwd = page.getByTestId('change-password-menu');
    const openOk = await changePwd.isVisible().catch(() => false);
    check('menu usuário abre', openOk);
    await page.screenshot({ path: `${evidenceDir}/qa-evidence-menu-open.png`, fullPage: true });
    await userMenu.click();
    await page.waitForTimeout(400);
    const closedOk = await changePwd.isHidden().catch(() => true);
    check('menu usuário fecha', closedOk);
  } else {
    check('botão menu usuário visível', false, 'não encontrado');
  }

  // 5. Entrar numa biblioteca e validar mensagem + botão de sync
  const libLinks = page.locator('a[href*="/series/"]');
  const libCount = await libLinks.count();
  check('bibliotecas listadas', libCount > 0, `${libCount} biblioteca(s)`);
  if (libCount > 0) {
    await libLinks.first().click();
    await page.waitForTimeout(1500);
    await page.screenshot({ path: `${evidenceDir}/qa-evidence-series.png`, fullPage: true });

    const forceSyncBtn = page.getByRole('button', { name: /Forçar sincronização/i });
    const btnVisible = await forceSyncBtn.isVisible().catch(() => false);
    if (btnVisible) {
      const msgVisible = await page
        .getByText('O catálogo desta biblioteca ainda não foi sincronizado.')
        .isVisible()
        .catch(() => false);
      check('mensagem catálogo não processado visível', msgVisible);
      check('botão Forçar sincronização visível', btnVisible);
      await forceSyncBtn.click();
      await page.waitForTimeout(3000);
      const feedback = await page
        .locator('[role="alert"]')
        .first()
        .textContent()
        .catch(() => '');
      check('feedback após forçar sync', !!feedback && feedback.trim().length > 0, (feedback || '').trim());
      await page.screenshot({ path: `${evidenceDir}/qa-evidence-sync-result.png`, fullPage: true });
    } else {
      const hasSeries = await page.locator('img').count();
      check(
        'biblioteca já possui séries (botão não aplicável)',
        hasSeries > 0,
        `${hasSeries} imagens; sem estado vazio`,
      );
    }
  }
} catch (err) {
  check('execução sem exceção', false, err.message);
} finally {
  await browser.close();
}

const failed = results.filter((r) => !r.ok);
console.log(`\n=== RESUMO: ${results.length - failed.length}/${results.length} PASS ===`);
if (failed.length) {
  console.log('FALHAS:');
  for (const f of failed) console.log(` - ${f.name}: ${f.detail}`);
  process.exit(1);
}
