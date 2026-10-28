# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: libraries.spec.js >> Bibliotecas de séries >> acessa /libraries e exibe apenas bibliotecas de tipo "show"
- Location: tests/libraries.spec.js:19:3

# Error details

```
TimeoutError: page.waitForSelector: Timeout 10000ms exceeded.
Call log:
  - waiting for locator('[data-testid="library-card"], .library-card, .library-item') to be visible

```

# Page snapshot

```yaml
- generic [ref=f1e3]:
  - heading "Bibliotecas" [level=1] [ref=f1e4]
  - generic [ref=f1e5]: Erro ao carregar bibliotecas
```

# Test source

```ts
  1  | const { test, expect } = require('@playwright/test');
  2  | const { loadEnv } = require('../utils/env');
  3  | 
  4  | loadEnv();
  5  | 
  6  | const BASE_URL = process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:8080';
  7  | const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;
  8  | 
  9  | test.describe('Bibliotecas de séries', () => {
  10 |   test.beforeEach(async ({ page }) => {
  11 |     // Faz login antes de cada teste
  12 |     await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  13 |     await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
  14 |     await page.fill('input[type="password"]', ADMIN_PASSWORD);
  15 |     await page.click('button');
  16 |     await page.waitForURL(`${BASE_URL}/dashboard`);
  17 |   });
  18 | 
  19 |   test('acessa /libraries e exibe apenas bibliotecas de tipo "show"', async ({ page }) => {
  20 |     await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });
  21 | 
  22 |     // Aguarda carregamento das bibliotecas
> 23 |     await page.waitForSelector('[data-testid="library-card"], .library-card, .library-item', {
     |                ^ TimeoutError: page.waitForSelector: Timeout 10000ms exceeded.
  24 |       timeout: 10000,
  25 |     });
  26 | 
  27 |     // Verifica que apenas bibliotecas do tipo "show" são exibidas
  28 |     const libraryCards = page.locator('[data-testid="library-card"], .library-card, .library-item');
  29 |     const count = await libraryCards.count();
  30 | 
  31 |     for (let i = 0; i < count; i++) {
  32 |       const card = libraryCards.nth(i);
  33 |       const typeText = await card.locator('[data-testid="library-type"], .library-type, .type').textContent();
  34 |       expect(typeText).toContain('show');
  35 |     }
  36 |   });
  37 | 
  38 |   test('exibe erro quando Plex está indisponível', async ({ page, context }) => {
  39 |     // Bloqueia requisições ao Plex para simular indisponibilidade
  40 |     await context.route('**/library/sections', (route) => {
  41 |       route.abort('internalservererror');
  42 |     });
  43 | 
  44 |     await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });
  45 | 
  46 |     // Verifica mensagem de erro
  47 |     const errorMsg = page.locator('.error, [data-testid="error"], .alert-error, .unavailable');
  48 |     await expect(errorMsg.first()).toBeVisible();
  49 |   });
  50 | });
  51 | 
```