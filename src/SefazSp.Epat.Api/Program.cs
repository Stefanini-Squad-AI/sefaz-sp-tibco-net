using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Microsoft.EntityFrameworkCore;
using SefazSp.Epat.Api.Composition;
using SefazSp.Epat.Api.Endpoints;
using SefazSp.Epat.Application.Abstractions.Legacy;
using SefazSp.Epat.Application.Abstractions.Processes;
using SefazSp.Epat.Application.Abstractions.Runtime;
using SefazSp.Epat.Application.Abstractions.Services;
using SefazSp.Epat.Application.UseCases.ATZINTPC;
using SefazSp.Epat.Application.UseCases.CALCPRPC;
using SefazSp.Epat.Application.UseCases.CRNOTPC;
using SefazSp.Epat.Application.UseCases.PRPINTPC;
using SefazSp.Epat.Application.Workflows.ATZINTPC;
using SefazSp.Epat.Application.Workflows.CALCPRPC;
using SefazSp.Epat.Application.Workflows.CRNOTPC;
using SefazSp.Epat.Application.Workflows.PRPINTPC;
using SefazSp.Epat.Application.Workflows.ServiceTemplate;
using SefazSp.Epat.Infrastructure.Integration.Doubles;
using SefazSp.Epat.Infrastructure.Legacy;
using SefazSp.Epat.Infrastructure.Persistence;
using SefazSp.Epat.Infrastructure.Runtime;
using SefazSp.Epat.Infrastructure.Workflow.Elsa;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Agpecaspc;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Controlopc;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Deat0050;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Graft;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.PocEpat;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.Seg006Parallel;
using SefazSp.Epat.Infrastructure.Workflow.Elsa.ServiceTemplate;
using SefazSp.Epat.Domain.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Track A durable persistence: one SQLite file shared by Elsa (management + runtime) and the ePAT stores.
var persistenceConnectionString = builder.Configuration.GetConnectionString("Persistence")
    ?? "Data Source=epat-poc.db;Cache=Shared";

// Elsa workflow engine (EF Core / SQLite runtime — suspended instances + bookmarks survive restart).
builder.Services.AddElsa(elsa =>
{
    elsa.UseWorkflowManagement(management => management.UseEntityFrameworkCore(ef => ef.UseSqlite(persistenceConnectionString)));
    elsa.UseWorkflowRuntime(runtime => runtime.UseEntityFrameworkCore(ef => ef.UseSqlite(persistenceConnectionString)));
    elsa.UseScheduling();
    elsa.AddWorkflow<BscenvpcElsaWorkflow>();
    elsa.AddWorkflow<ServiceTemplateWorkflow>();
    elsa.AddWorkflow<Deat0050ElsaWorkflow>();
    elsa.AddWorkflow<AgpecaspcElsaWorkflow>();
    elsa.AddWorkflow<Seg006ParallelElsaWorkflow>();
    elsa.AddWorkflow<ControlopcElsaWorkflow>();
    elsa.AddWorkflow<GraftParentElsaWorkflow>();
    elsa.AddWorkflow<PocEpatMainElsaWorkflow>();
    elsa.AddActivitiesFrom<BscenvpcElsaWorkflow>();
    elsa.AddActivity<ServiceRetryActivity>();
    elsa.AddActivity<Deat0050ElsaActivity>();
    elsa.AddActivity<AgpecaspcElsaActivity>();
    elsa.AddActivity<FinalizarAiimActivity>();
    elsa.AddActivity<BranchAExisteNotificacaoActivity>();
    elsa.AddActivity<BranchBSetNomeEtapa2Activity>();
    elsa.AddActivity<ControlopcElsaActivity>();
    elsa.AddActivity<GraftParentActivity>();
    elsa.AddActivity<PocEpatMainActivity>();
});

// Real correlation store backed by Elsa, replacing the in-memory stub.
builder.Services.AddScoped<ICorrelationStore, ElsaCorrelationStore>();

// ePAT external-state stores: durable SQLite context sharing the Elsa database (own migration history).
builder.Services.AddDbContextFactory<EpatRuntimeDbContext>(o =>
    o.UseSqlite(persistenceConnectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Epat")));

// Global demo switch for expression-deadline / boundary timers. Default ON (watchable demos);
// set "DeadlineTimer:Demo=false" to fire at the real computed absolute instant.
builder.Services.AddSingleton(new DeadlineDemoOptions
{
    Enabled = builder.Configuration.GetValue("DeadlineTimer:Demo", true),
});

// Anticorruption layer: iProcess builtins shim (base-1).
builder.Services.AddSingleton<IProcessBuiltins, ProcessBuiltins>();

// Service-template wiring (Batch 1). In-memory SOAP double drives the retry loop.
builder.Services.AddSingleton<IEpatServices>(_ => new EpatServicesDouble());
builder.Services.AddSingleton<IOperatorDecisionInbox, InMemoryOperatorDecisionInbox>();
builder.Services.AddSingleton<IServiceExecutionState, InMemoryServiceExecutionState>();
builder.Services.AddScoped<ManipularExcecaoUseCase>();
builder.Services.AddScoped<IServiceRetryTemplate, CrnotpcSeg016Workflow>();
builder.Services.AddScoped<ManipularExcecaoPrpintpcUseCase>();
builder.Services.AddScoped<IServiceRetryTemplate, PrpintpcSeg035Workflow>();
builder.Services.AddScoped<ManipularExcecaoAtzintpcUseCase>();
builder.Services.AddScoped<IServiceRetryTemplate, AtzintpcSeg041Workflow>();
builder.Services.AddScoped<ManipularExcecaoCalcprpcUseCase>();
builder.Services.AddSingleton<ICalcularPrazoSoapService>(_ => new CalcularPrazoServiceDouble());
builder.Services.AddScoped<IServiceRetryTemplate, CalcprpcSeg030Workflow>();

// DEAT0050 (Batch 2): external event (INICALC) + timer (Aguarda Defesa).
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<INOTFAIIM>(_ =>
{
    var d = new Deat0050CalculaPrazoDouble();
    d.WithScenario("default", new SefazSp.Epat.Application.Abstractions.ProcessCallResult(Started: true, ChildInstanceId: "calc-1", Failure: null));
    d.SetActiveScenario("default");
    return d;
});
builder.Services.AddSingleton<Deat0050StateStore>();

// AGPECASPC (Batch 2): external event (interposições) racing a boundary timer.
builder.Services.AddSingleton<AgpecaspcStateStore>();

// SEG006 (Batch 3): parallel AND-split (Finalizar AIIM → fork into two branches).
builder.Services.AddSingleton<Seg006StateStore>();

// CONTROPC (Batch 3 Wave 2): dynamic subprocess resolved by AGUARDAR (interface-registry-validated).
builder.Services.AddDynamicSubprocessRegistry();

// GRAFT (Batch 3 Wave 2): graft-step correlation-join (parent parks; children attach + complete).
builder.Services.AddSingleton<InMemoryGraftJoin>();
builder.Services.AddSingleton<IGraftJoin>(sp => sp.GetRequiredService<InMemoryGraftJoin>());

// POC_EpatProcess main flow (Batch 3 Wave 3): orchestrator over the SC-001 journey.
builder.Services.AddSingleton<PocEpatProcessState>();

// fundacao-motor-de-regras (Phase 4): Decisions rules engine (Corticon override fold).
builder.Services.AddSingleton<SefazSp.Epat.Application.Abstractions.Rules.IIntimacoesDecision,
    SefazSp.Epat.Infrastructure.Rules.Dmn.IntimacoesDecisionEvaluator>();

var app = builder.Build();

// Apply the ePAT store migration at startup (Elsa's own contexts self-migrate).
using (var scope = app.Services.CreateScope())
{
    using var epatDb = scope.ServiceProvider
        .GetRequiredService<IDbContextFactory<EpatRuntimeDbContext>>()
        .CreateDbContext();
    epatDb.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Pin the endpoint explicitly: Elsa pulls in NSwag, which otherwise hijacks the default route.
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SefazSp.Epat.Api v1");
});

// Map all resume endpoints
app.MapAgpecaspcResume();
app.MapDeat0050Resume();
app.MapInicalcResume();
app.MapIniciarAguardarNotificacaoResume();
app.MapIniciarNovoGraftResume();
app.MapPedidoDeVistasResume();
app.MapPocEpatProcessResume();

// Debug endpoint: run a workflow synchronously to inspect execution
app.MapDebugBscenvpc();

// Debug endpoint: start a real Elsa workflow instance (suspends on external event)
app.MapStartBscenvpcWorkflow();

// Debug endpoint: prove the iProcess builtins shim against the behavioral oracle
app.MapDebugBuiltins();

// Service template (Batch 1): start + operator resume for the 5 service subprocesses
app.MapStartServiceTemplate();
app.MapManipularExcecaoResume();

// DEAT0050 (Batch 2): start + INICALC external-event resume
app.MapStartDeat0050();
app.MapDeat0050InicalcResume();

// AGPECASPC (Batch 2): start + interposições external-event resume (races the boundary timer)
app.MapStartAgpecaspc();
app.MapAgpecaspcInterposicoesResume();

// SEG006 (Batch 3): start + 'Finalizar AIIM' resume (fires the parallel AND-split)
app.MapStartSeg006();
app.MapSeg006FinalizarAiimResume();

// CONTROPC (Batch 3 Wave 2): start dynamic-subprocess resolution by AGUARDAR
app.MapStartControlopc();

// GRAFT (Batch 3 Wave 2): parent park + child attach/complete + close valve (correlation-join)
app.MapGraftDemo();

// POC_EpatProcess main flow (Batch 3 Wave 3): start + 5 external-event resumes (SC-001 path)
app.MapPocEpatMain();

// fundacao-motor-de-regras (Phase 4): Decisions rules engine showcase
app.MapDecisionsEvaluate();

app.Run();

// Exposed so the restart-recovery tests can boot the real composition via WebApplicationFactory.
public partial class Program;
