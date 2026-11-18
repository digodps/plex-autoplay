import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { SUPPORTED_LOCALES, LOCALE_INFO, DEFAULT_LOCALE, getStoredLocale, setStoredLocale, loadAndSetLocale } from '@/i18n';

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

  async function init() {
    if (isInitialized.value) return;

    const stored = getStoredLocale();
    let initialLocale;
    if (stored) {
      initialLocale = stored;
    } else {
      initialLocale = DEFAULT_LOCALE;
      // Persist default locale so subsequent visits don't re-detect
      setStoredLocale(initialLocale);
    }

    locale.value = initialLocale;
    await loadAndSetLocale(initialLocale);
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