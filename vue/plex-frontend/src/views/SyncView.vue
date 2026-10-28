<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const status = ref('idle')
const processed = ref(0)
const skipped = ref(0)
const errors = ref(0)
const eventSource = ref(null)
const currentJobId = ref(null)

const startScan = async () => {
  const response = await fetch('/api/plex/scan', {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${localStorage.getItem('auth_token')}` }
  })
  if (response.ok) {
    const data = await response.json()
    currentJobId.value = data.job_id
    connectSSE(data.job_id)
  }
}

const connectSSE = (jobId) => {
  if (eventSource.value) eventSource.value.close()
  eventSource.value = new EventSource(`/api/sync/log/stream/${jobId}`)
  
  eventSource.value.onmessage = (event) => {
    const data = JSON.parse(event.data)
    status.value = data.status
    processed.value = data.processed
    skipped.value = data.skipped
  }
  
  eventSource.value.onerror = () => {
    eventSource.value.close()
  }
}

const cancelScan = async () => {
  if (!currentJobId.value) return
  await fetch(`/api/sync/cancel/${currentJobId.value}`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${localStorage.getItem('auth_token')}` }
  })
}

onUnmounted(() => {
  if (eventSource.value) eventSource.value.close()
})
</script>

<template>
  <div class="sync">
    <h1>Sincronização</h1>
    
    <div class="status">
      <span class="status-badge" :class="`status-${status}`">{{ status }}</span>
    </div>
    
    <div class="progress">
      <p>Processados: {{ processed }}</p>
      <p>Pulados: {{ skipped }}</p>
      <p>Erros: {{ errors }}</p>
    </div>
    
    <div class="actions">
      <button v-if="status !== 'running'" @click="startScan">Iniciar Escaneamento</button>
      <button v-if="status === 'running'" @click="cancelScan" class="cancel">Cancelar</button>
    </div>
  </div>
</template>

<style scoped>
.sync { padding: 2rem; }
.status-badge {
  display: inline-block;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  font-weight: 600;
  text-transform: uppercase;
}
.status-idle { background: #e5e7eb; color: #374151; }
.status-running { background: #dbeafe; color: #1e40af; }
.status-completed { background: #d1fae5; color: #065f46; }
.status-failed { background: #fee2e2; color: #991b1b; }
.status-cancelled { background: #f3f4f6; color: #4b5563; }
.progress { margin: 1rem 0; padding: 1rem; background: #f5f5f5; border-radius: 8px; }
.actions { margin-top: 1rem; }
button { padding: 0.75rem 1.5rem; border: none; border-radius: 4px; cursor: pointer; font-size: 1rem; }
button:first-child { background: #e5a00d; color: white; }
.cancel { background: #dc2626; color: white; margin-left: 0.5rem; }
button:disabled { opacity: 0.6; cursor: not-allowed; }
</style>
