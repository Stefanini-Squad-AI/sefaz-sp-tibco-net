# ePAT migration - always-on rules

This repository implements the SEFAZ-SP ePAT process, migrated from TIBCO iProcess to .NET. The backlog, the oracles and the architecture were produced deterministically from the TIBCO export, pinned by sha256 in manifest.json.

## The three rules that never bend

1. **The oracle decides.** Not the card text, not the reviewer, not you. Expected values are toolkit-owned and immutable.
2. **Identifiers are transcribed, never renamed.** The business term belongs in an XML comment. A renamed field is a defect that compiles.
3. **A gap is escalated, never resolved in code.** You may propose an option; the decision is human and is recorded in the glossary.

## How to read a card

- `content.checklist` is the work, in order. `entrouPor` other than `fluxo` means the link does NOT exist in the source and must be written explicitly - it is the easiest omission to make.
- `fulfills.segmento` gives the reference journey and the exact steps: that is what the oracle replays.
- `dimensao.peso = atravessa-o-sistema` means the card touches three or more projects. It is a valid card, but it is not one person's work.
- `content.injectedContext.hypotheses` are questions to confirm, never facts.

## Scope

This is a proof of concept, not the full migration. 215 of 215 in-scope nodes have a card; what was left out is recorded with a reason in context/scope.json. If something looks missing, check there before assuming it was forgotten.
