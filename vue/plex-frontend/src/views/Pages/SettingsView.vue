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
    <div class="bg-gray-50 dark:bg-gray-900 min-h-screen">
      <div class="border-b border-gray-200 dark:border-gray-800 sticky top-0 z-20 bg-gray-50 dark:bg-gray-900">
        <div class="max-w-2xl mx-auto px-4 py-3 flex items-center gap-2">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Ajustes</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">Configurações do Plex e sincronização.</p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8">
        <!-- Intervalo de Sincronização -->
        <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
            Intervalo de Sincronização (minutos)
          </label>
          <input
            v-model.number="interval"
            type="number"
            min="1"
            max="60"
            class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black transition"
            placeholder="5"
          />
          <p class="text-xs text-gray-500 dark:text-gray-400">Intervalo em minutos entre sincronizações automáticas.</p>
        </div>

        <div class="flex items-center justify-end gap-2">
          <button
            class="rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 px-4 py-2 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:border-gray-400 dark:hover:bg-gray-600 disabled:opacity-50"
            :disabled="saving"
            @click="saveInterval"
          >
            {{ saving ? "Salvando..." : "Salvar Intervalo" }}
          </button>
        </div>

        <p v-if="message" class="mt-2 text-sm text-gray-400 dark:text-gray-600">{{ message }}</p>

        <hr class="mt-6 border-t border-gray-200 dark:border-gray-600" />

        <!-- Configurações Plex -->
        <div class="mt-6">
          <h2 class="text-title-sm font-semibold text-gray-900 dark:text-gray-100 mb-3">Configurações do Plex</h2>

          <div v-if="!tokenFetched" class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">URL do Plex</label>
            <input
              v-model="plexUrl"
              type="text"
              placeholder="http://192.168.1.5:32400"
              class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
            />
          </div>

          <div v-if="!tokenFetched" class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Token do Plex</label>
            <input
              v-model="plexToken"
              type="text"
              class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
            />
          </div>

          <div v-if="tokenFetched" class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
            <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">Token do Plex</label>
            <input
              v-model="plexToken"
              type="text"
              class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
            />
          </div>

          <div class="flex items-end gap-2 mt-4">
            <button
              class="flex-1 rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 px-4 py-2 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:border-gray-400 dark:hover:bg-gray-600 disabled:opacity-50"
              :disabled="saving"
              @click="savePlexConfig"
            >
              {{ saving ? "Salvando..." : "Salvar Configurações" }}
            </button>
            <button
              class="rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 px-4 py-2 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:border-gray-400 dark:hover:bg-gray-600"
              @click="router.push('/settings')"
            >
              Cancelar
            </button>
          </div>
        </div>
      </div>
      </div>
    </DefaultLayout>
  </template>