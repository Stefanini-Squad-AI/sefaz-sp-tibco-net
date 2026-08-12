# TIBCO to .NET Transformation Analysis - SEFAZ-SP Project

**Project Date:** 2026-05-28  
**Language:** Portuguese (pt_BR)  
**Target System:** EPAT (Electronic Process for Labor Administrative Proceedings)  
**Current Platform:** TIBCO BusinessWorks / iProcess  
**Target Platform:** .NET (ASP.NET)

---

## 1. PROJECT OVERVIEW

This is a **Proof of Concept (PoC)** for migrating SEFAZ-SP (State Tax Authority of São Paulo) systems from TIBCO BusinessWorks to .NET. The project focuses on:

- **EPAT System**: Electronic Labor Process Management
- **AIIM Process**: Administrative Inspection Report workflow
- **Notifications Management**: Intimações (Legal Notifications)
- **Decision Engine**: TIBCO rules integrated with ASP.NET frontend

---

## 2. CURRENT ARCHITECTURE - TIBCO Stack

### 2.1 Process Layer (XPDL)
- **Format:** XPDL 2.1 (XML Process Definition Language)
- **Engine:** TIBCO BusinessWorks
- **Main Process Package:** `POC_Epat.xpdl`
- **Key Processes:**
  - POC_EpatProcess
  - VerificarRetornoDecisions (Verify Return Decisions)
  - Multiple external packages (NotificacaoAIIM, EPAT_SEGUNDA, Calendario, etc.)

### 2.2 Service Layer (WSDL)
- **Format:** WSDL (Web Services Description Language)
- **Services:** TIBCO BusinessWorks iProcessServiceAgent
- **Key WSDL Endpoints:**
  - `EPAT.wsdl` - Core EPAT service operations
  - `DecisionsEPAT.wsdl` - Decision management services
  - Multiple schema definitions for input/output operations

**Operations Include:**
- `alterarStatusArquivoEntrada/Saida` (Alter File Status)
- `alterarStatusIntimacaoSaida` (Alter Notification Status)
- `buscarInspetorPorAFR` (Search Inspector by AFR)
- `buscarAreaSaida` (Search Area)
- `qualificarPecas` (Qualify Pieces)
- `buscarUltimaDataDefesa` (Search Last Defense Date)
- And many more administrative operations...

### 2.3 Business Rules Engine
- **Format:** Corticon Rules Engine (`.ers` file - Rulesheet Asset)
- **File:** `intimacoes_Parametros.ers`
- **Vocabulary:** `intimacoes.ecore`
- **Rules:**
  - Decision rules for notifications/intimações
  - Condition-based logic (e.g., `motivoIntimacao = '2'`, `vicioRepresentacao = '1'`)
  - Result mapping for judicial outcomes

### 2.4 Data & Business Objects
- **Format:** `.bom` (Business Object Model)
- **File:** `POC_Epat.bom`
- **Type System:** Strong typing with XPDL Type Declarations
- **Key Entities:**
  - AiimWeb.TB_AIIM (Administrative Inspection Report)
  - NR_AIIM (Report Number)
  - NR_RAT (Ratification Number)
  - Custom complex types for requests/responses

### 2.5 Forms & UI Components
- **GWT-based Forms:** `.form.xslt` files for presentation
- **Presentation Resources:**
  - `.properties` and `.properties.json` (Localization)
  - `.locales.json` (Multi-language support)
  - `.gwt.json` (GWT widget definitions)
- **Form Data Definitions:** `.data.json` files

---

## 3. TARGET ARCHITECTURE - .NET Stack

### 3.1 Current .NET Implementation
**Existing ASP.NET Components:**
- **Framework:** ASP.NET WebForms (with C#)
- **Target:** `Fazenda.ePAT.WebApp` (ePAT Web Application)
- **UI Pages:** `.aspx` files
- **Code-Behind:** `.aspx.cs` C# classes
- **Master Pages:** Internet.Master
- **User Controls:** Pecas.ascx, Cabecalho_AIIM.ascx, AdicionarPecas.ascx, etc.

**Example Page:**
- `FechamentoAIIM.aspx` - Close AIIM Process page
- Uses Facade pattern: `Br.Gov.Sp.Fazenda.ePAT.Facade`
- TIBCO entity integration: `Fazenda.ePAT.Entities.TIBCO`

**Namespace Structure:**
```
Fazenda.ePAT.WebApp
├── Facade (Business Logic)
├── Entities.TIBCO (Current TIBCO entity mapping)
├── Entities.TIBCO.General
└── Entities.TIBCO.WorkItem
```

---

## 4. KEY TRANSFORMATION CHALLENGES

### 4.1 Process Translation
- **XPDL → .NET Workflow:** No direct 1:1 conversion tool
- **Multiple External Packages:** 14+ linked XPDL files need orchestration
- **Complex Decision Points:** DecisionsEPAT logic must be replicated

### 4.2 Service & API Migration
- **WSDL → WebAPI/REST:** TIBCO WSDL (140+ operations) needs mapping to ASP.NET
- **Schema Transformation:** Complex XSD schemas to .NET classes
- **iProcess Integration:** TIBCO-specific service patterns need replication

### 4.3 Rules Engine Migration
- **Corticon to .NET:** Rules (`.ers` files) need conversion to:
  - C# business logic classes
  - Rule engines like NRules or Drools for .NET
  - Or hardcoded decision trees

### 4.4 Data Model Mapping
- **Type System:** XPDL type declarations → C# classes
- **Legacy Entities:** TIBCO entities need reverse-engineered .NET classes
- **Database Synchronization:** Unknown DB schema

---

## 5. RECOMMENDED FRAMEWORKS & TOOLS

### 5.1 XPDL/BPM Translation
| Framework | Purpose | Recommendation |
|-----------|---------|---|
| **Camunda Modeler** | XPDL editing & validation | ✓ Excellent for BPM analysis |
| **Workflow Core** | .NET workflow engine | ⭐ Best option for .NET |
| **Orleans** | Actor model / distributed workflows | Good for async processes |
| **NServiceBus** | Orchestration & choreography | ✓ Enterprise patterns |
| **MassTransit** | Message-based workflows | ✓ For event-driven design |

### 5.2 WSDL/API Migration
| Tool | Purpose | Recommendation |
|------|---------|---|
| **WCF (Windows Communication Foundation)** | Legacy WSDL hosting | ⚠️ Deprecated, use ASP.NET Core |
| **ServiceReference (Visual Studio)** | Auto-generate proxy classes | ✓ Good for legacy services |
| **OpenAPI/Swagger** | Modern API documentation | ⭐ Best for new APIs |
| **Refit** | Type-safe HTTP client | ✓ For calling external services |
| **AutoMapper** | WSDL schemas ↔ .NET models | ✓ For data mapping |

### 5.3 Rules Engine Conversion
| Framework | Purpose | Recommendation |
|-----------|---------|---|
| **NRules** | .NET rules engine | ⭐ Best Corticon alternative |
| **Rules Engine** (Jint-based) | JSON rule definitions | Good for simple rules |
| **Drools for .NET** | Port of Java Drools | ✓ For complex logic |
| **Roslyn** | C# code generation | ✓ For dynamic rule compilation |

### 5.4 Project Structure Extraction
| Tool | Purpose | Recommendation |
|------|---------|---|
| **TIBCO EMS Parser** | Extract XPDL metadata | ✓ Direct parsing |
| **XML to Entity Generator** | Schema → C# classes | ✓ Automation |
| **Liquid/Scriban** | Template-based code generation | ⭐ For bulk translation |
| **T4 Templates** | Visual Studio code generation | ✓ Built-in solution |

### 5.5 Complete Migration Toolkit
| Framework | Purpose | Recommendation |
|-----------|---------|---|
| **TIBCO to .NET Migrator (Custom)** | End-to-end converter | ⭐⭐⭐ BEST OPTION |
| **Workflowgen** | No-code BPM for .NET | ✓ Commercial alternative |
| **Camunda 8 .NET SDK** | Modern BPM on .NET | ⭐ Next-gen approach |

---

## 6. RECOMMENDED MIGRATION STRATEGY

### Phase 1: Analysis & Design
```
1. Parse all XPDL files → Extract process definitions
2. Extract WSDL operations → Create .NET service contracts
3. Analyze Corticon rules → Design rule engine approach
4. Map data models → Create C# entity classes
5. Design new API surface (REST/gRPC)
```

### Phase 2: Core Framework Setup
```
1. Choose Workflow Core or NServiceBus for process orchestration
2. Setup NRules or similar for business rules
3. Create .NET Core/ASP.NET Core web API layer
4. Migrate database schema (if needed)
5. Implement service contracts
```

### Phase 3: Automated Conversion
```
1. Build XPDL parser → Convert to Workflow Core definitions
2. Build WSDL parser → Auto-generate .NET service classes
3. Convert rules → Generate NRules vocabulary & rules
4. Transform data models → Generate entities & mappings
```

### Phase 4: UI Migration
```
1. Modernize ASP.NET WebForms → ASP.NET Core MVC/Razor Pages
2. Add API consumption layer
3. Migrate GWT forms → JavaScript/React/Angular (if needed)
```

---

## 7. PROOF OF CONCEPT RECOMMENDATIONS

### Build an MVP Converter Tool
```csharp
public class TibcoToNetConverter
{
    // Parse XPDL and extract processes
    public WorkflowDefinition ParseXpdl(string xpdlPath) { }
    
    // Generate .NET workflow code
    public string GenerateWorkflowCode(WorkflowDefinition def) { }
    
    // Parse WSDL and generate service contracts
    public ServiceContract ParseWsdl(string wsdlPath) { }
    
    // Generate NRules vocabulary and rules from Corticon
    public RuleEngine ConvertCorticonRules(string ersPath) { }
}
```

### Technology Stack
- **Language:** C# / .NET 8+
- **Workflow:** Workflow Core + NServiceBus
- **Rules:** NRules
- **API:** ASP.NET Core Minimal APIs or MVC
- **Database:** SQL Server (likely, based on SEFAZ context)
- **Code Generation:** T4 Templates or Roslyn Analyzers

---

## 8. KEY ARTIFACTS DISCOVERED

| File | Type | Purpose |
|------|------|---------|
| `POC_Epat.xpdl` | XPDL | Main process definition |
| `EPAT.wsdl` | WSDL | Service contracts (140+ ops) |
| `DecisionsEPAT.wsdl` | WSDL | Decision service |
| `intimacoes_Parametros.ers` | Corticon | Business rules |
| `POC_Epat.bom` | Business Objects | Data model |
| `FechamentoAIIM.aspx*` | ASP.NET | UI (example) |
| 14+ External `.xpdl` files | XPDL | Sub-processes |

---

## 9. MIGRATION EFFORT ESTIMATION

| Component | Effort | Complexity |
|-----------|--------|-----------|
| XPDL → Workflow Core | 3-4 weeks | High |
| WSDL → ASP.NET APIs | 2-3 weeks | Medium |
| Corticon → NRules | 2-3 weeks | High |
| Data Model Migration | 1-2 weeks | Low-Medium |
| UI Modernization | 3-4 weeks | Medium |
| Testing & Integration | 2-3 weeks | Medium |
| **Total Estimated** | **13-19 weeks** | **—** |

---

## 10. NEXT STEPS

1. **Immediate:** Evaluate Workflow Core vs. NServiceBus for your use case
2. **Short-term:** Build XPDL parser prototype to extract process metadata
3. **Prototype:** Create sample converter for one simple XPDL process
4. **Design:** Document new .NET service architecture
5. **Implement:** Phase 1 (Phase 2 onwards with full team)

---

## 11. RESOURCES & REFERENCES

### .NET Frameworks
- **Workflow Core:** https://github.com/danielgerlag/workflow-core
- **NRules:** https://github.com/NRules/NRules
- **NServiceBus:** https://particular.net/nservicebus
- **Camunda .NET SDK:** https://github.com/camunda-community-hub/camunda-platform-dotnet

### TIBCO/XPDL
- **XPDL 2.1 Standard:** http://www.wfmc.org/standards/bpmnxpdl_31.xsd
- **Camunda Modeler:** https://camunda.com/download/modeler/
- **WSDL Specification:** https://www.w3.org/TR/wsdl/

### Code Generation
- **T4 Text Templates:** https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates
- **Roslyn Analyzers:** https://learn.microsoft.com/en-us/visualstudio/extensibility/getting-started-with-roslyn-analyzers
- **Liquid/Scriban:** https://github.com/scriban/scriban

---

**Document Prepared:** 2026-06-30  
**Analysis Type:** Architecture Assessment for TIBCO to .NET Migration  
**Status:** Ready for Implementation Planning
