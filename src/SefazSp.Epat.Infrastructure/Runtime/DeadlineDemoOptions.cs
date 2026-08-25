#nullable enable

using SefazSp.Epat.Domain.Abstractions;

namespace SefazSp.Epat.Infrastructure.Runtime;

/// <summary>
/// Global demo switch for expression-deadline / boundary timers.
/// <para>
/// When <see cref="Enabled"/> (the PoC default) the timer fires on the short demonstration delay,
/// so demos and smoke tests stay watchable. When disabled, the timer fires at the real computed
/// absolute instant — the faithful production behavior for the DATE+TIME expression-deadline
/// (NOEQ-expression-deadline = absolute-instant, ratificado 2026-08-06).
/// </para>
/// </summary>
public sealed class DeadlineDemoOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The delay to schedule from now: the <paramref name="demoDelay"/> when <see cref="Enabled"/>,
    /// otherwise the distance to the computed <paramref name="instant"/>. A past instant clamps to
    /// zero — the deadline has already passed, so it fires immediately (this is a one-shot calculation
    /// at scheduling time, not the refused recompute-on-resume policy).
    /// </summary>
    public TimeSpan DelayTo(DateTimeOffset instant, IClock clock, TimeSpan demoDelay)
    {
        if (Enabled) return demoDelay;
        var real = instant - clock.Now;
        return real > TimeSpan.Zero ? real : TimeSpan.Zero;
    }
}
