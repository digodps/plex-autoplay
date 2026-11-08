// Conclui o setup inicial do operador uma vez antes da suíte.
// O token é passado via SETUP_TOKEN; em produção ele vem do log do container.
module.exports = async () => {
  const base = (process.env.PLAYWRIGHT_BASE_URL || "http://localhost:8080").replace(/\/$/, "");
  const statusRes = await fetch(`${base}/api/auth/setup/status`);
  if (!statusRes.ok) {
    throw new Error(`Falha ao consultar setup: HTTP ${statusRes.status}`);
  }

  const { setupRequired } = await statusRes.json();
  if (!setupRequired) return;

  const token = process.env.SETUP_TOKEN;
  const password = process.env.OPERATOR_PASSWORD;
  if (!token || !password) {
    throw new Error(
      "Setup pendente: defina SETUP_TOKEN (log do container) e OPERATOR_PASSWORD para os testes e2e.",
    );
  }

  const res = await fetch(`${base}/api/auth/setup`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      token,
      newPassword: password,
      plexUrl: process.env.PLEX_URL || "http://plex-mock:32400",
      plexToken: process.env.PLEX_TOKEN || "e2e-token",
      mediaRoots: (process.env.MEDIA_ROOTS || process.env.MEDIA_ROOT || "/media")
        .split(/[\n,]/)
        .map((path) => path.trim())
        .filter(Boolean),
    }),
  });

  if (res.status !== 204 && res.status !== 409) {
    throw new Error(`Falha no setup inicial: HTTP ${res.status}`);
  }
};
