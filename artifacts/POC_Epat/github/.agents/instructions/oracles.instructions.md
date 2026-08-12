---
description: "Use when writing or changing any test in the ePAT migration. Covers which values may be authored and which are fixed by the toolkit."
applyTo: "tests/**"
---
# Oracles are immutable

Every card names an oracle in `acceptance.oracle`, with `immutable: true`. The expected values come from the toolkit and are derived from the TIBCO source.

- Wire the harness to the fixture. Never author or edit an expected value.
- If a test only passes when you change the expected value, stop and escalate: either the implementation is wrong, or the fixture does not cover the case the card describes - and the second is a defect of the toolkit, not of the test.
- `scenario-path` fixtures live in `oracles/scenarios/`, `decision-table` in `oracles/dmn/`, `contract` in `context/service-contracts.json`.
