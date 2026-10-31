<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import AppIcon from "../components/AppIcon.vue";
import { apiFetch } from "../api";

const router = useRouter();

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
      <div v-for="lib in libraries" :key="lib.key" class="library-card-wrapper">
        <div class="library-card" @click="navigateToSeries(lib.key)">
          <div class="library-card-icon"><AppIcon name="tv" /></div>
          <h3>{{ lib.title }}</h3>
          <span class="library-open">
            <AppIcon name="chevron-right" size="sm" />
            Ver séries
          </span>
        </div>
      </div>
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
.library-card-wrapper {
  display: flex;
  flex-direction: column;
}
.library-card {
  padding: 1.25rem;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  transition:
    transform var(--speed) var(--ease),
    box-shadow var(--speed) var(--ease);
  cursor: pointer;
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
  text-align: center;
  padding: 2rem;
}
.empty .icon {
  margin-bottom: 0.5rem;
  opacity: 0.5;
}

.alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  border-radius: var(--radius);
  background: var(--danger-bg, #fef2f2);
  color: var(--danger, #dc2626);
  margin-bottom: 1rem;
}
.alert .icon {
  flex-shrink: 0;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.5rem 1rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition:
    background var(--speed) var(--ease),
    border-color var(--speed) var(--ease),
    color var(--speed) var(--ease);
}
.btn:hover:not(:disabled) {
  background: var(--bg-sunken);
  border-color: var(--accent);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn-ghost {
  background: transparent;
}
.btn-ghost:hover:not(:disabled) {
  background: var(--bg-sunken);
}
.btn-sm {
  padding: 0.375rem 0.75rem;
  font-size: 0.8125rem;
}
</style>
