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

