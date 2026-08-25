#nullable enable

// Concept: expression-deadline (absolute-instant) runtime wiring — the global demo flag.
// Proves the DoD acceptance check "boundary fires at the calculated instant, not after a fixed
// duration": with the demo flag OFF the scheduled delay equals the distance to the computed instant
// and tracks it (10 days vs 20 days differ by 10 days); with the flag ON it stays the short demo delay.

using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Infrastructure.Runtime;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Concepts;

public sealed class DeadlineTimerFlagTests
{
    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset Now { get; } = now;
        public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;
    }

    private static readonly DateTimeOffset Now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan Demo = TimeSpan.FromSeconds(2);

    [Fact(DisplayName = "Demo flag ON: timer fires on the short demo delay, ignoring the computed instant")]
    public void DemoOn_UsesDemoDelay()
    {
        var opts = new DeadlineDemoOptions { Enabled = true };
        Assert.Equal(Demo, opts.DelayTo(Now.AddDays(10), new FakeClock(Now), Demo));
    }

    [Fact(DisplayName = "Demo flag OFF: timer fires at the computed absolute instant (delay == instant - now)")]
    public void DemoOff_FiresAtComputedInstant()
    {
        var opts = new DeadlineDemoOptions { Enabled = false };
        Assert.Equal(TimeSpan.FromDays(10), opts.DelayTo(Now.AddDays(10), new FakeClock(Now), Demo));
    }

    [Fact(DisplayName = "Demo flag OFF: the delay tracks the instant (not a fixed duration)")]
    public void DemoOff_TracksInstant_NotFixedDuration()
    {
        var opts = new DeadlineDemoOptions { Enabled = false };
        var clock = new FakeClock(Now);
        var d10 = opts.DelayTo(Now.AddDays(10), clock, Demo);
        var d20 = opts.DelayTo(Now.AddDays(20), clock, Demo);
        Assert.Equal(TimeSpan.FromDays(10), d20 - d10);
    }

    [Fact(DisplayName = "Demo flag OFF: a past instant clamps to zero (already-passed deadline fires now)")]
    public void DemoOff_PastInstant_ClampsToZero()
    {
        var opts = new DeadlineDemoOptions { Enabled = false };
        Assert.Equal(TimeSpan.Zero, opts.DelayTo(Now.AddMinutes(-5), new FakeClock(Now), Demo));
    }
}
