<<<<<<< HEAD
=======
using System.Diagnostics;
>>>>>>> origin/feature/dashboard-redesign-31-10-2026-13-29-43-856

namespace PlexAutoplay.Domain.Ports;

/// <summary>
/// Abstração para obter informações do processo atual (CPU, memória, uptime).
/// Permite injeção de dependência e testabilidade.
/// </summary>
public interface IProcessInfoService
{
    /// <summary>
    /// Obtém o tempo total de CPU consumido pelo processo atual.
    /// </summary>
    TimeSpan TotalProcessorTime { get; }

    /// <summary>
    /// Obtém a memória privada (bytes) do processo atual.
    /// </summary>
    long PrivateMemorySize64 { get; }

    /// <summary>
    /// Obtém o working set (bytes) do processo atual.
    /// </summary>
    long WorkingSet64 { get; }

    /// <summary>
    /// Obtém o tempo de início do processo atual (UTC).
    /// </summary>
    DateTime StartTimeUtc { get; }
}