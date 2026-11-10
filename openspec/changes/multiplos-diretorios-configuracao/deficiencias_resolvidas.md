# Deficiências resolvidas — multiplos-diretorios-configuracao

## EGM — Goldfish Review A/S/C

Requisitos revisados: página `/ajustes` usar `MediaRootsField` para múltiplos `media_roots`, backend já suporta `media_roots` array.

## Achados aceitos

| # | Revisor | Achado | Resolução |
|---|---------|--------|-----------|
| A1 | Arquitetura | Backend já aceita `media_roots` mas UI envia `media_root` | Frontend muda para `media_roots` array; nenhum breaking no contrato |
| A2 | Arquitetura | `MediaRootsField` já usado em `SetupView` — reutilizar | Importar componente existente; sem duplicação |
| S1 | SOLID | `SettingsView` mistura estado do form com regras de UI | Extrair `mediaRoots` ref separado; form fica só outros campos |
| C1 | Clean Code | Input único `media_root` é código morto (backend ignora) | Remover completamente; substituir por componente |

## Achados parciais

| # | Revisor | Achado | Resolução |
|---|---------|--------|-----------|
| A3 | Arquitetura | `SettingsRequest` já tem `media_roots` mas `form` usa singular | Ajustar payload; schema não muda |

## Achados rejeitados

| # | Revisor | Achado | Motivo |
|---|---------|--------|--------|
| A4 | Arquitetura | Deveria criar `AppSettingsViewModel` | YAGNI — form simples não justifica DTO adicional |
| C2 | Clean Code | Deveria validar existência do dir no frontend | Backend já valida; duplicação desnecessária |