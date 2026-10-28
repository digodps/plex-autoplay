<script setup>
import { ref, onMounted } from 'vue'

const markers = ref([])
const newMarker = ref({ ratingKey: '', kind: 'intro', startMs: 0, endMs: 0 })
const error = ref('')
const loading = ref(false)

const token = localStorage.getItem('auth_token')
const authHeaders = token ? { 'Authorization': `Bearer ${token}` } : {}

onMounted(loadMarkers)

async function loadMarkers() {
  try {
    const response = await fetch('/api/markers', { headers: authHeaders })
    if (response.ok) {
      markers.value = await response.json()
    }
  } catch (err) {
    error.value = 'Erro ao carregar marcadores'
  }
}

async function addMarker() {
  try {
    const response = await fetch('/api/markers', {
      method: 'POST',
      headers: { ...authHeaders, 'Content-Type': 'application/json' },
      body: JSON.stringify(newMarker.value)
    })
    if (response.ok) {
      newMarker.value = { ratingKey: '', kind: 'intro', startMs: 0, endMs: 0 }
      await loadMarkers()
    } else {
      error.value = 'Erro ao adicionar marcador'
    }
  } catch (err) {
    error.value = 'Erro de conexão'
  }
}

async function removeMarker(ratingKey, kind) {
  if (!confirm('Remover este marcador?')) return
  try {
    const response = await fetch(`/api/markers/${ratingKey}/${kind}`, {
      method: 'DELETE',
      headers: authHeaders
    })
    if (response.ok) {
      await loadMarkers()
    }
  } catch (err) {
    error.value = 'Erro ao remover marcador'
  }
}
</script>

<template>
  <div class="markers">
    <h1>Marcadores</h1>
    
    <div v-if="error" class="error">{{ error }}</div>
    
    <div class="add-form">
      <h3>Adicionar Marcador</h3>
      <div class="form-row">
        <input v-model="newMarker.ratingKey" placeholder="Rating Key" />
        <select v-model="newMarker.kind">
          <option value="intro">Intro</option>
          <option value="credits">Créditos</option>
        </select>
        <input v-model.number="newMarker.startMs" type="number" placeholder="Início (ms)" />
        <input v-model.number="newMarker.endMs" type="number" placeholder="Fim (ms)" />
        <button @click="addMarker">Adicionar</button>
      </div>
    </div>
    
    <div class="markers-list">
      <h3>Marcadores Existentes</h3>
      <table v-if="markers.length > 0">
        <thead>
          <tr>
            <th>Rating Key</th>
            <th>Tipo</th>
            <th>Início (ms)</th>
            <th>Fim (ms)</th>
            <th>Ação</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in markers" :key="m.ratingKey + m.kind">
            <td>{{ m.ratingKey }}</td>
            <td>{{ m.kind }}</td>
            <td>{{ m.startMs }}</td>
            <td>{{ m.endMs }}</td>
            <td>
              <button @click="removeMarker(m.ratingKey, m.kind)" class="remove">Remover</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-else class="empty">Nenhum marcador encontrado.</div>
    </div>
  </div>
</template>

<style scoped>
.markers { padding: 2rem; }
.error { color: #dc2626; margin-bottom: 1rem; }
.add-form { background: #f5f5f5; padding: 1rem; border-radius: 8px; margin-bottom: 2rem; }
.form-row { display: flex; gap: 0.5rem; align-items: flex-end; flex-wrap: wrap; }
.form-row input, .form-row select { padding: 0.5rem; border: 1px solid #ddd; border-radius: 4px; }
.form-row button { padding: 0.5rem 1rem; background: #e5a00d; color: white; border: none; border-radius: 4px; cursor: pointer; }
.markers-list table { width: 100%; border-collapse: collapse; }
.markers-list th, .markers-list td { padding: 0.75rem; text-align: left; border-bottom: 1px solid #ddd; }
.markers-list .remove { background: #dc2626; color: white; border: none; padding: 0.25rem 0.5rem; border-radius: 4px; cursor: pointer; }
.empty { color: #666; font-style: italic; }
</style>
