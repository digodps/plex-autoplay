/**
 * Helpers de autenticação: token em cookie + fetch com tratamento de 401.
 *
 * O token continua viajando no header `Authorization: Bearer` (o middleware do
 * backend só lê o header); o cookie substitui o localStorage como armazenamento.
 */

export const AUTH_COOKIE = "auth_token";

export function getCookie(name) {
  const match = document.cookie.match(new RegExp("(^|;\\s*)" + name + "=([^;]*)"));
  return match ? decodeURIComponent(match[2]) : "";
}

/**
 * @param {string} name
 * @param {string} value
 * @param {number} days 0/negativo => cookie de sessão (encerra com o navegador)
 */
export function setCookie(name, value, days = 0) {
  const expiry = days > 0 ? `; max-age=${days * 24 * 60 * 60}` : "";
  const secure = location.protocol === "https:" ? "; Secure" : "";
  document.cookie = `${name}=${encodeURIComponent(value)}${expiry}; path=/; SameSite=Lax${secure}`;
}

export function deleteCookie(name) {
  const secure = location.protocol === "https:" ? "; Secure" : "";
  document.cookie = `${name}=; max-age=0; path=/; SameSite=Lax${secure}`;
}

export function getToken() {
  return getCookie(AUTH_COOKIE);
}

function redirectToLoginPreservingRoute() {
  // Já no login: não redirecionar (evita loop com chamadas 401 da própria tela de login)
  if (window.location.pathname === "/login") return;
  const route = window.location.pathname + window.location.search;
  deleteCookie(AUTH_COOKIE);
  window.location.href = `/login?redirect=${encodeURIComponent(route)}`;
}

/**
 * Fetch com Authorization do cookie; em 401 limpa sessão e devolve ao login
 * (registrando rota retorno após autenticação).
 */
export async function apiFetch(url, options = {}) {
  const token = getToken();
  const headers = { ...(options.headers ?? {}) };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(url, { ...options, headers });

  if (res.status === 401 && url.startsWith("/api/")) {
    redirectToLoginPreservingRoute();
  }
  return res;
}

/**
 * Extrai uma mensagem legível do corpo de uma resposta de erro (RFC 7807
 * Problem Details ou texto simples). Retorna null quando não houver corpo.
 */
export async function extractErrorMessage(res) {
  try {
    const ct = res.headers.get("content-type") || "";
    if (ct.includes("application/json")) {
      const body = await res.json();
      return body.detail || body.title || body.message || JSON.stringify(body);
    }
    const text = await res.text();
    return text || null;
  } catch {
    return null;
  }
}

/**
 * Lança um erro contendo mensagem detalhada da resposta (quando disponível)
 * para que o cliente possa exibi-la ao usuário e sugerir uma ação.
 */
export async function throwHttpError(res, defaultMessage) {
  const detail = await extractErrorMessage(res);
  throw new Error(detail || defaultMessage);
}

/** Helpers de tema */
export async function fetchTheme() {
  const res = await apiFetch("/api/theme");
  if (!res.ok) return null;
  return await res.json();
}

/** Fim helpers de tema */

/** Helpers específicos do Dashboard */

/** Busca saúde do serviço + último job de sync + métricas do processo */
export async function fetchStatus() {
  const res = await apiFetch("/api/status");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar status do serviço");
  return res.json();
}

/** Busca bibliotecas de séries e agrega contagens */
export async function fetchLibraries() {
  const res = await apiFetch("/api/plex/libraries");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar bibliotecas");
  return res.json();
}

/** Busca todos os marcadores (flat list) para agregação client-side */
export async function fetchMarkers() {
  const res = await apiFetch("/api/markers");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar marcadores");
  return res.json();
}
