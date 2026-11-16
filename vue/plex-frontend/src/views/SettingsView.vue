<script setup>
import { ref, onMounted } from "vue";
import { apiFetch } from "../api";
const saving = ref(false);
const message = ref("");
const messageType = ref<"success" | "error">("success");

const syncInterval = ref(5);
const plexUrl = ref("");
const plexToken = ref("");
const advancedOptions = ref(false);



onMounted(async () => {
  try {
    const saved = localStorage.getItem("syncIntervalMinutes");
    if (saved) syncInterval.value = parseInt(saved);

    // Try to load Plex config from API
    const res = await apiFetch("/api/config/plex");
    if (res.ok) {
      const config = await res.json();
      plexUrl.value = config.url || "";
      plexToken.value = config.token || "";
    }
  } catch {
    // Ignore, use defaults
  }
});

const saveSettings = async () => {
  saving.value = true;
  message.value = "";
  try {
    localStorage.setItem("syncIntervalMinutes", syncInterval.value.toString());

    // Save Plex config if provided
    if (plexUrl.value.trim() || plexToken.value.trim()) {
      await apiFetch("/api/config/plex", {
        method: "POST",
        body: JSON.stringify({ url: plexUrl.value.trim(), token: plexToken.value.trim() }),
      });
    }

    message.value = "Configurações salvas com sucesso";
    messageType.value = "success";
  } catch (e) {
    message.value = "Erro ao salvar configurações: " + (e instanceof Error ? e.message : String(e));
    messageType.value = "error";
  } finally {
    saving.value = false;
  }
};

const resetToDefaults = async () => {
  syncInterval.value = 5;
  plexUrl.value = "";
  plexToken.value = "";
  localStorage.removeItem("syncIntervalMinutes");
  localStorage.removeItem("plex.config");
  message.value = "Configurações restauradas aos padrões";
  messageType.value = "success";
};

function validateInterval(value: number) {
  return value >= 1 && value <= 1440;
}
</script>

<template>
  <div class="settings-view">
    <!-- HERO: type-led, 1 screen, data-enter -->
    <section class="hero hero--statement" data-enter aria-labelledby="settings-hero-h">
      <div class="wrap grid">
        <div class="hero-copy span-6">
          <h1 id="settings-hero-h">Configurações</h1>
          <p class="t-lead">Configure o intervalo de sincronização, conexão com o Plex e opções avançadas.</p>
        </div>
        <div class="hero-visual span-6 start-7" aria-hidden="true">
          <div class="panel frame" data-fx="settings-mesh">
            <div class="stage at-corner" data-ui="settings-panel"></div>
          </div>
        </div>
      </div>
    </section>

    <!-- SECTION: Geral -->
    <section class="section" aria-labelledby="general-h" style="--h: 2">
      <div class="wrap">
        <h2 id="general-h" class="t-h2">Geral</h2>

        <div v-if="message" class="alert" :class="messageType" role="alert">
          <img :src="messageType === 'success' ? '/assets/icons/ui-circle-check-big.svg' : '/assets/icons/ui-triangle-alert.svg'" :data-icon="messageType === 'success' ? 'ui-circle-check-big' : 'ui-triangle-alert'" class="icon" width="20" height="20" aria-hidden="true" />
          <span>{{ message }}</span>
        </div>

        <div class="settings-grid grid" role="list">
          <article class="card span-6" role="listitem">
            <h3 class="card__title">Intervalo de Sincronização</h3>
            <p class="card__text">O serviço de sincronização será executado a cada N minutos para detectar novos episódios e gerar marcadores.</p>
            <div class="field" style="margin-top: var(--s-4);">
              <label for="sync-interval" class="field__label">Minutos</label>
              <input
                id="sync-interval"
                v-model.number="syncInterval"
                type="number"
                min="1"
                max="1440"
                class="field__input"
                :aria-invalid="!validateInterval(syncInterval)"
                @blur="syncInterval = Math.max(1, Math.min(1440, syncInterval || 1))"
              />
              <p class="field__hint">Mínimo: 1 minuto. Máximo: 1440 minutos (24 horas).</p>
            </div>
          </article>

          <article class="card span-6" role="listitem">
            <h3 class="card__title">Inicialização</h3>
            <p class="card__text">Comportamento do aplicativo ao iniciar.</p>
            <div class="field" style="margin-top: var(--s-4);">
              <label class="m-switch" role="switch" aria-checked="true" aria-label="Iniciar sincronização automaticamente">
                <span class="visually-hidden">Iniciar sincronização automaticamente</span>
              </label>
              <span class="field__label">Sincronização automática ao iniciar</span>
            </div>
            <div class="field" style="margin-top: var(--s-3);">
              <label class="m-switch" role="switch" aria-checked="false" aria-label="Verificar atualizações na inicialização">
                <span class="visually-hidden">Verificar atualizações na inicialização</span>
              </label>
              <span class="field__label">Verificar atualizações na inicialização</span>
            </div>
          </article>
        </div>
      </div>
    </section>

    <!-- SECTION: Conexão Plex -->
    <section class="section" aria-labelledby="plex-h" style="--h: 2">
      <div class="wrap">
        <h2 id="plex-h" class="t-h2">Conexão Plex</h2>
        <p class="t-lead" style="margin-bottom: var(--s-6);">Configure a URL e o token de acesso ao seu servidor Plex. O token pode ser obtido em <a href="https://www.plex.tv/claim/" target="_blank" rel="noopener" class="link">plex.tv/claim</a>.</p>

        <div class="settings-grid grid" role="list">
          <article class="card span-12" role="listitem">
            <div class="field" style="margin-top: var(--s-4);">
              <label for="plex-url" class="field__label">URL do Servidor Plex</label>
              <input
                id="plex-url"
                v-model="plexUrl"
                type="url"
                class="field__input"
                placeholder="http://localhost:32400"
                autocomplete="url"
              />
              <p class="field__hint">Exemplo: http://192.168.1.100:32400 ou https://plex.exemplo.com</p>
            </div>
            <div class="field" style="margin-top: var(--s-4);">
              <label for="plex-token" class="field__label">X-Plex-Token</label>
              <input
                id="plex-token"
                v-model="plexToken"
                type="password"
                class="field__input"
                placeholder="Token de acesso"
                autocomplete="off"
              />
              <p class="field__hint">Obtenha seu token em <a href="https://www.plex.tv/claim/" target="_blank" rel="noopener" class="link">plex.tv/claim</a></p>
            </div>
          </article>
        </div>
      </div>
    </section>

    <!-- SECTION: Sincronização -->
    <section class="section" aria-labelledby="sync-h" style="--h: 2">
      <div class="wrap">
        <h2 id="sync-h" class="t-h2">Sincronização</h2>

        <div class="settings-grid grid" role="list">
          <article class="card span-6" role="listitem">
            <h3 class="card__title">Opções de Escaneamento</h3>
            <p class="card__text">Configure como o escaneamento processa as bibliotecas.</p>
            <div class="field" style="margin-top: var(--s-4);">
              <label class="m-switch" role="switch" aria-checked="true" aria-label="Escanear apenas bibliotecas de séries">
                <span class="visually-hidden">Escanear apenas bibliotecas de séries</span>
              </label>
              <span class="field__label">Apenas bibliotecas de séries</span>
            </div>
            <div class="field" style="margin-top: var(--s-3);">
              <label class="m-switch" role="switch" aria-checked="false" aria-label="Incluir especiais e extras">
                <span class="visually-hidden">Incluir especiais e extras</span>
              </label>
              <span class="field__label">Incluir especiais e extras</span>
            </div>
            <div class="field" style="margin-top: var(--s-3);">
              <label class="m-switch" role="switch" aria-checked="true" aria-label="Atualizar marcadores existentes">
                <span class="visually-hidden">Atualizar marcadores existentes</span>
              </label>
              <span class="field__label">Atualizar marcadores existentes</span>
            </div>
          </article>

          <article class="card span-6" role="listitem">
            <h3 class="card__title">Processamento</h3>
            <p class="card__text">Limites e comportamento do processamento de marcadores.</p>
            <div class="field" style="margin-top: var(--s-4);">
              <label for="max-concurrent" class="field__label">Processos simultâneos máximos</label>
              <input
                id="max-concurrent"
                type="number"
                min="1"
                max="10"
                class="field__input"
                value="3"
              />
              <p class="field__hint">Quantas séries processar ao mesmo tempo.</p>
            </div>
            <div class="field" style="margin-top: var(--s-4);">
              <label for="timeout" class="field__label">Timeout por episódio (segundos)</label>
              <input
                id="timeout"
                type="number"
                min="30"
                max="600"
                class="field__input"
                value="120"
              />
              <p class="field__hint">Tempo máximo para processar um episódio.</p>
            </div>
          </article>
        </div>
      </div>
    </section>

    <!-- SECTION: Avançado -->
    <section class="section" aria-labelledby="advanced-h" style="--h: 2">
      <div class="wrap">
        <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: var(--s-6);">
          <h2 id="advanced-h" class="t-h2">Avançado</h2>
          <button class="btn btn--secondary btn-sm" @click="advancedOptions = !advancedOptions" :aria-expanded="advancedOptions">
            <img :src="advancedOptions ? '/assets/icons/ui-chevron-down.svg' : '/assets/icons/ui-chevron-down.svg'" :data-icon="advancedOptions ? 'ui-chevron-down' : 'ui-chevron-down'" class="icon" width="16" height="16" aria-hidden="true" :style="advancedOptions ? 'transform: scaleY(-1);' : ''" />
            {{ advancedOptions ? 'Ocultar' : 'Mostrar' }} opções avançadas
          </button>
        </div>

        <div v-show="advancedOptions" class="settings-grid grid" role="list">
          <article class="card span-6" role="listitem">
            <h3 class="card__title">Logs e Debug</h3>
            <p class="card__text">Configuração de logs e nível de detalhe.</p>
            <div class="field" style="margin-top: var(--s-4);">
              <label for="log-level" class="field__label">Nível de log</label>
              <select id="log-level" class="field__input">
                <option value="info">Info (padrão)</option>
                <option value="debug">Debug</option>
                <option value="warn">Apenas avisos</option>
                <option value="error">Apenas erros</option>
              </select>
            </div>
            <div class="field" style="margin-top: var(--s-3);">
              <label class="m-switch" role="switch" aria-checked="false" aria-label="Log detalhado de API">
                <span class="visually-hidden">Log detalhado de API</span>
              </label>
              <span class="field__label">Log detalhado de requisições API</span>
            </div>
          </article>

          <article class="card span-6" role="listitem">
            <h3 class="card__title">Banco de Dados</h3>
            <p class="card__text">Opções de manutenção do banco SQLite de marcadores.</p>
            <div class="field" style="margin-top: var(--s-4);">
              <label class="m-switch" role="switch" aria-checked="true" aria-label="Vacuum automático semanal">
                <span class="visually-hidden">Vacuum automático semanal</span>
              </label>
              <span class="field__label">Vacuum automático semanal</span>
            </div>
            <div class="field" style="margin-top: var(--s-3);">
              <label class="m-switch" role="switch" aria-checked="false" aria-label="Backup automático antes de mudanças">
                <span class="visually-hidden">Backup automático antes de mudanças</span>
              </label>
              <span class="field__label">Backup automático antes de mudanças</span>
            </div>
          </article>

          <article class="card span-12" role="listitem" style="margin-top: var(--s-6);">
            <div style="display: flex; gap: var(--s-3); flex-wrap: wrap;">
              <button class="btn btn--primary" :disabled="saving" @click="saveSettings">
                <img src="/assets/icons/ui-file-text.svg" data-icon="ui-file-text" class="icon" width="20" height="20" aria-hidden="true" />
                {{ saving ? "Salvando..." : "Salvar Configurações" }}
              </button>
              <button class="btn btn--secondary" :disabled="saving" @click="resetToDefaults">
                <img src="/assets/icons/ui-refresh-cw.svg" data-icon="ui-refresh-cw" class="icon" width="20" height="20" aria-hidden="true" />
                Restaurar Padrões
              </button>
            </div>
          </article>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.settings-view {
  /* Page wrapper */
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
.icon-lg { width: 48px; height: 48px; }

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

.t-lead {
  font-size: var(--t-lead);
  line-height: 1.5;
  color: var(--muted);
  margin: 0 0 var(--s-6);
  max-width: 60ch;
}

/* Alert */
.alert {
  display: flex;
  align-items: center;
  gap: var(--s-3);
  padding: var(--s-4) var(--s-5);
  border-radius: var(--r-md);
  margin-bottom: var(--s-6);
}
.alert.success {
  background: color-mix(in oklab, var(--success) 8%, transparent);
  border: 1px solid color-mix(in oklab, var(--success) 25%, transparent);
  color: var(--success);
}
.alert.error {
  background: color-mix(in oklab, var(--danger) 8%, transparent);
  border: 1px solid color-mix(in oklab, var(--danger) 25%, transparent);
  color: var(--danger);
}

/* Settings Grid */
.settings-grid {
  margin-top: var(--s-6);
}

/* Card */
.card__title {
  margin: 0 0 var(--s-2);
  font-family: var(--font-text);
  font-size: var(--t-card);
  font-weight: 560;
  letter-spacing: -0.012em;
  color: var(--ink);
  text-wrap: balance;
}

.card__text {
  margin: 0 0 var(--s-4);
  font-size: var(--t-body);
  line-height: 1.5;
  color: color-mix(in oklab, var(--ink) 84%, var(--card));
  max-width: 46ch;
  text-wrap: pretty;
}

/* Field */
.field {
  display: flex;
  flex-direction: column;
  gap: var(--s-1);
}
.field__label {
  font-size: var(--t-label);
  font-weight: 500;
  letter-spacing: 0.01em;
  color: var(--ink);
}
.field__input,
.field select {
  padding: var(--s-2) var(--s-3);
  font: var(--font-text);
  font-size: var(--t-body);
  color: var(--ink);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--r-md);
  transition: border-color var(--dur) var(--ease), box-shadow var(--dur) var(--ease);
}
.field__input:focus,
.field select:focus {
  outline: none;
  border-color: var(--action);
  box-shadow: 0 0 0 1px var(--action), 0 0 0 4px color-mix(in oklab, var(--action) 20%, transparent);
}
.field__hint {
  margin: 0;
  font-size: var(--t-small);
  color: var(--muted);
}
.field__hint a {
  color: var(--accent);
}

/* Switch (m-switch from motion.css) */
.m-switch {
  position: relative;
  flex: none;
  width: 40px;
  height: 24px;
  padding: 0;
  border: 0;
  border-radius: 999px;
  cursor: pointer;
  background: color-mix(in oklab, var(--ink) 48%, var(--bg));
  transition: background-color 180ms var(--m-ease);
}
.m-switch::before {
  content: "";
  position: absolute;
  left: 3px;
  top: 3px;
  width: 18px;
  height: 18px;
  border-radius: 999px;
  background: var(--bg);
  box-shadow: 0 1px 3px rgb(0 0 0 / .25);
  transform-origin: 0 50%;
  transition: transform 180ms var(--m-ease);
}
.m-switch[aria-checked="true"] {
  background: var(--action);
}
.m-switch[aria-checked="true"]::before {
  transform: translateX(16px);
  background: var(--action-ink);
}
.m-switch:active::before {
  transform: scaleX(1.22);
  transition-duration: var(--m-press);
}
.m-switch[aria-checked="true"]:active::before {
  transform: translateX(12px) scaleX(1.22);
}
@media (hover: hover) and (pointer: fine) {
  .m-switch:hover:not([aria-checked="true"]) {
    background: color-mix(in oklab, var(--ink) 58%, var(--bg));
  }
}
.m-switch:focus-visible {
  outline: 2px solid var(--action);
  outline-offset: 2px;
}

.field:has(.m-switch) {
  flex-direction: row;
  align-items: center;
  gap: var(--s-3);
}
.field:has(.m-switch) .field__label {
  flex: 1;
  margin: 0;
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

/* Link (motion.css underline animation) */
.link {
  text-decoration: none;
  background-image: linear-gradient(currentColor, currentColor), linear-gradient(var(--line-strong), var(--line-strong));
  background-repeat: no-repeat;
  background-size: 0% 1px, 100% 1px;
  background-position: right 0 bottom 0, 0 100%;
  transition: background-size var(--m-out) var(--m-ease), color var(--m-out) var(--m-ease);
  padding-bottom: 2px;
}
.link:hover,
.link:focus-visible {
  background-size: 100% 1px, 100% 1px;
  background-position: left 0 bottom 0, 0 100%;
  transition-duration: var(--m-in);
}

/* Card loops animation */
@media (prefers-reduced-motion: no-preference) {
  .settings-grid .panel {
    animation: card-loop 3.2s ease-in-out infinite;
  }
  .settings-grid .panel:nth-child(2) { animation-delay: 0.8s; }
  .settings-grid .panel:nth-child(3) { animation-delay: 1.6s; }
}

@keyframes card-loop {
  0%, 100% { opacity: 1; transform: translateY(0); }
  50% { opacity: 0.9; transform: translateY(-4px); }
}

.settings-grid .panel:hover {
  animation-play-state: paused;
}

/* Moment */
.moment--pinned-chapters {
  border-radius: var(--r-lg);
  overflow: hidden;
  background: var(--surface);
  border: 1px solid var(--border);
}
.moment--pinned-chapters .stage {
  aspect-ratio: 16 / 9;
  min-height: 200px;
}
</style>