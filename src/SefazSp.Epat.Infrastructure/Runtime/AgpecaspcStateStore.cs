#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Persistence;
using SefazSp.Epat.Infrastructure.Persistence.Serialization;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>
/// Estado do AGPECASPC que sobrevive à suspensão 'Aguardar Interposições'.
/// <see cref="Resolved"/> é o guarda da corrida evento⇄timer: o primeiro callback a resolver
/// marca-o; o segundo (perdedor) vê-o marcado e não age.
/// </summary>
public sealed class AgpecaspcSnapshot
{
    public required long IdAiim { get; init; }
    public required string ProcessId { get; init; }
    public required AiimCase Case { get; init; }
    public required int DemoTimerSeconds { get; init; }
    public bool Resolved { get; set; }
}

/// <summary>Guarda de forma durável o snapshot do AGPECASPC, correlacionado por PROCESS_ID.</summary>
public sealed class AgpecaspcStateStore
{
    public const string StoreKind = "agpecaspc";

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public AgpecaspcStateStore(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public void Save(string correlationKey, AgpecaspcSnapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, EpatJsonSerialization.Options);
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null)
            db.Snapshots.Add(new EpatSnapshotRow { StoreKind = StoreKind, ProcessId = correlationKey, DocumentJson = json, Version = 1 });
        else { row.DocumentJson = json; row.Version++; }
        db.SaveChanges();
    }

    public AgpecaspcSnapshot? Load(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        return row is null ? null : JsonSerializer.Deserialize<AgpecaspcSnapshot>(row.DocumentJson, EpatJsonSerialization.Options);
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
