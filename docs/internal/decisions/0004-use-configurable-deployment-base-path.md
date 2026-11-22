---
title: ADR 0004 - Use a configurable deployment base path
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: routing, assets, publication resources, and deployment
supersedes:
superseded-by:
related-specifications: [publication-profiles.md, base-path-resolution.md]
---

# ADR 0004: Use a configurable deployment base path

## Context

The reconstruction will be previewed under `/new/` and later replace production at `/`. Embedding `/new/` in content, templates, browser modules, or logical routes would require broad rewrites at cutover and create inconsistent links, assets, metadata, feeds, and fetch requests.

## Decision drivers

- Build equivalent logical routes at `/new/` and `/`.
- Make cutover configuration-driven.
- Prevent preview deployment details from contaminating authored content.
- Use one path-resolution policy across all generated references.

## Options considered

### Hard-code `/new/` during reconstruction

This is initially simple but creates cutover risk and prefix leakage.

### Use relative URLs everywhere

Relative references reduce some prefix concerns but are fragile across nested routes, canonical metadata, structured data, feeds, and browser fetches.

### Resolve logical routes through a configured base path

This centralizes deployment-specific behavior and keeps authored routes stable.

## Decision

Every publication profile MUST define a validated `base_path`. Logical routes and governed resource locations MUST remain independent of that prefix. One shared resolver MUST produce navigational links, asset references, browser-resource locations, and absolute public URLs.

`/new/` MUST exist only as preview-profile configuration and generated output. It MUST NOT appear as a source route prefix, module identifier, content classification, or template constant.

## Consequences

### Positive

- Preview and production builds share source and route identity.
- Cutover requires a profile change and full rebuild rather than path surgery.
- Prefix behavior becomes testable through one contract.

### Negative or limiting

- All output producers must use the resolver consistently.
- External libraries that assume root deployment may require adapters or rejection.

### Follow-up obligations

- Test root and nested base paths in all artifact-producing increments.
- Detect accidental `/new/` literals in production artifacts.

## Validation

The same source builds successfully for `/new/` and `/`, producing equivalent logical routes and valid internal references without source changes.
