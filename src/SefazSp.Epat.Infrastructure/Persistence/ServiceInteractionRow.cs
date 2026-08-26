#nullable enable

namespace SefazSp.Epat.Infrastructure.Persistence;

/// <summary>
/// A durable record of one external-service interaction (request/response), keyed by
/// <see cref="CorrelationId"/> (PROCESS_ID). Append-only.
/// </summary>
public sealed class ServiceInteractionRow
{
    public long Id { get; set; }
    public string CorrelationId { get; set; } = default!;
    public string Port { get; set; } = default!;
    public string Operation { get; set; } = default!;
    public string RequestJson { get; set; } = "null";
    public string ResponseJson { get; set; } = "null";
    public bool Success { get; set; }
    public string? Failure { get; set; }
    public DateTimeOffset At { get; set; }
    public long DurationMs { get; set; }
}
