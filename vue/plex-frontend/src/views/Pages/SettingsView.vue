<script setup>
import { onMounted, ref, reactive } from "vue";
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
  <div class="settings-view" data-enter>
    <header class="page-header" style="margin-bottom: var(--s-6);">
      <h1 class="t-h2" style="font: var(--font-display); font-weight: var(--display-weight); letter-spacing: var(--display-tracking); margin-bottom: var(--s-2);">{{ t("settings.title") }}</h1>
      <p class="t-body muted" style="margin: 0;">{{ t("settings.subtitle") }}</p>
    </header>

    <div v-if="message" class="alert alert--success" role="alert" style="margin-bottom: var(--s-6); display: flex; align-items: center; gap: var(--s-3);">
      <AppIcon name="circle-check-big" size="lg" style="color: var(--success, #16a34a);" />
      {{ message }}
    </div>

    <div v-if="error" class="alert alert--error" role="alert" style="margin-bottom: var(--s-6); display: flex; align-items: center; gap: var(--s-3);">
      <AppIcon name="triangle-alert" size="lg" />
      {{ error }}
    </div>

    <form class="settings-form" style="display: flex; flex-direction: column; gap: var(--s-6);" @submit.prevent="saveSettings">
      <section class="settings-section panel" style="--p-h: auto; padding: var(--s-4);">
        <h2 class="t-h3" style="font: var(--font-display); margin-bottom: var(--s-4); padding-bottom: var(--s-3); border-bottom: 1px solid var(--border);">{{ t("settings.plexConnection") }}</h2>

        <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
          <label for="plex-url" class="t-label">{{ t("settings.plexUrl") }}</label>
          <input
            id="plex-url"
            v-model="form.plex_url"
            type="url"
            class="field__input mono"
            :placeholder="t('settings.plexUrlPlaceholder')"
            :disabled="saving || loading"
            required
          />
          <p class="t-small muted" style="margin: 0;">{{ t("settings.plexUrlHelp") }}</p>
        </div>

        <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1); margin-top: var(--s-4);">
          <label for="plex-token" class="t-label">{{ t("settings.plexToken") }}</label>
          <div class="field__wrapper" style="position: relative; display: flex;">
            <input
              id="plex-token"
              ref="plexTokenRef"
              v-model="plexToken"
              :type="showPlexToken ? 'text' : 'password'"
              class="field__input mono"
              style="flex: 1; padding-right: 48px;"
              :placeholder="plexTokenSet ? t('settings.tokenPlaceholder') : t('settings.tokenPlaceholderNew')"
              autocomplete="off"
              :disabled="saving || loading"
            />
            <button
              type="button"
              class="field__toggle"
              style="position: absolute; right: var(--s-3); top: 50%; transform: translateY(-50%);"
              :aria-label="showPlexToken ? t('aria.collapse') : t('aria.expand')"
              :aria-pressed="showPlexToken"
              @click="togglePlexToken"
            >
              <AppIcon :name="showPlexToken ? 'eye-off' : 'eye'" size="sm" />
            </button>
          </div>
          <p class="t-small muted" style="margin: 0;">{{ t("settings.plexTokenHelp") }}</p>
        </div>
      </section>

      <section class="settings-section panel" style="--p-h: auto; padding: var(--s-4);">
        <h2 class="t-h3" style="font: var(--font-display); margin-bottom: var(--s-4); padding-bottom: var(--s-3); border-bottom: 1px solid var(--border);">{{ t("settings.mediaRoots") }}</h2>
        <MediaRootsField v-model="mediaRoots" :label="t('settings.mediaRootsLabel')" />
        <p class="t-small muted" style="margin: var(--s-2) 0 0;">{{ t("settings.mediaRootsHelp") }}</p>
      </section>

      <section class="settings-section panel" style="--p-h: auto; padding: var(--s-4);">
        <h2 class="t-h3" style="font: var(--font-display); margin-bottom: var(--s-4); padding-bottom: var(--s-3); border-bottom: 1px solid var(--border);">{{ t("settings.sync") }}</h2>
        <div class="field" style="display: flex; flex-direction: column; gap: var(--s-1);">
          <label for="sync-interval" class="t-label">{{ t("settings.syncInterval") }}</label>
          <input
            id="sync-interval"
            v-model.number="form.sync_interval_minutes"
            type="number"
            min="1"
            max="1440"
            class="field__input"
            style="width: 120px;"
            :disabled="saving || loading"
          />
          <p class="t-small muted" style="margin: 0;">{{ t("settings.syncIntervalHelp") }}</p>
        </div>
      </section>

      <div class="form-actions" style="display: flex; justify-content: flex-end; gap: var(--s-3); padding-top: var(--s-4); border-top: 1px solid var(--border);">
        <button type="button" class="btn btn--secondary" :disabled="loading" @click="loadSettings">
          <AppIcon name="refresh-cw" size="sm" />
          {{ t("common.reload") }}
        </button>
        <button type="submit" class="btn btn--primary btn--no-chevron" :disabled="saving || loading">
          <AppIcon :name="saving ? 'spinner' : 'check'" size="sm" :class="{ 'animate-spin': saving }" />
          {{ saving ? t("common.saving") : t("common.save") }}
        </button>
      </div>
    </form>
  </div>
</template>

<style scoped>
.settings-view {
  width: 100%;
}

.page-header {
  margin-bottom: var(--s-6);
}

.settings-form {
  display: flex;
  flex-direction: column;
  gap: var(--s-6);
}

.settings-section {
  padding: var(--s-4);
}

.settings-section > h2 {
  margin-bottom: var(--s-4);
  padding-bottom: var(--s-3);
  border-bottom: 1px solid var(--border);
}

.field {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
}

.field__input {
  width: 100%;
  padding: var(--s-3) var(--s-4);
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  background: color-mix(in oklab, var(--surface) 70%, var(--ink) 30%);
  border: 1px solid var(--border);
  border-radius: var(--r-md);
  transition: border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

.field__input:focus {
  outline: none;
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), 0 0 0 4px color-mix(in oklab, var(--action) 20%, transparent);
}

.field__input.mono {
  font-family: var(--font-mono);
}

.field__input:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.field__wrapper {
  position: relative;
  display: flex;
}

.field__toggle {
  position: absolute;
  right: var(--s-3);
  top: 50%;
  transform: translateY(-50%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--s-1);
  background: transparent;
  border: none;
  color: var(--ink);
  opacity: 0.5;
  cursor: pointer;
  transition: opacity var(--m-fast) var(--m-ease);
}

.field__toggle:hover {
  opacity: 1;
}

.field__toggle:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
  border-radius: var(--r-sm);
}

.alert {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  padding: var(--s-3) var(--s-4);
  border-radius: var(--r-md);
  font-size: var(--t-body);
  line-height: 1.5;
}

.alert--error {
  background: color-mix(in oklab, var(--danger, #e03e3e) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger, #e03e3e) 25%, transparent);
  color: var(--ink);
}

.alert--success {
  background: color-mix(in oklab, var(--success, #16a34a) 10%, transparent);
  border: 1px solid color-mix(in oklab, var(--success, #16a34a) 25%, transparent);
  color: var(--ink);
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--s-2);
  padding: var(--s-2) var(--s-5);
  font: var(--font-text);
  font-size: var(--t-sm);
  font-weight: 500;
  border-radius: var(--r-btn);
  border: 1px solid transparent;
  cursor: pointer;
  transition: transform var(--m-fast) var(--m-ease), background var(--m-fast) var(--m-ease), border-color var(--m-fast) var(--m-ease), box-shadow var(--m-fast) var(--m-ease);
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn--primary {
  background: var(--action);
  color: var(--action-ink);
  border-color: var(--action);
}

@media (hover: hover) and (pointer: fine) {
  .btn--primary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--action) 90%, black);
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
  }
  .btn--primary:active:not(:disabled) {
    transform: scale(0.98);
  }
}

.btn--primary:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn--secondary {
  background: var(--surface);
  color: var(--ink);
  border-color: var(--border);
}

@media (hover: hover) and (pointer: fine) {
  .btn--secondary:hover:not(:disabled) {
    background: var(--border);
    transform: translateY(-2px);
    box-shadow: var(--shadow-sm);
  }
  .btn--secondary:active:not(:disabled) {
    transform: scale(0.98);
  }
}

.btn--secondary:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn--no-chevron::after {
  content: none;
  display: none;
}

.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>