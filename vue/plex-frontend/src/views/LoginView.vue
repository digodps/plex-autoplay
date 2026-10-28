<script setup>
import { useAuthStore } from '../stores/auth'
import { useRouter } from 'vue-router'
import { ref } from 'vue'

const authStore = useAuthStore()
const router = useRouter()
const password = ref('')
const error = ref('')
const loading = ref(false)

const handleLogin = async () => {
  error.value = ''
  loading.value = true

  try {
    const response = await fetch('/auth/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ password: password.value })
    })

    if (response.ok) {
      const data = await response.json()
      authStore.login(data.token)
      router.push('/dashboard')
    } else {
      error.value = 'Senha inválida'
    }
  } catch (err) {
    error.value = 'Erro ao conectar com o servidor'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-container">
    <div class="login-card">
      <h1>Plex Autoplay</h1>
      <p>Painel Administrativo</p>
      
      <div v-if="error" class="error">{{ error }}</div>
      
      <div class="form-group">
        <label>Senha</label>
        <input type="password" v-model="password" placeholder="Digite a senha">
      </div>
      
      <button :disabled="loading" @click="handleLogin">{{ loading ? 'Entrando...' : 'Entrar' }}</button>
    </div>
  </div>
</template>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: #f5f5f5;
}
.login-card {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  width: 100%;
  max-width: 400px;
}
.login-card h1 {
  margin: 0 0 0.5rem;
  color: #e5a00d;
}
.login-card p {
  margin: 0 0 1.5rem;
  color: #666;
}
.error {
  color: #dc2626;
  font-size: 0.875rem;
  margin-bottom: 1rem;
}
.form-group {
  margin-bottom: 1rem;
}
.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 500;
}
.form-group input {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  box-sizing: border-box;
}
button {
  width: 100%;
  padding: 0.75rem;
  background: #e5a00d;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  cursor: pointer;
}
button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
