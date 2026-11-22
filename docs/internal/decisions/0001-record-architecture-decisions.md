---
title: ADR 0001 - Record architecture decisions
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
supersedes:
superseded-by:
related-specifications:
---

# ADR 0001: Record architecture decisions

## Context

The reconstruction introduces decisions that will constrain repository structure, publication contracts, deployment, privacy, provenance, and future extensibility. If those decisions exist only in implementations or conversations, later changes cannot reliably distinguish intentional architecture from incidental code.

## Decision drivers

- Preserve decision context and considered alternatives.
- Prevent implementation from silently establishing architecture.
- Make costly-to-reverse choices reviewable before development depends on them.
- Permit later decisions to supersede earlier ones without rewriting history.

## Options considered

### Keep decisions only in architecture documents

This would reduce document count, but architecture descriptions would mix current structure with historical reasoning and make supersession difficult to trace.

### Infer decisions from code and pull requests

This would create weak authority, hide rejected alternatives, and make repository archaeology necessary.

### Use immutable architecture decision records

This adds a small governance obligation while providing an explicit lifecycle and durable rationale.

## Decision

Material architecture decisions MUST be recorded as numbered Markdown ADRs under `docs/internal/decisions/`.

An accepted ADR is immutable in meaning. Typographical corrections and non-semantic clarification MAY be added, but a changed decision requires a new ADR that identifies the superseded record. Proposed, accepted, rejected, deprecated, and superseded statuses MUST be explicit.

An ADR is required when a choice changes a foundational boundary, public contract, schema versioning strategy, deployment authority, privacy or provenance model, or another decision that would be costly or risky to reverse.

## Consequences

### Positive

- Architectural intent becomes reviewable and traceable.
- Specifications and implementation can link to their governing decisions.
- Supersession preserves historical context.

### Negative or limiting

- Some changes require documentation before implementation.
- Poorly scoped ADRs could create unnecessary process, so routine implementation remains outside this mechanism.

### Follow-up obligations

- Maintain `docs/internal/decisions/README.md` and `adr-template.md`.
- Link relevant specifications and architecture documents to accepted ADRs.

## Validation

A repository review can identify the accepted decision governing each foundational contract without reading implementation history.
