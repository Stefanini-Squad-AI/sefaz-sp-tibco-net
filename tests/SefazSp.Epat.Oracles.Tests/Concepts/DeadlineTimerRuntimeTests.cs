#nullable enable

// Runtime evidence for expression-deadline (absolute-instant) via the global demo flag.
// Boots the real Program.cs composition and drives DEAT0050 to the 'Aguarda Defesa' timer:
//   demo ON  → fires at the short demo delay → the subprocess completes (snapshot cleared);
//   demo OFF → schedules for the real computed instant (today end-of-day, hours out) → the
//              subprocess does NOT complete within the test window (it is not using a fixed
//              demo duration). Together with the DeadlineTimerFlagTests unit proof this shows
//              the boundary fires at the calculated instant, not after a fixed duration.

using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SefazSp.Epat.Infrastructure.Runtime;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Concepts;

public sealed class DeadlineTimerRuntimeTests
{
    private sealed class DeadlineFactory(bool demo, string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Persistence", connectionString);
            builder.UseSetting("DeadlineTimer:Demo", demo ? "true" : "false");
            builder.UseEnvironment("Testing");
        }
    }

    private static (string dbPath, string conn) NewTempDb()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"epat-deadline-{Guid.NewGuid():N}.db");
        return (dbPath, $"Data Source={dbPath};Cache=Shared");
    }

    private static void Cleanup(string dbPath)
    {
        SqliteConnection.ClearAllPools();
        foreach (var f in new[] { dbPath, dbPath + "-wal", dbPath + "-shm" })
            try { if (File.Exists(f)) File.Delete(f); } catch { /* best effort */ }
    }

    private static async Task DriveToTimerAsync(HttpClient client, string pid)
    {
        (await client.PostAsJsonAsync("/debug/deat0050/start",
            new { processId = pid, idAiim = 5001L, demoDeadlineSeconds = 1 })).EnsureSuccessStatusCode();
        (await client.PostAsync($"/deat0050/{pid}/inicalc", content: null)).EnsureSuccessStatusCode();
    }

    [Fact(DisplayName = "Demo flag ON: DEAT0050 fires at the ~1s demo delay and completes")]
    public async Task DemoOn_FiresAtDemoDelay()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "DL-ON-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using var factory = new DeadlineFactory(demo: true, conn);
            await DriveToTimerAsync(factory.CreateClient(), pid);
            await Task.Delay(TimeSpan.FromSeconds(3));

            // Completed → the subprocess cleared its snapshot after the demo timer fired.
            Assert.Null(factory.Services.GetRequiredService<Deat0050StateStore>().Load(pid));
        }
        finally { Cleanup(dbPath); }
    }

    [Fact(DisplayName = "Demo flag OFF: DEAT0050 waits for the real computed instant (does not fire on a demo delay)")]
    public async Task DemoOff_WaitsForComputedInstant()
    {
        var (dbPath, conn) = NewTempDb();
        var pid = "DL-OFF-" + Guid.NewGuid().ToString("N")[..8];
        try
        {
            using var factory = new DeadlineFactory(demo: false, conn);
            await DriveToTimerAsync(factory.CreateClient(), pid);
            await Task.Delay(TimeSpan.FromSeconds(3));

            // Still waiting → the timer is scheduled for the real absolute instant (today end-of-day),
            // NOT the ~1s demo delay, so the snapshot has not been cleared.
            Assert.NotNull(factory.Services.GetRequiredService<Deat0050StateStore>().Load(pid));
        }
        finally { Cleanup(dbPath); }
    }
}
