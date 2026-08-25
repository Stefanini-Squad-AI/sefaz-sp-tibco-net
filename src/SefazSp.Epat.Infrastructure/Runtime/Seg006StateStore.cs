#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Persistence;
using SefazSp.Epat.Infrastructure.Persistence.Serialization;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>
/// Estado do segmento 006 (AND-split paralelo) que sobrevive à suspensão em 'Finalizar AIIM'.
/// <paramref name="AfrName"/> alimenta a chamada literal <c>GETATTRIBUTE("Name")</c>;
/// <paramref name="ExisteNotificacao"/> decide o ramo XOR 'Existe Notificação?'.
/// </summary>
public sealed record Seg006Snapshot(
    long IdAiim, string ProcessId, AiimCase Case, string AfrName, bool ExisteNotificacao);

/// <summary>Guarda de forma durável o snapshot do segmento 006, correlacionado por PROCESS_ID.</summary>
public sealed class Seg006StateStore
{
    public const string StoreKind = "seg006";

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public Seg006StateStore(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public void Save(string correlationKey, Seg006Snapshot snapshot)
    {
        var json = JsonSerializer.Serialize(snapshot, EpatJsonSerialization.Options);
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null)
            db.Snapshots.Add(new EpatSnapshotRow { StoreKind = StoreKind, ProcessId = correlationKey, DocumentJson = json, Version = 1 });
        else { row.DocumentJson = json; row.Version++; }
        db.SaveChanges();
    }

    public Seg006Snapshot? Load(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        return row is null ? null : JsonSerializer.Deserialize<Seg006Snapshot>(row.DocumentJson, EpatJsonSerialization.Options);
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
