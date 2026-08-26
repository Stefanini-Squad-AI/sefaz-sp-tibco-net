#nullable enable

namespace SefazSp.Epat.Application.Abstractions.Services;

/// <summary>
/// One recorded external-service (or subprocess) call — the literal request/response artifact the
/// DoD evidence package (section 7) names. Correlated by PROCESS_ID.
/// </summary>
public sealed record ServiceInteraction(
    string CorrelationId,
    string Port,
    string Operation,
    string RequestJson,
    string ResponseJson,
    bool Success,
    string? Failure,
    DateTimeOffset At,
    long DurationMs);

/// <summary>
/// Durable audit log of external-service interactions, per PROCESS_ID. Additive evidence: the
/// service ports (doubles today, SOAP later) are wrapped by a decorator that records each call.
/// </summary>
public interface IServiceInteractionLog
{
    Task RecordAsync(ServiceInteraction interaction, CancellationToken ct);
    Task<IReadOnlyList<ServiceInteraction>> GetAsync(string correlationId, CancellationToken ct);
}
