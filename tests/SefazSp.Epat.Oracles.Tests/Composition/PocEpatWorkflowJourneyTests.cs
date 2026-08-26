#nullable enable

// Part 2, Phase 1 — read model. Boots the real Program.cs composition (WebApplicationFactory) on a
// temp SQLite file and asserts GET /workflow/{processId}/journey reflects the durable traversed path
// and the recorded service interactions: completed run, suspended run, unknown PROCESS_ID, and the
// interaction projection (payloads omitted).

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Composition;

public sealed class PocEpatWorkflowJourneyTests
{
    private static readonly TimeSpan DeatTimerWait = TimeSpan.FromSeconds(3);

    private sealed class JourneyFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Persistence", connectionString);
            builder.UseEnvironment("Testing");
        }
    }

    private static (string dbPath, string conn) NewTempDb()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"epat-journey-{Guid.NewGuid():N}.db");
        return (dbPath, $"Data Source={dbPath};Cache=Shared");
    }

    private static void Cleanup(string dbPath)
    {
        SqliteConnection.ClearAllPools();
        foreach (var f in new[] { dbPath, dbPath + "-wal", dbPath + "-shm" })
            try { if (File.Exists(f)) File.Delete(f); } catch { /* best effort */ }
    }

    private static async Task Post(HttpClient c, string url)
        => (await c.PostAsync(url, content: null)).EnsureSuccessStatusCode();

    private static async Task PostJson(HttpClient c, string url, object body)
        => (await c.PostAsJsonAsync(url, body)).EnsureSuccessStatusCode();

    private sealed record JourneyResponse(
        string ProcessId, string BpmnKey, string Status,
        List<StepDto> Traversed, string? CurrentNodeId, List<InteractionDto> Interactions);
    private sealed record StepDto(int Index, string NodeId);
    private sealed record InteractionDto(
        string Port, string Operation, bool Success, string? Failure, DateTimeOffset At, long DurationMs);

    [Fact(DisplayName = "journey of a completed SC-001 run: 30 steps, Completed, endEvent last, no current node")]
    public async Task Completed_Sc001()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "JRN001-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using var host = new JourneyFactory(conn);
            var c = host.CreateClient();
            await PostJson(c, "/debug/pocepat/start", new { processId = pid, idAiim = 1001L });
            await Post(c, $"/pocepat/{pid}/iniciar-novo-graft");
            await PostJson(c, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
            await PostJson(c, $"/pocepat/{pid}/finalizar-aiim", new { afrName = "AFR-JRN" });
            await Post(c, $"/pocepat/{pid}/deat-inicalc");
            await Task.Delay(DeatTimerWait);
            await PostJson(c, $"/pocepat/{pid}/verificar-retorno", new { tipoVistas = "JUIZ" });
            await Post(c, $"/pocepat/{pid}/vistas-do-juiz");
            await Task.Delay(500);

            var view = await c.GetFromJsonAsync<JourneyResponse>($"/workflow/{pid}/journey");

            Assert.NotNull(view);
            Assert.Equal(pid, view!.ProcessId);
            Assert.Equal("POC_EpatProcess__MAIN", view.BpmnKey);
            Assert.Equal("Completed", view.Status);
            Assert.Equal(30, view.Traversed.Count);
            Assert.Equal(PocEpatMainActivity.Sc001NodePath[^1], view.Traversed[^1].NodeId);
            Assert.Equal(1, view.Traversed[0].Index);
            Assert.Equal(30, view.Traversed[^1].Index);
            Assert.Null(view.CurrentNodeId);
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "journey of a suspended run: Suspended, current node = last traversed, not terminal")]
    public async Task Suspended_MidFlight()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "JRNSUS-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using var host = new JourneyFactory(conn);
            var c = host.CreateClient();
            await PostJson(c, "/debug/pocepat/start", new { processId = pid, idAiim = 1002L });
            await Post(c, $"/pocepat/{pid}/iniciar-novo-graft");
            await PostJson(c, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
            // parked at 'Finalizar AIIM'

            var view = await c.GetFromJsonAsync<JourneyResponse>($"/workflow/{pid}/journey");

            Assert.NotNull(view);
            Assert.Equal("Suspended", view!.Status);
            Assert.NotEmpty(view.Traversed);
            Assert.Equal(view.Traversed[^1].NodeId, view.CurrentNodeId);
            Assert.NotEqual(PocEpatMainActivity.Sc001NodePath[^1], view.CurrentNodeId);
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "journey of an unknown PROCESS_ID returns 404")]
    public async Task Unknown_Returns404()
    {
        var (dbPath, conn) = NewTempDb();
        try
        {
            using var host = new JourneyFactory(conn);
            var c = host.CreateClient();

            var resp = await c.GetAsync($"/workflow/does-not-exist/journey");

            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "journey projects recorded interactions (no request/response payloads)")]
    public async Task Projects_Interactions()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "JRNINT-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using var host = new JourneyFactory(conn);
            var c = host.CreateClient();
            await PostJson(c, "/debug/pocepat/start", new { processId = pid, idAiim = 1003L });
            await Post(c, $"/pocepat/{pid}/iniciar-novo-graft");
            await PostJson(c, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });

            var log = host.Services.GetRequiredService<IServiceInteractionLog>();
            await log.RecordAsync(new ServiceInteraction(
                pid, "IEpatServices", "PrepararIntimacao",
                RequestJson: "{\"ProcessId\":\"" + pid + "\"}", ResponseJson: "{\"STATUS_CODE\":\"0\"}",
                Success: true, Failure: null, At: DateTimeOffset.UtcNow, DurationMs: 7), CancellationToken.None);

            var view = await c.GetFromJsonAsync<JourneyResponse>($"/workflow/{pid}/journey");

            Assert.NotNull(view);
            var one = Assert.Single(view!.Interactions);
            Assert.Equal("IEpatServices", one.Port);
            Assert.Equal("PrepararIntimacao", one.Operation);
            Assert.True(one.Success);
            Assert.Equal(7, one.DurationMs);
        }
        finally { Cleanup(dbPath); }
    }
}
