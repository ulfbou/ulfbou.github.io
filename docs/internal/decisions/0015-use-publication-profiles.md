---
title: ADR 0015 - Use publication profiles
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: local, preview, and production builds
supersedes:
superseded-by:
related-specifications: [publication-profiles.md, build-interface.md]
---

# ADR 0015: Use publication profiles

## Context

Local verification, `/new/` preview, and production publication share content and route semantics but differ in base URL, base path, indexing, canonical behavior, visible environment labeling, and deployment policy. Scattered environment conditionals would make these differences difficult to validate.

## Decision drivers

- Model deployment-specific behavior explicitly.
- Keep content and templates environment-independent.
- Make build intent reproducible and inspectable.
- Support configuration-driven cutover.

## Options considered

### Environment variables read throughout the code

This is flexible but creates hidden inputs and distributed policy.

### Separate branches or source copies per environment

This duplicates governed source and invites drift.

### Validated named publication profiles

This centralizes environment policy while preserving shared source.

## Decision

Every validation, build, and artifact-check command MUST select exactly one validated publication profile. The initial profiles are `local`, `preview`, and `production`.

Profiles own base URL, base path, indexing, canonical behavior, environment labeling, output policy, and approved feature policy. Content and templates MUST NOT branch on profile names or read environment variables directly.

Profile values are governed build inputs and MUST appear in reproducibility and provenance evidence where relevant.

## Consequences

### Positive

- Environment behavior becomes explicit and testable.
- Preview and production share content and generation logic.
- Cutover is a profile change followed by a full validated build.

### Negative or limiting

- Profile schemas and validation must be maintained.
- Unspecified environment variation is intentionally disallowed.

### Follow-up obligations

- Define exact profile fields and invariants.
- Test route equivalence and allowed metadata differences across profiles.

## Validation

The supported CLI rejects missing or unknown profiles, and the same governed content builds correctly for all initial profiles without source modification.
