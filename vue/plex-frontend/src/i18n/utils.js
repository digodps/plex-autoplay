// i18n utilities: locale detection, lazy loading, mappings

export const SUPPORTED_LOCALES = ['pt-BR', 'en', 'es'];

export const LOCALE_INFO = {
  'pt-BR': { label: 'Português', flag: '🇧🇷', icon: 'flag' },
  'en': { label: 'English', flag: '🇺🇸', icon: 'flag' },
  'es': { label: 'Español', flag: '🇪🇸', icon: 'flag' },
};

export const DEFAULT_LOCALE = 'pt-BR';
export const STORAGE_KEY = 'app.locale';

const LOCALE_MAP = {
  'pt': 'pt-BR',
  'pt-BR': 'pt-BR',
  'pt-PT': 'pt-BR',
  'en': 'en',
  'en-US': 'en',
  'en-GB': 'en',
  'es': 'es',
  'es-ES': 'es',
  'es-MX': 'es',
  'es-AR': 'es',
  'es-CO': 'es',
};

export function detectLocale() {
  if (typeof navigator === 'undefined') return DEFAULT_LOCALE;

  const lang = navigator.language || navigator.languages?.[0] || '';
  const primary = lang.split('-')[0].toLowerCase();

  // Try exact match first
  if (LOCALE_MAP[lang]) return LOCALE_MAP[lang];
  // Then try primary language
  if (LOCALE_MAP[primary]) return LOCALE_MAP[primary];

  return DEFAULT_LOCALE;
}

export function getStoredLocale() {
  if (typeof localStorage === 'undefined') return null;
  const stored = localStorage.getItem(STORAGE_KEY);
  if (stored && SUPPORTED_LOCALES.includes(stored)) {
    return stored;
  }
  return null;
}

export function setStoredLocale(locale) {
  if (typeof localStorage !== 'undefined') {
    localStorage.setItem(STORAGE_KEY, locale);
  }
}

// Lazy loading cache - only for non-default locales
const localeCache = {
  'en': null,
  'es': null,
};

const LOCALE_FILES = {
  'en': () => import('./locales/en.json'),
  'es': () => import('./locales/es.json'),
};

export async function loadLocaleMessages(locale) {
  if (locale === 'pt-BR') {
    return {}; // loaded eagerly in index.js
  }

  if (localeCache[locale]) {
    return localeCache[locale];
  }

  const loader = LOCALE_FILES[locale];
  if (!loader) {
    console.warn(`Unknown locale ${locale}, falling back to ${DEFAULT_LOCALE}`);
    return {};
  }

  try {
    const module = await loader();
    const messages = module.default || module;
    localeCache[locale] = messages;
    return messages;
  } catch (error) {
    console.warn(`Failed to load locale ${locale}, falling back to ${DEFAULT_LOCALE}`, error);
    return {};
  }
}

export function preloadLocale(locale) {
  if (locale !== 'pt-BR' && !localeCache[locale] && LOCALE_FILES[locale]) {
    loadLocaleMessages(locale).catch(() => {});
  }
}

export function applyDocumentLang(locale) {
  if (typeof document !== 'undefined') {
    document.documentElement.lang = locale;
  }
}