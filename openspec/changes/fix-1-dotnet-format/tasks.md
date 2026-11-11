# Tasks

## 1. Correção do achado [1]

- [x] 1.1 Verificado com `dotnet format PlexAutoplay.slnx --verify-no-changes` e `python scripts/code-review-skill.py`: nenhum ajuste necessário nos arquivos `src/PlexAutoplay.Web/Configuration/ServerCertificateProvider.cs` e `src/PlexAutoplay.Web/Program.cs`; achado não é reportado.

## Workflow follow-up

- Validar com `openspec validate --change fix-1-dotnet-format` antes de seguir para archive/apply.
