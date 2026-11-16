<script setup>
import { ref, computed } from "vue";
import { useI18n } from "vue-i18n";
import { useMarkerStore } from "@/stores/markers";

const props = defineProps({
  ratingKey: { type: String, required: true },
  kind: { type: String, required: true },
  episode: { type: Object, required: true },
});

const { t } = useI18n();
const store = useMarkerStore();

const isSaving = ref(false);
const showInputs = ref(false);
const inputStart = ref("");
const inputEnd = ref("");

const marker = computed(() => store.markersMap[props.ratingKey]?.[props.kind] ?? null);
const hasMarker = computed(() => marker.value !== null);
const isSavingKey = computed(() => `${props.ratingKey}:${props.kind}`);
const saving = computed(() => store.savingKeys[isSavingKey.value] === true);

const formatMs = (ms) => {
  if (!ms && ms !== 0) return "";
  const total = Math.round(ms / 1000);
  const h = Math.floor(total / 3600);
  const m = Math.floor((total % 3600) / 60);
  const s = total % 60;
  return h > 0 ? `${h}:${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}` : `${m}:${String(s).padStart(2, "0")}`;
};

const parseTime = (str) => {
  if (!str) return 0;
  const parts = str.split(":").map(Number);
  if (parts.length === 3) return (parts[0] * 3600 + parts[1] * 60 + parts[2]) * 1000;
  if (parts.length === 2) return (parts[0] * 60 + parts[1]) * 1000;
  return 0;
};

function toggleEdit() {
  if (hasMarker.value) {
    inputStart.value = formatMs(marker.value.startMs);
    inputEnd.value = formatMs(marker.value.endMs);
  } else {
    inputStart.value = "";
    inputEnd.value = "";
  }
  showInputs.value = !showInputs.value;
}

async function saveMarker() {
  const startMs = parseTime(inputStart.value);
  const endMs = parseTime(inputEnd.value);

  if (startMs >= endMs) {
    window.alert(t("markers.invalidRange"));
    return;
  }

  isSaving.value = true;
  try {
    if (hasMarker.value) {
      await store.updateMarker(props.ratingKey, props.kind, startMs, endMs);
    } else {
      await store.createMarker(props.ratingKey, props.kind, startMs, endMs);
    }
    showInputs.value = false;
  } finally {
    isSaving.value = false;
  }
}

async function deleteMarker() {
  if (!window.confirm(t("markers.confirmDelete", { kind: t(`markers.${props.kind}`) }))) return;
  await store.removeMarker(props.ratingKey, props.kind);
}

function getKindIcon(kind) {
  return kind === "intro" ? "ui-skip-forward" : "ui-skip-back";
}

function getKindLabel(kind) {
  return t(`markers.${kind}`);
}
</script>

<template>
  <div class="marker-editor-row">
    <div class="marker-label">{{ getKindLabel(kind) }}</div>

    <div class="marker-controls">
      <div v-if="!showInputs && !hasMarker" class="marker-empty">
        <button class="btn btn--secondary btn-sm" @click="toggleEdit" :disabled="saving" data-testid="marker-btn-create">
          <img :src="`/assets/icons/${getKindIcon(kind)}.svg`" :data-icon="getKindIcon(kind)" class="icon" width="16" height="16" aria-hidden="true" />
          {{ t("markers.add", { kind: t(`markers.${kind}`) }) }}
        </button>
      </div>

      <div v-else-if="!showInputs && hasMarker" class="marker-display">
        <div class="marker-times">
          <span class="time">{{ formatMs(marker.startMs) }} – {{ formatMs(marker.endMs) }}</span>
          <span class="badge" :class="marker.origin === 'auto' ? 'badge--auto' : 'badge--manual'">
            {{ marker.origin === 'auto' ? t("markers.auto") : t("markers.manual") }}
          </span>
        </div>
        <div class="marker-actions">
          <button class="btn btn--secondary btn-sm" @click="toggleEdit" :disabled="saving" data-testid="marker-btn-edit">
            <img src="/assets/icons/ui-square-pen.svg" data-icon="ui-square-pen" class="icon" width="14" height="14" aria-hidden="true" />
          </button>
          <button class="btn btn--secondary btn-sm" @click="deleteMarker" :disabled="saving" data-testid="marker-btn-delete">
            <img src="/assets/icons/ui-trash-2.svg" data-icon="ui-trash-2" class="icon" width="14" height="14" aria-hidden="true" />
          </button>
        </div>
      </div>

      <div v-else class="marker-form" style="display: flex; align-items: center; gap: var(--s-3); flex-wrap: wrap;">
        <div style="display: flex; align-items: center; gap: var(--s-2);">
          <label :for="`start-${ratingKey}-${kind}`" class="visually-hidden">{{ t("markers.start") }}</label>
          <input
            :id="`start-${ratingKey}-${kind}`"
            v-model="inputStart"
            type="text"
            placeholder="MM:SS ou HH:MM:SS"
            style="width: 100px;"
            class="field__input"
            @keyup.enter="saveMarker"
          />
          <span>–</span>
          <label :for="`end-${ratingKey}-${kind}`" class="visually-hidden">{{ t("markers.end") }}</label>
          <input
            :id="`end-${ratingKey}-${kind}`"
            v-model="inputEnd"
            type="text"
            placeholder="MM:SS ou HH:MM:SS"
            style="width: 100px;"
            class="field__input"
            @keyup.enter="saveMarker"
          />
        </div>
        <button class="btn btn--primary btn-sm" @click="saveMarker" :disabled="isSaving || saving" data-testid="marker-btn-save">
          <img src="/assets/icons/ui-check.svg" data-icon="ui-check" class="icon" width="14" height="14" aria-hidden="true" />
          {{ t("common.save") }}
        </button>
        <button class="btn btn--secondary btn-sm" @click="toggleEdit" :disabled="isSaving || saving" data-testid="marker-btn-cancel">
          <img src="/assets/icons/ui-circle-x.svg" data-icon="ui-circle-x" class="icon" width="14" height="14" aria-hidden="true" />
          {{ t("common.cancel") }}
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.marker-editor-row {
  display: grid;
  grid-template-columns: 120px 1fr;
  gap: var(--s-4);
  align-items: center;
}

.marker-label {
  font-size: var(--t-small);
  font-weight: 500;
  color: var(--muted);
  text-transform: capitalize;
}

.marker-controls {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  flex-wrap: wrap;
}

.marker-empty {
  display: flex;
  align-items: center;
}

.marker-display {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  gap: var(--s-3);
}

.marker-times {
  display: flex;
  align-items: center;
  gap: var(--s-3);
}

.marker-times .time {
  font-family: var(--font-mono);
  font-size: var(--t-body);
  color: var(--ink);
}

.badge {
  font-size: var(--t-label);
  font-weight: 500;
  padding: var(--s-1) var(--s-2);
  border-radius: var(--r-sm);
  text-transform: uppercase;
}

.badge--auto {
  background: color-mix(in oklab, var(--action) 12%, transparent);
  color: var(--action);
  border: 1px solid color-mix(in oklab, var(--action) 25%, transparent);
}

.badge--manual {
  background: color-mix(in oklab, var(--muted) 12%, transparent);
  color: var(--muted);
  border: 1px solid color-mix(in oklab, var(--muted) 25%, transparent);
}

.marker-actions {
  display: flex;
  gap: var(--s-2);
}

.marker-form {
  width: 100%;
  padding: var(--s-2) 0;
}

.field__input {
  width: 100%;
  padding: var(--s-2) var(--s-3);
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--r-sm);
  transition: border-color var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
.field__input:focus {
  outline: none;
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), 0 0 0 4px color-mix(in oklab, var(--action) 20%, transparent);
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

@media (max-width: 639px) {
  .marker-editor-row {
    grid-template-columns: 1fr;
    gap: var(--s-2);
  }
  .marker-label {
    text-transform: capitalize;
  }
  .marker-controls {
    flex-direction: column;
    align-items: flex-start;
    gap: var(--s-2);
  }
  .marker-display {
    flex-direction: column;
    align-items: flex-start;
  }
  .marker-actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>