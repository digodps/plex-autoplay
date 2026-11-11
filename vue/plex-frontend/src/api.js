/**
<<<<<<< HEAD
 * Authenticated fetch helpers. Token travels in the Authorization header;
 * a non-HttpOnly cookie is used as client-side storage.
 */

import { getToken, deleteCookie } from "./lib/auth";

export { getToken, setCookie, deleteCookie, AUTH_COOKIE } from "./lib/auth";

function redirectToLoginPreservingRoute() {
  if (window.location.pathname === "/login") return;
  const route = window.location.pathname + window.location.search;
  deleteCookie("auth_token");
  window.location.href = `/login?redirect=${encodeURIComponent(route)}`;
}

=======
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
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export async function apiFetch(url, options = {}) {
  const token = getToken();
  const headers = { ...(options.headers ?? {}) };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(url, { ...options, headers });

<<<<<<< HEAD
<<<<<<< HEAD
  const publicPage =
    window.location.pathname === "/login" || window.location.pathname === "/setup";
  if (res.status === 401 && url.startsWith("/api/") && !publicPage) {
=======
  if (res.status === 401 && url.startsWith("/api/")) {
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
=======
  if (res.status === 401 && url.startsWith("/api/") && window.location.pathname !== "/login") {
>>>>>>> origin/feature/fix-plex-episodes-endpoint-31-10-2026-19-14-11
    redirectToLoginPreservingRoute();
  }
  return res;
}

<<<<<<< HEAD
=======
/**
 * Extrai uma mensagem legível do corpo de uma resposta de erro (RFC 7807
 * Problem Details ou texto simples). Retorna null quando não houver corpo.
 */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
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

<<<<<<< HEAD
export async function extractErrors(res) {
  try {
    const body = await res.json();
    if (Array.isArray(body?.errors)) return body.errors;
    const message = body?.message || body?.detail || body?.title || JSON.stringify(body);
    return message ? [message] : [];
  } catch {
    return [];
  }
}

=======
/**
 * Lança um erro contendo mensagem detalhada da resposta (quando disponível)
 * para que o cliente possa exibi-la ao usuário e sugerir uma ação.
 */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export async function throwHttpError(res, defaultMessage) {
  const detail = await extractErrorMessage(res);
  throw new Error(detail || defaultMessage);
}

<<<<<<< HEAD
export async function fetchSetupStatus() {
  const res = await fetch("/api/auth/setup/status");
  if (!res.ok) return null;
  return await res.json();
}

export async function submitSetup(token, newPassword, plexUrl, plexToken, mediaRoots) {
  return fetch("/api/auth/setup", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token, newPassword, plexUrl, plexToken, mediaRoots }),
  });
}

export async function fetchDirectories(path) {
  const query = path ? "?path=" + encodeURIComponent(path) : "";
  return apiFetch("/api/filesystem/directories" + query);
}

export async function changePassword(currentPassword, newPassword) {
  return apiFetch("/api/auth/password", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ currentPassword, newPassword }),
  });
}

=======
/** Helpers de tema */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export async function fetchTheme() {
  const res = await apiFetch("/api/theme");
  if (!res.ok) return null;
  return await res.json();
}

<<<<<<< HEAD
=======
/** Fim helpers de tema */

/** Helpers específicos do Dashboard */

/** Busca saúde do serviço + último job de sync + métricas do processo */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export async function fetchStatus() {
  const res = await apiFetch("/api/status");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar status do serviço");
  return res.json();
}

<<<<<<< HEAD
=======
/** Busca bibliotecas de séries e agrega contagens */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export async function fetchLibraries() {
  const res = await apiFetch("/api/plex/libraries");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar bibliotecas");
  return res.json();
}

<<<<<<< HEAD
=======
/** Busca todos os marcadores (flat list) para agregação client-side */
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
export async function fetchMarkers() {
  const res = await apiFetch("/api/markers");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar marcadores");
  return res.json();
}
<<<<<<< HEAD

export async function syncRun() {
  return apiFetch("/api/sync/run", { method: "POST" });
}
=======
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
