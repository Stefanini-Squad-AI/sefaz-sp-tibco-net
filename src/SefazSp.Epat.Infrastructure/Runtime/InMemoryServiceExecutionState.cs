#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Infrastructure.Persistence;
using SefazSp.Epat.Infrastructure.Persistence.Serialization;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>Estado durável de execução do molde de serviço, correlacionado por PROCESS_ID.</summary>
public sealed class InMemoryServiceExecutionState : IServiceExecutionState
{
    public const string StoreKind = "svc-execution";

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public InMemoryServiceExecutionState(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public void Save(string correlationKey, ServiceExecutionSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, EpatJsonSerialization.Options);
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null)
            db.Snapshots.Add(new EpatSnapshotRow { StoreKind = StoreKind, ProcessId = correlationKey, DocumentJson = json, Version = 1 });
        else { row.DocumentJson = json; row.Version++; }
        db.SaveChanges();
    }

    public ServiceExecutionSnapshot? Load(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        return row is null ? null : JsonSerializer.Deserialize<ServiceExecutionSnapshot>(row.DocumentJson, EpatJsonSerialization.Options);
    }

    public void Clear(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null) return;
        db.Snapshots.Remove(row);
        db.SaveChanges();
    }
}
