# Spec: Globalization (i18n) — Português (Brasil), Inglês, Espanhol

## Resumo
Adicionar suporte a internacionalização (i18n) na aplicação Vue 3 com três idiomas: Português do Brasil (padrão), Inglês e Espanhol. Incluir seletor de idioma no cabeçalho com bandeira do país, persistência da preferência, e tradução de todas as páginas, componentes e mensagens.

## Requisitos Funcionais

### RF-01: Configuração i18n
- Usar `vue-i18n` v11+ (Composition API)
- Criar arquivos de tradução separados por idioma:
  - `src/locales/pt-BR.json` (padrão)
  - `src/locales/en.json`
  - `src/locales/es.json`
- Configurar fallback para `pt-BR`
- Detecção automática de idioma do navegador na primeira visita

### RF-02: Seletor de idioma no cabeçalho
- Botão dropdown no `AppHeader` ao lado do botão de tema
- Exibir bandeira do país + nome do idioma (ex: 🇧🇷 Português)
- Opções: Português (🇧🇷), English (🇺🇸), Español (🇪🇸)
- Persistir escolha no `localStorage` (chave: `app.locale`)
- Aplicar mudança imediatamente sem recarregar página

### RF-03: Tradução completa
Todas as strings visíveis ao usuário devem ser externalizadas:
- **Páginas**: Login, Setup, Dashboard, Libraries, Series, SeriesDetail, MarkerEditor, Logs, Sync, Settings, Sessions
- **Componentes**: AppHeader, AppLogo, StatusCard, LibraryMosaic, GenreCarousel, MarkerRow, PasswordModal, CardDataStats, LogFilterBar, LogCarouselView, ActionBar, HelloWorld, StatusCardSkeleton
- **Stores**: Mensagens de erro, toasts, validações
- **Composables**: useSeriesCatalog, etc.
- **Rotas**: meta.title traduzido

### RF-04: Persistência e detecção
- Salvar locale escolhido no `localStorage`
- Na primeira visita: detectar `navigator.language` e mapear para idiomas suportados
- Fallback: `pt-BR`
- Sincronizar com backend se usuário logado (opcional, fase futura)

### RF-05: Acessibilidade
- Botão de idioma com `aria-label` descritivo
- Dropdown navegável por teclado (Tab, Enter, Escape)
- `lang` attribute no `<html>` atualizado dinamicamente

## Requisitos Não-Funcionais
- **Performance**: Carregamento preguiçoso (lazy) dos arquivos de locale não-padrão
- **Bundle**: Não aumentar bundle inicial significativamente
- **TypeScript**: Tipos para chaves de tradução (opcional, via `vue-i18n` types)

## Fora do Escopo
- Tradução de mensagens de API/backend
- Formatação de datas/números/moedas (pode ser adicionado depois)
- RTL (Right-to-Left) — não necessário para idiomas alvo
- Pluralização complexa — usar sintaxe padrão do vue-i18n

## Critérios de Aceite
1. ✅ Aplicação carrega em `pt-BR` por padrão
2. ✅ Botão de idioma visível no header com bandeiras
3. ✅ Troca de idioma funciona instantaneamente
4. ✅ Preferência persiste ao recarregar página
5. ✅ Todas as páginas/views traduzidas (sem strings hardcoded em pt-BR)
6. ✅ Todos os componentes traduzidos
7. ✅ Meta titles das rotas traduzidos
8. ✅ Atributo `lang` do `<html>` atualizado
9. ✅ Acessibilidade: teclado e screen readers funcionam
10. ✅ Build passa (`npm run build`)
11. ✅ Lint passa (`npm run lint`)
12. ✅ QA UI: tema light/dark + menus funcionando com Playwright