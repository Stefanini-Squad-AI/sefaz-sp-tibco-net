# AGENTS.md

Migration of the SEFAZ-SP ePAT process from TIBCO iProcess to .NET, phase 2.

## What is here

| Folder | What it is |
|---|---|
| `context/` | The intermediate representation and the diagrams. Read-only reference corpus. |
| `oracles/` | Immutable fixtures. Wire to them; never edit them. |
| `backlog/` | The work, as cards validated against a schema. |
| `agents/` | Role manifests: who may write where, and when to stop. |
| `scaffold/` | Lossless .NET only - entities, ports, skeletons. No bodies. |
| `glossary/` | Human decisions already ratified. |
| `review/` | What is still undecided, including blockers. |

## The roles

| Agent | Writes in | Cards |
|---|---|---:|
| `autor-de-testes-application-tests` | `tests/SefazSp.Epat.Application.Tests/**` | 61 |
| `autor-de-testes-domain-tests` | `tests/SefazSp.Epat.Domain.Tests/**` | 30 |
| `autor-de-testes-oracles-tests` | `tests/SefazSp.Epat.Oracles.Tests/**` | 16 |
| `molde-subprocesso-de-servico` | - | 26 |
| `fundacao-anticorrupcao` | `src/SefazSp.Epat.Infrastructure/Legacy/**` | 0 |
| `fundacao-motor` | `src/SefazSp.Epat.Infrastructure/Workflow.Elsa/**` | 0 |
| `fundacao-motor-de-regras` | `src/SefazSp.Epat.Infrastructure/Rules.Dmn/**` | 0 |
| `fundacao-persistencia` | `src/SefazSp.Epat.Infrastructure/Persistence/**` | 0 |
| `fundacao-registo` | `src/SefazSp.Epat.Api/Composition/**` | 0 |
| `implementador-endpoints` | `src/SefazSp.Epat.Api/Endpoints/**` | 8 |
| `implementador-execution` | `src/SefazSp.Epat.Application/Execution/**` | 38 |
| `implementador-integration-doubles` | `src/SefazSp.Epat.Infrastructure/Integration.Doubles/**` | 14 |
| `implementador-integration-soap` | `src/SefazSp.Epat.Infrastructure/Integration.Soap/**` | 22 |
| `implementador-rules` | `src/SefazSp.Epat.Domain/Rules/**` | 23 |
| `implementador-usecases` | `src/SefazSp.Epat.Application/UseCases/**` | 22 |
| `implementador-workflows` | `src/SefazSp.Epat.Application/Workflows/**` | 46 |
| `revisor` | - | 80 |

## Order of attack

Read from risk, not from topology. It says where to start so that a mistake costs little.

| Step | Role | New cards | Unblocks |
|---:|---|---:|---:|
| 1 | `fundacao-anticorrupcao` | 0 | 0 |
| 2 | `fundacao-motor` | 0 | 0 |
| 3 | `fundacao-motor-de-regras` | 0 | 0 |
| 4 | `fundacao-registo` | 0 | 0 |
| 5 | `implementador-integration-soap` | 22 | 20 |
| 6 | `molde-subprocesso-de-servico` | 6 | 15 |
| 7 | `implementador-rules` | 4 | 14 |
| 8 | `implementador-usecases` | 8 | 23 |
| 9 | `implementador-workflows` | 8 | 3 |
| 10 | `implementador-execution` | 3 | 8 |
| 11 | `implementador-integration-doubles` | 5 | 30 |
| 12 | `implementador-endpoints` | 0 | 29 |
| 13 | `fundacao-persistencia` | 0 | 0 |
| 14 | `autor-de-testes-oracles-tests` | 16 | 0 |
| 15 | `autor-de-testes-domain-tests` | 0 | 0 |
| 16 | `autor-de-testes-application-tests` | 8 | 0 |

## Before you start

Re-validate `provenance.manifestSha256` on the card against `manifest.json`. A mismatch means the card must be regenerated, not implemented.
