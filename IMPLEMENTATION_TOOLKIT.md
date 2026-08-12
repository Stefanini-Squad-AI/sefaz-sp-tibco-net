# TIBCO to .NET Migration Toolkit Guide

## 1. Framework Selection Reference

### 1.1 Workflow Orchestration: Workflow Core vs. NServiceBus

#### **Workflow Core** (Recommended for this project) ⭐
```csharp
// Simple, lightweight, purpose-built for XPDL-like workflows
// Advantages:
// - Direct XPDL conversion path
// - Simple learning curve
// - In-process or distributed
// - Perfect for state machine workflows

public class EpatWorkflow : IWorkflow
{
    public string Id => "epat-workflow";
    public int Version => 1;

    public void Build(IWorkflowBuilder<MyData> builder)
    {
        builder
            .StartWith(context => ExecutionResult.Next())
            .Then("verify-notification", context => {
                // VerificarRetornoDecisions equivalent
                return ExecutionResult.Next();
            })
            .Then("update-status", context => {
                // alterarStatusIntimacao equivalent
                return ExecutionResult.Next();
            })
            .Decision(context => context.Data.RequiresApproval 
                ? "approval-step" 
                : "notify-user")
            .Then("approval-step", context => ExecutionResult.Next())
            .Then("notify-user", context => ExecutionResult.Next())
            .EndWorkflow();
    }
}

// Installation
dotnet add package WorkflowCore
```

**Use Case:** Perfect for SEFAZ-SP EPAT processes  
**License:** MIT (Open Source)  
**GitHub:** https://github.com/danielgerlag/workflow-core

---

#### **NServiceBus** (Alternative for complex scenarios)
```csharp
// Enterprise-grade orchestration with pub/sub, sagas, and routing
// Advantages:
// - Complex choreography scenarios
// - Built-in retry/error handling
// - Long-running saga patterns
// - Excellent monitoring

// Command Handler
public class StartEpatProcessHandler : IHandleMessages<StartEpatProcess>
{
    private readonly IMessageSession _messageSession;

    public async Task Handle(StartEpatProcess message, IMessageHandlerContext context)
    {
        // Handle process initiation
        await context.SendLocal(new VerifyNotificationCommand { /* ... */ });
    }
}

// Installation
dotnet add package NServiceBus
```

**Use Case:** For highly distributed or event-driven scenarios  
**License:** AGPL open-source (commercial license available)

---

### 1.2 Business Rules Engine: NRules (BEST FIT)

```csharp
// Direct replacement for Corticon rules engine
// Advantages:
// - Strong typing with C# LINQ
// - Pattern matching
// - Rule priority & salience
// - Fast rule execution
// - Easy to debug

using NRules.Fluent.Dsl;

public class IntimacaoRules : RuleSet
{
    public override void Define()
    {
        // Rule 1: Intimação tipo 2 com vício de representação
        Rule()
            .When()
                .Match<ResultadoJulgamento>(r => 
                    r.Request.MotivoIntimacao == "2" &&
                    r.Request.VicioRepresentacao == "1" &&
                    r.Request.Origem == "NA")
            .Then(ctx => 
            {
                var decisao = new DecisaoIntimacao 
                { 
                    Tipo = "VerificacaoRepresentacao",
                    DataDecisao = DateTime.UtcNow,
                    RequiresManualReview = true
                };
                ctx.Insert(decisao);
            })
            .Name("Regra-Intimacao-Vicio-Representacao")
            .Priority(100);
    }
}

// Usage
var factory = new RuleRepositoryFactory();
var repository = factory.CreateRepository(typeof(IntimacaoRules));
var engine = new RuleEngine(repository);

var context = engine.CreateContext();
context.Insert(resultadoJulgamento);
context.Fire();

var decisions = context.Query<DecisaoIntimacao>().ToList();
```

**Installation:**
```bash
dotnet add package NRules
dotnet add package NRules.RuleModel
```

**Reference:** https://github.com/NRules/NRules

---

### 1.3 Data Mapping: AutoMapper (Essential)

```csharp
// Auto-map WSDL-generated classes to domain models
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // WSDL DTO → Domain Entity
        CreateMap<BuscarInspetorPorAFRRequest, BuscarInspetorCommand>();
        
        CreateMap<InspetorResponse, InspetorEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.InspectorId))
            .ForMember(dest => dest.NomeInspector, opt => opt.MapFrom(src => src.Nome));

        // Reverse mapping for responses
        CreateMap<InspetorEntity, InspetorResponse>().ReverseMap();
        
        // Complex nested mappings
        CreateMap<AiimWebResponse, AiimEntity>()
            .ForMember(dest => dest.Pecas, 
                opt => opt.MapFrom(src => src.ListaPecas.Select(p => 
                    new PecaEntity 
                    { 
                        Tipo = p.TipoPeca,
                        Descricao = p.Descricao 
                    })));
    }
}

// Registration in Startup
services.AddAutoMapper(typeof(MappingProfile));

// Usage in Service
public class EpatService : IEpatService
{
    private readonly IMapper _mapper;
    
    public EpatService(IMapper mapper) => _mapper = mapper;
    
    public async Task<BuscarInspetorResponse> BuscarInspetorPorAFRAsync(
        BuscarInspetorPorAFRRequest request)
    {
        var command = _mapper.Map<BuscarInspetorCommand>(request);
        var result = await _repository.FindInspetoresAsync(command);
        return _mapper.Map<BuscarInspetorResponse>(result);
    }
}
```

**Installation:**
```bash
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

---

### 1.4 HTTP Client: Refit (For external service calls)

```csharp
// Type-safe HTTP client for calling legacy TIBCO services
using Refit;

[Headers("Accept: application/json", "Content-Type: application/json")]
public interface ILegacyEpatApi
{
    [Get("/epat/verificar-retorno")]
    Task<VerificarRetornoResponse> VerificarRetornoAsync(
        [Query] string processId,
        CancellationToken cancellationToken = default);

    [Post("/epat/alterar-status")]
    Task<AlterarStatusResponse> AlterarStatusAsync(
        [Body] AlterarStatusRequest request,
        CancellationToken cancellationToken = default);
}

// Registration
services.AddRefitClient<ILegacyEpatApi>()
    .ConfigureHttpClient(c => c.BaseAddress = 
        new Uri("https://legacy-tibco-services.sefaz.sp.gov.br"));

// Usage
var response = await _legacyApi.VerificarRetornoAsync(processId);
```

**Installation:**
```bash
dotnet add package Refit
```

---

## 2. Code Generation Tools

### 2.1 WSDL to C# Classes: ServiceReference

```bash
# Modern replacement for "Add Service Reference" in Visual Studio
dotnet add package dotnet-svcutil.xmlserializer --version 2.0

# Generate proxies from WSDL
dotnet-svcutil https://epat-service.sefaz.sp.gov.br/EPAT.wsdl \
    --outputDir ./Generated/ServiceProxies \
    --namespace Fazenda.ePAT.ServiceProxies
```

**Alternative: xsd.exe**
```bash
# For XSD schema to C# classes
xsd.exe EPAT.wsdl /l:CS /n:Fazenda.ePAT.ServiceProxies

# Then manually refactor to modern async patterns
```

### 2.2 XSD to Entity Framework: T4 Templates

```csharp
// Entity.tt (T4 Template for generating Entity classes)
<#@ template debug="false" hostSpecific="true" language="C#" #>
<#@ output extension=".cs" #>
<#@ import namespace="System.Xml" #>
<#@ import namespace="System.Xml.Linq" #>
<#
    XDocument doc = XDocument.Load("schema.xsd");
    var elements = doc.Descendants("{http://www.w3.org/2001/XMLSchema}element");
#>

namespace Fazenda.ePAT.Entities.Generated
{
<# foreach(var elem in elements) { #>
    /// <summary>
    /// Auto-generated from XSD: <#= elem.Attribute("name")?.Value #>
    /// </summary>
    public class <#= elem.Attribute("name")?.Value #>
    {
        <# 
        var complexType = elem.Element("{http://www.w3.org/2001/XMLSchema}complexType");
        if(complexType != null) {
            var sequence = complexType.Element("{http://www.w3.org/2001/XMLSchema}sequence");
            foreach(var child in sequence?.Elements("{http://www.w3.org/2001/XMLSchema}element") ?? Enumerable.Empty<XElement>()) {
        #>
        public <#= child.Attribute("type")?.Value ?? "string" #> <#= child.Attribute("name")?.Value #> { get; set; }
        <# } } #>
    }
<# } #>
}
```

**Installation:** Built into Visual Studio

### 2.3 XPDL Parser: Custom Implementation

```csharp
// Parse XPDL and convert to Workflow Core
using System.Xml;
using System.Xml.Linq;

public class XpdlParser
{
    public WorkflowDefinition ParseXpdl(string xpdlPath)
    {
        var doc = XDocument.Load(xpdlPath);
        var ns = XNamespace.Get("http://www.wfmc.org/2008/XPDL2.1");
        
        var package = doc.Root.Element(ns + "Package");
        var processes = package.Elements(ns + "WorkflowProcesses")
            .Elements(ns + "WorkflowProcess")
            .ToList();

        var workflow = new WorkflowDefinition();

        foreach (var process in processes)
        {
            var processId = process.Attribute("Id")?.Value;
            var activities = process.Elements(ns + "Activities")
                .Elements(ns + "Activity")
                .ToList();

            var steps = activities.Select(activity => new WorkflowStepDefinition
            {
                Id = activity.Attribute("Id")?.Value,
                Name = activity.Attribute("Name")?.Value,
                Type = DetermineActivityType(activity)
            }).ToList();

            var transitions = ParseTransitions(process, ns);
            
            workflow.AddProcess(new ProcessDefinition 
            { 
                Id = processId, 
                Steps = steps,
                Transitions = transitions
            });
        }

        return workflow;
    }

    private List<Transition> ParseTransitions(XElement process, XNamespace ns)
    {
        return process.Elements(ns + "Transitions")
            .Elements(ns + "Transition")
            .Select(t => new Transition
            {
                From = t.Attribute("From")?.Value,
                To = t.Attribute("To")?.Value,
                Condition = ParseCondition(t, ns)
            })
            .ToList();
    }

    private string DetermineActivityType(XElement activity)
    {
        // Determine: UserTask, ServiceTask, AutomaticTask, Decision, etc.
        if (activity.Element(ns + "Implementation")
            ?.Element(ns + "Task") != null)
            return "UserTask";
        
        // ... more logic
        return "AutomaticTask";
    }
}
```

---

## 3. Project Structure Template

```
Solution: Fazenda.ePAT.Migration/
├── Fazenda.ePAT.Core/                    # Domain models & entities
│   ├── Entities/
│   │   ├── Aiim.cs
│   │   ├── Intimacao.cs
│   │   └── Peca.cs
│   ├── ValueObjects/
│   ├── Enums/
│   └── Fazenda.ePAT.Core.csproj
│
├── Fazenda.ePAT.Workflows/               # Workflow orchestration
│   ├── Workflows/
│   │   ├── EpatWorkflow.cs
│   │   ├── IntimacaoWorkflow.cs
│   │   └── VerificacaoWorkflow.cs
│   ├── Steps/
│   │   ├── VerifyReturnStep.cs
│   │   ├── UpdateStatusStep.cs
│   │   └── NotifyUserStep.cs
│   └── Fazenda.ePAT.Workflows.csproj
│
├── Fazenda.ePAT.Rules/                   # Business rules (NRules)
│   ├── RuleSets/
│   │   ├── IntimacaoRules.cs
│   │   ├── QualificacaoPecasRules.cs
│   │   └── DistribuicaoRules.cs
│   └── Fazenda.ePAT.Rules.csproj
│
├── Fazenda.ePAT.Services/                # Service layer
│   ├── EpatService.cs
│   ├── IntimacaoService.cs
│   ├── Contracts/
│   │   ├── IBuscarInspetorService.cs
│   │   └── IAlterarStatusService.cs
│   └── Fazenda.ePAT.Services.csproj
│
├── Fazenda.ePAT.Data/                    # Data access
│   ├── Context/
│   │   └── EpatDbContext.cs
│   ├── Repositories/
│   │   ├── AiimRepository.cs
│   │   └── IntimacaoRepository.cs
│   ├── Migrations/
│   └── Fazenda.ePAT.Data.csproj
│
├── Fazenda.ePAT.Web/                     # ASP.NET Core UI
│   ├── Controllers/
│   │   ├── EpatController.cs
│   │   ├── AiimController.cs
│   │   └── IntimacaoController.cs
│   ├── Pages/
│   │   ├── PAT/
│   │   │   ├── FechamentoAIIM.cshtml
│   │   │   ├── FechamentoAIIM.cshtml.cs
│   │   │   └── Index.cshtml
│   │   └── Shared/
│   ├── Models/
│   │   ├── FechamentoViewModel.cs
│   │   └── IntimacaoViewModel.cs
│   ├── appsettings.json
│   ├── Startup.cs (or Program.cs for Minimal APIs)
│   └── Fazenda.ePAT.Web.csproj
│
├── Fazenda.ePAT.Api/                     # REST API (optional separate project)
│   ├── Controllers/
│   ├── Middleware/
│   └── Fazenda.ePAT.Api.csproj
│
├── Fazenda.ePAT.Migration/               # TIBCO → .NET converters
│   ├── XpdlParser.cs
│   ├── WsdlConverter.cs
│   ├── CorticonConverter.cs
│   └── Fazenda.ePAT.Migration.csproj
│
├── Tests/
│   ├── Fazenda.ePAT.Workflows.Tests/
│   ├── Fazenda.ePAT.Rules.Tests/
│   ├── Fazenda.ePAT.Services.Tests/
│   └── Fazenda.ePAT.Integration.Tests/
│
└── Fazenda.ePAT.sln
```

---

## 4. Dependency Injection Setup (Program.cs)

```csharp
// Program.cs - ASP.NET Core 6+ Minimal API style

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services
    // Workflow Core
    .AddWorkflowEngine()
    .AddWorkflow<EpatWorkflow>()
    .AddWorkflow<IntimacaoWorkflow>()
    
    // NRules
    .AddSingleton<IRuleEngine>(sp => {
        var factory = new RuleRepositoryFactory();
        var repository = factory.CreateRepository(
            typeof(IntimacaoRules),
            typeof(QualificacaoPecasRules),
            typeof(DistribuicaoRules)
        );
        return new RuleEngine(repository);
    })
    
    // Entity Framework
    .AddDbContext<EpatDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
    
    // Repositories
    .AddScoped<IAiimRepository, AiimRepository>()
    .AddScoped<IIntimacaoRepository, IntimacaoRepository>()
    
    // Services
    .AddScoped<IEpatService, EpatService>()
    .AddScoped<IIntimacaoService, IntimacaoService>()
    
    // Mapping
    .AddAutoMapper(typeof(MappingProfile))
    
    // HTTP Clients
    .AddHttpClient<ILegacyEpatClient>()
    
    // Controllers
    .AddControllers();

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Middleware
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Workflow background processor
app.Services.GetRequiredService<IWorkflowEngine>().StartAsync(app.Services);

app.Run();
```

---

## 5. Testing Framework Setup

```csharp
// Using xUnit + Moq

[Collection("Workflow Tests")]
public class EpatWorkflowTests
{
    private readonly IWorkflowEngine _engine;

    public EpatWorkflowTests()
    {
        _engine = new WorkflowEngine();
    }

    [Fact]
    public async Task ShouldStartEpatWorkflow()
    {
        // Arrange
        var workflow = new EpatWorkflow();

        // Act
        var instance = await _engine.StartWorkflow<EpatWorkflow>();

        // Assert
        Assert.NotNull(instance);
    }
}

[Collection("Rules Tests")]
public class IntimacaoRulesTests
{
    private readonly IRuleEngine _engine;

    public IntimacaoRulesTests()
    {
        var factory = new RuleRepositoryFactory();
        var repository = factory.CreateRepository(typeof(IntimacaoRules));
        _engine = new RuleEngine(repository);
    }

    [Fact]
    public void ShouldFireIntimacaoVicioRepresentacao()
    {
        // Arrange
        var resultadoJulgamento = new ResultadoJulgamento
        {
            Request = new Request
            {
                MotivoIntimacao = "2",
                VicioRepresentacao = "1",
                Origem = "NA"
            }
        };

        var context = _engine.CreateContext();
        context.Insert(resultadoJulgamento);

        // Act
        context.Fire();

        // Assert
        var decisions = context.Query<DecisaoIntimacao>().ToList();
        Assert.Single(decisions);
        Assert.Equal("VerificacaoRepresentacao", decisions[0].Tipo);
    }
}
```

---

## 6. NuGet Packages Checklist

```xml
<!-- Common packages for TIBCO migration -->
<ItemGroup>
    <!-- Core Framework -->
    <PackageReference Include="Microsoft.NET.Sdk.Web" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    
    <!-- Orchestration -->
    <PackageReference Include="WorkflowCore" Version="5.15.0" />
    <PackageReference Include="NServiceBus" Version="8.1.0" />
    
    <!-- Rules Engine -->
    <PackageReference Include="NRules" Version="2.2.1" />
    
    <!-- Mapping & DTOs -->
    <PackageReference Include="AutoMapper" Version="13.0.1" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    
    <!-- HTTP Clients -->
    <PackageReference Include="Refit" Version="7.0.0" />
    <PackageReference Include="Refit.HttpClientFactory" Version="7.0.0" />
    
    <!-- API Documentation -->
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    
    <!-- Logging & Monitoring -->
    <PackageReference Include="Serilog" Version="3.1.1" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="ApplicationInsights" Version="2.22.4" />
    
    <!-- Testing -->
    <PackageReference Include="xunit" Version="2.6.6" />
    <PackageReference Include="Moq" Version="4.20.70" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
</ItemGroup>
```

---

## 7. Deployment Checklist

- [ ] Configure SQL Server connection strings
- [ ] Setup Entity Framework migrations
- [ ] Configure workflow persistence layer
- [ ] Deploy NRules rule repositories
- [ ] Configure logging (Serilog, App Insights)
- [ ] Setup API authentication (JWT/OAuth)
- [ ] Configure CORS if needed
- [ ] Performance testing
- [ ] Load testing
- [ ] UAT validation
- [ ] Production deployment

---

**Toolkit Version:** 1.0  
**Last Updated:** 2026-06-30
