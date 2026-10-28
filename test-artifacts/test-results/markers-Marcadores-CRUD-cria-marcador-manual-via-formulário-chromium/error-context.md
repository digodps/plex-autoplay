# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: markers.spec.js >> Marcadores (CRUD) >> cria marcador manual via formulário
- Location: tests/markers.spec.js:19:3

# Error details

```
TimeoutError: page.fill: Timeout 10000ms exceeded.
Call log:
  - waiting for locator('[name="rating_key"], input[data-testid="rating_key"]')

```

# Page snapshot

```yaml
- generic [ref=f1e3]:
  - heading "Marcadores" [level=1] [ref=f1e4]
  - generic [ref=f1e5]:
    - heading "Adicionar Marcador" [level=3] [ref=f1e6]
    - generic [ref=f1e7]:
      - textbox "Rating Key" [ref=f1e8]
      - combobox [ref=f1e9]:
        - option "Intro" [selected]
        - option "Créditos"
      - spinbutton "Início (ms)" [ref=f1e10]: "0"
      - spinbutton "Fim (ms)" [ref=f1e11]: "0"
      - button "Adicionar" [ref=f1e12] [cursor=pointer]
  - generic [ref=f1e13]:
    - heading "Marcadores Existentes" [level=3] [ref=f1e14]
    - generic [ref=f1e15]: Nenhum marcador encontrado.
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
  9  | test.describe('Marcadores (CRUD)', () => {
  10 |   test.beforeEach(async ({ page }) => {
  11 |     // Faz login antes de cada teste
  12 |     await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  13 |     await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
  14 |     await page.fill('input[type="password"]', ADMIN_PASSWORD);
  15 |     await page.click('button');
  16 |     await page.waitForURL(`${BASE_URL}/dashboard`);
  17 |   });
  18 | 
  19 |   test('cria marcador manual via formulário', async ({ page }) => {
  20 |     await page.goto(`${BASE_URL}/markers`, { waitUntil: 'networkidle' });
  21 | 
  22 |     // Preenche formulário de criação
> 23 |     await page.fill('[name="rating_key"], input[data-testid="rating_key"]', '12345');
     |                ^ TimeoutError: page.fill: Timeout 10000ms exceeded.
  24 |     await page.selectOption('[name="kind"], select[data-testid="kind"]', 'intro');
  25 |     await page.fill('[name="start_ms"], input[data-testid="start_ms"]', '1000');
  26 |     await page.fill('[name="end_ms"], input[data-testid="end_ms"]', '90000');
  27 | 
  28 |     // Submete
  29 |     await page.click('button, [data-testid="add-marker"]');
  30 | 
  31 |     // Verifica que o marcador aparece na listagem
  32 |     await expect(page.locator('[data-testid="marker-12345-intro"]')).toBeVisible();
  33 |   });
  34 | 
  35 |   test('remove marcador existente', async ({ page }) => {
  36 |     await page.goto(`${BASE_URL}/markers`, { waitUntil: 'networkidle' });
  37 | 
  38 |     // Assume que existe um marcador para remover (ou cria um primeiro)
  39 |     const addButton = page.locator('[data-testid="add-marker"]');
  40 |     if ((await addButton.count()) > 0) {
  41 |       await page.fill('[name="rating_key"], input[data-testid="rating_key"]', '99999');
  42 |       await page.selectOption('[name="kind"], select[data-testid="kind"]', 'credits');
  43 |       await page.fill('[name="start_ms"], input[data-testid="start_ms"]', '3600000');
  44 |       await page.fill('[name="end_ms"], input[data-testid="end_ms"]', '3660000');
  45 |       await page.click('button');
  46 |       await expect(page.locator('[data-testid="marker-99999-credits"]')).toBeVisible();
  47 |     }
  48 | 
  49 |     // Remove o marcador
  50 |     const removeBtn = page.locator('[data-testid="remove-marker-99999-credits"]').first();
  51 |     if ((await removeBtn.count()) > 0) {
  52 |       await removeBtn.click();
  53 |       await expect(page.locator('[data-testid="marker-99999-credits"]')).not.toBeVisible();
  54 |     }
  55 |   });
  56 | 
  57 |   test('lista marcadores via GET /api/markers', async ({ page, request }) => {
  58 |     // Faz login para obter token
  59 |     await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  60 |     await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
  61 |     await page.fill('input[type="password"]', ADMIN_PASSWORD);
  62 |     await page.click('button');
  63 |     await page.waitForURL(`${BASE_URL}/dashboard`);
  64 | 
  65 |     // Obtém token do localStorage
  66 |     const token = await page.evaluate(() => localStorage.getItem('auth_token'));
  67 | 
  68 |     // Chama a API de marcadores
  69 |     const response = await request.get(`${BASE_URL}/api/markers`, {
  70 |       headers: { Authorization: `Bearer ${token}` },
  71 |     });
  72 | 
  73 |     expect(response.status()).toBe(200);
  74 |     const markers = await response.json();
  75 |     expect(Array.isArray(markers)).toBe(true);
  76 |   });
  77 | });
  78 | 
```