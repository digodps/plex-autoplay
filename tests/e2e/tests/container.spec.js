const { test, expect } = require('@playwright/test');
const { execSync } = require('child_process');
const { loadEnv } = require('../utils/env');
const fs = require('fs');
const path = require('path');

loadEnv();

const PROJECT_ROOT = path.resolve(__dirname, '../../..');

function runCommand(cmd) {
  return execSync(cmd, { cwd: PROJECT_ROOT, encoding: 'utf8', timeout: 120000 });
}

function containerIsRunning() {
  try {
    const output = runCommand('docker compose ps --services --filter status=running');
    return output.includes('plex-autoplay');
  } catch (e) {
    return false;
  }
}

test.describe('Ciclo de vida do container Docker', () => {
  test('inicia o container com ./start.sh e verifica que está rodando', () => {
    try {
      execSync('docker --version', { stdio: 'pipe' });
    } catch (e) {
      test.skip(true, 'Docker não está disponível');
    }

    if (!fs.existsSync(`${PROJECT_ROOT}/.env`)) {
      test.skip(true, '.env não encontrado — crie a partir do .env.example');
    }

    // Se o container já está rodando, não precisa iniciar novamente
    if (!containerIsRunning()) {
      runCommand('./scripts/start.sh --detach');
    }

    // Aguarda e verifica que o container está rodando
    expect(containerIsRunning()).toBe(true);
  });

  test('verifica que o script stop.sh existe e é executável', () => {
    const stopScript = `${PROJECT_ROOT}/scripts/stop.sh`;
    expect(fs.existsSync(stopScript)).toBe(true);
    expect(fs.statSync(stopScript).isFile()).toBe(true);
    // Verifica permissão de execução
    const mode = fs.statSync(stopScript).mode;
    expect(mode & 0o111).toBeGreaterThan(0);
  });

  test('verifica logs via ./logs.sh --no-follow --tail 10', () => {
    try {
      execSync('docker --version', { stdio: 'pipe' });
    } catch (e) {
      test.skip(true, 'Docker não está disponível');
    }

    if (!containerIsRunning()) {
      test.skip(true, 'Container não está rodando');
    }

    // Executa logs.sh e verifica que retorna saída sem erro
    const output = runCommand('./scripts/logs.sh --no-follow --tail 10');
    expect(output.length).toBeGreaterThan(0);
  });
});
