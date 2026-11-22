---
title: Development documentation prerequisites
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
---

# Development documentation prerequisites

## Governing rule

A document is a prerequisite only when the next development increment would otherwise create or depend on an unresolved contract.

Documentation must remain ahead of the implementation that consumes it, but distant features must not block the static platform foundation or be specified without sufficient evidence.

## Gate A: required before the first platform implementation

### Vision and scope

- `docs/internal/vision/platform-vision.md`
- `docs/internal/vision/product-principles.md`
- `docs/internal/vision/scope.md`
- `docs/internal/vision/non-goals.md`
- `docs/internal/vision/glossary.md`

### Foundational architecture

- `docs/internal/architecture/system-context.md`
- `docs/internal/architecture/repository-structure.md`
- `docs/internal/architecture/dependency-rules.md`
- `docs/internal/architecture/static-first-architecture.md`
- `docs/internal/architecture/url-and-routing.md`
- `docs/internal/architecture/publication-architecture.md`

### Initial normative specifications

- `docs/internal/specifications/build-interface.md`
- `docs/internal/specifications/publication-profiles.md`
- `docs/internal/specifications/base-path-resolution.md`
- `docs/internal/specifications/output-artifact.md`
- `docs/internal/specifications/page-model.md`
- `docs/internal/specifications/content-model.md`

### Development and quality rules

- `docs/internal/standards/python-coding-standard.md`
- `docs/internal/standards/generated-artifact-standard.md`
- `docs/internal/standards/deterministic-build-standard.md`
- `docs/internal/testing/test-strategy.md`
- `docs/internal/testing/quality-gates.md`

### Governance and decision process

- `docs/internal/governance/change-control.md`
- `docs/internal/governance/content-governance.md`
- `docs/internal/decisions/README.md`
- `docs/internal/decisions/adr-template.md`

### Initial accepted ADRs

- `0001-record-architecture-decisions.md`
- `0002-rebuild-as-python-static-publication-platform.md`
- `0004-use-configurable-deployment-base-path.md`
- `0005-separate-base-url-and-base-path.md`
- `0006-generate-complete-static-baselines.md`
- `0007-use-progressive-enhancement.md`
- `0013-separate-authored-content-from-generated-output.md`
- `0014-require-deterministic-builds.md`
- `0015-use-publication-profiles.md`
- `0018-control-publication-capabilities-through-profile-policy.md`
- `0019-preserve-logical-route-equivalence-across-publication-profiles.md`
- `0020-centralize-public-reference-resolution.md`
- `0022-govern-content-through-a-common-lifecycle-and-model.md`
- `0023-bound-browser-enhancement-by-static-page-contracts.md`

## Gate B: required before content migration

Complete the provenance model, source and authorship policies, content-integrity rules, schema and taxonomy governance, content-type specifications, content inventory, disposition, and metadata mapping before migrating real content.

## Gate C: required before browser-side features

Complete the browser runtime, module, search, privacy, local-data, personalization, recommendation, and client-resource contracts before implementing JavaScript modules, search, IndexedDB, or personalization. These do not block the initial static generator.

## Gate D: required before `/new/` preview deployment

Complete the deployment architecture, artifact composition, canonical and indexing rules, workflow security, artifact validation, branch integration, and deployment coordination decisions before publishing the reconstruction preview.

## Gate E: required before production cutover

Complete security and privacy assessments, accessibility evidence, production and rollback procedures, route and metadata migration, release readiness, emergency withdrawal, final content disposition, and public trust documentation before replacing the current production site.

## Increment readiness test

An implementation increment is ready only when:

1. its public or internal behavior is bounded by accepted architecture or specifications;
2. unresolved foundational choices are either decided or explicitly excluded;
3. acceptance criteria identify observable results;
4. relevant tests and quality gates are known;
5. required security, privacy, accessibility, provenance, and migration implications have been addressed;
6. implementation will not silently establish a new public contract.

A missing document blocks an increment only when one of these conditions cannot otherwise be satisfied.
