#nullable enable

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Infrastructure.Persistence;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>
/// Implementação durável do <see cref="IGraftJoin"/> (graft-step correlation-join, ratificado
/// 2026-08-06), persistida em SQLite via <see cref="EpatRuntimeDbContext"/> por PROCESS_ID.
/// O contrato fica no pai: os filhos apenas anexam (<see cref="AttachAsync"/>) e sinalizam conclusão
/// (<see cref="SignalCompletedAsync"/>); o pai agrega e decide quando prosseguir — e o estado
/// sobrevive a um reinício do processo entre anexações/conclusões/fecho.
///
/// Decisões da POC (Wave 2, propostas ratificadas):
///   • chave de correlação = PROCESS_ID do pai (via SW_PARENTCASE — F1).
///   • critério de fecho    = valve explícita <see cref="Close"/> + todos os filhos anexados concluíram.
///   • prosseguir uma vez   = compare-and-swap durável (token de concorrência), substitui o Interlocked.
/// </summary>
public sealed class InMemoryGraftJoin : IGraftJoin
{
    public const string StoreKind = "graft-join";

    private sealed class GraftState
    {
        public Dictionary<string, bool> Children { get; set; } = new(); // childId → completed
        public bool Closed { get; set; }
        public int Resolved { get; set; } // 0/1 — o pai só prossegue uma vez
    }

    private readonly IDbContextFactory<EpatRuntimeDbContext> _factory;

    public InMemoryGraftJoin(IDbContextFactory<EpatRuntimeDbContext> factory) => _factory = factory;

    public Task<GraftToken> ParkAsync(string correlationKey, CancellationToken ct)
    {
        var state = Mutate(correlationKey, _ => { });
        return Task.FromResult(new GraftToken(correlationKey, state.Children.Count));
    }

    public Task AttachAsync(string correlationKey, string childInstanceId, CancellationToken ct)
    {
        Mutate(correlationKey, s => s.Children[childInstanceId] = false);
        return Task.CompletedTask;
    }

    public Task SignalCompletedAsync(string correlationKey, string childInstanceId, CancellationToken ct)
    {
        Mutate(correlationKey, s =>
        {
            if (s.Children.ContainsKey(childInstanceId))
                s.Children[childInstanceId] = true;
        }, createIfMissing: false);
        return Task.CompletedTask;
    }

    /// <summary>Valve explícita de fecho da janela do graft (critério de fecho ratificado para a POC).</summary>
    public void Close(string correlationKey)
        => Mutate(correlationKey, s => s.Closed = true, createIfMissing: false);

    /// <summary>Pai pode prosseguir: janela fechada E todos os filhos anexados concluíram.</summary>
    public bool IsReadyToProceed(string correlationKey)
    {
        var s = Load(correlationKey);
        return s is not null && s.Closed && s.Children.Count > 0 && s.Children.Values.All(done => done);
    }

    /// <summary>Guarda atómica (compare-and-swap durável): devolve <c>true</c> só ao primeiro chamador.</summary>
    public bool TryResolve(string correlationKey)
    {
        while (true)
        {
            using var db = _factory.CreateDbContext();
            var row = db.Snapshots.Find(StoreKind, correlationKey);
            if (row is null) return true; // sem estado → primeiro (mantém a semântica original)

            var state = Deserialize(row.DocumentJson);
            if (state.Resolved != 0) return false; // já resolvido

            state.Resolved = 1;
            row.DocumentJson = Serialize(state);
            row.Version++;
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Outro chamador resolveu em paralelo — relê e decide.
            }
        }
    }

    public (int Attached, int Completed) Snapshot(string correlationKey)
    {
        var s = Load(correlationKey);
        return s is null ? (0, 0) : (s.Children.Count, s.Children.Values.Count(done => done));
    }

    public void Clear(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        if (row is null) return;
        db.Snapshots.Remove(row);
        db.SaveChanges();
    }

    private GraftState Mutate(string correlationKey, Action<GraftState> mutate, bool createIfMissing = true)
    {
        while (true)
        {
            using var db = _factory.CreateDbContext();
            var row = db.Snapshots.Find(StoreKind, correlationKey);
            if (row is null)
            {
                var fresh = new GraftState();
                if (!createIfMissing) return fresh;
                mutate(fresh);
                db.Snapshots.Add(new EpatSnapshotRow
                {
                    StoreKind = StoreKind,
                    ProcessId = correlationKey,
                    DocumentJson = Serialize(fresh),
                    Version = 1,
                });
                try { db.SaveChanges(); return fresh; }
                catch (DbUpdateException) { continue; } // corrida na criação — relê
            }

            var state = Deserialize(row.DocumentJson);
            mutate(state);
            row.DocumentJson = Serialize(state);
            row.Version++;
            try { db.SaveChanges(); return state; }
            catch (DbUpdateConcurrencyException) { /* relê e reaplica */ }
        }
    }

    private GraftState? Load(string correlationKey)
    {
        using var db = _factory.CreateDbContext();
        var row = db.Snapshots.Find(StoreKind, correlationKey);
        return row is null ? null : Deserialize(row.DocumentJson);
    }

    private static string Serialize(GraftState s) => JsonSerializer.Serialize(s);
    private static GraftState Deserialize(string json) => JsonSerializer.Deserialize<GraftState>(json) ?? new GraftState();
}
