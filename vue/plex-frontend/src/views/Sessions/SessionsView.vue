<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from "vue";
import { apiFetch } from "@/api";

const POLL_INTERVAL = 5000;
let timer = null;

const loading = ref(true);
const error = ref(null);
const activeSessions = ref([]);
const recentSkips = ref([]);

async function load() {
  try {
    loading.value = true;
    const res = await apiFetch("/api/sessions/skip-status");
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();
    activeSessions.value = data.activeSessions ?? [];
    recentSkips.value = data.recentSkips ?? [];
    error.value = null;
  } catch (e) {
    error.value = e.message || "Falha ao carregar sessões e skips.";
    console.warn("[Sessions] load failed:", e);
  } finally {
    loading.value = false;
  }
}

function startPolling() {
  if (timer) return;
  load();
  timer = setInterval(load, POLL_INTERVAL);
}

function stopPolling() {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
}

const pageVisible = computed(() => document.visibilityState === "visible");

watch(pageVisible, (vis) => {
  if (vis) startPolling();
  else stopPolling();
});

onMounted(() => {
  startPolling();
  document.addEventListener("visibilitychange", handleVisibility);
});

onUnmounted(() => {
  stopPolling();
  document.removeEventListener("visibilitychange", handleVisibility);
});

function handleVisibility() {
  if (document.visibilityState === "visible") startPolling();
  else stopPolling();
}

function fmtMs(ms) {
  const total = Math.max(0, Math.floor(Number(ms) / 1000));
  const m = Math.floor(total / 60).toString().padStart(2, "0");
  const s = (total % 60).toString().padStart(2, "0");
  return `${m}:${s}`;
}

function fmtTime(iso) {
  if (!iso) return "—";
  try {
    return new Date(iso).toLocaleString("pt-BR", {
      dateStyle: "short",
      timeStyle: "short"
    });
  } catch {
    return iso;
  }
}

</script>

<template>
  <div class="mx-auto max-w-6xl px-4 py-3">
    <div class="flex items-center justify-between gap-3">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-gray-100">Sessões &amp; Skips</h1>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Sessões ativas no Plex, próximo skip e histórico de skips executados.
          </p>
        </div>
      </div>

    <div class="p-4 md:p-6 lg:p-8 max-w-6xl mx-auto">
      <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
        <section class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700">
          <h2 class="text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-4 border-b border-stroke pb-2 dark:border-gray-700">
            Sessões ativas
          </h2>
          <div v-if="loading" class="text-sm text-gray-500">Carregando...</div>
          <div v-else-if="error" class="text-xs text-meta-1">{{ error }}</div>
          <template v-else>
            <div v-if="activeSessions.length === 0" class="py-4 text-center text-gray-500 dark:text-gray-400 text-sm">
              Nenhuma sessão ativa no momento.
            </div>

            <!-- Mobile cards view (< md) -->
            <div class="space-y-3 md:hidden" v-if="activeSessions.length > 0">
              <div
                v-for="s in activeSessions"
                :key="'mobile-' + s.machineIdentifier + s.ratingKey"
                class="rounded-lg border border-stroke dark:border-gray-700 p-3 bg-gray-50 dark:bg-gray-900/50 space-y-2"
              >
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div class="font-medium text-gray-900 dark:text-white text-sm">{{ s.product }} ({{ s.platform }})</div>
                    <div class="text-[11px] text-gray-500 dark:text-gray-400 break-all">{{ s.machineIdentifier }}</div>
                  </div>
                  <span
                    class="inline-flex shrink-0 items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium"
                    :class="
                      s.state === 'playing'
                        ? 'text-success bg-success/10 dark:bg-success/10 border border-success/30'
                        : 'text-warning bg-warning/10 dark:bg-warning/10 border border-warning/30'
                    "
                  >
                    <span class="h-1.5 w-1.5 rounded-full" :style="{ backgroundColor: s.state === 'playing' ? 'var(--success)' : 'var(--warning)' }" />
                    {{ s.state }}
                  </span>
                </div>

                <div>
                  <div class="flex justify-between text-xs text-gray-500 dark:text-gray-400 mb-1">
                    <span>Posição / Duração</span>
                    <span class="font-medium text-gray-900 dark:text-white">{{ fmtMs(s.positionMs) }} / {{ fmtMs(s.durationMs) }}</span>
                  </div>
                  <div class="w-full h-1.5 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                    <div
                      :class="s.durationMs > 0 ? 'h-full bg-accent' : 'h-full bg-gray-400 dark:bg-gray-500'"
                      :style="{ width: s.durationMs > 0 ? ((s.positionMs / s.durationMs) * 100).toFixed(0) + '%' : '100%' }"
                    />
                  </div>
                </div>

                <div class="pt-1">
                  <span
                    v-if="s.nextMarker"
                    class="inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium text-warning bg-warning/10 border border-warning/30"
                  >
                    <span class="h-1.5 w-1.5 rounded-full bg-warning" />
                    {{ s.nextMarker.kind }} em {{ fmtMs(s.nextMarker.msUntilSkip) }}
                  </span>
                  <span v-else class="text-xs text-gray-500 dark:text-gray-400">Sem marcador à frente</span>
                </div>
              </div>
            </div>

            <!-- Desktop table view (>= md) -->
            <table v-if="activeSessions.length > 0" class="hidden md:table w-full text-left text-sm">
              <thead>
                <tr class="border-b border-stroke dark:border-gray-700">
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Cliente</th>
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Posição / Duração</th>
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Próximo skip</th>
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Estado</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="s in activeSessions"
                  :key="s.machineIdentifier + s.ratingKey"
                  class="border-b border-stroke/50 dark:border-gray-700/50"
                >
                  <td class="py-3">
                    <div class="font-medium text-gray-900 dark:text-white">{{ s.product }} ({{ s.platform }})</div>
                    <div class="text-xs text-gray-500 dark:text-gray-400">{{ s.machineIdentifier }}</div>
                  </td>
                  <td class="py-3">
                    <div class="font-medium text-gray-900 dark:text-white">
                      {{ fmtMs(s.positionMs) }} / {{ fmtMs(s.durationMs) }}
                    </div>
                    <div
                      class="w-full h-1.5 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden mt-1"
                    >
                      <div
                        :class="
                          s.durationMs > 0
                            ? 'h-full bg-accent'
                            : 'h-full bg-gray-400 dark:bg-gray-500'
                        "
                        :style="{ width: s.durationMs > 0 ? ((s.positionMs / s.durationMs) * 100).toFixed(0) + '%' : '100%' }"
                      />
                    </div>
                  </td>
                  <td class="py-3">
                    <span
                      v-if="s.nextMarker"
                      class="inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-xs font-medium text-warning bg-warning/10 dark:bg-warning/10 border border-warning/30"
                    >
                      <span class="h-1.5 w-1.5 rounded-full bg-warning" />
                      {{ s.nextMarker.kind }} em {{ fmtMs(s.nextMarker.msUntilSkip) }}
                    </span>
                    <span v-else class="text-xs text-gray-500 dark:text-gray-400">Sem marcador à frente</span>
                  </td>
                  <td class="py-3">
                    <span
                      class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium"
                      :class="
                        s.state === 'playing'
                          ? 'text-success bg-success/10 dark:bg-success/10 border border-success/30'
                          : 'text-warning bg-warning/10 dark:bg-warning/10 border border-warning/30'
                      "
                    >
                      <span class="h-1.5 w-1.5 rounded-full" :style="{ backgroundColor: s.state === 'playing' ? 'var(--success)' : 'var(--warning)' }" />
                      {{ s.state }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </template>
        </section>

        <section class="rounded-xl border border-stroke bg-white dark:bg-gray-800 p-5 dark:border-gray-700 col-span-1 md:col-span-2">
          <h2 class="text-sm font-semibold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-4 border-b border-stroke pb-2 dark:border-gray-700">
            Histórico de skips (últimos 50)
          </h2>
          <div v-if="loading" class="text-sm text-gray-500">Carregando...</div>
          <div v-else-if="error" class="text-xs text-meta-1">{{ error }}</div>
          <template v-else>
            <div v-if="recentSkips.length === 0" class="py-4 text-center text-gray-500 dark:text-gray-400 text-sm">
              Nenhum skip executado ainda.
            </div>

            <!-- Mobile cards view (< md) -->
            <div class="space-y-3 md:hidden" v-if="recentSkips.length > 0">
              <div
                v-for="s in recentSkips"
                :key="'mobile-' + s.timestamp + s.ratingKey"
                class="rounded-lg border border-stroke dark:border-gray-700 p-3 bg-gray-50 dark:bg-gray-900/50 space-y-1.5"
              >
                <div class="flex items-start justify-between gap-2">
                  <div>
                    <div class="text-xs font-medium text-gray-900 dark:text-white">{{ fmtTime(s.timestamp) }}</div>
                    <div class="text-xs text-gray-500 dark:text-gray-400">{{ s.product }}</div>
                  </div>
                  <span
                    :class="
                      s.markerKind === 'intro'
                        ? 'text-warning border-warning/30 bg-warning/10'
                        : 'text-danger border-danger/30 bg-danger/10'
                    "
                    class="inline-flex shrink-0 items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium border"
                  >
                    <span class="h-1.5 w-1.5 rounded-full" :style="{ backgroundColor: s.markerKind === 'intro' ? 'var(--warning)' : 'var(--danger)' }" />
                    {{ s.markerKind }}
                  </span>
                </div>
                <div class="font-mono text-xs text-gray-700 dark:text-gray-300 pt-1">
                  {{ fmtMs(s.skippedFromMs) }} → {{ fmtMs(s.skippedToMs) }}
                </div>
              </div>
            </div>

            <!-- Desktop table view (>= md) -->
            <table v-if="recentSkips.length > 0" class="hidden md:table w-full text-left text-sm">
              <thead>
                <tr class="border-b border-stroke dark:border-gray-700">
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Data/Hora</th>
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Cliente</th>
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">Tipo</th>
                  <th class="pb-2 font-medium text-gray-600 dark:text-gray-300">De → Para</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="s in recentSkips"
                  :key="s.timestamp + s.ratingKey"
                  class="border-b border-stroke/50 dark:border-gray-700/50"
                >
                  <td class="py-3 font-medium text-gray-900 dark:text-white">{{ fmtTime(s.timestamp) }}</td>
                  <td class="py-3 text-sm text-gray-700 dark:text-gray-300">{{ s.product }}</td>
                  <td class="py-3">
                    <span
                      :class="
                        s.markerKind === 'intro'
                          ? 'text-warning border-warning/30'
                          : 'text-danger border-danger/30'
                      "
                      class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-xs font-medium border"
                    >
                      <span class="h-1.5 w-1.5 rounded-full" :style="{ backgroundColor: s.markerKind === 'intro' ? 'var(--warning)' : 'var(--danger)' }" />
                      {{ s.markerKind }}
                    </span>
                  </td>
                  <td class="py-3 font-mono text-sm text-gray-700 dark:text-gray-300">
                    {{ fmtMs(s.skippedFromMs) }} → {{ fmtMs(s.skippedToMs) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </template>
        </section>
      </div>
    </div>
  </div>
</template>