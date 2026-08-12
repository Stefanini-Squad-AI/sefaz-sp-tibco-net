# Intermediate artifacts — TIBCO POC_Epat → .NET

Machine-readable, engine-agnostic capture of the TIBCO ActiveMatrix BPM / iProcess
solution. These four documents are the **only** input a .NET generation step should
need; the original XPDL / WSDL / Corticon files are not required downstream.

Artifacts live under `artifacts/<package>/`, driven by a manifest in
`config/packages/<package>.json`. Regenerate everything from the repository root:

```powershell
& .\tools\run-extraction.ps1 -Package POC_Epat
```

That runs five stages:

| Stage | What it does |
|---|---|
| **S0 Pin** | SHA-256 every source file, so input drift is detectable later |
| **S1 Extract** | the five generators, in dependency order (1 → 2, 1 → 3 → 4, {1,2} → 5) |
| **S2 Validate** | `validate-artifacts.ps1`, including source-coverage checks |
| **S3 Emit BPMN** | `emit-bpmn.ps1`, one diagram per scope, for analyst review |
| **S4 Emit DMN** | `emit-dmn.ps1`, then `verify-dmn-equivalence.ps1` proves it against the Corticon fold |

Non-zero exit means a failed invariant, a source-coverage gap, a count that drifted
from the baseline in the manifest, a broken reference in the emitted BPMN, or a DMN
that no longer reproduces Corticon.
Suitable as a CI gate. `-SkipValidation`, `-SkipBpmn` and `-SkipDmn` disable the later stages.

**Artifacts carry no timestamp.** Everything volatile — when the run happened,
which inputs, what hashes — lives in the sidecar `manifest.json`. Regenerating
from unchanged sources therefore produces byte-identical files, so any git diff
on an artifact means a real semantic change. (Verified: two consecutive runs
produce identical SHA-256 for all four.)

The generators can still be run individually; every path is a parameter. Use
`run-extraction.ps1` unless you are debugging one of them.

| Artifact | Source | Size | Content |
|---|---|---:|---|
| [process-model.json](POC_Epat/process-model.json) | `POC_Epat.xpdl` | 400 KB | 9 processes, 215 nodes, edges, scopes, hazards |
| [case-field-dictionary.json](POC_Epat/case-field-dictionary.json) | process-model | 259 KB | 209 case fields, CLR types, usage graph |
| [service-contracts.json](POC_Epat/service-contracts.json) | `EPAT.wsdl`, `DecisionsEPAT.wsdl` | 874 KB | 127 operations, flattened typed payloads |
| [decision-tables.json](POC_Epat/decision-tables.json) | `intimacoes_Parametros.ers` | 301 KB | 49 Corticon rules × 21 conditions |
| [review-dossier.json](POC_Epat/review-dossier.json) | the four above | 200 KB | open questions + the graph evidence to answer them |
| [bpmn/](POC_Epat/bpmn) | process-model + glossary | 14 files | BPMN 2.0 for Camunda Modeler, `isExecutable="false"` |
| [dmn/](POC_Epat/dmn) | decision-tables + glossary | 2 files | DMN 1.3 for Camunda Modeler, review copy + traceability mirror |
| [manifest.json](POC_Epat/manifest.json) | — | 2 KB | run metadata, source hashes, artifact hashes, counts |

The DMN files are named after the rulesheet, not after this package, and
`dmn/index.json` records which one is the reviewable copy. Nothing downstream
guesses the filename.

Two files are **not** generated and must not be overwritten by tooling:
`config/packages/<pkg>.json` (which sources belong to a package, plus the expected
count baseline) and `config/glossary/<pkg>.yaml` (business meaning, authored by an
analyst). The dossier generator seeds the glossary and refreshes its evidence
comments, but preserves every value a human has written.

---

## 0. Two identifier sets that are NOT case fields

Surfaced by `validate-artifacts.ps1`; both must be modelled separately from the
209 domain fields.

**Technical envelope (19), now declared.** `SW_MAINCASE`, `SW_MAINPROC`,
`SW_PARENTCASE`, `SW_CASENUM`, `SW_PARENTPROC`, `SW_HOSTNAME`, `DATETIME`,
`PROCESS_ID`, `STATUS_CODE`, `SERVICE_NAME`, `STERRORCODE`, `STERRORDESC`,
`DUMP`, `ISAPPERROR`, `ISTECHERROR`, `NUMAPPRETRIES`, `MAXRETRIES`, `OUTCOME`,
`PARTICIPANTE`. They are DataMapping targets absent from the XPDL declarations,
but the **TIBCO `.form` files declare every one of them with a type, direction and
length** — see `technicalFields[]`. Business payload always binds under `BODY/…`;
these bind under `HEADER/…` and `RESULT/…`, or are engine-supplied `IPESystemValues`.

> `SW_PARENTCASE` is passed into the **body** of `criarNotificacoesAIIM` — it is
> the correlation key by which graft-step children attach to a waiting parent.

They stay **out of `fields[]`** on purpose: they are not business data. What the
declarations settle:

| Identifier | Type | Direction | Consequence |
|---|---|---|---|
| `MAXRETRIES` | Integer | `IN` | a configured budget, never written by the process |
| `NUMAPPRETRIES` | Integer | `INOUT` | the application-level counter, mutated — declared only on `REALATVI` |
| `OUTCOME` | Text(10) | `INOUT` | the human's decision on the exception form, written back |
| `ISAPPERROR`, `ISTECHERROR` | Text(**1**) | `IN` | single-character flags, **not booleans** despite the `IS` prefix |

The five `MANEXC` / `MANEXCPC` forms are the manual exception-handling step of each
service template: the operator sees the error envelope (`DUMP`, `STERRORDESC`,
`SERVICE_NAME`) and sets `OUTCOME`, which steers the retry/escalation branch.

---

## 1. `process-model.json` — normalized process model

`$schema: sefaz-sp/tibco-intermediate/process-model/v1`

```
source, externalPackages, participants, typeDeclarations
processes[]
  id, name, displayName
  formalParameters[] { name, mode: IN|OUT|INOUT, dataType }
  dataFields[]       { name, isArray, dataType }
  scopes[]           { scope, scopeId, nodes[], edges[] }
derived { linkEdges[], signalEdges[], migrationHazards[] }
statistics
```

**Scopes.** `scope: "MAIN"` is the process body; every other scope is an XPDL
`ActivitySet` (an embedded sub-process). The `subProcessScope` node kind carries
`activitySetId`, which is the foreign key into the matching scope.

**Node kinds.** `startEvent`, `endEvent`, `gateway`, `userTask`, `serviceTask`,
`emailTask`, `scriptTask`, `receiveTask`, `callActivity`, `subProcessScope`,
`linkThrow`, `linkCatch`, `signalThrow`, `signalCatch`, `timerEvent`,
`intermediateEvent:<trigger>`.

Kind-specific payloads: `script{grammar,body}`, `operation{operationName,transport,wsdl,…}`
plus `inputMappings`/`outputMappings`, `call{targetId,targetName,resolved,processIdentifierField,dynamic,isGraftStep}`
plus `mappings`, `form{formType,uri,external}`, `performers[]`, `assignmentScript`,
`email{to,cc,bcc,subject,body,tokens[]}`, `deadline{grammar,expression,days,hours,minutes}`.

**Boundary events.** `boundary: true`, `attachedTo: <hostNodeId>`, and
`interrupting`. `interrupting: false` comes from `xpdExt:ContinueOnTimeout="true"` —
the host task keeps running while the side branch fires. There is exactly one of
these (*Fim de Prazo Mantendo Atividade*) and it must not be modelled as a
cancelling timeout.

**Edges.** `{from,to,condition,conditionType,isDefault}`. `conditionType`
`OTHERWISE` marks the default branch of an exclusive gateway. Conditions are
JavaScript expressions over case-field names.

**`derived.linkEdges` (10).** XPDL Link throw/catch pairs used as cross-lane GOTOs.
They are already resolved to `from` → `to`; flatten them into ordinary edges.

**`derived.signalEdges` (2).** Broadcast signals `Signal4` and `fimDRF` used to
cancel the sibling branch of a parallel split (mutual cancellation between
*Pedido de Vistas* and *Realizar Atividade Vista Mista*).

**`derived.migrationHazards` (54).** Each has `severity`, `category`, `node`,
`nodeId`, `process`, `detail`.

| Category | Count | Why it matters |
|---|---:|---|
| `iprocess-builtin` | 17 | `IPESystemValues.*`, `IPEStringUtil.*`, `IPEDateTimeUtil.*` need a shim |
| `link-goto` | 20 | implicit jumps, must become explicit edges |
| `external-event` | 6 | deferred steps needing a correlation key + inbound API |
| `expression-deadline` | 4 | deadline is a `DATE; TIME;` pair, not a duration |
| `dynamic-subprocess` | 3 | callee name read from a case field at runtime |
| `graft-step` | 3 | children attach themselves to a waiting parent; runtime cardinality |
| `non-interrupting-boundary` | 1 | host task survives the event |

---

## 2. `case-field-dictionary.json` — state model

`$schema: sefaz-sp/tibco-intermediate/case-field-dictionary/v1`

The package contains **no BOM** (`POC_Epat.bom` is an empty UML model), so the flat
iProcess case fields *are* the domain model. 209 fields, unified across all 9
processes.

```
fields[]
  name, fullName, nameTruncated, labelSuggestion, labelConflictsWith
  clrType, clrNullable, xpdlType, declaredType, maxLength, precision, scale
  declaredIn[] { process, kind: formalParameter|dataField, mode }
  modes[], isArray, usesSwNaSentinel, sentinelNote, arrayNote, semanticRole
  readBy[], writtenBy[], usedInConditions[], boundToService[],
  boundToSubProcess[], usedInEmail[],
  usedInForm[] { form, process, label, declaredType, inout, maxLength }
technicalFields[]   { name, clrType, declaredType, maxLength, inout, label,
                      isEngineVariable, declaredIn[], note }
typeDisagreements[] { field, fromXpdl, fromForm, xpdlPrecision, form }
```

**Field names are truncated at 15 characters** by iProcess. The length histogram
peaks at exactly 15 (28 fields), and the form labels show what was cut —
`IDDECISAODEBITO` is really `idDecisaoDebitoFiscal`, so the truncation swallowed a
whole word. Where the name is a prefix of the label the recovery is mechanical, so
it is imported as `fullName` (14 fields, 11 of them at the cap).

Labels that are *not* a de-truncation are **suggestions only** (`labelSuggestion`,
23 fields) and are never applied automatically — they are third-party business
claims and some are wrong. Two collide with a different field's name and are
flagged as `labelConflictsWith`: `NR_RATORIG` is labelled `NR_RAT`, and
`STSPETICAO` is labelled `StatusPeticao` while a separate `STATUSPETICAO` exists.
Accepting those blindly would mislabel the wrong field.

**Type mapping** (also embedded as `typeMapping`):

| XPDL | CLR |
|---|---|
| `STRING(n)` | `string` with `MaxLength = n` |
| `INTEGER(p)` | `int` when `p ≤ 9`, else `long` |
| `FLOAT(p,s)` | `decimal` — monetary, never `double` |
| `BOOLEAN` | `bool` |
| `DATE` / `TIME` / `DATETIME` | `DateOnly` / `TimeOnly` / `DateTime` |
| `PERFORMER` | `string` (user/role principal) |

Distribution: 89 `string`, 47 `int`, 28 `long`, 22 `bool`, 16 `DateOnly`,
4 `DateTime`, 2 `TimeOnly`, 1 `decimal`.

**`usesSwNaSentinel` (18 fields).** `IPESystemValues.SW_NA` is a distinct
"not available" value — **not** `null` and **not** `""`. Branches such as
`TIPOVISTAS == 'JUIZ' || TIPOVISTAS == SW_NA` change meaning if it is collapsed to
`null`. Port as an explicit optional wrapper or a well-known sentinel constant.
Affected: `BCCRELATORIO`, `CCRELATORIO`, `CNTPECA1..4`, `CODUADTJ`, `DATACONTROLE`,
`DTCIENCIA`, `IDPROCESSO`, `ORIGEM`, `SFPECA1..4`, `STSADMTITCNT`, `STSADMTITDRF`,
`TIPOVISTAS`.

**`isArray` (4 fields).** `DEAT0050`, `NRSUBPRO`, `ARRAYINT` are declared
`IsArray="true"` in the XPDL; `AGUARDAR` is inferred from `AGUARDAR[0] = …` in the
`ISetSubProc` script. iProcess array fields are fixed-width and 1-based, and are
frequently paired with pipe-delimited packed strings (`IDSINTIMADOS`, `IDPECASCNT`,
`IDPECASSF`) — see the `prepSub` script node.

`clrNullable` is `true` when the field uses the `SW_NA` sentinel or is `OUT`-only.
62 fields are declared but never referenced — candidates for removal, but verify
against the external packages first.

---

## 3. `service-contracts.json` — integration surface

`$schema: sefaz-sp/tibco-intermediate/service-contracts/v1`

```
invokedOperations[]              # the 5 operations this package actually calls
processBindings[]                # XPDL service task -> operation + case-field map
services[]
  file, targetNamespace
  endpoints[]          { service, port, binding, location, transport }
  technicalEnvelope    { HEADER, RESULT, ERROR, APPLICATIONDATA(S) }
  operations[]         { name, logicalPath, isInvokedByProcess, input[], output[] }
                       input/output: [{ partName, element, fields[] }]
                       fields: { path, xsdType, clrType, required, repeating }
```

**Transport.** Both WSDLs resolve to `tcp://srv35796:7222` — SOAP **over JMS**
through a BusinessWorks *iProcess Service Agent*, not HTTP. A .NET port needs
either an HTTP facade in front of the same backend or an EMS/AMQP client.

**Error handling is in-band.** There are no SOAP faults. Every response carries
`RESULT/STATUS_CODE` (`int`) and an optional `RESULT/ERROR` with
`SERVICE_NAME, ERROR_CODE, ERROR_DESCRIPTION, ERROR_STACKTRACE, PROCESS_STACK, DUMP_ANALYSIS`.
`STATUS_CODE != 0` is an application error; the XPDL wrappers retry, then escalate
to a human *Manipular Excecao* task. Requests carry
`HEADER/{TRANSACTION_ID, PROCESS_ID, DATETIME, APPLICATIONDATAS}`.

**Naming.** Operation names are BusinessWorks resource paths with `__sol_` = `/`
and `_sp_` = space; `logicalPath` has the decoded form.

**Reachability.** 127 operations exist; only 5 have `isInvokedByProcess: true`
(`obterPrimeiroDiaUtilAposPeriodoDeDiasCorridosDEAT`, `buscarVistasAtivasPorAiim`,
`PrepararIntimacao`, `atualizarIntimacao`, `criarNotificacoesAIIM`). Generate
clients for those first; the remaining 122 are the wider ePAT surface area.

`processBindings[].inputs/outputs` give `{caseField, soapPath}` — the direct
binding between §2 and the payload paths above, with XPDL array indices stripped.

---

## 4. `decision-tables.json` — Corticon rules

`$schema: sefaz-sp/tibco-intermediate/decision-tables/v1`

```
source           { engine, version, vocabulary, language, invokedVia }
vocabulary[]     { path, entity, attribute, datatype, clrType, role: input|output }
conditionRows[]  { row, lhs, clrType, distinctValues[], usedByRuleCount }
actionRows[]     { lhs, clrType, distinctValues[], setByRuleCount }
caseFieldMapping[] { direction, caseField, soapPath }
decisionTable[]  { column, name, when{lhs: value|'-'}, then{lhs: value} }
rules[]          { column, name, post, conditions[], actions[] }
  conditions[]: { row, lhs, rhs, matchType: equals|inSet, values[], expression }
  actions[]:    { lhs, rhs, expression }
```

49 rule columns × 21 condition rows, 276 populated action cells.

**Evaluation semantics — do not translate to `if/else if`.** Corticon fires **all**
matching columns in order; several columns write the same response attribute and
later writes override earlier ones. Preserve column order and apply every match.

**Cells.** `'-'` means *don't care*, not `false`. `matchType: inSet` comes from a
Corticon set literal (`{'1', '8'}`) and means membership. Values are string
literals even when numeric — they are ePAT domain codes (peças, textos, prazos).

**No default column.** If nothing matches, the response attributes stay unset,
which surfaces in iProcess as `SW_NA` — tying back to the sentinel rule in §2.

**Inputs (21 rows).** `Request.{instancia, flgAlcada, statusRecursos, statusPRJ,
anulacaoDefesa, exclusaoSolidarios, existeRespPRM, statusAdmissaoTitCNT,
statusAdmissaoTitDRF}` and `ResultadoJulgamento.request.{tipoImpugnacao,
motivoIntimacao, vicioRepresentacao, defesaAdmitida, diligencia, destinoDiligencia,
idDecisaoAiim, idDecisaoDebito, recursoOficio, contraRazao, origem, statusPeticao}`.

**Outputs (11 rows).** `ResultadoJulgamento.response.{codTexto, tempoResposta,
contraRazao, qtPecasCNT, cntPeca1, cntPeca2, qtPecasSefaz, sfPeca1, sfPeca2,
sfPeca3, sfDiasRepresentacao}`.

`caseFieldMapping` (39 entries) links each vocabulary attribute to the iProcess
case field it is fed from / written back to, via the `PrepararIntimacao` call in
process `PRPINTPC`, activity `PREPVTPC`.

### 5.1 `dmn/` — the reviewable form of the same rules

DMN has no hit policy for Corticon's fold. `RULE ORDER` keeps the ordering but
returns a *collection*, and it forces an unwritten cell to be emitted as `null`,
which reads as "set this to null" rather than "leave it alone". An analyst applying
a first-match mental model to such a table approves the wrong behaviour silently,
so shipping one with a warning comment attached is not good enough.

The emitter transforms instead. For a **single** attribute, "every matching column
fires in order, later writes win" is exactly "the **last** matching column that
writes it wins". Reverse the column order and that becomes "the **first** match
wins" — hit policy `FIRST`, which nobody can misread.

| File | Hit policy | Use |
|---|---|---|
| `intimacoes.dmn` | `FIRST` | **the reviewable copy** — 11 decisions, one per response attribute, columns reversed, 276 rules total |
| `intimacoes-espelho.dmn` | `RULE ORDER` | 1:1 mirror of the 49×21 rulesheet, for tracing against the `.ers` only |

Each decision carries only the rules that write its attribute and only the
condition columns those rules constrain, which is what shrinks 21 columns to a
readable handful. Every rule keeps its original Corticon column number in both its
`<description>` and an annotation column, so the reversal never costs traceability.

The rewrite is valid only while no action can affect a condition. That holds here —
conditions read `request.*`, actions write `response.*`, intersection empty — and it
is **checked at runtime, not assumed**: `emit-dmn.ps1` refuses to emit the
decomposition if the sets ever overlap.

`verify-dmn-equivalence.ps1` then proves the result by differential testing. It
builds the Corticon fold from `decision-tables.json` and a second evaluator by
**parsing the emitted `.dmn` back**, then diffs them over rule-seeded random inputs.
Uniform sampling is useless here (21 independent columns almost never satisfy a
16-column rule — an early version fired a rule in 3 of 20 000 cases and passed
vacuously), so each case is seeded from a rule and the run **fails if no case
exercised an actual override**. Current baseline: 3 000 cases, 2 703 firing, 244
with overlapping writes, depth up to 3, zero divergences.

---

## Cross-artifact keys

| From | Key | To |
|---|---|---|
| process-model `node.id` | node id | field-dictionary `readBy/writtenBy[].nodeId`, contracts `processBindings[].nodeId` |
| process-model `node.operation.operationName` | operation name | contracts `services[].operations[].name` |
| process-model `node.activitySetId` | activity set id | process-model `scopes[].scopeId` |
| field-dictionary `fields[].name` | case field | contracts `processBindings[].inputs[].caseField`, decision-tables `caseFieldMapping[].caseField` |
| contracts `processBindings[].soapPath` | payload path | contracts `operations[].input/output[].fields[].path` |
| decision-tables `vocabulary[].path` | attribute path | decision-tables `conditionRows[].lhs` / `actionRows[].lhs` |
