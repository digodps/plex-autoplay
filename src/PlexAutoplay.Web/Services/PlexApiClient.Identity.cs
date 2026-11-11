using PlexAutoplay.Application;
using PlexAutoplay.Infrastructure.PlexProxy;
using Serilog;

namespace PlexAutoplay.Web.Services;

public partial class PlexApiClient
{
    public async Task<PlexIdentityResult> GetIdentityAsync(string plexUrl, string plexToken)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var url = plexUrl.TrimEnd('/') + "/identity";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("X-Plex-Token", plexToken);
            request.Headers.Accept.ParseAdd("application/xml");
            
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                Log.Warning("Plex API returned {StatusCode} for GetIdentity", statusCode);
                return PlexIdentityResult.FailedResult($"Plex API retornou status {statusCode} ao validar identidade.");
            }

            var xml = await response.Content.ReadAsStringAsync();
            var identity = PlexResponseParser.ParseIdentity(xml);
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Information("Plex API call: GetIdentity {@Data}", new { external_service = "plex", duration_ms = durationMs, status_code = (int)response.StatusCode });

            return identity;
        }
        catch (Exception ex)
        {
            var durationMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Log.Error(ex, "Plex API call failed: GetIdentity {@Data}", new { external_service = "plex", duration_ms = durationMs, error_type = ex.GetType().Name });
            return PlexIdentityResult.FailedResult($"Falha ao contatar a API do Plex: {ex.Message}");
        }
    }
}
