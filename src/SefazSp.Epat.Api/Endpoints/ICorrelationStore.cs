#nullable enable

// Contrato provisório definido aqui enquanto fundacao-motor não cria
// src/SefazSp.Epat.Application/Abstractions/Runtime/ICorrelationStore.cs.
// Decisão: NOEQ-external-event → bookmark-correlation (ratificado 2026-08-06).
// Chave de correlação: PROCESS_ID = 'idAiim-<n>idProc-<n>' — não inventar, transcrita do script legado.

namespace SefazSp.Epat.Application.Abstractions.Runtime;

/// <summary>
/// Porta de correlação por bookmark para eventos externos.
/// O motor (Elsa) mantém a instância em espera até que o endpoint de retomada
/// invoque <see cref="ResumeAsync"/> com a chave correcta.
/// gaps.external-event = bookmark-correlation (NOEQ-external-event).
/// </summary>
public interface ICorrelationStore
{
    /// <summary>
    /// Retoma a instância de workflow que aguarda o bookmark identificado por
    /// <paramref name="correlationKey"/> (PROCESS_ID do caso).
    /// Lança <see cref="InvalidOperationException"/> se não existir instância em espera.
    /// </summary>
    Task ResumeAsync(string correlationKey, object? payload, CancellationToken ct);

    /// <summary>
    /// Devolve true se existir uma instância à espera do bookmark correspondente
    /// a <paramref name="correlationKey"/>.
    /// </summary>
    Task<bool> HasBookmarkAsync(string correlationKey, CancellationToken ct);
}
