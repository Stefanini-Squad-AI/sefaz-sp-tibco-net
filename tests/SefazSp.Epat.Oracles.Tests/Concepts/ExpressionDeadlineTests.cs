#nullable enable

// Concept: expression-deadline (NOEQ-expression-deadline = absolute-instant, ratificado 2026-08-06).
// The deadline is NOT a fixed duration: it is an absolute instant computed from a DATE field
// (PRAZODEFESA, derived from the DAYSOVER case field) + a TIME field (PRAZODEFESAT), in the
// America/Sao_Paulo timezone, fixed at scheduling time. This clock-controlled test proves the
// boundary fires at the *calculated instant*, not after a fixed duration — the DoD acceptance
// check for SLA/deadline control.

using SefazSp.Epat.Application.Execution;
using SefazSp.Epat.Application.Workflows.AGPECASPC;
using SefazSp.Epat.Application.Workflows.Deat0050;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Domain.Cases;
using SefazSp.Epat.Infrastructure.Legacy;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Concepts;

public sealed class ExpressionDeadlineTests
{
    private sealed class FakeClock(DateTimeOffset now, TimeZoneInfo tz) : IClock
    {
        public DateTimeOffset Now { get; } = now;
        public TimeZoneInfo TimeZone { get; } = tz;
    }

    private static readonly TimeZoneInfo SaoPaulo = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    // Fixed reference "now": 2026-01-15 10:00 in Sao Paulo (UTC-03:00, no DST since 2019).
    private static FakeClock ClockAt15Jan() =>
        new(new DateTimeOffset(2026, 1, 15, 10, 0, 0, TimeSpan.FromHours(-3)), SaoPaulo);

    [Fact(DisplayName = "HoraFimSC deadline is calculated from the DAYSOVER field, not a fixed duration")]
    public void Deadline_IsCalculatedFromField_NotFixedDuration()
    {
        var clock = ClockAt15Jan();

        var caso10 = new AiimCase();
        HoraFimScExecutor.Execute(caso10, clock, daysOver: 10);

        var caso20 = new AiimCase();
        HoraFimScExecutor.Execute(caso20, clock, daysOver: 20);

        // Same "now", different case field → different deadline DATE (proves field-driven).
        Assert.Equal(new DateOnly(2026, 1, 25), caso10.PRAZODEFESA);
        Assert.Equal(new DateOnly(2026, 2, 4), caso20.PRAZODEFESA);
        Assert.Equal(new TimeOnly(23, 59, 59), caso10.PRAZODEFESAT);
        Assert.Equal(10, caso20.PRAZODEFESA.DayNumber - caso10.PRAZODEFESA.DayNumber);
    }

    [Fact(DisplayName = "Aguarda Defesa fires at the absolute DATE+TIME instant (Sao Paulo), not now+duration")]
    public void Deadline_IsAbsoluteInstant_TimezoneAware()
    {
        var clock = ClockAt15Jan();
        var caso = new AiimCase();
        HoraFimScExecutor.Execute(caso, clock, daysOver: 10); // PRAZODEFESA=2026-01-25, 23:59:59

        var viaExecutor = HoraFimScExecutor.ToAbsoluteInstant(caso.PRAZODEFESA, caso.PRAZODEFESAT, clock);
        var viaRule = Deat0050DeadlineRules.ComputeAguardaDefesaDeadline(caso, clock);

        // 2026-01-25 23:59:59 Sao Paulo (UTC-03:00) == 2026-01-26 02:59:59 UTC.
        Assert.Equal(new DateTime(2026, 1, 26, 2, 59, 59, DateTimeKind.Utc), viaExecutor.UtcDateTime);
        Assert.Equal(TimeSpan.FromHours(-3), viaExecutor.Offset);
        Assert.Equal(viaExecutor.UtcDateTime, viaRule.UtcDateTime);

        // Not a fixed duration from "now": the instant is an end-of-day, ~10 days out.
        Assert.NotEqual(clock.Now.AddHours(1).UtcDateTime, viaRule.UtcDateTime);
    }

    [Fact(DisplayName = "Rewriting the deadline field re-computes a new instant (absolute-instant rearm mitigation)")]
    public void Deadline_RearmsWhenFieldRewritten()
    {
        var clock = ClockAt15Jan();
        var caso = new AiimCase();
        HoraFimScExecutor.Execute(caso, clock, daysOver: 10);

        var before = Deat0050DeadlineRules.ComputeAguardaDefesaDeadline(caso, clock);
        caso.PRAZODEFESA = caso.PRAZODEFESA.AddDays(30); // prorrogacao do prazo apos o agendamento
        var after = Deat0050DeadlineRules.ComputeAguardaDefesaDeadline(caso, clock);

        Assert.Equal(30, (after.UtcDateTime - before.UtcDateTime).TotalDays);
    }

    [Fact(DisplayName = "CALCTIME builtin computes the TIME component (days roll, time-of-day preserved)")]
    public void CalcTime_ComputesTimeComponent()
    {
        var builtins = new ProcessBuiltins();

        // HRFIMCQ = CALCTIME('23:59', 0, 0, DAYSOVER): the days argument rolls whole days,
        // it does not shift the time-of-day.
        Assert.Equal(new TimeOnly(23, 59), builtins.CalcTime(new TimeOnly(23, 59), 0, 0, 10));

        // PRAZODEFESAT = CALCTIME(SW_TIME, 1, 0, DAYSOVER): base + 1 hour.
        Assert.Equal(new TimeOnly(11, 0), builtins.CalcTime(new TimeOnly(10, 0), 1, 0, 0));
    }

    [Fact(DisplayName = "AGPECASPC boundary is the fixed 1h duration (Hours=1), distinct from the DATE+TIME expression")]
    public void AgpecaspcBoundary_IsFixedOneHour()
    {
        var clock = ClockAt15Jan();
        var boundary = AgpecaspcDeadlineRules.ComputeAguardarInterposicoesDeadline(clock);
        Assert.Equal(clock.Now.AddHours(1), boundary);
    }
}
