using System.Text.Json;

namespace PlexAutoplay.Application;

/// <summary>
/// Regras únicas da lista de raízes de mídia e da URL do Plex.
/// Usado pelo setup inicial, pela página de Ajustes e pela validação de identidade.
/// </summary>
public static class MediaRoots
{
    public const int MaxRoots = 32;

    /// <summary>
    /// Canonicaliza (Path.GetFullPath, sem barra final), exige diretório absoluto
    /// existente, remove duplicatas e rejeita "/". Erros prefixados com "media_roots:".
    /// </summary>
    public static (IReadOnlyList<string> Roots, IReadOnlyList<string> Errors) Normalize(IEnumerable<string>? roots)
    {
        var errors = new List<string>();
        var result = new List<string>();
        var items = (roots ?? []).Where(root => !string.IsNullOrWhiteSpace(root)).ToList();

        if (items.Count == 0)
        {
            errors.Add("media_roots: informe ao menos um diretório");
            return (result, errors);
        }

        if (items.Count > MaxRoots)
        {
            errors.Add($"media_roots: máximo de {MaxRoots} diretórios");
            return (result, errors);
        }

        foreach (var raw in items)
        {
            var trimmed = raw.Trim();
            if (!Path.IsPathRooted(trimmed))
            {
                errors.Add($"media_roots: {trimmed} não é um diretório absoluto existente");
                continue;
            }

            var full = Path.GetFullPath(trimmed);
            if (full.Length > 1)
            {
                full = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            if (full.Length == 0 || full == "/")
            {
                errors.Add("media_roots: / não é um diretório de mídia válido");
                continue;
            }

            if (!Directory.Exists(full))
            {
                errors.Add($"media_roots: {trimmed} não é um diretório absoluto existente");
                continue;
            }

            if (!result.Contains(full, StringComparer.Ordinal))
            {
                result.Add(full);
            }
        }

        return (result, errors);
    }

    public static string Serialize(IEnumerable<string> roots) => JsonSerializer.Serialize(roots.ToArray());

    /// <summary>JSON array; qualquer valor inválido vira lista vazia.</summary>
    public static IReadOnlyList<string> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    /// <summary>Aceita JSON array ou lista separada por \n, "," ou ":".</summary>
    public static IReadOnlyList<string> ParseEnvironment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith('['))
        {
            return Parse(trimmed);
        }

        return trimmed
            .Split(['\n', '\r', ',', ':'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }
}

/// <summary>Validação única da URL do Plex (http/https absoluta, sem barra final).</summary>
public static class PlexUrlRules
{
    public static (string Url, string? Error) Normalize(string? value)
    {
        var url = (value ?? "").Trim().TrimEnd('/');
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return (url, "plex_url: deve ser uma URL absoluta http:// ou https://");
        }

        return (url, null);
    }
}
