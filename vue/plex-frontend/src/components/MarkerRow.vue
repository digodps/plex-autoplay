<script setup>
import { ref, computed } from "vue";
import { useMarkerStore } from "@/stores/markers";
import AppIcon from "@/components/AppIcon.vue";

const props = defineProps({
  ratingKey: { type: String, required: true },
  kind: { type: String, required: true },
});

const store = useMarkerStore();
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
  <div
    class="flex flex-wrap items-center gap-3 rounded-md border border-transparent px-2 py-1.5 transition hover:border-stroke hover:bg-gray-2 dark:hover:border-strokedark dark:hover:bg-meta-4"
    :class="{ 'opacity-60': isSaving }"
    :data-testid="`marker-row-${ratingKey}-${kind}`"
  >
    <span class="inline-flex w-28 flex-none items-center gap-1.5 text-xs font-semibold text-body dark:text-bodydark">
      <AppIcon :name="kind === 'intro' ? 'play' : 'bookmark'" size="xs" />
      {{ kind === "intro" ? "Intro" : "Créditos" }}
    </span>

    <template v-if="editing">
      <div class="flex flex-1 flex-wrap gap-3">
        <label class="flex flex-col gap-1 text-[11px] text-body dark:text-bodydark">
          Início (ms)
          <input
            v-model="editing.startMs"
            class="w-32 rounded border border-stroke bg-transparent px-2 py-1 text-sm text-black outline-none focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            type="number"
            min="0"
            step="10"
            data-testid="marker-start"
          />
        </label>
        <label class="flex flex-col gap-1 text-[11px] text-body dark:text-bodydark">
          Fim (ms)
          <input
            v-model="editing.endMs"
            class="w-32 rounded border border-stroke bg-transparent px-2 py-1 text-sm text-black outline-none focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
            type="number"
            min="0"
            step="10"
            data-testid="marker-end"
          />
        </label>
      </div>
      <div class="ml-auto flex gap-2">
        <button
          class="inline-flex items-center gap-1 rounded bg-primary px-3 py-1.5 text-xs font-medium text-white transition hover:bg-opacity-90 disabled:opacity-60"
          :disabled="isSaving"
          data-testid="marker-save"
          @click="saveEdit"
        >
          <AppIcon :name="isSaving ? 'spinner' : 'check'" size="xs" :class="{ 'animate-spin': isSaving }" />
          Salvar
        </button>
        <button
          class="rounded border border-stroke px-3 py-1.5 text-xs font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:text-bodydark"
          @click="cancelEdit"
        >
          Descartar
        </button>
      </div>
    </template>

    <template v-else>
      <span class="font-mono text-sm text-black dark:text-white">
        {{ fmtMs(marker?.startMs) }} → {{ fmtMs(marker?.endMs) }}
      </span>
      <span
        v-if="marker"
        class="inline-flex rounded-full px-2 py-0.5 text-[11px] font-medium capitalize"
        :class="marker.origin === 'detected' ? 'bg-meta-5/10 text-meta-5' : 'bg-meta-8/10 text-meta-8'"
      >
        {{ marker.origin === "detected" ? "detectado" : "manual" }}
      </span>
      <div class="ml-auto flex gap-1">
        <button
          v-if="marker"
          class="flex h-8 w-8 items-center justify-center rounded text-body transition hover:bg-gray-2 hover:text-primary dark:text-bodydark dark:hover:bg-meta-4"
          title="Editar"
          data-testid="marker-edit"
          @click="startEdit"
        >
          <AppIcon name="edit" size="sm" />
        </button>
        <button
          v-else
          class="flex h-8 w-8 items-center justify-center rounded text-body transition hover:bg-meta-3/10 hover:text-meta-3 dark:text-bodydark"
          title="Adicionar marcador"
          data-testid="marker-add"
          @click="startEdit"
        >
          <AppIcon name="plus" size="sm" />
        </button>
        <button
          v-if="marker"
          class="flex h-8 w-8 items-center justify-center rounded text-body transition hover:bg-meta-1/10 hover:text-meta-1 dark:text-bodydark"
          title="Remover"
          data-testid="marker-remove"
          @click="removeMarker"
        >
          <AppIcon name="trash" size="sm" />
        </button>
      </div>
    </template>
  </div>
</template>