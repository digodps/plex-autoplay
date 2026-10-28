<script setup>
import { ref, computed } from "vue";
import { useMarkerStore } from "../stores/markers";
import AppIcon from "./AppIcon.vue";

const props = defineProps({
  ratingKey: { type: String, required: true },
  kind: { type: String, required: true },
});

const store = useMarkerStore();

// Estado local de edição inline: { startMs, endMs } quando aberta, null na leitura.
const editing = ref(null);

const marker = computed(() => store.markersMap?.[props.ratingKey]?.[props.kind]);
const isSaving = computed(() => !!store.savingKeys[`${props.ratingKey}:${props.kind}`]);

function fmtMs(ms) {
  if (ms == null) return "--:--";
  const totalSec = Math.floor(ms / 1000);
  const m = Math.floor(totalSec / 60);
  const s = totalSec % 60;
  const cs = Math.floor((ms % 1000) / 10);
  return `${String(m).padStart(2, "0")}:${String(s).padStart(2, "0")}.${String(cs).padStart(2, "0")}`;
}

function startEdit() {
  editing.value = { startMs: marker.value?.startMs ?? 0, endMs: marker.value?.endMs ?? 0 };
}

function cancelEdit() {
  editing.value = null;
}

async function saveEdit() {
  if (!editing.value) return;
  const startMs = Number(editing.value.startMs) || 0;
  const endMs = Number(editing.value.endMs) || 0;
  const exists = !!marker.value;
  const ok = exists
    ? await store.updateMarker(props.ratingKey, props.kind, startMs, endMs)
    : await store.createMarker(props.ratingKey, props.kind, startMs, endMs);
  if (ok) cancelEdit();
}

async function removeMarker() {
  await store.removeMarker(props.ratingKey, props.kind);
}
</script>

<template>
  <div class="marker-row" :class="{ 'is-saving': isSaving }">
    <span class="kind-label">
      <AppIcon :name="kind === 'intro' ? 'play' : 'bookmark'" size="sm" />
      {{ kind === "intro" ? "Intro" : "Créditos" }}
    </span>

    <!-- em edição inline -->
    <template v-if="editing">
      <div class="edit-fields">
        <label>
          Início (ms)
          <input v-model="editing.startMs" class="input" type="number" min="0" step="10" />
        </label>
        <label>
          Fim (ms)
          <input v-model="editing.endMs" class="input" type="number" min="0" step="10" />
        </label>
      </div>
      <div class="row-actions">
        <button class="btn btn-sm" :disabled="isSaving" @click="saveEdit">
          <AppIcon v-if="!isSaving" name="check" size="sm" />
          <AppIcon v-else name="spinner" size="sm" class="spin" />
          Salvar
        </button>
        <button class="btn btn-ghost btn-sm" @click="cancelEdit">Descartar</button>
      </div>
    </template>

    <!-- leitura -->
    <template v-else>
      <span class="times">
        {{ fmtMs(marker?.startMs) }}
        →
        {{ fmtMs(marker?.endMs) }}
      </span>
      <span
        v-if="marker"
        class="badge"
        :class="marker.origin === 'detected' ? 'badge-detected' : 'badge-manual'"
      >
        {{ marker.origin === "detected" ? "detectado" : "manual" }}
      </span>
      <div class="row-actions">
        <button v-if="marker" class="btn-icon info" title="Editar" @click="startEdit">
          <AppIcon name="edit" size="sm" />
        </button>
        <button v-else class="btn-icon add" title="Adicionar marcador" @click="startEdit">
          <AppIcon name="plus" size="sm" />
        </button>
        <button v-if="marker" class="btn-icon remove" title="Remover" @click="removeMarker">
          <AppIcon name="trash" size="sm" />
        </button>
      </div>
    </template>
  </div>
</template>

<style scoped src="./marker-row.css"></style>
