namespace PlexAutoplay.UnitTests;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public sealed class FakePlexServer : IDisposable
{
    private readonly HttpListener _listener;
    private string _responseJson;
    private readonly int _statusCode;
    private readonly Task _runTask;

    public string BaseUrl { get; }
    public Dictionary<string, string> LastHeaders { get; } = new();
    public string? LastQuery { get; private set; }
    public string? LastPath { get; private set; }

    public FakePlexServer(string responseJson, int statusCode = 200)
    {
        _responseJson = responseJson;
        _statusCode = statusCode;
        
        // Find an available port using a temporary listener on port 0
        int port;
        using (var tempListener = new TcpListener(IPAddress.Loopback, 0))
        {
            tempListener.Start();
            port = ((IPEndPoint)tempListener.LocalEndpoint).Port;
            tempListener.Stop();
        }

        _listener = new HttpListener();
        BaseUrl = $"http://127.0.0.1:{port}/";
        _listener.Prefixes.Add(BaseUrl);
        _listener.Start();

        _runTask = RunAsync();
    }

    public void ResetResponse(string newJson)
    {
        _responseJson = newJson;
    }

    private async Task RunAsync()
    {
        try
        {
            while (_listener.IsListening)
            {
                var context = await _listener.GetContextAsync();
                LastPath = context.Request.Url?.AbsolutePath;
                LastQuery = context.Request.Url?.Query;
                LastHeaders.Clear();
                foreach (string key in context.Request.Headers.AllKeys)
                {
                    LastHeaders[key] = context.Request.Headers[key] ?? "";
                }
                _ = HandleRequestAsync(context);
            }
        }
        catch (ObjectDisposedException) { }
        catch (HttpListenerException) { }
    }

    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        try
        {
            context.Response.StatusCode = _statusCode;
            context.Response.ContentType = "application/json";

            var buffer = Encoding.UTF8.GetBytes(_responseJson);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }
        catch { }
        finally
        {
            context.Response.Close();
        }
    }

    public void Dispose()
    {
        try
        {
            _listener.Stop();
            _listener.Close();
        }
        catch { }
    }
}
