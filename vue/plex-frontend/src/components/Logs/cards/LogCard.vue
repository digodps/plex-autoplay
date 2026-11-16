<script setup>
import { computed } from 'vue'
import LogCardHeader from './LogCardHeader.vue'
import LogCardContent from './LogCardContent.vue'

const props = defineProps({
  log: {
    type: Object,
    required: true,
  },
  index: {
    type: Number,
    required: true,
  },
  isExpanded: {
    type: Boolean,
    required: true,
  },
  onToggle: {
    type: Function,
    required: true,
  },
})

const contentId = `log-card-content-${props.log.timestamp}`
const headerId = `log-card-heading-${props.log.timestamp}`
const buttonId = `log-card-button-${props.log.timestamp}`

const cardClasses = computed(() => [
  'log-card',
  'relative',
  'overflow-hidden',
  'rounded-xl',
  'border',
  'border-gray-800',
  'bg-[#1f2937]',
  'shadow-xl',
  'transition-all',
  'duration-300',
  'ease-out',
  'hover:shadow-2xl',
  'hover:-translate-y-0.5',
  'animate-in',
  'fade-in',
  'slide-in-from-bottom-4',
  'duration-300',
  { 'ring-2 ring-accent/50': props.isExpanded },
])

const contentStyle = computed(() => ({
  maxHeight: props.isExpanded ? '2000px' : '0',
  opacity: props.isExpanded ? 1 : 0,
  padding: props.isExpanded ? '0 24px 24px' : '0 24px',
  transition: 'max-height 300ms ease-out, opacity 200ms ease-out, padding 300ms ease-out',
}))
</script>

<template>
  <article
    :class="cardClasses"
    :style="`--stagger-delay: ${index * 50}ms`"
    role="region"
    :aria-labelledby="headerId"
    :data-testid="`log-card-${index}`"
  >
    <!-- Floating glow effect on hover -->
    <div class="absolute inset-0 rounded-xl bg-gradient-to-b from-transparent to-black/20 opacity-0 transition-opacity duration-300 pointer-events-none" aria-hidden="true"></div>

    <LogCardHeader
      :log="log"
      :is-expanded="props.isExpanded"
      :on-toggle="onToggle"
      :id="buttonId"
    />

    <!-- Expanded Content with smooth height animation -->
    <div
      :id="contentId"
      role="region"
      :aria-labelledby="headerId"
      class="log-card-content-wrapper overflow-hidden"
      :style="contentStyle"
    >
      <LogCardContent :log="log" v-show="props.isExpanded" />
    </div>
  </article>
</template>

<style scoped>
/* Staggered entrance animation via CSS custom property */
.log-card {
  animation-delay: var(--stagger-delay, 0ms);
}

/* Smooth expand/collapse using max-height trick */
.log-card-content-wrapper {
  /* Using max-height for smooth animation */
  /* 2000px is a safe max for expanded content */
}

/* Hover glow effect */
.log-card:hover > div:first-child {
  opacity: 1;
}

/* Focus ring for keyboard navigation */
.log-card:focus-within {
  box-shadow: 0 0 0 2px var(--color-accent), 0 25px 50px -12px rgb(0 0 0 / 0.5);
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .log-card,
  .log-card-content-wrapper,
  .log-card > div:first-child {
    animation: none !important;
    transition: none !important;
  }
}

/* Dark theme adjustments */
.dark .log-card {
  background-color: #1f2937;
  border-color: #374151;
}

.dark .log-card:hover {
  box-shadow: 0 25px 50px -12px rgb(0 0 0 / 0.5);
}
</style>