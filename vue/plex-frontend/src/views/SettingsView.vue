<script setup>
import { ref, onMounted } from "vue";

const interval = ref(5);
const saving = ref(false);
const message = ref("");

onMounted(async () => {
  try {
    // Read current interval from app-config via a settings endpoint
    // For now, read from localStorage or default
    const saved = localStorage.getItem("syncIntervalMinutes");
    if (saved) interval.value = parseInt(saved);
  } catch {
    message.value = "Erro ao carregar configurações";
  }
});

const saveInterval = async () => {
  saving.value = true;
  message.value = "";

  try {
    // In a full implementation, this would POST to a settings endpoint
    // that writes to app-config.json
    localStorage.setItem("syncIntervalMinutes", interval.value.toString());
    message.value = "Intervalo salvo com sucesso";
  } catch {
    message.value = "Erro ao salvar configurações";
  } finally {
    saving.value = false;
  }
};
</script>

<template>
  <div class="settings">
    <h1>Configurações</h1>

    <div v-if="message" class="message">
      {{ message }}
    </div>

    <div class="setting-group">
      <label>Intervalo de Sincronização (minutos)</label>
      <input v-model.number="interval" type="number" min="1" />
      <p class="hint">O serviço de sincronização será executado a cada N minutos.</p>
    </div>

    <button :disabled="saving" @click="saveInterval">
      {{ saving ? "Salvando..." : "Salvar" }}
    </button>
  </div>
</template>

<style scoped>
.settings {
  padding: 2rem;
}
.message {
  color: #065f46;
  background: #d1fae5;
  padding: 0.75rem;
  border-radius: 4px;
  margin-bottom: 1rem;
}
.setting-group {
  margin-bottom: 1.5rem;
}
.setting-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
}
.setting-group input {
  padding: 0.75rem;
  width: 150px;
  border: 1px solid #ddd;
  border-radius: 4px;
}
.hint {
  font-size: 0.875rem;
  color: #666;
  margin-top: 0.25rem;
}
button {
  padding: 0.75rem 1.5rem;
  background: #e5a00d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
