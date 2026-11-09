# Deficiências resolvidas — EGM Goldfish Review (A/S/C)

## Revisor A — Arquitetura, dependências e integração

**A-1. Detecção baseada em `product/platform/version` do `<Player>` — ACEITADO**
- Usa dados já retornados por `/status/sessions` do PMS; não exige nova dependência de rede nem config do usuário.
- Riscos: nomes de `product` podem mudar entre versões do Plex; mitigação no design usa substring (`contains`) para Smart TVs e exato para legacy, com `Unknown` como fallback best-effort.

**A-2. `PlexModernClientDetector` separado do serviço principal — ACEITADO**
- Separação de responsabilidades: detecção de categoria é responsabilidade própria; `PlexAutoSkipBackgroundService` apenas consome a estratégia.
- Sugestão: expor via DI (`IClientSeekStrategyProvider`) para facilitar testes unitários sem mockar HTTP. — **Parcial, aplicar na implementação.**

**A-3. PMS proxy como estratégia padrão para clientes modernos — ACEITADO**
- Mantém integração existente (`X-Plex-Target-Client-Identifier`), não quebra clientes LAN.
- Limitação: sessões remotas (não LAN) já são escopo fora do projeto (documentado).

**A-4. `CommandID` em todos os seeks — ACEITADO**
- Plex exige `commandID` para deduplicação; já parcialmente implementado, agora generalizado para proxy também.

**A-5. Manter conexão direta primeiro para `LegacyCompanion` — ACEITADO**
- Preserva comportamento atual para Android TV/Mobile, Apple TV, Roku.

**Veredito:** nenhum achado bloqueante. Prossiga.

## Revisor S — SOLID e coesão

**S-1. SRP — ACEITADO**
- `PlexModernClientDetector` tem única responsabilidade: classificar cliente e retornar estratégia de seek.

**S-2. OCP — ACEITADO (parcial)**
- Novas categorias de cliente podem ser adicionadas no `switch` sem tocar no serviço. Porém `switch` centralizado no detector viola OCP estrito; aceitável para escopo pequeno.

**S-3. LSP — N/A (não há herança)**

**S-4. ISP — ACEITADO**
- Contrato pequeno: `Detect(product, platform, version)`.

**S-5. DIP — REJEITADO (ajustar)**
- Design prevê `IClientSeekStrategyProvider` mas não especifica injeção no serviço. Se `PlexModernClientDetector` for instância estática (`static`), o serviço fica acoplado concretamente e não é testável por DI.
- **Ação:** injetar abstração (`IClientSeekStrategyProvider`) em `PlexAutoSkipBackgroundService` ou permitir substituição por método virtual/factory.

**S-6. Coesão — ACEITADO**
- `SeekToEndOfMarkerAsync` fica responsável apenas por orquestrar estratégias; lógica de classificação fica no detector.

**Veredito:** 1 achado (S-5) exige ajuste na implementação (injeção de abstração).

## Revisor C — Clean Code, escopo e testabilidade

**C-1. Remoção de `IsBrokenClient`/`VersionCompare` — ACEITADO**
- Elimina hardcoding de versões quebradas, substituto por detecção por categoria. Melhora manutenibilidade.

**C-2. `IsSeekSuccess(HttpResponseMessage)` — ACEITADO**
- Centraliza tratamento de respostas não-XML (`OK`, XML parcial, 204) que variam por cliente (Samsung, Android, Plexamp).

**C-3. Escopo limitado ao backend — ACEITADO**
- Não mexe no frontend Vue nem em contratos de API públicos. Não há requirement de WebSocket client-side (players oficiais não conectam ao `/ws`).

**C-4. Testes planejados — ACEITADO**
- `tasks.md` lista testes xunit para detecção e `IsSeekSuccess`. Necessário criar project de teste ou adicionar ao `ArchitectureTests` existente (referencia Infrastructure).

**C-5. `CommandID` único — ACEITADO**
- Evita seeks duplicados; boa prática.

**C-6. Comentários/área cinza — PARCIAL**
- `PlexSessionInfo` pode precisar de campos `Device`/`Model` extraídos de `<Player>`; verificar se XML já expõe (não expõe atualmente). Decisão: não exigir campos novos, usar só `product/platform/version` já disponíveis.

**Veredito:** nenhum achado bloqueante. C-6 é documentação de decisão.

---

## Resumo de ações obrigatórias antes da implementação

1. Injetar abstração de estratégia de cliente em `PlexAutoSkipBackgroundService` (atende S-5).
2. Não adicionar campos `Device`/`Model` a `PlexSessionInfo` (atende C-6).
3. Manter escopo backend-only (atende C-3).

## Armazenado em
`openspec/changes/autoskip-clientes-modernos/deficiencias_resolvidas.md`
