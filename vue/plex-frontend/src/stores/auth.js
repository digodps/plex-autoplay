import { defineStore } from "pinia";
import { getToken, setCookie, deleteCookie, AUTH_COOKIE } from "../api";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    token: getToken(),
  }),
  getters: {
    isAuthenticated: (state) => !!state.token,
  },
  actions: {
<<<<<<< HEAD
    login(token) {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
      setCookie(AUTH_COOKIE, token, 0);
=======
    /**
     * @param {string} token
     * @param {boolean} keepSigned true => cookie persistente (30 dias);
     * false => cookie de sessão do navegador
     */
    login(token, keepSigned = false) {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
      setCookie(AUTH_COOKIE, token, keepSigned ? 30 : 0);
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
      this.token = token;
    },
    logout() {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
    },
  },
<<<<<<< HEAD
});
=======
});
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856
