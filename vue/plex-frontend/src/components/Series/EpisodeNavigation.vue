<script setup lang="ts">
import { computed } from "vue";
import AppIcon from "@/components/AppIcon.vue";

interface Props {
  hasPrevious: boolean;
  hasNext: boolean;
  previousTitle?: string;
  nextTitle?: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{ previous: []; next: [] }>();

const showPreviousTooltip = ref(false);
const showNextTooltip = ref(false);

function handlePrevious() {
  if (props.hasPrevious) emit("previous");
}

function handleNext() {
  if (props.hasNext) emit("next");
}

function handleKeydown(e: KeyboardEvent, direction: "previous" | "next") {
  if ((direction === "previous" && e.key === "ArrowLeft") || (direction === "next" && e.key === "ArrowRight")) {
    if (direction === "previous" && props.hasPrevious) emit("previous");
    if (direction === "next" && props.hasNext) emit("next");
  }
}
</script>

<template>
  <div class="episode-navigation" role="group" aria-label="Navegação de episódios">
    <button
      class="episode-navigation__btn"
      :class="{ 'episode-navigation__btn--disabled': !hasPrevious }"
      :disabled="!hasPrevious"
      @click="handlePrevious"
      @keydown="($event) => handleKeydown($event, 'previous')"
      aria-label="Episódio anterior"
      :title="previousTitle || 'Episódio anterior (←)'"
    >
      <AppIcon name="chevron-left" size="md" />
      <span class="episode-navigation__label">Anterior</span>
      <span v-if="previousTitle" class="episode-navigation__tooltip" :style="{ visibility: showPreviousTooltip ? 'visible' : 'hidden' }">
        {{ previousTitle }}
      </span>
    </button>

    <button
      class="episode-navigation__btn"
      :class="{ 'episode-navigation__btn--disabled': !hasNext }"
      :disabled="!hasNext"
      @click="handleNext"
      @keydown="($event) => handleKeydown($event, 'next')"
      aria-label="Próximo episódio"
      :title="nextTitle || 'Próximo episódio (→)'"
    >
      <span class="episode-navigation__label">Próximo</span>
      <AppIcon name="chevron-right" size="md" />
      <span v-if="nextTitle" class="episode-navigation__tooltip" :style="{ visibility: showNextTooltip ? 'visible' : 'hidden' }">
        {{ nextTitle }}
      </span>
    </button>
  </div>
</template>

<style scoped>
.episode-navigation {
  display: flex;
  gap: 0.5rem;
}

.episode-navigation__btn {
  position: relative;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--surface);
  color: var(--text);
  font: inherit;
  font-weight: 500;
  cursor: pointer;
  transition: background var(--speed) var(--ease), border-color var(--speed) var(--ease), color var(--speed) var(--ease);
}

.episode-navigation__btn:hover:not(.episode-navigation__btn--disabled) {
  background: var(--bg-sunken);
  border-color: var(--border-strong);
}

.episode-navigation__btn:focus-visible {
  outline: 2px solid var(--accent);
  outline-offset: 2px;
}

.episode-navigation__btn--disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.episode-navigation__label {
  font-size: 0.85rem;
}

.episode-navigation__tooltip {
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  margin-bottom: 0.5rem;
  padding: 0.35rem 0.6rem;
  background: var(--text);
  color: var(--bg);
  font-size: 0.7rem;
  font-weight: 500;
  border-radius: var(--radius-sm);
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity var(--speed) var(--ease);
}

.episode-navigation__tooltip::after {
  content: "";
  position: absolute;
  top: 100%;
  left: 50%;
  transform: translateX(-50%);
  border: 4px solid transparent;
  border-top-color: var(--text);
}

.episode-navigation__btn:hover .episode-navigation__tooltip {
  opacity: 1;
  visibility: visible !important;
}
</style>
