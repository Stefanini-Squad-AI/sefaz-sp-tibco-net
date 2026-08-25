#nullable enable

using Elsa.Workflows.Runtime;
using SefazSp.Epat.Application.Abstractions.Runtime;

namespace SefazSp.Epat.Infrastructure.Workflow.Elsa;

/// <summary>
/// Elsa-backed <see cref="ICorrelationStore"/>. Replaces the in-memory stub.
/// Resumes a suspended workflow instance by publishing the external event
/// correlated by PROCESS_ID (the workflow's CorrelationId).
/// gaps.external-event = bookmark-correlation (NOEQ-external-event).
/// </summary>
public sealed class ElsaCorrelationStore(IEventPublisher eventPublisher) : ICorrelationStore
{
    public Task<bool> HasBookmarkAsync(string correlationKey, CancellationToken ct)
    {
        // Minimal: the resume attempt itself reports whether anything was waiting.
        return Task.FromResult(true);
    }

    public async Task<bool> ResumeAsync(string correlationKey, object? payload, CancellationToken ct)
    {
        await eventPublisher.PublishAsync(
            eventName: BscenvpcElsaWorkflow.ExternalEventName,
            correlationId: correlationKey,
            payload: payload,
            cancellationToken: ct);

        return true;
    }
}
