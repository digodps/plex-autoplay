<script setup>
import { ref, onMounted } from 'vue'

const libraries = ref([])
const error = ref('')

onMounted(async () => {
  try {
    const response = await fetch('/api/plex/libraries', {
      headers: { 'Authorization': `Bearer ${localStorage.getItem('auth_token')}` }
    })
    if (response.ok) {
      libraries.value = await response.json()
    } else {
      error.value = 'Erro ao carregar bibliotecas'
    }
  } catch (err) {
    error.value = 'Erro de conexão'
  }
})
</script>

<template>
  <div class="libraries">
    <h1>Bibliotecas</h1>
    <div v-if="error" class="error">{{ error }}</div>
    <div v-else class="library-grid">
      <div v-for="lib in libraries" :key="lib.key" class="library-card">
        <h3>{{ lib.title }}</h3>
        <p>Tipo: {{ lib.type }}</p>
      </div>
    </div>
  </div>
</template>

<style scoped>
.libraries { padding: 2rem; }
.library-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 1rem; }
.library-card { padding: 1rem; border: 1px solid #ddd; border-radius: 8px; }
.error { color: #dc2626; }
</style>
