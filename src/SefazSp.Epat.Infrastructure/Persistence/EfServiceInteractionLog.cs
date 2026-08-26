#nullable enable

using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Application.Abstractions.Services;

namespace SefazSp.Epat.Infrastructure.Persistence;

/// <summary>Durable <see cref="IServiceInteractionLog"/> over the shared SQLite context.
/// Uses <see cref="IDbContextFactory{TContext}"/> so it is safe to resolve from singletons
/// (the service decorators are singletons).</summary>
public sealed class EfServiceInteractionLog(IDbContextFactory<EpatRuntimeDbContext> factory) : IServiceInteractionLog
{
    public async Task RecordAsync(ServiceInteraction i, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        db.ServiceInteractions.Add(new ServiceInteractionRow
        {
            CorrelationId = i.CorrelationId,
            Port = i.Port,
            Operation = i.Operation,
            RequestJson = i.RequestJson,
            ResponseJson = i.ResponseJson,
            Success = i.Success,
            Failure = i.Failure,
            At = i.At,
            DurationMs = i.DurationMs,
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ServiceInteraction>> GetAsync(string correlationId, CancellationToken ct)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var rows = await db.ServiceInteractions
            .AsNoTracking()
            .Where(r => r.CorrelationId == correlationId)
            .OrderBy(r => r.Id)
            .ToListAsync(ct);
        return rows.Select(r => new ServiceInteraction(
            r.CorrelationId, r.Port, r.Operation, r.RequestJson, r.ResponseJson,
            r.Success, r.Failure, r.At, r.DurationMs)).ToList();
    }
}
