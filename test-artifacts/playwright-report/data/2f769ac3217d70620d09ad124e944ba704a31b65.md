# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: container.spec.js >> Ciclo de vida do container Docker >> verifica que o script stop.sh existe e é executável
- Location: tests/container.spec.js:43:3

# Error details

```
Error: expect(received).toBe(expected) // Object.is equality

Expected: true
Received: false
```

# Test source

```ts
  1  | const { test, expect } = require('@playwright/test');
  2  | const { execSync } = require('child_process');
  3  | const { loadEnv } = require('../utils/env');
  4  | const fs = require('fs');
  5  | const path = require('path');
  6  | 
  7  | loadEnv();
  8  | 
  9  | const PROJECT_ROOT = path.resolve(__dirname, '../../..');
  10 | 
  11 | function runCommand(cmd) {
  12 |   return execSync(cmd, { cwd: PROJECT_ROOT, encoding: 'utf8', timeout: 120000 });
  13 | }
  14 | 
  15 | function containerIsRunning() {
  16 |   try {
  17 |     const output = runCommand('docker compose ps --services --filter status=running');
  18 |     return output.includes('plex-autoplay');
  19 |   } catch (e) {
  20 |     return false;
  21 |   }
  22 | }
  23 | 
  24 | test.describe('Ciclo de vida do container Docker', () => {
  25 |   test('inicia o container com ./start.sh e verifica que está rodando', () => {
  26 |     try {
  27 |       execSync('docker --version', { stdio: 'pipe' });
  28 |     } catch (e) {
  29 |       test.skip(true, 'Docker não está disponível');
  30 |     }
  31 | 
  32 |     if (!fs.existsSync(`${PROJECT_ROOT}/.env`)) {
  33 |       test.skip(true, '.env não encontrado — crie a partir do .env.example');
  34 |     }
  35 | 
  36 |     // Inicia o container (se já estiver rodando, o start.sh recria)
  37 |     runCommand('./start.sh --detach');
  38 | 
  39 |     // Aguarda e verifica que o container está rodando
  40 |     expect(containerIsRunning()).toBe(true);
  41 |   });
  42 | 
  43 |   test('verifica que o script stop.sh existe e é executável', () => {
  44 |     const stopScript = `${PROJECT_ROOT}/stop.sh`;
> 45 |     expect(fs.existsSync(stopScript)).toBe(true);
     |                                       ^ Error: expect(received).toBe(expected) // Object.is equality
  46 |     expect(fs.statSync(stopScript).isFile()).toBe(true);
  47 |     // Verifica permissão de execução
  48 |     const mode = fs.statSync(stopScript).mode;
  49 |     expect(mode & 0o111).toBeGreaterThan(0);
  50 |   });
  51 | 
  52 |   test('verifica logs via ./logs.sh --no-follow --tail 10', () => {
  53 |     try {
  54 |       execSync('docker --version', { stdio: 'pipe' });
  55 |     } catch (e) {
  56 |       test.skip(true, 'Docker não está disponível');
  57 |     }
  58 | 
  59 |     if (!containerIsRunning()) {
  60 |       test.skip(true, 'Container não está rodando');
  61 |     }
  62 | 
  63 |     // Executa logs.sh e verifica que retorna saída sem erro
  64 |     const output = runCommand('./logs.sh --no-follow --tail 10');
  65 |     expect(output.length).toBeGreaterThan(0);
  66 |   });
  67 | });
  68 | 
```