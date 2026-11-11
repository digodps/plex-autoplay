<<<<<<< HEAD
// Load environment variables from .env file (mimics the shell 'source .env')
// This is used by E2E acceptance tests to read config values.

const fs = require("fs");
const path = require("path");

// tests/e2e/utils -> raiz do projeto
const projectRoot = path.resolve(__dirname, "..", "..", "..");
const envPath = path.join(projectRoot, ".env");
=======
const fs = require("fs");
const path = require("path");

/**
 * Carrega variáveis de ambiente de um arquivo .env para process.env.
 * Variáveis já definidas no ambiente não são sobrescritas.
 * @param {string} envPath - Caminho para o arquivo .env (padrão: raiz do projeto)
 */
function loadEnv(envPath = path.resolve(__dirname, "../../../.env")) {
  if (!fs.existsSync(envPath)) {
    throw new Error(
      `Arquivo .env não encontrado em ${envPath}. ` +
        "Copie .env.example para .env e preencha com seus valores.",
    );
  }

  const envText = fs.readFileSync(envPath, "utf8");
  const lines = envText.split("\n");
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

function loadEnv() {
  if (!fs.existsSync(envPath)) return;

  const envContent = fs.readFileSync(envPath, "utf-8");
  envContent.split("\n").forEach((line) => {
    const trimmed = line.trim();
<<<<<<< HEAD
    if (!trimmed || trimmed.startsWith("#")) return;

    const eqIdx = trimmed.indexOf("=");
    if (eqIdx === -1) return;

    const key = trimmed.substring(0, eqIdx).trim();
    const value = trimmed.substring(eqIdx + 1).trim();
    const cleanValue = value.replace(/^["']|["']$/g, "");
=======
    // Ignora linhas vazias e comentários
    if (!trimmed || trimmed.startsWith("#")) continue;

    const eqIndex = trimmed.indexOf("=");
    if (eqIndex === -1) continue;

    const key = trimmed.slice(0, eqIndex).trim();
    const value = trimmed
      .slice(eqIndex + 1)
      .trim()
      .replace(/^"|"$|^'|'$/g, "");
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

    if (typeof process.env[key] === "undefined") {
      process.env[key] = cleanValue;
    }
<<<<<<< HEAD
  });
=======
  }

  // Valida variáveis obrigatórias
  const required = ["PLEX_URL", "PLEX_TOKEN", "JWT_SECRET", "ADMIN_PASSWORD"];
  for (const key of required) {
    if (!process.env[key]) {
      missing.push(key);
    }
  }

  if (missing.length > 0) {
    throw new Error(
      `Variáveis obrigatórias não definidas no .env: ${missing.join(", ")}. ` +
        "Copie .env.example para .env e preencha com seus valores.",
    );
  }

  return process.env;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
}

module.exports = { loadEnv };
