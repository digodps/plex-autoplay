<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import { apiFetch } from "../api";

const router = useRouter();
const libraries = ref([]);
const error = ref("");

async function loadLibraries() {
  try {
    const response = await apiFetch("/api/plex/libraries");
    if (response.ok) {
      libraries.value = await response.json();
    } else {
      error.value = "Erro ao carregar bibliotecas";
    }
  } catch {
    error.value = "Erro de conexão";
  }
}

onMounted(loadLibraries);

function navigateToSeries(libraryKey) {
  router.push({ name: "series", params: { key: libraryKey } });
}
</script>

<template>
  <div class="libraries">
    <h1>Bibliotecas</h1>
    <p class="subtitle">Bibliotecas de séries detectadas no Plex.</p>
    <div v-if="error" class="alert">
      {{ error }}
    </div>
    <div v-else class="library-grid">
      <button
        v-for="lib in libraries"
        :key="lib.key"
        class="library-card"
        type="button"
        @click="navigateToSeries(lib.key)"
      >
        <span class="library-card-icon"><AppIcon name="tv" /></span>
        <h3>{{ lib.title }}</h3>
        <span class="library-open">
          <AppIcon name="chevron-right" size="sm" />
          Ver séries
        </span>
      </button>
    </div>
  </div>

  <div v-if="!error && libraries.length === 0" class="empty">
    Nenhuma biblioteca de séries encontrada.
  </div>
</template>

