#nullable enable

using System.ComponentModel.DataAnnotations;

namespace SefazSp.Epat.Infrastructure.Persistence;

/// <summary>
/// A durable snapshot document for one ePAT external store, keyed by
/// (<see cref="StoreKind"/>, <see cref="ProcessId"/>). <see cref="Version"/> is an optimistic
/// concurrency token — a database compare-and-swap that replaces the RAM-only Interlocked guards
/// used by the DRF race and the graft join.
/// </summary>
public sealed class EpatSnapshotRow
{
    /// <summary>Logical store this row belongs to (e.g. "poc-epat-main", "graft").</summary>
    public string StoreKind { get; set; } = default!;

    /// <summary>Correlation key — PROCESS_ID of the instance.</summary>
    public string ProcessId { get; set; } = default!;

    /// <summary>The serialized snapshot document.</summary>
    public string DocumentJson { get; set; } = "{}";

    /// <summary>Optimistic-concurrency token (compare-and-swap on write).</summary>
    [ConcurrencyCheck]
    public int Version { get; set; }
}
