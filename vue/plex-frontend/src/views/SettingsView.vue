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
  } catch (err) {
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
  } catch (err) {
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
  } catch (err) {
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
        type="number"
        id="interval-input"
        min="1"
        max="60"
        v-model.number="interval"
        style="width: 150px; padding: 0.75rem; border: 1px solid var(--border); border-radius: var(--radius-sm);"
      />
      <p class="hint">O serviço de sincronização roda a cada N minutos</p>
    </div>

    <button
      @click="saveInterval"
      style="
        width: 100%;
        margin-top: 0.5rem;
        padding: 0.75rem;
        background: var(--accent);
        color: var(--accent-contrast);
        border: none;
        border-radius: var(--radius-sm);
        font-weight: 500;
        cursor: pointer;
        transition: var(--transition-fast);
      "
      :disabled="saving"
    >
      {{ saving ? "Salvando..." : "Salvar Intervalo" }}
    </button>
    <p
      v-if="message"
      style="
        margin-top: 0.5rem;
        font-size: 0.875rem;
        color: var(--text-tertiary);
      "
    >
      {{ message }}
    </p>

    <!-- --- Separador --- -->
    <hr style="margin: 1.5rem 0; border: none; border-top: 1px solid var(--border);" />

    <!-- Configurações Plex -->
    <div class="setting-group">
      <label>URL do Plex</label>
      <input
        type="text"
        v-model="plexUrl"
        placeholder="http://192.168.18.5:32400"
        style="
          width: 100%;
          padding: 0.75rem;
          border: 1px solid var(--border);
          border-radius: var(--radius-sm);
        "
      />
    </div>

    <div v-if="!tokenFetched" class="setting-group">
      <label>Token do Plex</label>
      <input
        type="text"
        v-model="plexToken"
        readonly
        style="
          width: 150px;
          padding: 0.75rem;
          border: 1px solid var(--border);
          border-radius: var(--radius-sm);
          background: var(--bg-sunken);
        "
      />
      <button
        @click="fetchPlexIdentity"
        style="
          margin-left: 0.5rem;
          padding: 0.5rem 1rem;
          background: var(--accent);
          color: var(--accent-contrast);
          border: none;
          border-radius: var(--radius-sm);
          cursor: pointer;
          font-size: 0.875rem;
        "
      >
        Buscar Token
      </button>
    </div>

    <div v-if="tokenFetched" class="setting-group">
      <label>Token do Plex</label>
      <input
        type="text"
        v-model="plexToken"
        readonly
        style="
          width: 150px;
          padding: 0.75rem;
          border: 1px solid var(--border);
          border-radius: var(--radius-sm);
        "
      />
    </div>

    <button
      @click="savePlexConfig"
      v-if="tokenFetched"
      style="
        width: 100%;
        margin-top: 0.5rem;
        padding: 0.75rem;
        background: var(--accent);
        color: var(--accent-contrast);
        border: none;
        border-radius: var(--radius-sm);
        font-weight: 500;
        cursor: pointer;
        transition: var(--transition-fast);
      "
      :disabled="saving"
    >
      {{ saving ? "Salvando..." : "Salvar Configurações" }}
    </button>

    <p v-if="message" style="margin-top: 0.5rem; font-size: 0.875rem; color: var(--text-tertiary);">
      {{ message }}
    </p>
  </div>
</template>