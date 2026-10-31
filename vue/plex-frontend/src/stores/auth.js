import { defineStore } from "pinia";
import { deleteCookie, AUTH_COOKIE } from "../api";

export const useAuthStore = defineStore("auth", {
  state: () => ({
    isAuthenticated: false,
  }),
  actions: {
    login() {
      this.isAuthenticated = true;
    },
    logout() {
      this.isAuthenticated = false;
      deleteCookie(AUTH_COOKIE);
    },
    setAuthenticated(value) {
      this.isAuthenticated = value;
    },
  },
});
