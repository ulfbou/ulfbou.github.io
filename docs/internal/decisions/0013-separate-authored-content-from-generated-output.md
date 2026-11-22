---
title: ADR 0013 - Separate authored content from generated output
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: repository, builds, and deployment
supersedes:
superseded-by:
related-specifications: [output-artifact.md]
---

# ADR 0013: Separate authored content from generated output

## Context

The current project experience has shown that generated Pages output can be confused with source-controlled deliverables. Committing generated HTML creates duplicate authority, noisy diffs, stale-output risks, and validation that does not reflect the actual CI artifact.

## Decision drivers

- Maintain one authoritative source for public content and generation rules.
- Ensure CI validates what it builds.
- Avoid manual editing and stale generated files.
- Keep deployment concerns from shaping repository semantics.

## Options considered

### Commit the complete generated site

This supports direct branch-based hosting but duplicates source and encourages generated-file review and drift.

### Commit selected generated files

This creates ambiguous exceptions and still requires synchronization policy.

### Generate and deploy artifacts in CI

This keeps source authoritative and makes publication reproducible.

## Decision

Authored content, configuration, schemas, templates, source code, tests, assets, and Markdown documentation MUST remain separate from generated publication output.

Generated HTML, indexes, feeds, sitemap, copied or fingerprinted assets, publication resources, and composed Pages trees MUST NOT be committed as normal source. They MUST be created, validated, and uploaded by local builds or CI.

Manual changes to generated output are prohibited. Corrections MUST be made to governed inputs or generator logic.

## Consequences

### Positive

- Source and output have unambiguous ownership.
- Pull requests focus on intentional inputs and code.
- CI artifact validation reflects deployment reality.

### Negative or limiting

- Local preview requires running the build.
- Deployment workflows must upload artifacts rather than rely on committed output.

### Follow-up obligations

- Ignore all generated output paths.
- Provide simple local build and preview commands.
- Require an ADR for any future generated-file exception.

## Validation

A clean checkout can reproduce the public artifact, and deleting all local generated output has no effect on authoritative repository state.
