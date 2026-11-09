<script setup>
import { onMounted, reactive, ref } from "vue";
import DefaultLayout from "@/components/layouts/DefaultLayout.vue";
import MediaRootsField from "@/components/MediaRootsField.vue";
import { apiFetch, extractErrorMessage, extractErrors } from "@/api";

const form = reactive({
  plex_url: "",
  media_roots: [],
  sync_interval_minutes: 5,
  sync_catalog_interval_minutes: 30,
  auto_skip_authorized: false,
});
const plexToken = ref("");
const plexTokenSet = ref(false);
const showAuthModal = ref(false);
const loading = ref(false);
const saving = ref(false);
const message = ref("");
const error = ref("");
const errors = ref([]);

function applySettings(data) {
  form.plex_url = data.plex_url ?? "";
  form.media_roots = data.media_roots ?? [];
  form.sync_interval_minutes = data.sync_interval_minutes ?? 5;
  form.sync_catalog_interval_minutes = data.sync_catalog_interval_minutes ?? 30;
  form.auto_skip_authorized = data.auto_skip_authorized ?? false;
  plexTokenSet.value = Boolean(data.plex_token_set);
  plexToken.value = "";
}

async function loadSettings() {
  loading.value = true;
  error.value = "";
  try {
    const res = await apiFetch("/api/settings");
    if (!res.ok) throw new Error(await extractErrorMessage(res) || "Erro ao carregar configurações");
    applySettings(await res.json());
  } catch (err) {
    error.value = err.message;
  } finally {
    loading.value = false;
  }
}

async function saveSettings() {
  saving.value = true;
  message.value = "";
  error.value = "";
  errors.value = [];
  try {
    const payload = {
      plex_url: form.plex_url,
      media_roots: form.media_roots,
      sync_interval_minutes: Number(form.sync_interval_minutes),
      sync_catalog_interval_minutes: Number(form.sync_catalog_interval_minutes),
      auto_skip_authorized: form.auto_skip_authorized,
    };
    if (plexToken.value) payload.plex_token = plexToken.value;

    const res = await apiFetch("/api/settings", {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!res.ok) {
      const messages = await extractErrors(res);
      if (messages.length) {
        errors.value = messages;
        return;
      }
      error.value = "Erro ao salvar configurações";
      return;
    }

    applySettings(await res.json());
    message.value = "Configurações salvas com sucesso";
  } catch (err) {
    error.value = err.message || "Erro ao salvar configurações";
  } finally {
    saving.value = false;
  }
}

async function toggleAutoSkip() {
  if (form.auto_skip_authorized) return; // already true, proceed
  // trying to turn ON → show confirmation modal Netflix-style
  showAuthModal.value = true;
  // revert checkbox; user must confirm
  form.auto_skip_authorized = false;
}

function confirmAutoSkip() {
  showAuthModal.value = false;
  form.auto_skip_authorized = true;
  saveSettings();
}

function cancelAutoSkip() {
  showAuthModal.value = false;
}

onMounted(loadSettings);
</script>

<template>
  <DefaultLayout>
    <div>
      <div>
        <div class="max-w-2xl mx-auto px-4 py-3">
          <h1 class="text-28 font-bold text-gray-900 dark:text-gray-100">Ajustes</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Configurações do Plex e sincronização, persistidas no banco.
          </p>
        </div>
      </div>

      <div class="p-4 md:p-6 lg:p-8">
        <div class="max-w-2xl mx-auto space-y-6">
          <div v-if="loading" class="text-sm text-gray-500 dark:text-gray-400">Carregando...</div>

          <form v-else class="space-y-6" @submit.prevent="saveSettings">
            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="sync-interval">
                Intervalo de Sincronização (minutos)
              </label>
              <input
                id="sync-interval"
                v-model.number="form.sync_interval_minutes"
                type="number"
                min="1"
                max="1440"
                required
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
              <p class="text-xs text-gray-500 dark:text-gray-400">
                Intervalo entre varreduras automáticas das bibliotecas.
              </p>
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="catalog-interval">
                Intervalo de Sincronização do Catálogo (minutos)
              </label>
              <input
                id="catalog-interval"
                v-model.number="form.sync_catalog_interval_minutes"
                type="number"
                min="1"
                max="1440"
                required
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="flex items-center gap-3 cursor-pointer">
                <input
                  id="auto-skip-authorized"
                  type="checkbox"
                  :checked="form.auto_skip_authorized"
                  @change="toggleAutoSkip"
                  class="w-5 h-5 text-primary border-gray-300 rounded focus:ring-primary"
                />
                <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Autorizar skip automático</span>
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 ml-8">
                Quando habilitado, o Plex pulará introduções e créditos automaticamente. Requer confirmação.
              </p>
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="plex-url">
                URL do Plex
              </label>
              <input
                id="plex-url"
                v-model="form.plex_url"
                type="url"
                required
                placeholder="http://192.168.1.5:32400"
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="plex-token">
                Token do Plex
              </label>
              <input
                id="plex-token"
                v-model="plexToken"
                type="password"
                autocomplete="new-password"
                :placeholder="plexTokenSet ? 'Token configurado — deixe vazio para manter' : 'Informe o token do Plex'"
                class="w-full rounded border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 text-black dark:text-white focus:border-primary focus:bg-white focus:text-black"
              />
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{ plexTokenSet ? "Token já configurado. Preencha apenas para substituir." : "Nenhum token configurado." }}
              </p>
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <MediaRootsField v-model="form.media_roots" />
            </div>

            <div class="rounded-lg border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 p-4 space-y-3">
              <label class="flex items-center gap-3 cursor-pointer">
                <input
                  id="auto-skip-authorized"
                  type="checkbox"
                  :checked="form.auto_skip_authorized"
                  @change="toggleAutoSkip"
                  class="w-5 h-5 text-primary border-gray-300 rounded focus:ring-primary"
                />
                <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Autorizar skip automático</span>
              </label>
              <p class="text-xs text-gray-500 dark:text-gray-400 ml-8">
                Quando habilitado, o Plex pulará introduções e créditos automaticamente.
              </p>
            </div>

            <p v-if="message" class="text-sm text-green-600 dark:text-green-400">{{ message }}</p>
            <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
            <ul v-if="errors.length" class="space-y-1 text-sm text-red-600 dark:text-red-400">
              <li v-for="(item, index) in errors" :key="index">{{ item }}</li>
            </ul>

            <div class="flex items-center justify-end gap-2">
              <button
                type="button"
                class="rounded-lg border border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-700 px-4 py-2 text-sm font-medium text-gray-800 dark:text-gray-100 transition hover:border-gray-400 dark:hover:bg-gray-600 disabled:opacity-50"
                :disabled="saving"
                @click="loadSettings"
              >
                Recarregar
              </button>
              <button
                type="submit"
                class="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition hover:opacity-90 disabled:opacity-50"
                :disabled="saving"
              >
                {{ saving ? "Salvando..." : "Salvar Configurações" }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </DefaultLayout>

  <!-- Modal estilo Netflix para autorização auto-skip -->
  <Teleport to="body">
    <Transition name="fade">
      <div v-if="showAuthModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4" @click.self="cancelAutoSkip">
        <div class="bg-white dark:bg-gray-800 rounded-xl p-6 max-w-md w-full shadow-2xl animate-in slide-in-from-top-4 duration-200">
          <div class="flex items-center gap-3 mb-4">
            <div class="w-12 h-12 bg-orange-100 dark:bg-orange-900/30 rounded-full flex items-center justify-center">
              <svg class="w-6 h-6 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
            </div>
            <h2 class="text-xl font-semibold text-gray-900 dark:text-gray-100">Autorizar skip automático?</h2>
          </div>
          <p class="text-gray-600 dark:text-gray-300 mb-6">
            Ao autorizar, o sistema <strong>pulará automaticamente introduções e créditos</strong> 
            durante a reprodução, sem pedir confirmação a cada episódio.
          </p>
          <div class="flex gap-3 justify-end">
            <button 
              @click="cancelAutoSkip"
              class="px-4 py-2 text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-lg font-medium transition-colors"
            >
              Cancelar
            </button>
            <button 
              @click="confirmAutoSkip"
              class="px-4 py-2 bg-orange-500 text-white hover:bg-orange-600 rounded-lg font-medium transition-colors"
            >
              Autorizar
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>
