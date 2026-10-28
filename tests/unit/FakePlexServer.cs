namespace PlexAutoplay.UnitTests;

using System.Net;
using System.Text;

/// <summary>
/// Servidor HTTP local de teste que responde JSON fixo e captura a requisição
/// (path, query e cabeçalhos) para asserções.
/// </summary>
public class FakePlexServer : IDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    public string BaseUrl { get; }
    public string? LastPath { get; private set; }
    public string? LastQuery { get; private set; }
    public System.Collections.Specialized.NameValueCollection LastHeaders { get; private set; } = new();

    private volatile string _response;
    private volatile int _statusCode;

    public void ResetResponse(string jsonResponse, int statusCode = 200)
    {
        _response = jsonResponse;
        _statusCode = statusCode;
    }

    public FakePlexServer(string jsonResponse, int statusCode = 200)
    {
        _response = jsonResponse;
        _statusCode = statusCode;
        var port = new Random().Next(20000, 40000);
        BaseUrl = $"http://127.0.0.1:{port}/";
        _listener = new HttpListener();
        _listener.Prefixes.Add(BaseUrl);
        _listener.Start();
        _loop = ServeAsync();
    }

    private async Task ServeAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                LastPath = context.Request.Url!.AbsolutePath;
                LastQuery = context.Request.Url!.Query;
                LastHeaders = new System.Collections.Specialized.NameValueCollection(
                    context.Request.Headers);

                var body = Encoding.UTF8.GetBytes(_response);
                context.Response.StatusCode = _statusCode;
                context.Response.ContentLength64 = body.Length;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(body, _cts.Token);
                context.Response.Close();
            }
            catch (Exception) when (_cts.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _listener.Stop(); } catch { }
        try { _loop.Wait(500); } catch { }
        _cts.Dispose();
    }
}
