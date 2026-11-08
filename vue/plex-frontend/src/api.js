/**
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

export async function apiFetch(url, options = {}) {
  const token = getToken();
  const headers = { ...(options.headers ?? {}) };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = await fetch(url, { ...options, headers });

  const publicPage =
    window.location.pathname === "/login" || window.location.pathname === "/setup";
  if (res.status === 401 && url.startsWith("/api/") && !publicPage) {
    redirectToLoginPreservingRoute();
  }
  return res;
}

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

export async function throwHttpError(res, defaultMessage) {
  const detail = await extractErrorMessage(res);
  throw new Error(detail || defaultMessage);
}

export async function fetchSetupStatus() {
  const res = await fetch("/api/auth/setup/status");
  if (!res.ok) return null;
  return await res.json();
}

export async function submitSetup(token, newPassword) {
  return fetch("/api/auth/setup", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ token, newPassword }),
  });
}

export async function changePassword(currentPassword, newPassword) {
  return apiFetch("/api/auth/password", {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ currentPassword, newPassword }),
  });
}

export async function fetchTheme() {
  const res = await apiFetch("/api/theme");
  if (!res.ok) return null;
  return await res.json();
}

export async function fetchStatus() {
  const res = await apiFetch("/api/status");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar status do serviço");
  return res.json();
}

export async function fetchLibraries() {
  const res = await apiFetch("/api/plex/libraries");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar bibliotecas");
  return res.json();
}

export async function fetchMarkers() {
  const res = await apiFetch("/api/markers");
  if (!res.ok) await throwHttpError(res, "Falha ao buscar marcadores");
  return res.json();
}