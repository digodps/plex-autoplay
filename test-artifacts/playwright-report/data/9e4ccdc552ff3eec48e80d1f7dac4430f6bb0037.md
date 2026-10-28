# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: images.spec.js >> Proxy de imagens do Plex >> requisita imagem via proxy e verifica Content-Type
- Location: tests/images.spec.js:19:3

# Error details

```
Error: expect(received).not.toBe(expected) // Object.is equality

Expected: not 500
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
  9  | test.describe('Proxy de imagens do Plex', () => {
  10 |   test.beforeEach(async ({ page }) => {
  11 |     // Faz login antes de cada teste
  12 |     await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  13 |     await page.waitForSelector('input[type="password"]', { state: 'visible', timeout: 10000 });
  14 |     await page.fill('input[type="password"]', ADMIN_PASSWORD);
  15 |     await page.click('button');
  16 |     await page.waitForURL(`${BASE_URL}/dashboard`);
  17 |   });
  18 | 
  19 |   test('requisita imagem via proxy e verifica Content-Type', async ({ page }) => {
  20 |     // Intercepta a requisição de imagem para verificar headers
  21 |     let imageResponse = null;
  22 |     page.on('response', async (response) => {
  23 |       if (response.url().includes('/api/images/')) {
  24 |         imageResponse = response;
  25 |       }
  26 |     });
  27 | 
  28 |     // Navega para uma página que carrega imagens de biblioteca
  29 |     await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });
  30 |     await page.waitForTimeout(3000); // Aguarda carregamento de imagens
  31 | 
  32 |     // Se houve requisição de imagem, verifica Content-Type
  33 |     if (imageResponse) {
  34 |       const contentType = imageResponse.headers()['content-type'] || '';
  35 |       expect(contentType).toMatch(/image\/(jpeg|jpg|png|webp)/);
  36 |     } else {
  37 |       // Testa diretamente via API request
  38 |       const token = await page.evaluate(() => localStorage.getItem('auth_token'));
  39 |       const imgResponse = await page.request.get(`${BASE_URL}/api/images/thumb/test123`, {
  40 |         headers: { Authorization: `Bearer ${token}` },
  41 |       });
  42 |       // Pode retornar 404 se a imagem não existe, mas não deve expor token
> 43 |       expect(imgResponse.status()).not.toBe(500);
     |                                        ^ Error: expect(received).not.toBe(expected) // Object.is equality
  44 |     }
  45 |   });
  46 | 
  47 |   test('token Plex não é exposto em requisições de imagem', async ({ page }) => {
  48 |     let exposedToken = false;
  49 | 
  50 |     // Monitora todas as requisições de rede para vazamento de token
  51 |     page.on('request', (request) => {
  52 |       const url = request.url();
  53 |       const headers = request.headers();
  54 | 
  55 |       // Verifica se PLEX_TOKEN aparece na URL
  56 |       if (url.includes('PLEX_TOKEN=') || url.includes('X-Plex-Token:')) {
  57 |         exposedToken = true;
  58 |       }
  59 | 
  60 |       // Verifica headers
  61 |       for (const [key, value] of Object.entries(headers)) {
  62 |         if (String(value).includes(process.env.PLEX_TOKEN || '')) {
  63 |           exposedToken = true;
  64 |         }
  65 |       }
  66 |     });
  67 | 
  68 |     await page.goto(`${BASE_URL}/libraries`, { waitUntil: 'networkidle' });
  69 |     await page.waitForTimeout(3000);
  70 | 
  71 |     expect(exposedToken).toBe(false);
  72 |   });
  73 | });
  74 | 
```