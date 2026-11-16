<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
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
  <div class="libraries-view">
    <!-- HERO: photograph-led, 1 screen, data-enter -->
    <section class="hero hero--split" data-enter aria-labelledby="libraries-hero-h">
      <img class="layer" src="/assets/layout/img/photo-m61660576.jpg" alt="" aria-hidden="true" />
      <div class="wrap grid hero-text hero-split">
        <div class="hero-copy span-6">
          <h1 id="libraries-hero-h">Bibliotecas</h1>
          <p class="t-lead">Bibliotecas de séries detectadas no Plex. Selecione uma para gerenciar marcadores.</p>
          <div class="btn-row">
            <button class="btn btn--primary" :disabled="libraries.length === 0" data-testid="btn-refresh-libraries" @click="loadLibraries">
              <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="20" height="20" aria-hidden="true" />
              Atualizar
            </button>
          </div>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame" data-fx="mesh">
            <div class="stage at-corner" data-ui="library-mosaic" data-seed="libraries"></div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Suas Bibliotecas -->
    <section class="section" aria-labelledby="libraries-h" style="--h: 2">
      <div class="wrap grid">
        <h2 id="libraries-h" class="t-h2">Suas Bibliotecas</h2>

        <div v-if="error" class="alert" role="alert" style="grid-column: 1 / -1;">
          <img src="/assets/icons/ui-triangle-alert.svg" data-icon="ui-triangle-alert" class="icon" width="20" height="20" aria-hidden="true" />
          <span>{{ error }}</span>
          <button class="btn btn--secondary btn-sm" @click="loadLibraries">
            <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="16" height="16" aria-hidden="true" />
            Tentar novamente
          </button>
        </div>

        <div v-else-if="libraries.length === 0" class="empty-state" style="grid-column: 1 / -1;">
          <img src="/assets/icons/ui-tv.svg" data-icon="ui-tv" class="icon icon-lg" width="64" height="64" aria-hidden="true" />
          <p>Nenhuma biblioteca de séries encontrada.</p>
          <p class="t-small muted">Execute um escaneamento ou verifique a conexão com o Plex.</p>
          <div class="btn-row" style="margin-top: var(--s-6);">
            <button class="btn btn--primary" @click="loadLibraries">
              <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="20" height="20" aria-hidden="true" />
              Escanear novamente
            </button>
          </div>
        </div>

        <div v-else class="library-grid grid" role="list" aria-label="Bibliotecas de séries">
          <article
            v-for="lib in libraries"
            :key="lib.key"
            role="listitem"
            class="card span-3"
            tabindex="0"
            @click="navigateToSeries(lib.key)"
            @keydown.enter="navigateToSeries(lib.key)"
            @keydown.space.prevent="navigateToSeries(lib.key)"
          >
            <div class="panel" style="--p-h: 140px;">
              <div class="stage at-corner" data-ui="library-card" :data-title="lib.title"></div>
            </div>
            <h3 class="card__title">{{ lib.title }}</h3>
            <p class="card__text">{{ lib.count || 0 }} séries</p>
            <a class="card__link" @click.stop="navigateToSeries(lib.key)">
              <span>Ver séries</span>
              <img src="/assets/icons/ui-chevron-right.svg" data-icon="ui-chevron-right" class="icon" width="14" height="14" aria-hidden="true" />
            </a>
          </article>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.libraries-view {
  /* Page wrapper - sections handle layout */
}

.hero {
  position: relative;
  min-height: calc(100vh - var(--nav-h, 64px));
  display: flex;
  align-items: center;
}

.hero-text {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  column-gap: var(--gutter);
  align-items: center;
  height: 100%;
}

.hero-copy {
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.hero-copy h1 {
  font: var(--font-display);
  font-size: var(--t-h1);
  font-weight: var(--display-weight);
  line-height: 1.1;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-4);
  color: var(--ink);
}

.hero-copy .t-lead {
  font-size: var(--t-lead);
  line-height: 1.6;
  color: var(--muted);
  margin: 0 0 var(--s-6);
  max-width: 60ch;
}

.btn-row {
  display: flex;
  gap: var(--s-3);
  flex-wrap: wrap;
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
  transition: background var(--dur) var(--ease), border-color var(--dur) var(--ease), color var(--dur) var(--ease), transform var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.btn:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.btn--primary {
  background: var(--p-action);
  color: var(--p-action-ink);
  border-color: var(--p-action);
}
@media (hover: hover) and (pointer: fine) {
  .btn--primary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--p-action) 84%, var(--bg));
    transform: translateY(-1px);
  }
}
.btn--primary:active:not(:disabled) {
  transform: translateY(1px) scale(0.985);
}

.btn--secondary {
  background: var(--surface);
  color: var(--ink);
  border-color: var(--border);
}
@media (hover: hover) and (pointer: fine) {
  .btn--secondary:hover:not(:disabled) {
    background: color-mix(in oklab, var(--bg) 92%, var(--ink));
    border-color: color-mix(in oklab, var(--ink) 32%, transparent);
    transform: translateY(-1px);
  }
}
.btn--secondary:active:not(:disabled) {
  transform: translateY(1px) scale(0.985);
}

.btn-sm {
  padding: var(--s-1) var(--s-3);
  font-size: var(--t-small);
}

.icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}
.icon-lg { width: 64px; height: 64px; }

.hero-visual {
  display: flex;
  align-items: center;
  justify-content: center;
}

.frame {
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
}

.section {
  padding: var(--padt) 0 calc(var(--padb) + var(--bar));
}

.t-h2 {
  font: var(--font-display);
  font-size: var(--t-h2);
  font-weight: 600;
  letter-spacing: var(--display-tracking);
  margin: 0 0 var(--s-6);
  color: var(--ink);
}

/* Alert */
.alert {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  padding: var(--s-4) var(--s-5);
  border-radius: var(--r-md);
  background: color-mix(in oklab, var(--danger) 8%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger) 25%, transparent);
  color: var(--danger);
  margin-bottom: var(--s-6);
}

/* Empty state */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--s-10) var(--s-6);
  text-align: center;
  gap: var(--s-4);
  color: var(--muted);
}
.empty-state .icon-lg {
  opacity: 0.4;
  margin-bottom: var(--s-2);
}

/* Library grid cards */
.library-grid {
  margin-top: var(--s-6);
}
.library-grid .card {
  cursor: pointer;
  transition: transform var(--m-out) var(--m-ease), border-color var(--m-out) var(--m-ease), box-shadow var(--m-out) var(--m-ease);
}
@media (hover: hover) and (pointer: fine) {
  .library-grid .card:hover {
    transform: translateY(var(--m-lift));
    border-color: color-mix(in oklab, var(--ink) 16%, transparent);
    box-shadow: inset 0 1px 0 var(--c-hi, transparent), 0 1px 2px rgb(0 0 0 / .06),
                0 14px 32px -18px oklch(from var(--card, #fff) 0 0 0 / calc(0.75 - l * 0.55));
    transition-duration: 180ms;
  }
}

.card__title {
  margin: var(--s-4) 0 var(--s-2);
  font-family: var(--font-text);
  font-size: var(--t-card);
  font-weight: 560;
  letter-spacing: -0.012em;
  color: var(--ink);
  text-wrap: balance;
}

.card__text {
  margin: 0 0 var(--s-3);
  font-size: var(--t-body);
  line-height: 1.5;
  color: color-mix(in oklab, var(--ink) 84%, var(--card));
  max-width: 46ch;
  text-wrap: pretty;
}

.card__link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin-top: var(--s-3);
  width: max-content;
  font-size: 15px;
  font-weight: 500;
  line-height: 22px;
  color: var(--ink);
  text-decoration: none;
}
.card__link svg { width: 14px; height: 14px; transition: transform 200ms cubic-bezier(.2,.7,.2,1); }
@media (hover: hover) and (pointer: fine) {
  .card__link:hover svg { transform: translateX(2px); }
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .library-grid .panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
  .library-grid .panel:nth-child(2) { animation-delay: 0.8s; }
  .library-grid .panel:nth-child(3) { animation-delay: 1.6s; }
  .library-grid .panel:nth-child(4) { animation-delay: 2.4s; }
  .library-grid .panel:nth-child(5) { animation-delay: 0.4s; }
  .library-grid .panel:nth-child(6) { animation-delay: 1.2s; }
  .library-grid .panel:nth-child(7) { animation-delay: 2.0s; }
  .library-grid .panel:nth-child(8) { animation-delay: 2.8s; }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

.library-grid .panel:hover {
  animation-play-state: paused;
}

/* Breadcrumb spacing */
.section > .wrap > .breadcrumb + * {
  margin-top: var(--s-4);
}
</style>