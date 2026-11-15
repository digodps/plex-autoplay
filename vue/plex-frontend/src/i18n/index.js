import { createI18n } from 'vue-i18n';
import {
  DEFAULT_LOCALE,
  SUPPORTED_LOCALES,
  LOCALE_INFO,
  loadLocaleMessages,
  applyDocumentLang,
  getStoredLocale,
  setStoredLocale,
  detectLocale,
} from './utils';

// Load default locale eagerly
import ptBR from './locales/pt-BR.json';

// Re-export utilities for consumers
export {
  DEFAULT_LOCALE,
  SUPPORTED_LOCALES,
  LOCALE_INFO,
  loadLocaleMessages,
  applyDocumentLang,
  getStoredLocale,
  setStoredLocale,
  detectLocale,
};

const i18n = createI18n({
  legacy: false, // Use Composition API
  locale: DEFAULT_LOCALE,
  fallbackLocale: DEFAULT_LOCALE,
  messages: {
    'pt-BR': ptBR,
  },
  availableLocales: SUPPORTED_LOCALES,
  sync: true, // Sync with document.documentElement.lang
  silentTranslationWarn: true,
  silentFallbackWarn: true,
});

export async function loadAndSetLocale(locale) {
  if (!SUPPORTED_LOCALES.includes(locale)) {
    locale = DEFAULT_LOCALE;
  }

  if (locale !== DEFAULT_LOCALE && !i18n.global.availableLocales.includes(locale)) {
    const messages = await loadLocaleMessages(locale);
    i18n.global.setLocaleMessage(locale, messages);
  }

  i18n.global.locale.value = locale;
  applyDocumentLang(locale);
  return locale;
}

export function getCurrentLocale() {
  return i18n.global.locale.value;
}

export function getAvailableLocales() {
  return SUPPORTED_LOCALES.map(code => ({
    code,
    label: i18n.global.t(`languageSelector.${code}`) || code,
    flag: code === 'pt-BR' ? '🇧🇷' : code === 'en' ? '🇺🇸' : '🇪🇸',
  }));
}

export default i18n;