<script setup>
import { ref, computed } from "vue";
import { useMarkerStore } from "@/stores/markers";
import AppIcon from "@/components/AppIcon.vue";
import { msToReligious, religiousToMs } from "@/utils/timeFormat";

const props = defineProps({
  ratingKey: { type: String, required: true },
  kind: { type: String, required: true },
});

const store = useMarkerStore();
const editing = ref(null);

const marker = computed(() => store.markersMap?.[props.ratingKey]?.[props.kind]);
const isSaving = computed(() => !!store.savingKeys[`${props.ratingKey}:${props.kind}`]);

function startEdit() {
  editing.value = {
    startMs: marker.value?.startMs ?? 0,
    endMs: marker.value?.endMs ?? 0,
    startStr: msToReligious(marker.value?.startMs ?? 0),
    endStr: msToReligious(marker.value?.endMs ?? 0),
  };
}

function cancelEdit() {
  editing.value = null;
}

async function saveEdit() {
  if (!editing.value) return;
  const startMs = religiousToMs(editing.value.startStr);
  const endMs = religiousToMs(editing.value.endStr);
  const exists = !!marker.value;
  const ok = exists
    ? await store.updateMarker(props.ratingKey, props.kind, startMs, endMs)
    : await store.createMarker(props.ratingKey, props.kind, startMs, endMs);
  if (ok) cancelEdit();
}

async function removeMarker() {
  await store.removeMarker(props.ratingKey, props.kind);
}

function onStartChange(val) {
  if (editing.value) editing.value.startStr = val;
}
function onEndChange(val) {
  if (editing.value) editing.value.endStr = val;
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
      <div class="flex flex-1 flex-wrap items-center gap-2">
        <label class="sr-only" for="marker-start">Início (HH:MM:SS.cs)</label>
        <input
          id="marker-start"
          :value="editing.startStr"
          class="w-32 min-h-[44px] min-w-[44px] rounded border border-stroke bg-transparent px-2 py-1.5 font-mono text-sm text-black outline-none focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          type="text"
          placeholder="00:00:00.00"
          aria-label="Início (HH:MM:SS.cs)"
          data-testid="marker-start"
          @input="onStartChange($event.target.value)"
        />
        <label class="sr-only" for="marker-end">Fim (HH:MM:SS.cs)</label>
        <input
          id="marker-end"
          :value="editing.endStr"
          class="w-32 min-h-[44px] min-w-[44px] rounded border border-stroke bg-transparent px-2 py-1.5 font-mono text-sm text-black outline-none focus:border-primary dark:border-form-strokedark dark:bg-form-input dark:text-white"
          type="text"
          placeholder="00:00:00.00"
          aria-label="Fim (HH:MM:SS.cs)"
          data-testid="marker-end"
          @input="onEndChange($event.target.value)"
        />
        <button
          class="inline-flex items-center gap-1 min-h-[44px] min-w-[44px] rounded bg-primary px-3 py-1.5 text-xs font-medium text-white transition hover:bg-opacity-90 disabled:opacity-60"
          :disabled="isSaving"
          data-testid="marker-save"
          @click="saveEdit"
        >
          <AppIcon :name="isSaving ? 'spinner' : 'check'" size="xs" :class="{ 'animate-spin': isSaving }" />
          Salvar
        </button>
        <button
          class="min-h-[44px] min-w-[44px] rounded border border-stroke px-3 py-1.5 text-xs font-medium text-body transition hover:border-primary hover:text-primary dark:border-strokedark dark:text-bodydark"
          @click="cancelEdit"
        >
          Descartar
        </button>
      </div>
    </template>

    <template v-else>
      <span class="font-mono text-sm text-black dark:text-white">
        {{ msToReligious(marker?.startMs) }} → {{ msToReligious(marker?.endMs) }}
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