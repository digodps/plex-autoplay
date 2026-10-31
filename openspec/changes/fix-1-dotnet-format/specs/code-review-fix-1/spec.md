# Spec Delta

## Purpose

Corrigir o achado [1] da revisão de código: dotnet-format detectou diferenças de formatação em arquivos .cs

## ADDED Requirements

### Requirement: Corrigir achado [1]
O sistema SHALL corrigir o problema: dotnet-format detectou diferenças de formatação em arquivos .cs

#### Scenario: Correção aplicada
- **WHEN** a correção é aplicada nos arquivos src/PlexAutoplay.Web/Configuration/ServerCertificateProvider.cs, src/PlexAutoplay.Web/Program.cs
- **THEN** o achado [1] não é mais reportado na próxima execução de /code-review
