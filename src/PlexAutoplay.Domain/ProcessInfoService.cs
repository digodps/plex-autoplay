using System;
using System.Diagnostics;

using PlexAutoplay.Domain.Ports;

namespace PlexAutoplay.Domain;

/// <summary>
/// Implementação concreta da abstração IProcessInfoService.
/// </summary>
public sealed class ProcessInfoService : IProcessInfoService
{
    public TimeSpan TotalProcessorTime => Process.GetCurrentProcess().TotalProcessorTime;

    public long PrivateMemorySize64 => Process.GetCurrentProcess().PrivateMemorySize64;

    public long WorkingSet64 => Process.GetCurrentProcess().WorkingSet64;

    public DateTime StartTimeUtc => Process.GetCurrentProcess().StartTime.ToUniversalTime();
}