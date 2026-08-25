# POC_Epat Fulfillment Plan

**Purpose:** Define exactly what must be delivered and demonstrated to fulfill the POC.

**Source of truth:** `Documento Prova de Conceito.docx`, normalized into
`artifacts/POC_Epat/intent-map.json` and `artifacts/POC_Epat/conformance.json`.

**Important scope rule:** This POC does not migrate the complete legacy ePAT system.
It validates that the target BPM platform can execute the representative POC_Epat
scenario and the BPM capabilities exercised by it.

## 1. Scenario Boundary

The business scenario starts when an AFR initiates `Finalizar AIIM` for an AIIM
that already exists in AIIMWeb. It ends after `Controle Intimados` completes and
the process instance is closed.

```mermaid
flowchart LR
    A[Inicio] --> B[Finalizar AIIM]
    B --> C[Notificacao do AIIM]
    C --> D[Decisions]
    D --> E[Verificar Retorno Decisions]
    E --> F[Fluxos paralelos]
    F --> G[Controle Intimados]
    G --> H[Fim da instancia]
```

Included:

- Main process `POC_EpatProcess`.
- Supporting workflows `DEAT0050`, `CALCPRPC`, `BSCENVPC`, `PRPINTPC`,
  `CONTROPC`, `ATZINTPC`, `AGPECASPC`, and `CRNOTPC`.
- User tasks, service calls, Decisions rules, timers, signals, email, parallel
  execution, asynchronous correlation, and dynamic subprocess behavior.
- State required by this scenario, including business fields and the separate
  technical process envelope.

Excluded unless separately approved:

- Migration of the complete ePAT application or every legacy process.
- Business activity before the AFR selects an existing AIIM.
- The complete legal or administrative lifecycle after `Controle Intimados`.
- Undelivered external packages except where a test double or agreed contract is
  required to demonstrate this scenario.

## 2. Current Baseline

| Measure | Current state |
|---|---:|
| Workflows extracted | 9 |
| Process nodes / transitions | 215 / 221 |
| Case fields | 209 |
| Service operations discovered | 127 |
| POC concepts extracted | 19 of 19 |
| Concepts proven by execution | 1 of 19 |
| Concepts awaiting execution proof | 18 of 19 |
| Authored POC stages | 7 |
| Expected results proven | 1 of 11 |

Extraction proves that a feature exists in the source. It does not prove that the
target platform executes it correctly. At present, only the Decisions equivalence
has executable proof; the remaining capabilities require runtime evidence.

## 3. Definition of Done

The POC is fulfilled only when all of the following are true:

- [ ] The seven-stage scenario runs end to end from `Finalizar AIIM` to process end.
- [ ] Every one of the 11 expected results is marked `proven` with reproducible evidence.
- [ ] Every one of the 19 concepts has execution evidence, not only extraction evidence.
- [ ] All blocking migration decisions are approved and recorded.
- [ ] The seven suggested gateway questions are reviewed by a business analyst.
- [ ] External dependencies are available, simulated by contract-faithful test doubles,
      or explicitly accepted as exclusions.
- [ ] Automated tests cover the normal path, alternate branches, timeout paths,
      retry/error handling, correlation, and restart/recovery.
- [ ] A clean extraction and validation run passes without source drift or broken artifacts.
- [ ] A witnessed demonstration produces the evidence package described in section 7.

## 4. Decisions to Close Before Implementation

Record each decision in `config/glossary/POC_Epat.yaml` or the relevant architecture
decision record. Do not silently choose behavior in implementation code.

| Decision | Required outcome | Acceptance check |
|---|---|---|
| Dynamic subprocess | Define the allow-listed runtime resolution of dynamic callees and behavior for an unknown callee. | Known target starts; unknown target fails visibly and audibly. |
| Graft Step | Define how child instances attach to a waiting parent and how many children may attach. | Child correlates through the parent key and resumes the correct parent instance. |
| External event | Define inbound API/message, durable subscription, correlation key, idempotency, and late/duplicate handling. | Restart-safe event delivery with duplicate-event test. |
| Expression deadline | Define how `DATE; TIME` expressions become an absolute instant, including timezone and changed field values. | Boundary fires at the calculated instant, not after a fixed duration. |
| iProcess built-ins | Specify compatibility semantics for `SW_NA`, dates, retry counters, process identity, and utility functions. | Branch behavior matches source fixtures, including three-state `SW_NA`. |
| Link/goto | Replace XPDL link throw/catch jumps with explicit, testable transitions. | Every derived link edge reaches the same target without hidden control flow. |
| Non-interrupting boundary | Preserve the host activity while its timeout branch proceeds. | Test proves both host and boundary path remain active. |
| Suggested gateway labels | Approve or replace the seven inferred business questions. | Human reviewer records `question` in the glossary. |

## 5. Delivery Steps

### Step 1 - Freeze and Validate the Source Baseline

- [ ] Confirm the package manifest points to the approved XPDL, WSDL, ERS, forms,
      screens, and POC document.
- [ ] Run `./tools/run-extraction.ps1 -Package POC_Epat`.
- [ ] Resolve any count drift instead of accepting it silently.
- [ ] Run `./tools/validate-artifacts.ps1`.
- [ ] Archive the generated manifest hashes with the POC evidence.

Exit criterion: extraction, source coverage, BPMN integrity, and DMN equivalence pass.

### Step 2 - Close Human and Architecture Decisions

- [ ] Review all entries on the `Questoes em aberto` page.
- [ ] Approve the seven suggested decision labels.
- [ ] Choose an option for each migration gap in the glossary.
- [ ] Resolve or explicitly simulate every required undelivered external package.
- [ ] Obtain business and architecture approval for the decisions in section 4.

Exit criterion: no POC behavior depends on an undocumented implementation guess.

### Step 3 - Build the Runtime Foundation

- [ ] Implement durable workflow instance persistence and optimistic concurrency.
- [ ] Implement the 209-field business state separately from the technical envelope.
- [ ] Implement correlation, idempotency, retries, error capture, and operator recovery.
- [ ] Implement the approved iProcess compatibility layer.
- [ ] Configure observability with process ID, case ID, node ID, transition, and timestamp.
- [ ] Provide test doubles for unavailable services with the extracted WSDL contracts.

Exit criterion: a minimal persisted workflow survives restart and resumes deterministically.

### Step 4 - Implement and Prove the Seven Stages

#### Stage 1 - Fechamento do AIIM

- [ ] Start a process instance and execute `Finalizar AIIM` as a user task.
- [ ] Select an eligible AIIM, confirm its header, and persist transferred data.
- [ ] Prove user assignment, form input, variables, completion, and audit history.

Evidence: instance trace, user-task screenshots, before/after state, and audit record.

#### Stage 2 - Notificacao do AIIM

- [ ] Start and correlate `DEAT0050` through the approved Graft Step design.
- [ ] Execute deadline calculation through `CALCPRPC`.
- [ ] Demonstrate asynchronous wait, event correlation, email, and loop/retry behavior.

Evidence: parent/child trace, correlation test, timer timestamps, email capture, retry trace.

#### Stage 3 - Integracao com Decisions

- [ ] Invoke the Decisions component with the expected typed payload.
- [ ] Preserve the existing proven DMN/Corticon equivalence.
- [ ] Capture request, response, selected rule, and resulting process values.

Evidence: contract test plus existing `verify-dmn-equivalence.ps1` result.

#### Stage 4 - Verificacao do Retorno do Decisions

- [ ] Execute `Verificar Retorno Decisions`.
- [ ] Set and persist `TIPOVISTAS`.
- [ ] Prove each relevant exclusive branch and its default path.

Evidence: parameterized branch tests and process traces for each outcome.

#### Stage 5 - Validacao de Gateways Paralelos

- [ ] Split both branches at each `Execucao paralela` gateway.
- [ ] Demonstrate simultaneous work without duplicate or lost state updates.
- [ ] Demonstrate signal behavior and branch synchronization.
- [ ] Continue only after the required branches complete or cancel as specified.

Evidence: concurrent timeline, signal trace, join assertion, and restart-during-parallel test.

#### Stage 6 - Controle de Intimados

- [ ] Start `CONTROPC` using the approved dynamic-procedure design.
- [ ] Exercise preparation, creation, search, and update flows through `PRPINTPC`,
      `CRNOTPC`, `BSCENVPC`, and `ATZINTPC` where selected by scenario data.
- [ ] Prove service success, application error, technical error, retry, and manual recovery.

Evidence: subprocess tree, service contract tests, error-path traces, and recovery audit.

#### Stage 7 - Encerramento

- [ ] Complete `Controle Intimados` and reach the intended end event.
- [ ] Persist final state and prevent further normal work on the closed instance.
- [ ] Prove that pending subscriptions and timers are cleaned up or retained by policy.

Evidence: final state, end-event trace, persistence query, and no-orphan-resource check.

### Step 5 - Prove Cross-Cutting Behavior

- [ ] Start/end lifecycle and instance persistence.
- [ ] User tasks and authorization.
- [ ] Service tasks and typed integration contracts.
- [ ] Exclusive and parallel gateways.
- [ ] Timers, expression deadlines, and non-interrupting boundaries.
- [ ] Signals, messages, asynchronous waits, and correlation.
- [ ] Email notification.
- [ ] Variables and technical envelope separation.
- [ ] Linked flow replacement and subprocess chaining.
- [ ] Dynamic procedures and Graft Step.
- [ ] Restart, duplicate delivery, retry exhaustion, and operator recovery.

Exit criterion: every concept in `conformance.json` is `proven` with an evidence reference.

## 6. Expected-Result Acceptance Matrix

Status reflects whether **reproducible execution evidence** exists (per section 7).
Human sign-off (section 8) is a separate, still-pending gate. Test references are under
`tests/SefazSp.Epat.Oracles.Tests/`.

| Expected result | Status | Evidence (reproducible) |
|---|---|---|
| BPMN modeling | **Proven** | XOR/AND/start/end paths: `ScenarioPathOracleTests`, `ScenarioPath/Etapa1-7PercursoOracleTests`, `ScenarioPath/GatewaysAndConceptTests`, `Composition/PocEpatSc001JourneyTests` (SC-001/012/010/014/015). |
| Chained subprocesses | **Proven** | `GraftStep/GraftStepConceitoTests`; `Composition/PocEpatRestartRecoveryTests.GraftReal_SurvivesRestart`; `ScenarioPath/Etapa4-5` (CONTROPC→AGURETPC, DEAT0050, BSCENVPC descent). |
| External service integration | **Proven** (contract-faithful doubles) | `Contract/*ContractTests` (4 WSDL contracts); SC-* journeys drive the doubles. Real SOAP-over-JMS deferred to MVP (ratified). |
| Decision-rule processing | **Proven** | `EquivalenciaCorticonDmnTests`; `Rules/IntimacoesDecisionEvaluatorTests`. |
| Dynamic activity creation | **Proven** | Ratified interface-registry-validated design; `Concepts/DynamicSubprocessRegistryTests` (known target resolves; unknown target fails visibly; missing destination fails at startup); `ScenarioPath/Etapa4` (CONTROPC/Aguardar Retorno); SC-001 node 29. |
| Asynchronous correlation | **Proven** | Durable correlation: `Composition/PocEpatRestartRecoveryTests` (8 restart scenarios); duplicate delivery: `Composition/PocEpatDuplicateDeliveryTests`. |
| SLA and deadline control | **Proven** | Clock-controlled `Concepts/ExpressionDeadlineTests` prove the absolute-instant calculation (DATE(PRAZODEFESA from DAYSOVER)+TIME(PRAZODEFESAT) in America/Sao_Paulo, field-driven, re-armed on rewrite); `Concepts/DeadlineTimerFlagTests` prove the scheduled delay equals the distance to the computed instant (tracks the field, not a fixed duration); `Concepts/DeadlineTimerRuntimeTests` prove the runtime wiring (demo-off fires at the real instant, not the demo delay). The global `DeadlineTimer:Demo` flag keeps the short delay for demos/smoke tests. |
| Parallel processing and synchronization | **Proven** | `ScenarioPath/GatewaysAndConceptTests` (3 AND points, split/join concurrent-timeline); `ScenarioPath/Etapa5-7` (Validação Paralelos). |
| Notifications | **Proven** (contract-faithful double) | `ScenarioPath/Etapa3-4` (Email Limite Rel 1 emailTask node in path); `Contract/BuscarVistasAtivasPorAiimContractTests` (EMAILVISTAS). Send is a double. |
| Process-variable manipulation | **Proven** | Before/after across SQLite persistence: `Persistence/SnapshotSerializationTests`; journeys mutate CORRECAO/TIPOVISTAS/AFR through the durable snapshot. |
| iProcess conceptual compatibility | **Proven** | Tri-state SW_NA: `Concepts/FieldValueTriStateTests` (SW_NA is a distinct third state; Match exhaustive) + `Persistence/SnapshotSerializationTests` (SW_NA survives persistence); Graft Step: `GraftStep/GraftStepConceitoTests`; Decisions: `Rules/IntimacoesDecisionEvaluatorTests`; link-goto + non-interrupting-boundary: `ScenarioPath/Etapa5-7`. |

## 7. Evidence Package

For each test or demonstration, retain:

- Source manifest and commit identifier.
- Scenario/test case identifier and input fixture.
- Process instance ID and business case ID.
- Ordered node/transition trace with timestamps.
- Before/after variable snapshots with sensitive values redacted.
- Service request/response or test-double interaction record.
- User-task and email screenshots where applicable.
- Timer due time, actual fire time, and timezone.
- Expected result, actual result, and pass/fail assertion.
- Reviewer name, date, and approval for human decisions.

Execution evidence must be reproducible. A diagram or extracted node count alone is
not sufficient proof.

## 8. Final POC Sign-Off

- [ ] Business owner confirms the seven-stage scenario and suggested gateway labels.
- [ ] Architecture approves every no-equivalent design decision.
- [ ] Security approves identities, authorization, inbound events, and secret handling.
- [ ] Operations approves persistence, retries, monitoring, and recovery procedures.
- [ ] QA confirms all 11 expected results and 19 concepts are proven.
- [ ] Product sponsor accepts documented exclusions and external test doubles.
- [ ] Final witnessed run completes from `Finalizar AIIM` through process termination.

The final decision must be recorded as one of:

- **Approved:** All mandatory results are proven.
- **Approved with conditions:** Remaining limitations are explicit, owned, and dated.
- **Rejected:** One or more mandatory capabilities cannot be demonstrated.
