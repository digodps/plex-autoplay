using System.Text;
using PlexAutoplay.Application;

namespace PlexAutoplay.Infrastructure.Configuration;

/// <summary>
/// Lê/escreve o .env do container, persistindo PLEX_URL e PLEX_TOKEN.
/// </summary>
public class PlexConfigStore : IPlexConfigStore
{
    private const string EnvFilePath = "/home/hermes/plex-autoplay/.env";

    public async Task SaveAsync(string plexUrl, string plexToken)
    {
        string[] envLines;
        if (File.Exists(EnvFilePath))
        {
            envLines = await File.ReadAllLinesAsync(EnvFilePath);
        }
        else
        {
            // Create default .env if doesn't exist
            envLines = new[]
            {
                "# Plex",
                $"PLEX_URL={plexUrl}",
                $"PLEX_TOKEN={plexToken}",
                "",
                "# Auth",
                "JWT_SECRET=7f3c9b1e5a2d48c6b0e94f17ad63c852e1b47d90f3a6c28e5b1d704f9a3c6e21",
                "ADMIN_PASSWORD=admin",
                "",
                "# Ports",
                "WEB_PORT=8080",
                "WS_PORT=8081",
                "HTTPS_PORT=443",
                "DISABLE_HTTP=false",
                "",
                "# App",
                "ASPNETCORE_ENVIRONMENT=Production",
                "LOG_LEVEL=Information",
                "ALLOWED_HOSTS=*",
                "SYNC_INTERVAL_MINUTES=5",
                "APP_CONFIG_PATH=/data/app-config.json",
                "",
                "# Cert (empty = self-signed)",
                "CERT_PATH=",
                "CERT_PASSWORD=",
                "CERT_BASE64=",
                "",
                "# OTEL",
                "# OTEL_SERVICE_NAME=plex-autoplay"
            };
        }

        // Update PLEX_URL and PLEX_TOKEN
        bool updated = false;
        for (int i = 0; i < envLines.Length; i++)
        {
            if (envLines[i].StartsWith("PLEX_URL="))
            {
                envLines[i] = $"PLEX_URL={plexUrl}";
                updated = true;
            }
            else if (envLines[i].StartsWith("PLEX_TOKEN="))
            {
                envLines[i] = $"PLEX_TOKEN={plexToken ?? ""}";
                updated = true;
            }
        }

        // If .env didn't have PLEX_URL/PLEX_TOKEN, add them before the first empty line after "# Plex"
        if (!updated)
        {
            // Find the "# Plex" section and add at the end of that section
            int plexSectionEnd = 0;
            for (int i = 0; i < envLines.Length; i++)
            {
                if (envLines[i].StartsWith("# Plex"))
                {
                    plexSectionEnd = i;
                    // Find next section or end
                    for (int j = i + 1; j < envLines.Length; j++)
                    {
                        if (string.IsNullOrWhiteSpace(envLines[j]) || envLines[j].StartsWith("#"))
                        {
                            plexSectionEnd = j;
                            break;
                        }
                    }
                    break;
                }
            }

            var newLines = new List<string>(envLines);
            newLines.Insert(plexSectionEnd, $"PLEX_URL={plexUrl}");
            newLines.Insert(plexSectionEnd + 1, $"PLEX_TOKEN={plexToken ?? ""}");
            envLines = newLines.ToArray();
        }

        // Write back the .env file
        await File.WriteAllLinesAsync(EnvFilePath, envLines, Encoding.UTF8);
    }
}
