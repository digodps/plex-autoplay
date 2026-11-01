// Load environment variables from .env file (mimics the shell 'source .env')
// This is used by E2E acceptance tests to read config values.

const fs = require('fs');
const path = require('path');

// Caminho relativo: a partir de tests/e2e/utils/ -> projeta root é ../..
const projectRoot = path.resolve(__dirname, '..', '..');
const envPath = path.join(projectRoot, '.env');

if (fs.existsSync(envPath)) {
  const envContent = fs.readFileSync(envPath, 'utf-8');
  // Parse linhas KEY=VALUE e defini-las como variáveis de ambiente
  envContent.split('\n').forEach((line) => {
    const trimmed = line.trim();
    // Ignora comentários e linhas vazias
    if (!trimmed || trimmed.startsWith('#')) return;

    const eqIdx = trimmed.indexOf('=');
    if (eqIdx === -1) return;

    const key = trimmed.substring(0, eqIdx).trim();
    const value = trimmed.substring(eqIdx + 1).trim();

    // Remove as aspas se presentes
    const cleanValue = value.replace(/^["']|["']$/g, '');

    // Define apenas se não já existir (para não sobrescrever variáveis do container)
    if (typeof process.env[key] === 'undefined') {
      process.env[key] = cleanValue;
    }
  });
}

module.exports = { loadEnv };
module.exports.loadEnv = loadEnv;