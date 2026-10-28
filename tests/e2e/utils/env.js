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

  const missing = [];

  for (const line of lines) {
    const trimmed = line.trim();
    // Ignora linhas vazias e comentários
    if (!trimmed || trimmed.startsWith("#")) continue;

    const eqIndex = trimmed.indexOf("=");
    if (eqIndex === -1) continue;

    const key = trimmed.slice(0, eqIndex).trim();
    const value = trimmed
      .slice(eqIndex + 1)
      .trim()
      .replace(/^"|"$|^'|'$/g, "");

    // Não sobrescreve variáveis já definidas no ambiente
    if (!(key in process.env)) {
      process.env[key] = value;
    }
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
}

module.exports = { loadEnv };
