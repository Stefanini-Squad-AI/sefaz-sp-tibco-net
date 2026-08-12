# QUICK REFERENCE: TIBCO → .NET Migration

## Executive Summary

**Project:** SEFAZ-SP EPAT (Electronic Labor Process)  
**Current:** TIBCO BusinessWorks + iProcess + Corticon Rules  
**Target:** .NET 8+ with ASP.NET Core  
**Timeline:** 13-19 weeks  
**Best Frameworks:** Workflow Core + NRules + Entity Framework Core  

---

## At-a-Glance Comparison

| Aspect | TIBCO | .NET |
|--------|-------|-----|
| Processes (XPDL) | TIBCO Designer | **Workflow Core** |
| Services (WSDL) | iProcess Agent | **ASP.NET Core APIs** |
| Rules (Corticon) | Rules Designer | **NRules** |
| Data (XSD) | Type System | **EF Core + C# Classes** |
| UI (GWT/Forms) | TIBCO Forms | **Razor Pages/MVC** |
| Operations | 140+ WSDL ops | **REST endpoints** |

---

## Critical Artifacts

| File | Type | Rows | Contains |
|------|------|------|----------|
| `POC_Epat.xpdl` | XPDL | ~2000 | Main process, 14+ subprocesses |
| `EPAT.wsdl` | WSDL | ~1000 | 140+ service operations |
| `intimacoes_Parametros.ers` | Rules | ~500 | Notification decision logic |
| `POC_Epat.bom` | Business Objects | ~300 | Entity definitions |
| FechamentoAIIM.aspx* | ASP.NET | ~200 | Sample UI page |

---

## Framework Recommendations (Ranked)

### Tier 1 - Essential (Required)
```
✅ Workflow Core        - Process orchestration
✅ NRules              - Business rules engine
✅ ASP.NET Core        - Web framework
✅ Entity Framework    - ORM/Data access
✅ AutoMapper          - DTO/Entity mapping
```

### Tier 2 - Highly Recommended
```
✓ Swagger/OpenAPI     - API documentation
✓ Serilog            - Structured logging
✓ xUnit              - Unit testing
✓ Moq                - Test mocking
```

### Tier 3 - Optional
```
○ NServiceBus        - For complex choreography
○ Refit              - For legacy service calls
○ MassTransit        - For event-driven scenarios
```

---

## Translation Patterns

### Pattern 1: XPDL Process → Workflow Core

```csharp
// FROM (XPDL):
// <xpdl2:Activity Id="VerificarRetorno" Name="Verify Return"/>
// <xpdl2:Transition From="Start" To="VerificarRetorno"/>

// TO (Workflow Core):
builder
    .StartWith(context => ExecutionResult.Next())
    .Then("verify-return", context => {
        // Implement verification logic
        return ExecutionResult.Next();
    });
```

### Pattern 2: WSDL Operation → ASP.NET API

```csharp
// FROM (WSDL):
// <operation name="buscarInspetorPorAFR">

// TO (ASP.NET):
[HttpPost("buscar-inspetor-por-afr")]
public async Task<IActionResult> BuscarInspetorPorAFR(
    BuscarInspetorRequest request)
{
    var result = await _service.BuscarInspetorAsync(request);
    return Ok(result);
}
```

### Pattern 3: Corticon Rule → NRules

```csharp
// FROM (Corticon):
// IF motivoIntimacao = '2' AND vicioRepresentacao = '1'
// THEN apply DecisionX

// TO (NRules):
Rule()
    .When()
        .Match<Intimacao>(i => 
            i.MotivoIntimacao == "2" && 
            i.VicioRepresentacao == "1")
    .Then(ctx => ctx.Insert(new Decisao { Tipo = "X" }))
    .Name("Regra-Vicio-Representacao");
```

---

## Implementation Phases

```
PHASE 1 (Weeks 1-4): Analysis & Conversion Tools
├─ XPDL Parser development
├─ WSDL Code generator setup
├─ Corticon rules converter
└─ Data model extraction

PHASE 2 (Weeks 5-10): Framework Implementation
├─ Workflow Core setup (weeks 5-6)
├─ NRules engine deployment (weeks 7-8)
├─ ASP.NET Core API layer (weeks 9-10)
└─ Database migration

PHASE 3 (Weeks 11-14): Integration & Testing
├─ UI modernization (weeks 11-12)
├─ Integration testing (weeks 13-14)
└─ Performance optimization

PHASE 4 (Weeks 15-19): Deployment
├─ UAT validation
├─ Load testing
└─ Production rollout
```

---

## Key Files to Create

```
Fazenda.ePAT.Migration/
├── XpdlParser.cs              ← Parse XPDL files
├── WsdlConverter.cs           ← Generate APIs from WSDL
├── CorticonConverter.cs       ← Convert .ers to NRules
├── WorkflowDefinitions/       ← Generated from XPDL
│   ├── EpatWorkflow.cs
│   └── IntimacaoWorkflow.cs
├── RuleDefinitions/           ← Generated from .ers
│   └── IntimacaoRules.cs
├── ServiceLayer/
│   ├── EpatService.cs
│   └── IntimacaoService.cs
└── Controllers/
    ├── EpatController.cs
    └── IntimacaoController.cs
```

---

## Installation Commands

```bash
# Create solution
dotnet new sln -n Fazenda.ePAT.Migration

# Create projects
dotnet new classlib -n Fazenda.ePAT.Core
dotnet new classlib -n Fazenda.ePAT.Workflows
dotnet new classlib -n Fazenda.ePAT.Rules
dotnet new classlib -n Fazenda.ePAT.Services
dotnet new classlib -n Fazenda.ePAT.Data
dotnet new webapi -n Fazenda.ePAT.Web

# Add projects to solution
dotnet sln add Fazenda.ePAT.Core/Fazenda.ePAT.Core.csproj
dotnet sln add Fazenda.ePAT.Workflows/Fazenda.ePAT.Workflows.csproj
dotnet sln add Fazenda.ePAT.Rules/Fazenda.ePAT.Rules.csproj
dotnet sln add Fazenda.ePAT.Services/Fazenda.ePAT.Services.csproj
dotnet sln add Fazenda.ePAT.Data/Fazenda.ePAT.Data.csproj
dotnet sln add Fazenda.ePAT.Web/Fazenda.ePAT.Web.csproj

# Add NuGet packages
cd Fazenda.ePAT.Web
dotnet add package WorkflowCore
dotnet add package NRules
dotnet add package AutoMapper
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog
```

---

## Decision Matrix

**Choose Workflow Core if:**
- ✓ You need XPDL-like state machines
- ✓ Relatively simple process flows
- ✓ Want easy debugging
- ✓ Prefer lightweight solution

**Choose NServiceBus if:**
- ✓ Complex choreography required
- ✓ Event-driven architecture needed
- ✓ Long-running sagas involved
- ✓ Distributed systems

**Choose NRules for business rules:**
- ✓ Replaces Corticon engine
- ✓ Strong C# typing
- ✓ Fast execution
- ✓ Easy to maintain

---

## Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Complex XPDL conversion | High | Build parser prototype first |
| 140+ WSDL operations | High | Automate code generation |
| Corticon rules mapping | Medium | Create rule conversion toolkit |
| Database schema changes | Medium | Plan migration incrementally |
| Team skill gap | Medium | Training on .NET frameworks |
| Legacy system integration | Low | Use Refit for proxies |

---

## Success Criteria

- [ ] All XPDL processes converted to Workflow Core
- [ ] All 140+ WSDL operations exposed as REST APIs
- [ ] All Corticon rules executing in NRules
- [ ] Data integrity validated (no data loss)
- [ ] Performance ≥ TIBCO baseline
- [ ] UAT sign-off from stakeholders
- [ ] Monitoring & alerting configured
- [ ] Documentation complete

---

## Performance Expectations

| Metric | TIBCO | .NET Core | Target |
|--------|-------|----------|--------|
| API Response | ~200ms | ~50ms | < 100ms |
| Rules Execution | ~100ms | ~10ms | < 50ms |
| Workflow Start | ~500ms | ~100ms | < 200ms |
| Throughput | 100 ops/s | 1000+ ops/s | > 500 ops/s |
| Memory Footprint | 2-3GB | 500MB-1GB | < 1.5GB |

---

## Support Resources

**Documentation:**
- Workflow Core: https://github.com/danielgerlag/workflow-core/wiki
- NRules: https://nrules.readthedocs.io/
- ASP.NET Core: https://learn.microsoft.com/en-us/aspnet/core/

**Community:**
- .NET Foundation: https://dotnetfoundation.org/
- Stack Overflow: #workflow-core, #nrules, #asp.net-core
- GitHub Discussions: Each framework repository

**Training:**
- Microsoft Learn: Free online modules
- Pluralsight: .NET-specific courses
- LinkedIn Learning: Architecture patterns

---

## Next Steps

1. **Week 0:** Review this analysis with team
2. **Week 0:** Decide between Workflow Core vs NServiceBus
3. **Week 1:** Build XPDL parser prototype
4. **Week 2:** Create sample WSDL conversion
5. **Week 3:** Prototype NRules rule converter
6. **Week 4:** Present Phase 1 results to stakeholders

---

**Created:** 2026-06-30  
**Status:** Ready for Project Kickoff  
**Confidence Level:** HIGH (Based on direct code analysis)
