<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useLocaleStore } from '@/stores/locale';
import AppIcon from '@/components/AppIcon.vue';

const localeStore = useLocaleStore();

const isOpen = ref(false);
const buttonRef = ref(null);
const listRef = ref(null);
const highlightedIndex = ref(0);

const currentLocale = computed(() => localeStore.currentLocaleInfo);
const availableLocales = computed(() => localeStore.availableLocales);

function toggle() {
  isOpen.value = !isOpen.value;
  if (isOpen.value) {
    highlightedIndex.value = availableLocales.value.findIndex(l => l.code === localeStore.locale);
    nextTick(() => {
      const items = listRef.value?.querySelectorAll('[role="option"]');
      items?.[highlightedIndex.value]?.focus();
    });
  }
}

function selectLocale(code) {
  localeStore.setLocale(code);
  isOpen.value = false;
}

function close() {
  isOpen.value = false;
}

function handleKeydown(event) {
  const options = availableLocales.value;
  const maxIndex = options.length - 1;

  switch (event.key) {
    case 'ArrowDown':
      event.preventDefault();
      highlightedIndex.value = Math.min(highlightedIndex.value + 1, maxIndex);
      break;
    case 'ArrowUp':
      event.preventDefault();
      highlightedIndex.value = Math.max(highlightedIndex.value - 1, 0);
      break;
    case 'Enter':
    case ' ':
      event.preventDefault();
      selectLocale(options[highlightedIndex.value].code);
      break;
    case 'Escape':
      close();
      buttonRef.value?.focus();
      break;
    case 'Tab':
      close();
      break;
  }
}

function handleClickOutside(event) {
  if (buttonRef.value && !buttonRef.value.contains(event.target)) {
    if (listRef.value && !listRef.value.contains(event.target)) {
      close();
    }
  }
}

onMounted(() => {
  document.addEventListener('click', handleClickOutside);
});

watch(() => isOpen.value, (open) => {
  if (open) {
    document.addEventListener('keydown', handleKeydown);
  } else {
    document.removeEventListener('keydown', handleKeydown);
  }
});

import { nextTick } from 'vue';
</script>

<template>
  <div class="relative">
    <button
      type="button"
      ref="buttonRef"
      :aria-expanded="isOpen"
      :aria-haspopup="listbox"
      :aria-label="$t('languageSelector.label')"
      class="flex items-center gap-2 rounded-lg p-2 text-gray-700 hover:text-primary hover:bg-gray-100 dark:text-gray-300 dark:hover:bg-gray-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary transition-colors"
      data-testid="language-selector-button"
      @click="toggle"
    >
      <span class="text-lg" aria-hidden="true">{{ currentLocale.flag }}</span>
      <span class="hidden sm:inline font-medium">{{ currentLocale.label }}</span>
      <AppIcon name="chevron-down" size="sm" class="text-gray-500" />
    </button>

    <div
      v-show="isOpen"
      ref="listRef"
      role="listbox"
      aria-label="Idiomas disponíveis"
      class="absolute right-0 mt-2 w-40 sm:w-48 rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 shadow-lg py-1 z-50 animate-fade-in"
      data-testid="language-selector-dropdown"
    >
      <div
        v-for="(locale, index) in availableLocales"
        :key="locale.code"
        role="option"
        :aria-selected="locale.code === localeStore.locale"
        :tabindex="index === highlightedIndex ? 0 : -1"
        class="flex items-center gap-3 px-3 py-2 text-sm text-gray-900 dark:text-gray-100 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer focus-visible:outline-none focus-visible:bg-gray-100 dark:focus-visible:bg-gray-700"
        data-testid="language-option"
        @click="selectLocale(locale.code)"
        @focus="highlightedIndex = index"
      >
        <span class="text-lg" aria-hidden="true">{{ locale.flag }}</span>
        <span class="font-medium">{{ locale.label }}</span>
        <AppIcon
          v-if="locale.code === localeStore.locale"
          name="check"
          size="sm"
          class="ml-auto text-primary"
          aria-hidden="true"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes fade-in {
  from {
    opacity: 0;
    transform: translateY(-4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in {
  animation: fade-in 120ms ease-out;
}
</style>