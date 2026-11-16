import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { SUPPORTED_LOCALES, LOCALE_INFO, DEFAULT_LOCALE, getStoredLocale, setStoredLocale, detectLocale, loadAndSetLocale } from '@/i18n';

export const useLocaleStore = defineStore('locale', () => {
  const locale = ref(DEFAULT_LOCALE);
  const isInitialized = ref(false);

  const availableLocales = computed(() =>
    SUPPORTED_LOCALES.map(code => ({
      code,
      label: LOCALE_INFO[code].label,
      flag: LOCALE_INFO[code].flag,
    }))
  );

  const currentLocaleInfo = computed(() => LOCALE_INFO[locale.value]);

  function setLocale(newLocale) {
    if (!SUPPORTED_LOCALES.includes(newLocale)) return;

    locale.value = newLocale;
    setStoredLocale(newLocale);
    loadAndSetLocale(newLocale);
  }

  function init() {
    if (isInitialized.value) return;

    const stored = getStoredLocale();
    let initialLocale;
    if (stored) {
      initialLocale = stored;
    } else {
      initialLocale = detectLocale();
      // Don't persist auto-detected locale to localStorage
      // User must explicitly choose a locale to persist it
    }

    locale.value = initialLocale;
    if (stored) {
      setStoredLocale(initialLocale);
    }
    loadAndSetLocale(initialLocale);
    isInitialized.value = true;
  }

  return {
    locale,
    isInitialized,
    availableLocales,
    currentLocaleInfo,
    setLocale,
    init,
  };
});