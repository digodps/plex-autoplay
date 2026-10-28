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
  document.cookie = `${name}=${encodeURIComponent(value)}${expiry}; path=/; SameSite=Lax`;
}

export function deleteCookie(name) {
  document.cookie = `${name}=; max-age=0; path=/`;
}

export function getToken() {
  return getCookie(AUTH_COOKIE);
}

function redirectToLoginPreservingRoute() {
  const route = window.location.pathname + window.location.search;
  deleteCookie(AUTH_COOKIE);
  window.location.href = `/login?redirect=${encodeURIComponent(route)}`;
}

/**
 * fetch com Authorization do cookie; em 401 limpa a sessão e devolve ao login
 * (registrando a rota para retorno após autenticação).
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
 * Helpers específicos do Dashboard
 */

/** Busca saúde do serviço + último job de sync + métricas do processo */
export async function fetchStatus() {
  const res = await apiFetch("/api/status");
  if (!res.ok) throw new Error("Falha ao buscar status do serviço");
  return res.json();
}

/** Busca bibliotecas de séries e agrega contagens */
export async function fetchLibraries() {
  const res = await apiFetch("/api/plex/libraries");
  if (!res.ok) throw new Error("Falha ao buscar bibliotecas");
  return res.json();
}

/** Busca todos os marcadores (flat list) para agregação client-side */
export async function fetchMarkers() {
  const res = await apiFetch("/api/markers");
  if (!res.ok) throw new Error("Falha ao buscar marcadores");
  return res.json();
}
