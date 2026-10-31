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
    login(token) {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
      // Cookie de sessão (encerra quando o navegador fecha)
      setCookie(AUTH_COOKIE, token, 0);
      this.token = token;
    },
    logout() {
      this.token = "";
      deleteCookie(AUTH_COOKIE);
    },
  },
});
