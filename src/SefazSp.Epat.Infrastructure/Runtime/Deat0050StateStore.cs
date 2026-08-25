#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Persistence;
using SefazSp.Epat.Infrastructure.Persistence.Serialization;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>Estado do DEAT0050 que sobrevive às duas suspensões (INICALC + Aguarda Defesa).</summary>
public sealed record Deat0050Snapshot(
    long IdAiim, string ProcessId, AiimCase Case, ProcessExecutionContext Ctx, int DemoDeadlineSeconds);

/// <summary>Guarda de forma durável o snapshot do DEAT0050, correlacionado por PROCESS_ID.</summary>
public sealed class Deat0050StateStore
{
    public const string StoreKind = "deat0050";

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public Deat0050StateStore(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public void Save(string correlationKey, Deat0050Snapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, EpatJsonSerialization.Options);
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null)
            db.Snapshots.Add(new EpatSnapshotRow { StoreKind = StoreKind, ProcessId = correlationKey, DocumentJson = json, Version = 1 });
        else { row.DocumentJson = json; row.Version++; }
        db.SaveChanges();
    }

    public Deat0050Snapshot? Load(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        return row is null ? null : JsonSerializer.Deserialize<Deat0050Snapshot>(row.DocumentJson, EpatJsonSerialization.Options);
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
