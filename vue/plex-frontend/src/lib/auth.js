export const AUTH_COOKIE = "auth_token";

export function getCookie(name) {
  const match = document.cookie.match(new RegExp("(^|;\\s*)" + name + "=([^;]*)"));
  return match ? decodeURIComponent(match[2]) : "";
}

function buildCookieString(name, value, options = {}) {
  const { maxAge, path = "/", sameSite = "Lax", secure = location.protocol === "https:" } = options;
  const expiry = maxAge !== undefined ? `; max-age=${maxAge}` : "";
  const secureFlag = secure ? "; Secure" : "";
  return `${name}=${encodeURIComponent(value)}${expiry}; path=${path}; SameSite=${sameSite}${secureFlag}`;
}

export function setCookie(name, value, maxAgeSeconds = 0) {
  const maxAge = maxAgeSeconds > 0 ? maxAgeSeconds : undefined;
  document.cookie = buildCookieString(name, value, { maxAge });
}

export function deleteCookie(name) {
  const attrs = [
    { secure: true, sameSite: "Lax" },
    { secure: false, sameSite: "Lax" },
    { secure: true, sameSite: "None" },
    { secure: false, sameSite: "None" },
    { secure: true, sameSite: "Strict" },
    { secure: false, sameSite: "Strict" },
  ];
  for (const opt of attrs) {
    document.cookie = buildCookieString(name, "", { maxAge: 0, ...opt });
  }
}

export function getToken() {
  return getCookie(AUTH_COOKIE);
}