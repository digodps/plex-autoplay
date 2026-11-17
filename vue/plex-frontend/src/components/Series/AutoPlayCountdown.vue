<script setup lang="ts">
/* global Event, KeyboardEvent */
import { ref, onMounted, onUnmounted, computed } from "vue";
import AppIcon from "@/components/AppIcon.vue";

interface Props {
  seconds: number;
  nextEpisodeTitle: string;
}

const props = defineProps<Props>();
const emit = defineEmits<{ cancel: [] }>();

const remaining = ref(props.seconds);
const progress = computed(() => (remaining.value / props.seconds) * 100);
const circumference = 2 * Math.PI * 45; // r=45
const strokeDashoffset = computed(() => circumference * (1 - progress.value / 100));

let intervalId: ReturnType<typeof setInterval> | null = null;

function start() {
  intervalId = setInterval(() => {
    remaining.value -= 1;
    if (remaining.value <= 0) {
      stop();
    }
  }, 1000);
}

function stop() {
  if (intervalId) {
    clearInterval(intervalId);
    intervalId = null;
  }
}

function handleCancel() {
  stop();
  emit("cancel");
}

function handleKeydown(e: Event) {
  if ((e as KeyboardEvent).key === "Escape") {
    handleCancel();
  }
}

onMounted(() => {
  start();
  document.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
  stop();
  document.removeEventListener("keydown", handleKeydown);
});
</script>

<template>
  <div
    class="autoplay-countdown"
    role="dialog"
    aria-modal="true"
    aria-label="Auto-play countdown"
    aria-live="polite"
    @keydown="handleKeydown"
  >
    <div class="autoplay-countdown__overlay"></div>

    <div class="autoplay-countdown__content">
      <div class="autoplay-countdown__timer" aria-hidden="true">
        <svg class="autoplay-countdown__svg" viewBox="0 0 100 100">
          <circle
            class="autoplay-countdown__track"
            cx="50"
            cy="50"
            r="45"
            fill="none"
            stroke="currentColor"
            stroke-width="8"
          />
          <circle
            class="autoplay-countdown__progress"
            cx="50"
            cy="50"
            r="45"
            fill="none"
            stroke="currentColor"
            stroke-width="8"
            stroke-linecap="round"
            :stroke-dasharray="circumference"
            :stroke-dashoffset="strokeDashoffset"
            style="transform: rotate(-90deg); transform-origin: 50% 50%;"
          />
        </svg>
        <span class="autoplay-countdown__seconds">{{ remaining }}</span>
      </div>

      <p class="autoplay-countdown__message">
        Próximo episódio inicia em
      </p>
      <h3 class="autoplay-countdown__next-title">{{ nextEpisodeTitle }}</h3>

      <button
        class="autoplay-countdown__cancel btn btn-ghost"
        aria-label="Cancelar auto-play"
        @click="handleCancel"
      >
        <AppIcon name="x" size="sm" />
        Cancelar
      </button>
    </div>
  </div>
</template>

<style scoped>
.autoplay-countdown {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  background: color-mix(in srgb, var(--bg) 95%, transparent);
  backdrop-filter: blur(4px);
}

.autoplay-countdown__overlay {
  position: absolute;
  inset: 0;
  background: color-mix(in srgb, var(--bg) 80%, transparent);
}

.autoplay-countdown__content {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1.5rem;
  padding: 2.5rem;
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-2);
  text-align: center;
  min-width: 320px;
  max-width: 90vw;
}

.autoplay-countdown__timer {
  position: relative;
  width: 100px;
  height: 100px;
  color: var(--accent);
}

.autoplay-countdown__svg {
  width: 100%;
  height: 100%;
  transform: rotate(-90deg);
}

.autoplay-countdown__track {
  opacity: 0.2;
}

.autoplay-countdown__progress {
  transition: stroke-dashoffset 1s linear;
}

.autoplay-countdown__seconds {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  font-weight: 700;
  font-family: var(--mono);
  color: var(--text);
}

.autoplay-countdown__message {
  margin: 0;
  font-size: 1rem;
  color: var(--text-secondary);
}

.autoplay-countdown__next-title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
  color: var(--text);
  max-width: 280px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.autoplay-countdown__cancel {
  margin-top: 0.5rem;
}
</style>
