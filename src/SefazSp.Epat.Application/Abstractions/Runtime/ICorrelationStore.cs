#nullable enable

namespace SefazSp.Epat.Application.Abstractions.Runtime;

/// <summary>
/// Porta de correlação por bookmark para eventos externos.
/// O motor (Elsa) mantém a instância em espera até que o endpoint de retomada
/// invoque <see cref="ResumeAsync"/> com a chave correcta.
/// gaps.external-event = bookmark-correlation (NOEQ-external-event).
/// </summary>
public interface ICorrelationStore
{
    Task<bool> HasBookmarkAsync(string correlationKey, CancellationToken ct);

    /// <summary>
    /// Retoma a instancia de workflow suspensa identificada por <paramref name="correlationKey"/>.
    /// </summary>
    Task<bool> ResumeAsync(string correlationKey, object? payload, CancellationToken ct);
}
