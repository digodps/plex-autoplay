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
    /**
     * @param {string} token
     * @param {boolean} keepSigned true => cookie persistente (30 dias);
     * false => cookie de sessão do navegador
     */
    login(token, keepSigned = false) {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
      setCookie(AUTH_COOKIE, token, keepSigned ? 30 : 0);
      this.token = token;
    },
    logout() {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
    },
  },
});
