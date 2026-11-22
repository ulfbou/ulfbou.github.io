---
title: Documentation index
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
---

# Documentation index

This directory contains the governing documentation for the reconstruction of `ulfbou.github.io` as a Python 3 static publication platform.

## Authority

Documents under `docs/internal/` govern architecture, development, publication, and operations. Documents under `docs/public/` explain the platform to visitors. All human-facing documentation is Markdown.

When documents conflict, authority is resolved in this order:

1. accepted architecture decision records;
2. accepted normative specifications;
3. accepted governance policies;
4. accepted architecture documents;
5. accepted standards;
6. plans and guidance.

A later document does not silently override an earlier authoritative document. Conflicts require an explicit update or a superseding ADR.

## Status values

- `draft`: incomplete and non-normative;
- `proposed`: ready for review but not yet binding;
- `accepted`: approved and binding within its stated scope;
- `deprecated`: retained for history and no longer recommended;
- `superseded`: replaced by an identified document or ADR.

## Initial documentation areas

- `internal/vision/`: purpose, principles, scope, non-goals, and terminology;
- `internal/architecture/`: system boundaries and structural design;
- `internal/governance/`: ownership and change control;
- `internal/provenance/`: source, authorship, and derivation rules;
- `internal/standards/`: implementation and documentation conventions;
- `internal/specifications/`: normative contracts implemented by code;
- `internal/testing/`: test strategy and quality gates;
- `internal/security/`, `privacy/`, and `accessibility/`: cross-cutting requirements;
- `internal/operations/`: build and deployment operation;
- `internal/migration/`: semantic migration and cutover;
- `internal/decisions/`: immutable ADRs;
- `internal/planning/`: sequencing, risks, and readiness.

## Documentation rule

Documentation is created just before the increment that depends on it. A document is a development prerequisite only when implementation would otherwise create or depend on an unresolved contract. The authoritative staged prerequisite list is `internal/planning/development-prerequisites.md`.
