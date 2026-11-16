<script setup>
import { onMounted, reactive, ref } from "vue";
import { useI18n } from "vue-i18n";
import { apiFetch, extractErrorMessage } from "@/api";
import AppIcon from "@/components/AppIcon.vue";
import MediaRootsField from "@/components/MediaRootsField.vue";

const { t } = useI18n();

const form = reactive({
  plex_url: "",
  sync_interval_minutes: 5,
});
const mediaRoots = ref([]);
const plexToken = ref("");
const plexTokenSet = ref(false);
const loading = ref(false);
const saving = ref(false);
const message = ref("");
const error = ref("");
const showPlexToken = ref(false);
const plexTokenRef = ref(null);

function togglePlexToken() {
  showPlexToken.value = !showPlexToken.value;
  plexTokenRef.value?.focus();
}

function applySettings(data) {
  form.plex_url = data.plex_url ?? "";
  mediaRoots.value = data.media_roots ?? [];
  form.sync_interval_minutes = data.sync_interval_minutes ?? 5;
  plexTokenSet.value = Boolean(data.plex_token_set);
  plexToken.value = "";
}

async function loadSettings() {
  loading.value = true;
  error.value = "";
  try {
    const res = await apiFetch("/api/settings");
    if (!res.ok) throw new Error(await extractErrorMessage(res) || t("errors.generic"));
    applySettings(await res.json());
  } catch (err) {
    error.value = err.message;
  } finally {
    loading.value = false;
  }
}

async function saveSettings() {
  if (mediaRoots.value.length === 0) {
    error.value = t("settings.mediaRootsRequired");
    return;
  }
  saving.value = true;
  message.value = "";
  error.value = "";
  try {
    const payload = {
      plex_url: form.plex_url,
      media_roots: mediaRoots.value,
      sync_interval_minutes: Number(form.sync_interval_minutes),
    };
    if (plexToken.value) payload.plex_token = plexToken.value;

    const res = await apiFetch("/api/settings", {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    if (!res.ok) throw new Error(await extractErrorMessage(res) || t("errors.generic"));

    applySettings(await res.json());
    message.value = t("settings.changesSaved");
  } catch (err) {
    error.value = err.message;
  } finally {
    saving.value = false;
  }
}

onMounted(loadSettings);
</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-3">
    <div class="flex items-center justify-between gap-3 flex-wrap mb-6">
      <div>
        <h1 class="text-2xl font-semibold text-gray-900 dark:text-gray-100">{{ t("settings.title") }}</h1>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          {{ t("settings.subtitle") }}
        </p>
      </div>
      <div class="flex items-center gap-2">
        <button
          type="button"
          class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark disabled:opacity-50"
          :disabled="saving"
          @click="loadSettings"
        >
          {{ t("common.refresh") }}
        </button>
        <button
          type="submit"
          form="settings-form"
          class="rounded-lg border border-stroke px-4 py-2 text-sm font-medium text-gray-700 transition hover:border-accent hover:text-accent dark:border-strokedark dark:text-bodydark disabled:opacity-50"
          :disabled="saving"
        >
          {{ saving ? t("common.loading") : t("common.save") }}
        </button>
      </div>
    </div>

    <div class="p-4 md:p-6 lg:p-8 max-w-6xl mx-auto">
      <div v-if="loading" class="text-sm text-gray-500 dark:text-gray-400">{{ t("common.loading") }}</div>

      <form v-else id="settings-form" class="space-y-6" @submit.prevent="saveSettings">
        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="sync-interval">
            {{ t("settings.syncInterval") }}
          </label>
          <input
            id="sync-interval"
            v-model.number="form.sync_interval_minutes"
            type="number"
            min="1"
            max="1440"
            required
            class="w-full rounded-lg border border-stroke bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 placeholder-gray-400 px-4 py-2.5 focus:border-accent focus:outline-none focus:ring-1 focus:ring-accent/30 dark:border-gray-600"
          />
          <p class="text-xs text-gray-500 dark:text-gray-400">
            {{ t("settings.syncIntervalHelp") }}
          </p>
        </div>

        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="plex-url">
            {{ t("auth.plexUrl") }}
          </label>
          <input
            id="plex-url"
            v-model="form.plex_url"
            type="url"
            required
            placeholder="http://192.168.1.5:32400"
            class="w-full rounded-lg border border-stroke bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 placeholder-gray-400 px-4 py-2.5 focus:border-accent focus:outline-none focus:ring-1 focus:ring-accent/30 dark:border-gray-600"
          />
        </div>

        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300" for="plex-token">
            {{ t("auth.plexToken") }}
          </label>
          <div class="relative">
            <input
              id="plex-token"
              ref="plexTokenRef"
              v-model="plexToken"
              :type="showPlexToken ? 'text' : 'password'"
              autocomplete="new-password"
              :placeholder="plexTokenSet ? t('settings.tokenConfigured') : t('settings.enterToken')"
              class="w-full rounded-lg border border-stroke bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 placeholder-gray-400 px-4 py-2.5 pr-12 focus:border-accent focus:outline-none focus:ring-1 focus:ring-accent/30 dark:border-gray-600"
              data-testid="plex-token"
            />
            <button
              type="button"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 transition"
              :aria-label="showPlexToken ? t('aria.collapse') : t('aria.expand')"
              :aria-pressed="showPlexToken"
              data-testid="toggle-password-plex-token"
              @click="togglePlexToken"
            >
              <AppIcon :name="showPlexToken ? 'eye-off' : 'eye'" size="sm" />
            </button>
          </div>
          <p class="text-xs text-gray-500 dark:text-gray-400">
            {{ plexTokenSet ? t("settings.tokenConfiguredHelp") : t("settings.noTokenConfigured") }}
          </p>
        </div>

        <div class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <MediaRootsField v-model="mediaRoots" :label="t('settings.mediaRootsLabel')" />
        </div>

        <p v-if="message" class="text-sm text-green-600 dark:text-green-400">{{ message }}</p>
        <p v-if="error" class="text-sm text-red-600 dark:text-red-400">{{ error }}</p>
      </form>
    </div>
  </div>
</template>