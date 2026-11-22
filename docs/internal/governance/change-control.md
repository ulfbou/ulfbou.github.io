---
title: Change control
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: repository and platform changes
---

# Change control

## Change classes

### Class 1: local implementation

A change is local when it remains inside accepted architecture and specifications, preserves public contracts, and is readily reversible. It requires normal code review, relevant tests, and documentation updates where behavior is user-visible.

Examples include refactoring an adapter, improving diagnostics without changing rule meaning, or adding tests for an existing contract.

### Class 2: contract change

A change affects a documented schema, CLI, content model, route rule, generated resource, publication profile, or operational procedure. It requires the affected specification to be proposed or updated before implementation, compatibility analysis, migration handling, and contract tests.

### Class 3: architectural change

A change alters system boundaries, dependency direction, publication ownership, deployment strategy, privacy model, provenance policy, or another costly-to-reverse decision. It requires a proposed ADR, affected architecture and specification updates, explicit alternatives, and acceptance before implementation crosses the boundary.

### Class 4: emergency change

An urgent security correction, content withdrawal, or production recovery may use an approved runbook before normal documentation is complete. The smallest safe change is applied, evidence is retained, and the missing ADR, specification, or incident review is completed promptly afterward. Emergency status does not authorize unrelated changes.

## Required change record

A pull request MUST identify:

- objective and bounded scope;
- change class;
- affected contracts and documents;
- visitor and operational impact;
- tests and review evidence;
- generated output affected but not committed;
- migration, compatibility, rollback, security, privacy, accessibility, and provenance implications where relevant.

## Approval

The owner of the affected contract approves the change. When one person holds multiple roles, the pull request still records separate architectural, editorial, and implementation considerations to avoid implicit self-approval of unresolved concerns.

## Compatibility

Breaking changes require an explicit version or migration decision. Silent reinterpretation of existing source, content state, routes, taxonomy, provenance, or browser resources is prohibited.

## Documentation timing

Architecture decisions and normative contracts MUST be accepted before dependent implementation. Operational guidance and examples MAY be completed within the same bounded pull request when they do not determine architecture.

## Scope discipline

A change MUST preserve unrelated work and avoid opportunistic cleanup. Newly discovered issues outside scope are recorded separately unless they make the requested change unsafe or unverifiable.
