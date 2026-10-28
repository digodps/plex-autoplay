import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('auth_token') || '',
    isAuthenticated: !!localStorage.getItem('auth_token')
  }),
  actions: {
    login(token) {
      this.token = token
      this.isAuthenticated = true
      localStorage.setItem('auth_token', token)
    },
    logout() {
      this.token = ''
      this.isAuthenticated = false
      localStorage.removeItem('auth_token')
    }
  }
})
