# Architecture Comparison: TIBCO vs .NET Target

## Current TIBCO Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      TIBCO BusinessWorks                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Process Layer (XPDL 2.1)                                │  │
│  │ ├─ POC_Epat.xpdl (Main Package)                         │  │
│  │ ├─ NotificacaoAIIM.xpdl (Sub-process)                  │  │
│  │ ├─ EPAT_SEGUNDA.xpdl                                    │  │
│  │ ├─ Decisions.xpdl → DecisionsEPAT.wsdl                 │  │
│  │ ├─ Calendario.xpdl                                      │  │
│  │ ├─ GED.xpdl (Document Management)                       │  │
│  │ └─ [+10 more sub-processes]                             │  │
│  └──────────────────────────────────────────────────────────┘  │
│                           ↓ (Orchestrates)                      │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Service Layer (WSDL / iProcess)                         │  │
│  │ ├─ EPAT.wsdl (140+ operations)                          │  │
│  │ │  ├─ alterarStatusArquivo (Alter File Status)          │  │
│  │ │  ├─ alterarStatusIntimacao (Alter Notification)       │  │
│  │ │  ├─ buscarInspetorPorAFR (Search Inspector)           │  │
│  │ │  ├─ qualificarPecas (Qualify Pieces)                  │  │
│  │ │  └─ [+136 more operations]                            │  │
│  │ └─ DecisionsEPAT.wsdl                                    │  │
│  └──────────────────────────────────────────────────────────┘  │
│                           ↓                                      │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Business Rules Engine (Corticon)                        │  │
│  │ ├─ intimacoes_Parametros.ers                            │  │
│  │ ├─ vocabulary: intimacoes.ecore                         │  │
│  │ └─ Rules: Decision trees for judicial outcomes          │  │
│  └──────────────────────────────────────────────────────────┘  │
│                           ↓                                      │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ Data Model Layer (Business Objects)                     │  │
│  │ ├─ POC_Epat.bom                                         │  │
│  │ ├─ Type System: XPDL Type Declarations                  │  │
│  │ ├─ AiimWeb.TB_AIIM (Administrative Report)              │  │
│  │ ├─ Custom complex types                                 │  │
│  │ └─ XSD Schemas (embedded in WSDL)                       │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                           ↓
            ┌──────────────┴──────────────┐
            ↓                             ↓
    ┌─────────────────┐        ┌─────────────────┐
    │  GWT Forms      │        │ ASP.NET Forms   │
    │ (.form.xslt)    │        │ (Existing UI)   │
    │ (Web UI)        │        │ (WebForms)      │
    └─────────────────┘        └─────────────────┘
            ↓                             ↓
        [Browser]                    [Browser]
```

---

## Target .NET Architecture

```
┌────────────────────────────────────────────────────────────────┐
│               ASP.NET Core / .NET 8+ Platform                  │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Presentation Layer (ASP.NET Core)                       │ │
│  │ ├─ Razor Pages / MVC Controllers                        │ │
│  │ ├─ API Controllers (REST/gRPC)                          │ │
│  │ ├─ JavaScript / React / Angular (optional)              │ │
│  │ └─ Localization (i18n support)                          │ │
│  └──────────────────────────────────────────────────────────┘ │
│                           ↓                                    │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ API Gateway / Service Layer                             │ │
│  │ ├─ REST Endpoints (converted from WSDL)                 │ │
│  │ ├─ Service Contracts (C# interfaces)                    │ │
│  │ ├─ AutoMapper (WSDL Schema → C# Classes)                │ │
│  │ ├─ Refit (HTTP client layer)                            │ │
│  │ └─ OpenAPI/Swagger documentation                        │ │
│  └──────────────────────────────────────────────────────────┘ │
│                           ↓                                    │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Business Logic / Orchestration                          │ │
│  │ ├─ Workflow Core / NServiceBus                          │ │
│  │ │  ├─ Process Orchestration (from XPDL)                 │ │
│  │ │  ├─ Message Routing & Choreography                    │ │
│  │ │  ├─ Saga Pattern for long-running workflows           │ │
│  │ │  └─ Event sourcing support                            │ │
│  │ ├─ NRules (Business Rules Engine)                       │ │
│  │ │  ├─ Corticon rules → NRules vocabulary                │ │
│  │ │  ├─ Rule execution with caching                       │ │
│  │ │  └─ Dynamic rule compilation                          │ │
│  │ └─ Service Facade / Domain Logic                        │ │
│  └──────────────────────────────────────────────────────────┘ │
│                           ↓                                    │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Data Access Layer                                       │ │
│  │ ├─ Entity Framework Core (ORM)                          │ │
│  │ ├─ Generated Entity Classes (from XSD/WSDL)             │ │
│  │ ├─ Repository Pattern                                   │ │
│  │ └─ Unit of Work Pattern                                 │ │
│  └──────────────────────────────────────────────────────────┘ │
│                           ↓                                    │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ Database Layer                                          │ │
│  │ ├─ SQL Server (likely)                                  │ │
│  │ ├─ Migrations (EF Core Migrations)                      │ │
│  │ └─ Stored Procedures (legacy integration)               │ │
│  └──────────────────────────────────────────────────────────┘ │
│                                                                │
└────────────────────────────────────────────────────────────────┘
                           ↓
            ┌──────────────┴──────────────┐
            ↓                             ↓
    ┌─────────────────┐        ┌─────────────────┐
    │  Web Browser    │        │  Mobile App     │
    │  (MVC/Razor)    │        │  (Consuming API)│
    └─────────────────┘        └─────────────────┘
```

---

## Mapping TIBCO Components → .NET

| TIBCO Component | Format | .NET Target | Technology |
|---|---|---|---|
| **XPDL Process** | XML | Workflow Definition | Workflow Core / NServiceBus |
| **WSDL Services** | XML | C# Service Contracts | WCF / ASP.NET Core APIs |
| **Corticon Rules** | .ers (XMI) | NRules Vocabulary | NRules Framework |
| **Business Objects** | .bom (XMI) | Entity Classes | EF Core / AutoMapper |
| **XSD Schemas** | XML | C# DTOs | xsd.exe / OpenAPI Generator |
| **GWT Forms** | .xslt | Razor Pages / MVC | ASP.NET Core |
| **Properties (i18n)** | .properties | Resource Files | .resx files / i18n lib |
| **Type Declarations** | XPDL embedded | C# Classes | Code Generation |

---

## Translation Matrix: XPDL → .NET Workflow Core

```
XPDL Element              →  Workflow Core Concept
─────────────────────────────────────────────────
<xpdl2:Package>           →  WorkflowDefinition
<xpdl2:Process>           →  WorkflowStepDefinition
<xpdl2:Activity>          →  Step (Execute/UserTask)
<xpdl2:Transition>        →  NextStepId / Conditions
<xpdl2:ExternalPackage>   →  SubWorkflow / Saga
<xpdl2:TypeDeclaration>   →  C# Class / Generic Type
<xpdl2:DataField>         →  Workflow Variable
Decision Point / Gateway  →  ConditionalStep / Switch
Service Task              →  ServiceStepDefinition
Manual Task               →  UserTask (ASP.NET form)
```

---

## Service Translation: WSDL → ASP.NET Core

### TIBCO WSDL Operation

```xml
<wsdl:operation name="buscarInspetorPorAFR">
  <wsdl:input message="ns127:buscarInspetorPorAFRRequest"/>
  <wsdl:output message="ns128:buscarInspetorPorAFRResponse"/>
</wsdl:operation>
```

### Target .NET Equivalent

```csharp
// Service Interface
public interface IEpatService
{
    Task<BuscarInspetorPorAFRResponse> BuscarInspetorPorAFRAsync(
        BuscarInspetorPorAFRRequest request,
        CancellationToken cancellationToken = default);
}

// API Controller
[ApiController]
[Route("api/epat")]
public class EpatController : ControllerBase
{
    private readonly IEpatService _service;
    
    [HttpPost("buscar-inspetor-por-afr")]
    public async Task<ActionResult<BuscarInspetorPorAFRResponse>> BuscarInspetorPorAFR(
        BuscarInspetorPorAFRRequest request)
    {
        var result = await _service.BuscarInspetorPorAFRAsync(request);
        return Ok(result);
    }
}

// DTO Classes (Auto-generated from XSD)
[DataContract]
public class BuscarInspetorPorAFRRequest
{
    [DataMember]
    public string AFR { get; set; }
    
    [DataMember]
    public DateTime DataInicio { get; set; }
}

[DataContract]
public class BuscarInspetorPorAFRResponse
{
    [DataMember]
    public InspetorInfo[] Inspetores { get; set; }
    
    [DataMember]
    public ResultInfo Result { get; set; }
}
```

---

## Rules Engine Translation: Corticon → NRules

### Corticon Rule (intimacoes_Parametros.ers)

```
IF ResultadoJulgamento.request.motivoIntimacao = '2'
   AND ResultadoJulgamento.request.vicioRepresentacao = '1'
   AND ResultadoJulgamento.request.origem = 'NA'
THEN [Apply Decision X]
```

### NRules Equivalent

```csharp
public class IntimacaoRules : RuleSet
{
    public override void Define()
    {
        Rule()
            .When()
                .Match<ResultadoJulgamento>(r => 
                    r.Request.MotivoIntimacao == "2" &&
                    r.Request.VicioRepresentacao == "1" &&
                    r.Request.Origem == "NA")
            .Then(ctx => ctx.Insert(new DecisaoIntimacao { 
                Tipo = "X",
                Timestamp = DateTime.UtcNow 
            }))
            .Name("Regra de Intimação tipo X");
    }
}

// Usage
var engine = new RuleEngine();
engine.LoadRules(typeof(IntimacaoRules));
var resultSet = engine.Fire(new[] { resultadoJulgamento });
```

---

## Development Workflow

### Phase 1: Conversion Tools Development
```
Week 1-2:  Build XPDL Parser
           └─ Extract processes, activities, transitions
           
Week 3:    Build WSDL Parser
           └─ Generate C# service contracts & DTOs
           
Week 4:    Build Corticon Converter
           └─ Parse .ers files → NRules code
```

### Phase 2: Framework Implementation
```
Week 5-6:  Setup Workflow Core
           └─ Implement orchestration layer
           
Week 7-8:  Implement NRules Engine
           └─ Deploy rules vocabulary & execution
           
Week 9-10: Create ASP.NET Core API
           └─ REST endpoints for all 140+ WSDL operations
```

### Phase 3: Data & UI Migration
```
Week 11:   Entity Framework Core Setup
           └─ Database mapping & migrations
           
Week 12-13: UI Modernization
            └─ Razor Pages / MVC conversion
            
Week 14:   Integration Testing
           └─ E2E workflow validation
```

### Phase 4: Deployment
```
Week 15-16: Performance Tuning
            └─ Caching, async optimization
            
Week 17-19: UAT & Production Deployment
            └─ Phased rollout, monitoring
```

---

## Key Advantages of .NET Migration

| Aspect | TIBCO | .NET Core |
|--------|-------|----------|
| **Performance** | Moderate | Excellent |
| **Cost** | High licensing | Free/low-cost |
| **Developer Availability** | Specialist-dependent | Large pool |
| **Cloud Support** | Limited | Azure native |
| **Scalability** | Vertical | Horizontal (containers) |
| **DevOps Integration** | Complex | Native (Docker, K8s) |
| **API Modernization** | REST wrapper needed | Native REST/gRPC |
| **Async Support** | Limited | First-class async/await |

---

**Document Version:** 1.0  
**Created:** 2026-06-30  
**Status:** Ready for Architecture Review
