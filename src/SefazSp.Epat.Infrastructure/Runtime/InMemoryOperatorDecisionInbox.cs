#nullable enable

using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Infrastructure.Persistence;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>Caixa de entrada durável da decisão do operador (take-once), correlacionada por PROCESS_ID.</summary>
public sealed class InMemoryOperatorDecisionInbox : IOperatorDecisionInbox
{
    public const string StoreKind = "svc-operator-inbox";

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public InMemoryOperatorDecisionInbox(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public void Set(string correlationKey, string outcome)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null)
            db.Snapshots.Add(new EpatSnapshotRow { StoreKind = StoreKind, ProcessId = correlationKey, DocumentJson = outcome, Version = 1 });
        else { row.DocumentJson = outcome; row.Version++; }
        db.SaveChanges();
    }

    public bool TryTake(string correlationKey, out string outcome)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null) { outcome = null!; return false; }
        outcome = row.DocumentJson;
        db.Snapshots.Remove(row);
        db.SaveChanges();
        return true;
    }
}
