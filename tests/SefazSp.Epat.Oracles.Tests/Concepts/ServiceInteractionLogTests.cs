#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Application.Abstractions;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Domain.Abstractions;
using SefazSp.Epat.Infrastructure.Integration.Logging;
using SefazSp.Epat.Infrastructure.Persistence;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Concepts;

/// <summary>
/// The interaction log is additive integration evidence: the service decorator records each
/// request/response durably, correlated by PROCESS_ID, and a query returns them in order.
/// </summary>
public sealed class ServiceInteractionLogTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"epat-interactions-{Guid.NewGuid():N}.db");
    private readonly TestFactory _factory;

    public ServiceInteractionLogTests()
    {
        _factory = new TestFactory($"Data Source={_dbPath}");
        using var db = _factory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    [Fact(DisplayName = "Decorator records a successful IEpatServices call durably, keyed by PROCESS_ID")]
    public async Task Records_Success()
    {
        var log = new EfServiceInteractionLog(_factory);
        var inner = new FakeEpatServices(new ServiceEnvelope("0", null, null));
        var svc = new LoggingEpatServices(inner, log, new FakeClock(DateTimeOffset.UnixEpoch));

        var envelope = await svc.PrepararintimacaoAsync(new AiimCaseRef(42, "PID-1"), CancellationToken.None);

        Assert.Equal("0", envelope.STATUS_CODE);
        var recorded = await log.GetAsync("PID-1", CancellationToken.None);
        var one = Assert.Single(recorded);
        Assert.Equal("PID-1", one.CorrelationId);
        Assert.Equal("IEpatServices", one.Port);
        Assert.Equal("PrepararIntimacao", one.Operation);
        Assert.True(one.Success);
        Assert.Null(one.Failure);
        Assert.Contains("PID-1", one.RequestJson);
        Assert.Contains("\"0\"", one.ResponseJson);
    }

    [Fact(DisplayName = "Decorator records a failing (non-zero STATUS_CODE) call with Success=false")]
    public async Task Records_ApplicationError()
    {
        var log = new EfServiceInteractionLog(_factory);
        var inner = new FakeEpatServices(new ServiceEnvelope("1", "APP", "erro simulado"));
        var svc = new LoggingEpatServices(inner, log, new FakeClock(DateTimeOffset.UnixEpoch));

        await svc.CriarnotificacoesaiimAsync(new AiimCaseRef(7, "PID-ERR"), CancellationToken.None);

        var one = Assert.Single(await log.GetAsync("PID-ERR", CancellationToken.None));
        Assert.Equal("CriarNotificacoesAiim", one.Operation);
        Assert.False(one.Success);
        Assert.Null(one.Failure); // returned an error envelope, did not throw
    }

    [Fact(DisplayName = "Decorator records a thrown exception as a failure and rethrows")]
    public async Task Records_Exception_And_Rethrows()
    {
        var log = new EfServiceInteractionLog(_factory);
        var inner = new ThrowingEpatServices(new InvalidOperationException("boom"));
        var svc = new LoggingEpatServices(inner, log, new FakeClock(DateTimeOffset.UnixEpoch));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AtualizarintimacaoAsync(new AiimCaseRef(9, "PID-EX"), CancellationToken.None));

        var one = Assert.Single(await log.GetAsync("PID-EX", CancellationToken.None));
        Assert.Equal("AtualizarIntimacao", one.Operation);
        Assert.False(one.Success);
        Assert.Equal("boom", one.Failure);
        Assert.Equal("null", one.ResponseJson);
    }

    [Fact(DisplayName = "Interactions are returned per PROCESS_ID, in chronological order")]
    public async Task Isolated_And_Ordered_By_Correlation()
    {
        var clock = new FakeClock(DateTimeOffset.UnixEpoch);
        var log = new EfServiceInteractionLog(_factory);
        var svc = new LoggingEpatServices(new FakeEpatServices(new ServiceEnvelope("0", null, null)), log, clock);

        await svc.PrepararintimacaoAsync(new AiimCaseRef(1, "A"), CancellationToken.None);
        clock.Advance(TimeSpan.FromSeconds(1));
        await svc.AtualizarintimacaoAsync(new AiimCaseRef(1, "A"), CancellationToken.None);
        await svc.PrepararintimacaoAsync(new AiimCaseRef(2, "B"), CancellationToken.None);

        var a = await log.GetAsync("A", CancellationToken.None);
        Assert.Equal(2, a.Count);
        Assert.Equal("PrepararIntimacao", a[0].Operation);
        Assert.Equal("AtualizarIntimacao", a[1].Operation);
        Assert.Single(await log.GetAsync("B", CancellationToken.None));
    }

    public void Dispose()
    {
        try { if (File.Exists(_dbPath)) File.Delete(_dbPath); } catch { /* best-effort cleanup */ }
    }

    private sealed class TestFactory(string connectionString) : IDbContextFactory<EpatRuntimeDbContext>
    {
        public EpatRuntimeDbContext CreateDbContext()
            => new(new DbContextOptionsBuilder<EpatRuntimeDbContext>().UseSqlite(connectionString).Options);
    }

    private sealed class FakeClock(DateTimeOffset start) : IClock
    {
        public DateTimeOffset Now { get; private set; } = start;
        public TimeZoneInfo TimeZone => TimeZoneInfo.Utc;
        public void Advance(TimeSpan by) => Now += by;
    }

    private sealed class FakeEpatServices(ServiceEnvelope envelope) : IEpatServices
    {
        public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef c, CancellationToken ct) => Task.FromResult(envelope);
        public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef c, CancellationToken ct) => Task.FromResult(envelope);
        public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef c, CancellationToken ct) => Task.FromResult(envelope);
        public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef c, CancellationToken ct) => Task.FromResult(envelope);
        public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef c, CancellationToken ct) => Task.FromResult(envelope);
    }

    private sealed class ThrowingEpatServices(Exception ex) : IEpatServices
    {
        public Task<ServiceEnvelope> PrepararintimacaoAsync(AiimCaseRef c, CancellationToken ct) => Task.FromException<ServiceEnvelope>(ex);
        public Task<ServiceEnvelope> AtualizarintimacaoAsync(AiimCaseRef c, CancellationToken ct) => Task.FromException<ServiceEnvelope>(ex);
        public Task<ServiceEnvelope> BuscarvistasativasporaiimAsync(AiimCaseRef c, CancellationToken ct) => Task.FromException<ServiceEnvelope>(ex);
        public Task<ServiceEnvelope> CriarnotificacoesaiimAsync(AiimCaseRef c, CancellationToken ct) => Task.FromException<ServiceEnvelope>(ex);
        public Task<ServiceEnvelope> ObterprimeirodiautilaposperiododediascorridosdeatAsync(AiimCaseRef c, CancellationToken ct) => Task.FromException<ServiceEnvelope>(ex);
    }
}
