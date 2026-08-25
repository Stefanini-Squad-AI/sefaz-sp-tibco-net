#nullable enable

using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using SefazSp.Epat.Infrastructure.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;
using Xunit;

namespace SefazSp.Epat.Oracles.Tests.Composition;

public sealed class PocEpatDuplicateDeliveryTests
{
    private sealed class TestFactory(string connectionString) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("ConnectionStrings:Persistence", connectionString);
            builder.UseEnvironment("Testing");
        }
    }

    [Fact(DisplayName = "Duplicate main-flow event is accepted once and does not duplicate the SC-015 path")]
    public async Task DuplicateIniciarNovoGraft_IsIdempotent()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"epat-duplicate-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbPath};Cache=Shared";
        var processId = "DUP-" + Guid.NewGuid().ToString("N")[..8];

        try
        {
            using var host = new TestFactory(connectionString);
            var client = host.CreateClient();

            await PostJson(client, "/debug/pocepat/start", new { processId, idAiim = 4015L });

            // The first delivery consumes the bookmark; the duplicate must be a no-op.
            await Post(client, $"/pocepat/{processId}/iniciar-novo-graft");
            await Post(client, $"/pocepat/{processId}/iniciar-novo-graft");

            await PostJson(client, $"/pocepat/{processId}/preparar-notificacao", new { correcao = false });
            await Task.Delay(500);

            var snapshot = host.Services.GetRequiredService<PocEpatProcessState>().Load(processId);
            Assert.NotNull(snapshot);
            Assert.Equal(PocEpatMainActivity.Sc015NodePath, snapshot!.Path);
            Assert.Equal(snapshot.Path.Count, snapshot.Path.Distinct(StringComparer.Ordinal).Count());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            foreach (var file in new[] { dbPath, dbPath + "-wal", dbPath + "-shm" })
                try { if (File.Exists(file)) File.Delete(file); } catch { }
        }
    }

    private static async Task Post(HttpClient client, string url)
        => (await client.PostAsync(url, content: null)).EnsureSuccessStatusCode();

    private static async Task PostJson(HttpClient client, string url, object body)
        => (await client.PostAsJsonAsync(url, body)).EnsureSuccessStatusCode();
}