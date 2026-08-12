#nullable enable

namespace SefazSp.Epat.Application.Abstractions.Runtime;

public interface ICorrelationStore
{
    Task RegisterAsync(string correlationKey, string instanceId, CancellationToken ct);
    Task<bool> ResumeAsync(string correlationKey, object payload, CancellationToken ct);
    Task UnregisterAsync(string correlationKey, CancellationToken ct);
}
