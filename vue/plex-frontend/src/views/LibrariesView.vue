<script setup>
import { ref, onMounted } from "vue";
import AppIcon from "../components/AppIcon.vue";
import { apiFetch } from "../api";

const libraries = ref([]);
const error = ref("");

onMounted(async () => {
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
});
</script>

<template>
  <div class="libraries">
    <h1>Bibliotecas</h1>
    <p class="subtitle">Bibliotecas de séries detectadas no Plex.</p>
    <div v-if="error" class="alert">
      {{ error }}
    </div>
    <div v-else class="library-grid">
      <router-link
        v-for="lib in libraries"
        :key="lib.key"
        :to="{ name: 'series', params: { key: lib.key } }"
        class="library-card"
      >
        <div class="library-card-icon"><AppIcon name="tv" /></div>
        <h3>{{ lib.title }}</h3>
        <span class="library-open">Ver séries <AppIcon name="chevron-right" size="sm" /></span>
      </router-link>
    </div>
    <div v-if="!error && libraries.length === 0" class="empty">
      Nenhuma biblioteca de séries encontrada.
    </div>
  </div>
</template>

<style scoped>
.libraries {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}
.libraries h1 {
  font-size: 1.6rem;
}
.subtitle {
  color: var(--text-secondary);
  margin: 0.25rem 0 1.5rem;
}
.library-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
  gap: 1rem;
}
.library-card {
  padding: 1.25rem;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  transition:
    transform var(--speed) var(--ease),
    box-shadow var(--speed) var(--ease);
}
.library-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-2);
}
.library-card-icon {
  color: var(--accent);
  margin-bottom: 0.5rem;
}
.library-card h3 {
  font-size: 1rem;
}
.library-open {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  margin-top: 0.4rem;
  font-size: 0.8rem;
  color: var(--text-secondary);
}
.empty {
  color: var(--text-tertiary);
  font-style: italic;
}
</style>
