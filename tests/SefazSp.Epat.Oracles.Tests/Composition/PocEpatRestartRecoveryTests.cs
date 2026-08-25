#nullable enable

// Track A restart-recovery: proves the 5 SC-* journeys survive a simulated mid-suspension restart.
// Each test boots the real Program.cs composition (WebApplicationFactory) on a unique temp SQLite
// file, drives events up to a mid-journey suspension, DISPOSES host A (simulated process restart),
// boots a fresh host B on the SAME file, delivers the remaining events, and asserts the durable
// final node Path equals the immutable SC-* oracle. Node paths must stay IDÊNTICO across restart.

using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SefazSp.Epat.Infrastructure.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Composition;

public sealed class PocEpatRestartRecoveryTests
{
    // The Aguarda Defesa timer in the orchestrator is a 2s demo DelayFor; wait a little longer.
    private static readonly TimeSpan DeatTimerWait = TimeSpan.FromSeconds(3);

    private sealed class RestartFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Persistence", connectionString);
            builder.UseEnvironment("Testing");
        }
    }

    private static (string dbPath, string conn) NewTempDb()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"epat-restart-{Guid.NewGuid():N}.db");
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

    private static IReadOnlyList<string> DurablePath(WebApplicationFactory<Program> host, string processId)
    {
        var snap = host.Services.GetRequiredService<PocEpatProcessState>().Load(processId);
        Assert.NotNull(snap);
        return snap!.Path;
    }

    [Fact(DisplayName = "SC-001 (JUIZ) journey stays IDÊNTICO across a restart at 'Finalizar AIIM'")]
    public async Task Sc001_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "SC001-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                await PostJson(ca, "/debug/pocepat/start", new { processId = pid, idAiim = 1001L });
                await Post(ca, $"/pocepat/{pid}/iniciar-novo-graft");
                await PostJson(ca, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
                // parked at 'Finalizar AIIM'
            }

            using (var b = new RestartFactory(conn))
            {
                var cb = b.CreateClient();
                await PostJson(cb, $"/pocepat/{pid}/finalizar-aiim", new { afrName = "AFR-RESTART" });
                await Post(cb, $"/pocepat/{pid}/deat-inicalc");
                await Task.Delay(DeatTimerWait);
                await PostJson(cb, $"/pocepat/{pid}/verificar-retorno", new { tipoVistas = "JUIZ" });
                await Post(cb, $"/pocepat/{pid}/vistas-do-juiz");
                await Task.Delay(500);

                Assert.Equal(PocEpatMainActivity.Sc001NodePath, DurablePath(b, pid));
            }
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "SC-012 (MISTA) journey stays IDÊNTICO across a restart at 'Verificar Retorno'")]
    public async Task Sc012_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "SC012-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                await PostJson(ca, "/debug/pocepat/start", new { processId = pid, idAiim = 1012L });
                await Post(ca, $"/pocepat/{pid}/iniciar-novo-graft");
                await PostJson(ca, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
                await PostJson(ca, $"/pocepat/{pid}/finalizar-aiim", new { afrName = "AFR-M" });
                await Post(ca, $"/pocepat/{pid}/deat-inicalc");
                await Task.Delay(DeatTimerWait);
                // parked at 'Verificar Retorno'
            }

            using (var b = new RestartFactory(conn))
            {
                var cb = b.CreateClient();
                await PostJson(cb, $"/pocepat/{pid}/verificar-retorno", new { tipoVistas = "MISTA" });
                await Post(cb, $"/pocepat/{pid}/realizar-vista-mista");
                await Task.Delay(500);

                Assert.Equal(PocEpatMainActivity.Sc012MistaPath, DurablePath(b, pid));
            }
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "SC-010 (DRF, boundary timer wins) stays IDÊNTICO across a restart at 'Verificar Retorno'")]
    public async Task Sc010_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "SC010-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                await PostJson(ca, "/debug/pocepat/start", new { processId = pid, idAiim = 1010L });
                await Post(ca, $"/pocepat/{pid}/iniciar-novo-graft");
                await PostJson(ca, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
                await PostJson(ca, $"/pocepat/{pid}/finalizar-aiim", new { afrName = "AFR-D" });
                await Post(ca, $"/pocepat/{pid}/deat-inicalc");
                await Task.Delay(DeatTimerWait);
                // parked at 'Verificar Retorno'
            }

            using (var b = new RestartFactory(conn))
            {
                var cb = b.CreateClient();
                // DRF branch: 'Pedido de Vistas' races the 2s boundary timer, which wins → SC-010.
                await PostJson(cb, $"/pocepat/{pid}/verificar-retorno", new { tipoVistas = "DRF" });
                await Task.Delay(TimeSpan.FromSeconds(3));

                Assert.Equal(PocEpatMainActivity.Sc010DrfPath, DurablePath(b, pid));
            }
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "SC-014 (Existe Notificação=Sim) stays IDÊNTICO across a restart at 'Finalizar AIIM'")]
    public async Task Sc014_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "SC014-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                await PostJson(ca, "/debug/pocepat/start", new { processId = pid, idAiim = 1014L, existeNotificacao = true });
                await Post(ca, $"/pocepat/{pid}/iniciar-novo-graft");
                await PostJson(ca, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
                // parked at 'Finalizar AIIM'
            }

            using (var b = new RestartFactory(conn))
            {
                var cb = b.CreateClient();
                await PostJson(cb, $"/pocepat/{pid}/finalizar-aiim", new { afrName = "AFR-N" });
                await Task.Delay(500);

                Assert.Equal(PocEpatMainActivity.Sc014NodePath, DurablePath(b, pid));
            }
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "SC-015 (Corrigir?=No) stays IDÊNTICO across a restart at 'Preparar Notificacao'")]
    public async Task Sc015_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "SC015-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                await PostJson(ca, "/debug/pocepat/start", new { processId = pid, idAiim = 1015L });
                await Post(ca, $"/pocepat/{pid}/iniciar-novo-graft");
                // parked at 'Preparar Notificacao'
            }

            using (var b = new RestartFactory(conn))
            {
                var cb = b.CreateClient();
                await PostJson(cb, $"/pocepat/{pid}/preparar-notificacao", new { correcao = false });
                await Task.Delay(500);

                Assert.Equal(PocEpatMainActivity.Sc015NodePath, DurablePath(b, pid));
            }
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "Graft-real (SC-001) stays IDÊNTICO across a restart with a child attached mid-window")]
    public async Task GraftReal_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "GRAFT-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                await PostJson(ca, "/debug/pocepat/start", new { processId = pid, idAiim = 1099L, graftMode = true });
                await Post(ca, $"/pocepat/{pid}/iniciar-novo-graft");
                await PostJson(ca, $"/pocepat/{pid}/preparar-notificacao", new { correcao = true });
                await PostJson(ca, $"/pocepat/{pid}/finalizar-aiim", new { afrName = "AFR-G" });
                // parent parked at the graft; attach a DEAT0050 child, then crash before it completes.
                await PostJson(ca, $"/pocepat/{pid}/graft-attach", new { childId = "deat-1" });
            }

            using (var b = new RestartFactory(conn))
            {
                var cb = b.CreateClient();
                // The attached child (from host A) must survive: complete it, close the window → parent proceeds.
                await PostJson(cb, $"/pocepat/{pid}/graft-complete", new { childId = "deat-1" });
                await PostJson(cb, $"/pocepat/{pid}/graft-close", new { });
                await Task.Delay(500);
                await PostJson(cb, $"/pocepat/{pid}/verificar-retorno", new { tipoVistas = "JUIZ" });
                await Post(cb, $"/pocepat/{pid}/vistas-do-juiz");
                await Task.Delay(500);

                Assert.Equal(PocEpatMainActivity.Sc001NodePath, DurablePath(b, pid));
            }
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "AGPECASPC (event⇄timer race) snapshot survives a restart at 'Aguardar Interposições'")]
    public async Task Agpecaspc_SurvivesRestart()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "AGP-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using (var a = new RestartFactory(conn))
            {
                var ca = a.CreateClient();
                // Long demo timer so the boundary timer cannot win before the restart + event.
                await PostJson(ca, "/debug/agpecaspc/start", new { processId = pid, idAiim = 3001L, demoTimerSeconds = 30 });
                // suspended at 'Aguardar Interposições'
            }

            using (var b = new RestartFactory(conn))
            {
                // The race snapshot (with Resolved=false) must have survived the restart.
                var survived = b.Services.GetRequiredService<AgpecaspcStateStore>().Load(pid);
                Assert.NotNull(survived);
                Assert.False(survived!.Resolved);

                // Event wins the race on host B → the wait resolves and the loop completes (snapshot cleared).
                var cb = b.CreateClient();
                await Post(cb, $"/agpecaspc/{pid}/interposicoes");
                await Task.Delay(500);
                Assert.Null(b.Services.GetRequiredService<AgpecaspcStateStore>().Load(pid));
            }
        }
        finally { Cleanup(dbPath); }
    }
}
