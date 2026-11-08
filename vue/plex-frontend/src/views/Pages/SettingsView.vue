<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import { apiFetch } from "@/api";

// Plex URL e Token (backend stores server config via /api/plex/identity / /api/plex/save)
const router = useRouter();
const plexUrl = ref(localStorage.getItem("plexUrl") || "");
const plexToken = ref(localStorage.getItem("plexToken") || "");
const tokenFetched = ref(false);
const saving = ref(false);
const message = ref("");

// Intervalo de sincronização (localStorage apenas)
const interval = ref(localStorage.getItem("syncIntervalMinutes") ? parseInt(localStorage.getItem("syncIntervalMinutes")) : 5);

async function fetchPlexIdentity() {
  if (!plexUrl.value) {
    message.value = "Informe a URL do Plex primeiro";
    return;
  }
  saving.value = true;
  message.value = "";
  try {
    const res = await apiFetch("/api/plex/identity", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ url: plexUrl.value }),
    });
    const data = await res.json().catch(() => ({}));
    if (res.ok) {
      plexToken.value = data.token || "";
      tokenFetched.value = true;
      message.value = "Token do Plex obtido com sucesso";
    } else {
      message.value = data.detail || "Erro ao obter token";
    }
  } catch {
    message.value = "Erro de conexão com o Plex";
  } finally {
    saving.value = false;
  }
}

async function savePlexConfig() {
  saving.value = true;
  message.value = "";
  try {
    const res = await apiFetch("/api/plex/save", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ plexUrl: plexUrl.value, plexToken: plexToken.value }),
    });
    const data = await res.json().catch(() => ({}));
    if (res.ok) {
      localStorage.setItem("plexConfig", JSON.stringify({ plexUrl: plexUrl.value, plexToken: plexToken.value }));
      message.value = "Configurações do Plex salvas com sucesso";
    } else {
      message.value = data?.detail || "Erro ao salvar configurações";
    }
  } catch {
    message.value = "Erro ao salvar configurações";
  } finally {
    saving.value = false;
  }
}

function saveInterval() {
  localStorage.setItem("syncIntervalMinutes", String(interval.value));
  message.value = "Intervalo salvo com sucesso";
}
</script>

<template>
  <DefaultLayout>
    <div class="mb-6">
      <h1 class="text-title-md font-bold text-black dark:text-white">Ajustes</h1>
    </div>

    <!-- Intervalo de Sincronização -->
    <div class="setting-group rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
      <label class="block text-sm font-medium text-body dark:text-bodydark mb-2">
        Intervalo de Sincronização (minutos)
      </label>
      <input
        v-model.number="interval"
        type="number"
        min="1"
        max="60"
        class="settings-input settings-input--number rounded border border-stroke bg-white text-black focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
      />
      <p class="text-xs text-bodydark2 mt-2">Intervalo em minutos entre sincronizações automáticas.</p>
    </div>

    <div class="flex items-center justify-end gap-2 mt-4">
      <button
        class="rounded-lg border border-stroke bg-white px-5 py-2.5 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:bg-boxdark dark:text-bodydark"
        :disabled="saving"
        @click="saveInterval"
      >
        {{ saving ? "Salvando..." : "Salvar Intervalo" }}
      </button>
      <button
        class="rounded-lg border border-stroke bg-white px-5 py-2.5 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:bg-boxdark dark:text-bodydark"
        @click="router.push('/settings')"
      >
        Cancelar
      </button>
    </div>

    <p v-if="message" class="mt-2 text-sm text-meta-1">{{ message }}</p>

    <hr class="mt-6 border-t border-stroke dark:border-strokedark" />

    <!-- Configurações Plex -->
    <div class="mt-6">
      <h2 class="text-title-sm font-semibold text-black dark:text-white mb-3">Configurações do Plex</h2>

      <div v-if="!tokenFetched" class="setting-group rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
        <label class="block text-sm font-medium text-body dark:text-bodydark mb-2">URL do Plex</label>
        <input
          v-model="plexUrl"
          type="text"
          placeholder="http://192.168.1.5:32400"
          class="settings-input rounded border border-stroke bg-white text-black focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
        />
      </div>

      <div v-if="!tokenFetched" class="setting-group rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-form-input dark:text-white">
        <label class="block text-sm font-medium text-body dark:text-bodydark mb-2">Token do Plex</label>
        <input
          v-model="plexToken"
          type="text"
          class="settings-input rounded border border-stroke bg-white text-black focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
        />
      </div>

      <div v-if="tokenFetched" class="setting-group rounded-sm border border-stroke bg-white px-5 py-4 shadow-default dark:border-strokedark dark:bg-boxdark">
        <label class="block text-sm font-medium text-body dark:text-bodydark mb-2">Token do Plex</label>
        <input
          v-model="plexToken"
          type="text"
          class="settings-input rounded border border-stroke bg-white text-black focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
        />
      </div>

      <div class="flex items-end gap-2 mt-4">
        <button
          class="flex-1 rounded-lg border border-stroke bg-white px-5 py-2.5 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:bg-boxdark dark:text-bodydark"
          :disabled="saving"
          @click="savePlexConfig"
        >
          {{ saving ? "Salvando..." : "Salvar Configurações" }}
        </button>
        <button
          class="rounded-lg border border-stroke bg-white px-5 py-2.5 text-sm font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:bg-boxdark dark:text-bodydark"
          @click="router.push('/settings')"
        >
          Cancelar
        </button>
      </div>
    </div>
  </DefaultLayout>
</template>