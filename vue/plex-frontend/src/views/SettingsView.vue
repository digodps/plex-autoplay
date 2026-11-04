<script setup>
import { ref } from "vue"

const interval = ref(5)
const saving = ref(false)
const message = ref("")

const savedIntervalLocal = localStorage.getItem("syncIntervalMinutes")
if (savedIntervalLocal) {
  interval.value = parseInt(savedIntervalLocal)
}

const saveInterval = async () => {
  saving.value = true
  message.value = ""
  try {
    const newInterval = document.getElementById("interval-input")?.value
    if (newInterval) {
      localStorage.setItem("syncIntervalMinutes", newInterval)
      interval.value = parseInt(newInterval)
    }
    message.value = "Intervalo salvo com sucesso"
  } catch {
    message.value = "Erro ao salvar intervalo"
  } finally {
    saving.value = false
  }
}

// Plex URL e Token
const plexUrl = ref(localStorage.getItem("plexUrl") || "")
const plexToken = ref(localStorage.getItem("plexToken") || "")
const tokenFetched = ref(false)

const fetchPlexIdentity = async () => {
  if (!plexUrl.value) {
    message.value = "Informe a URL do Plex primeiro"
    return
  }
  saving.value = true
  message.value = ""
  try {
    const response = await fetch("/api/plex/identity", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ url: plexUrl.value }),
    })
    const data = await response.json()
    if (response.ok) {
      plexToken.value = data.token
      tokenFetched.value = true
      message.value = "Token do Plex obtido com sucesso"
    } else {
      message.value = data.error || "Erro ao obter token"
    }
  } catch {
    message.value = "Erro de conexão com o Plex"
  } finally {
    saving.value = false
  }
}

const savePlexConfig = async () => {
  saving.value = true
  message.value = ""
  try {
    const config = {
      plexUrl: plexUrl.value,
      plexToken: plexToken.value,
    }
    const response = await fetch("/api/plex/save", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(config),
    })
    const data = await response.json()
    if (response.ok) {
      localStorage.setItem("plexConfig", JSON.stringify(config))
      message.value = "Configurações do Plex salvas com sucesso"
    } else {
      message.value = data.error || "Erro ao salvar configurações"
    }
  } catch {
    message.value = "Erro ao salvar configurações"
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="settings-view">
    <h2>Configurações</h2>

    <!-- Intervalo de Sincronização -->
    <div class="setting-group">
      <label>Intervalo de Sincronização (minutos)</label>
      <input
        id="interval-input"
        v-model.number="interval"
        type="number"
        min="1"
        max="60"
        class="settings-input settings-input--number"
      />
      <p class="hint">O serviço de sincronização roda a cada N minutos</p>
    </div>

    <button
      class="btn btn--primary btn--block"
      :disabled="saving"
      @click="saveInterval"
    >
      {{ saving ? "Salvando..." : "Salvar Intervalo" }}
    </button>
    <p v-if="message" class="settings-msg">
      {{ message }}
    </p>

    <!-- --- Separador --- -->
    <hr class="settings-hr" />

    <!-- Configurações Plex -->
    <div class="setting-group">
      <label>URL do Plex</label>
      <input
        v-model="plexUrl"
        type="text"
        placeholder="http://192.168.18.5:32400"
        class="settings-input"
      />
    </div>

    <div v-if="!tokenFetched" class="setting-group">
      <label>Token do Plex</label>
      <input
        v-model="plexToken"
        type="text"

        class="settings-input"
      />
      <button class="btn btn--primary btn--inline" @click="fetchPlexIdentity">
        Buscar token
      </button>
    </div>

    <div v-if="tokenFetched" class="setting-group">
      <label>Token do Plex</label>
      <input
        v-model="plexToken"
        type="text"

        class="settings-input settings-input--number"
      />
    </div>

    <button
      v-if="tokenFetched"
      class="btn btn--primary btn--block"
      :disabled="saving"
      @click="savePlexConfig"
    >
      {{ saving ? "Salvando..." : "Salvar Configurações" }}
    </button>

    <p v-if="message" class="settings-msg">
      {{ message }}
    </p>
  </div>
</template>

<style scoped>
.settings-view {
  max-width: 34rem;
}

.setting-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  margin-top: var(--space-4);
}

.settings-input {
  width: 100%;
  padding: var(--space-3);
  border: 1px solid var(--border-input);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
}

.settings-input:focus-visible {
  outline: 2px solid var(--focus-ring);
  outline-offset: 2px;
}

.settings-input--number {
  width: 150px;
}

.settings-input--readonly {
  background: var(--bg-sunken, var(--surface-raised));
}

.hint {
  font-size: 0.875rem;
  color: var(--text-tertiary);
}

.settings-msg {
  margin-top: var(--space-2);
  font-size: 0.875rem;
  color: var(--text-tertiary);
}

.settings-hr {
  margin: var(--space-6) 0;
  border: none;
  border-top: 1px solid var(--border);
}

.btn--primary {
  background: var(--accent);
  color: var(--accent-contrast);
  border: none;
  font-weight: 500;
  cursor: pointer;
  transition: var(--transition-fast, 150ms ease);
}

.btn--primary:hover:not(:disabled) {
  background: var(--accent-hover, var(--accent));
}

.btn--primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn--block {
  width: 100%;
  margin-top: var(--space-2);
  padding: var(--space-3);
  border-radius: var(--radius-sm);
}

.btn--inline {
  align-self: flex-start;
  margin-top: var(--space-2);
  padding: var(--space-2) var(--space-4);
  border-radius: var(--radius-sm);
  font-size: 0.875rem;
}
</style>