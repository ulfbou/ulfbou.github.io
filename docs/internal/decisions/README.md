---
title: Architecture decision records
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
---

# Architecture decision records

Architecture decision records document decisions that materially constrain platform structure, public contracts, publication, security, privacy, accessibility, or long-term maintenance.

## When an ADR is required

Create an ADR when a change:

- establishes or changes a foundational architecture boundary;
- selects between materially different long-lived approaches;
- introduces or changes a public contract, schema versioning strategy, or migration obligation;
- changes publication, deployment, security, privacy, or provenance policy;
- would be costly or risky to reverse after implementation.

Routine implementation detail within an accepted contract does not require an ADR.

## Lifecycle

1. Create an ADR from `adr-template.md` with status `proposed`.
2. Review it with the affected architecture, governance, and implementation documents.
3. Mark it `accepted`, `rejected`, or `superseded`.
4. Do not rewrite the reasoning of an accepted ADR to match later events. Add clarifications that do not alter the decision, or supersede it with a new ADR.
5. Link superseding and superseded ADRs in both records.

## Numbering

ADR filenames use a four-digit sequence followed by a concise kebab-case title. Reserved numbers may be used to preserve an agreed decision map. A missing number does not imply a missing decision.

## Authority

Accepted ADRs govern within their stated scope. Specifications define the detailed contracts that implement them. If implementation reveals that an accepted decision is unsuitable, development pauses at that boundary until the ADR is superseded.
