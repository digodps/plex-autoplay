# Spec Delta: Atualizar Testes

## Purpose
Atualizar todos os testes da solução para refletir as alterações recentes implementadas no backend (.NET), frontend (Vue 3) e testes de aceitação (Playwright).

## MODIFIED Requirements

### Requirement: Unit tests match current implementation
System SHALL provide updated unit tests for backend (.NET xUnit) and frontend (Vitest/Node) that pass successfully against current code.

#### Scenario: Running .NET unit tests
- **WHEN** user executes `.NET` test suite (`dotnet test`)
- **THEN** all unit tests pass successfully without failure

#### Scenario: Running frontend tests
- **WHEN** user executes frontend tests
- **THEN** all tests pass successfully without failure
