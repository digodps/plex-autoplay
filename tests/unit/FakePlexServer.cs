namespace PlexAutoplay.UnitTests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:0/");
        _listener.Start();

        var prefix = _listener.Prefixes.GetEnumerator();
        prefix.MoveNext();
        BaseUrl = prefix.Current.TrimEnd('/') + "/";

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