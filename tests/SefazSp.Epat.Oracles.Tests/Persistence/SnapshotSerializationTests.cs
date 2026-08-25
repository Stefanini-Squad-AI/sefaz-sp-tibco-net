#nullable enable

using System.Text.Json;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Domain.ValueObjects;
using SefazSp.Epat.Infrastructure.Persistence.Serialization;
using SefazSp.Epat.Infrastructure.Runtime;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Persistence;

public sealed class SnapshotSerializationTests
{
    [Fact(DisplayName = "PocEpatMainSnapshot round-trips path, flags, seeds and tri-state case fields")]
    public void Snapshot_RoundTrips()
    {
        var snap = new PocEpatMainSnapshot(1001, "SC001-RT");
        snap.Path.Add("node-a");
        snap.Path.Add("node-b");
        snap.PendingAfrName = "AFR-XYZ";
        snap.RaceResolved = 1;
        snap.ExisteNotificacaoSim = true;
        snap.CorrigirNo = false;
        snap.GraftMode = true;
        snap.PrpintpcFails = true;
        snap.PrpintpcAttempt = 2;
        snap.DecisionsSeed["TIPOVISTAS"] = "JUIZ";
        snap.Case.CORRECAO = true;
        snap.Case.EXISTENOTIFICAC = true;
        snap.Case.AFR = "AFR-XYZ";
        snap.Case.CNTPECA1 = FieldValue<string>.NotAvailable; // SW_NA sentinel must survive
        snap.Case.BCCRELATORIO = FieldValue<string>.Of("R-1");

        var json = JsonSerializer.Serialize(snap, EpatJsonSerialization.Options);
        var back = JsonSerializer.Deserialize<PocEpatMainSnapshot>(json, EpatJsonSerialization.Options)!;

        Assert.Equal(1001, back.IdAiim);
        Assert.Equal("SC001-RT", back.ProcessId);
        Assert.Equal(new[] { "node-a", "node-b" }, back.Path);
        Assert.Equal("AFR-XYZ", back.PendingAfrName);
        Assert.Equal(1, back.RaceResolved);
        Assert.True(back.ExisteNotificacaoSim);
        Assert.False(back.CorrigirNo);
        Assert.True(back.GraftMode);
        Assert.True(back.PrpintpcFails);
        Assert.Equal(2, back.PrpintpcAttempt);
        Assert.Equal("JUIZ", back.DecisionsSeed["TIPOVISTAS"]);
        Assert.True(back.Case.CORRECAO);
        Assert.True(back.Case.EXISTENOTIFICAC);
        Assert.Equal("AFR-XYZ", back.Case.AFR);
        Assert.True(back.Case.CNTPECA1.IsNotAvailable);
        Assert.True(back.Case.BCCRELATORIO.HasValue);
        Assert.Equal("R-1", back.Case.BCCRELATORIO.Match(v => v, () => "NA", () => "EMPTY"));
    }

    [Fact(DisplayName = "Standalone subprocess snapshots round-trip (AiimCase/SW_NA, required-init, exec context)")]
    public void SubprocessSnapshots_RoundTrip()
    {
        // ServiceExecutionSnapshot (record + ProcessExecutionContext).
        var svc = new ServiceExecutionSnapshot("CRNOTPC", "SVC-1", 2001,
            new SefazSp.Epat.Application.Execution.ProcessExecutionContext { MAXRETRIES = 5, NUMAPPRETRIES = 2, ISAPPERROR = "Y" });
        var svcBack = RoundTrip(svc);
        Assert.Equal("CRNOTPC", svcBack.ProcessKey);
        Assert.Equal(2, svcBack.Ctx.NUMAPPRETRIES);
        Assert.Equal("Y", svcBack.Ctx.ISAPPERROR);

        // Deat0050Snapshot (record + AiimCase + ProcessExecutionContext).
        var deatCase = new SefazSp.Epat.Domain.Cases.AiimCase { AFR = "AFR-D" };
        deatCase.CNTPECA1 = FieldValue<string>.NotAvailable;
        var deat = new Deat0050Snapshot(2002, "DEAT-1", deatCase,
            new SefazSp.Epat.Application.Execution.ProcessExecutionContext { MAXRETRIES = 5 }, 7);
        var deatBack = RoundTrip(deat);
        Assert.Equal("DEAT-1", deatBack.ProcessId);
        Assert.Equal(7, deatBack.DemoDeadlineSeconds);
        Assert.True(deatBack.Case.CNTPECA1.IsNotAvailable);

        // AgpecaspcSnapshot (required-init class + AiimCase + race flag).
        var agCase = new SefazSp.Epat.Domain.Cases.AiimCase { AFR = "AFR-A" };
        var ag = new AgpecaspcSnapshot { IdAiim = 2003, ProcessId = "AG-1", Case = agCase, DemoTimerSeconds = 3, Resolved = true };
        var agBack = RoundTrip(ag);
        Assert.Equal(2003, agBack.IdAiim);
        Assert.Equal("AG-1", agBack.ProcessId);
        Assert.Equal(3, agBack.DemoTimerSeconds);
        Assert.True(agBack.Resolved);
        Assert.Equal("AFR-A", agBack.Case.AFR);

        // Seg006Snapshot (record + AiimCase).
        var seg = new Seg006Snapshot(2004, "SEG-1", new SefazSp.Epat.Domain.Cases.AiimCase { AFR = "AFR-S" }, "AFR-NAME", true);
        var segBack = RoundTrip(seg);
        Assert.Equal("SEG-1", segBack.ProcessId);
        Assert.Equal("AFR-NAME", segBack.AfrName);
        Assert.True(segBack.ExisteNotificacao);
    }

    private static T RoundTrip<T>(T value)
        => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, EpatJsonSerialization.Options), EpatJsonSerialization.Options)!;
}
