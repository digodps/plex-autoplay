# Design: Globalization (i18n)

## Arquitetura

### Estrutura de arquivos
```
src/
├── i18n/
│   ├── index.js           # Configuração principal do vue-i18n
│   ├── locales/
│   │   ├── pt-BR.json     # Português Brasil (padrão)
│   │   ├── en.json        # Inglês
│   │   └── es.json        # Espanhol
│   └── utils.js           # Helpers: detecção, persistência, lazy loading
├── components/
│   └── LanguageSelector.vue  # Novo componente: seletor no header
├── stores/
│   └── locale.js          # Novo store Pinia para estado do locale
└── router/
    └── index.js           # Atualizar: meta.title via i18n, lang attribute
```

### Fluxo de inicialização
1. `main.js` importa `i18n` e registra no app
2. `i18n/index.js`:
   - Lê `localStorage.getItem('app.locale')`
   - Se não existir: detecta `navigator.language` → mapeia para suportados
   - Carrega locale padrão (`pt-BR`) eager
   - Configura fallback para `pt-BR`
   - Define `document.documentElement.lang = locale`
3. `AppHeader` usa `LanguageSelector` que consome `useLocaleStore()`

### Detecção de idioma
```js
// Mapeamento navigator.language -> nossos códigos
const LOCALE_MAP = {
  'pt': 'pt-BR',
  'pt-BR': 'pt-BR',
  'pt-PT': 'pt-BR',  // fallback
  'en': 'en',
  'en-US': 'en',
  'en-GB': 'en',
  'es': 'es',
  'es-ES': 'es',
  'es-MX': 'es',
};
```

### Lazy loading de locales
- `pt-BR`: import eager (padrão)
- `en`, `es`: dynamic import quando usuário seleciona
- Cache em memória após primeiro carregamento

## Componentes

### LanguageSelector.vue
- Props: nenhum (usa store)
- Estado local: `isOpen` (dropdown)
- Métodos: `selectLocale(code)`, `toggle()`
- A11y: `aria-haspopup="listbox"`, `aria-expanded`, role="listbox", role="option"
- Teclado: Enter/Space abre, ArrowUp/Down navega, Enter seleciona, Escape fecha

### AppHeader.vue (modificação)
- Importar `LanguageSelector`
- Adicionar antes do botão de tema (ordem: breadcrumb → language → theme → user menu)
- Manter responsividade (mobile: dropdown full-width)

## Stores

### locale.js (Pinia)
```js
export const useLocaleStore = defineStore('locale', () => {
  const locale = ref('pt-BR');
  const availableLocales = [
    { code: 'pt-BR', label: 'Português', flag: '🇧🇷' },
    { code: 'en', label: 'English', flag: '🇺🇸' },
    { code: 'es', label: 'Español', flag: '🇪🇸' },
  ];

  function setLocale(code) {
    if (!availableLocales.find(l => l.code === code)) return;
    locale.value = code;
    localStorage.setItem('app.locale', code);
    // i18n.global.locale.value = code; // via watcher ou direto
    document.documentElement.lang = code;
  }

  function init() {
    const saved = localStorage.getItem('app.locale');
    if (saved && availableLocales.find(l => l.code === saved)) {
      setLocale(saved);
    } else {
      const detected = detectLocale();
      setLocale(detected);
    }
  }

  return { locale, availableLocales, setLocale, init };
});
```

## Tradução de rotas (router/index.js)
- `meta.title` virar chave i18n: `meta: { titleKey: 'nav.dashboard' }`
- Navigation guard: `document.title = t(to.meta.titleKey)`
- Atualizar `lang` attribute no `beforeEach` guard

## Convenção de chaves i18n
```
Namespace por feature/página:
- nav.*           → navegação, breadcrumb, títulos de página
- auth.*          → login, setup, change password
- dashboard.*     → dashboard
- libraries.*     → bibliotecas, séries
- series.*        → detalhes da série, marcadores
- markers.*       → editor de marcadores
- logs.*          → logs
- sync.*          → sincronização
- settings.*      → configurações
- sessions.*      → sessões
- common.*        → botões, labels genéricos (salvar, cancelar, etc.)
- errors.*        → mensagens de erro
- aria.*          → labels de acessibilidade
```

## Arquivos de locale (exemplo pt-BR.json)
```json
{
  "nav": {
    "dashboard": "Dashboard",
    "libraries": "Bibliotecas",
    "series": "Séries",
    "markers": "Marcadores",
    "logs": "Logs",
    "sync": "Sincronização",
    "settings": "Configurações",
    "sessions": "Sessões"
  },
  "auth": {
    "login": "Entrar",
    "email": "E-mail",
    "password": "Senha",
    "signIn": "Entrar",
    "setupTitle": "Configuração Inicial",
    "changePassword": "Alterar Senha"
  },
  "common": {
    "save": "Salvar",
    "cancel": "Cancelar",
    "delete": "Excluir",
    "edit": "Editar",
    "loading": "Carregando...",
    "error": "Erro",
    "success": "Sucesso",
    "confirm": "Confirmar",
    "close": "Fechar",
    "language": "Idioma"
  },
  "languageSelector": {
    "label": "Selecionar idioma",
    "pt-BR": "Português",
    "en": "English",
    "es": "Español"
  }
  // ... demais namespaces
}
```

## Integração com Theme Store
- `LanguageSelector` e theme toggle independentes
- Ambos persistem no localStorage
- Ordem no header: [Logo] [Breadcrumb] [Language] [Theme] [User Menu]

## Testes
- Unit: `LanguageSelector.vue` (abre/fecha, seleciona, teclado, a11y)
- Unit: `locale store` (init, setLocale, persistência, detecção)
- E2E Playwright: troca de idioma, persistência, lang attribute, todas as páginas