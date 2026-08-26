using SefazSp.Epat.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Server-side HttpClient to the ePAT Api (Phase 1 read model). No CORS — server-to-server.
builder.Services.AddHttpClient("epat-api", (sp, http) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["ApiBaseUrl"] ?? "http://localhost:5000";
    http.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
