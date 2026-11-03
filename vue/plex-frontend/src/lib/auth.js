/** Helpers de token/cookie — funções puras, sem rede. */

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
