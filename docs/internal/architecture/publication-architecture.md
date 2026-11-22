---
title: Publication architecture
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: build, validation, artifact assembly, and deployment
related-adrs:
  - 0002
  - 0013
  - 0014
  - 0015
---

# Publication architecture

## Purpose

Publication transforms governed repository sources into one complete, validated, replaceable static artifact. The pipeline separates source validation, planning, rendering, assembly, artifact validation, and deployment so that no stage silently compensates for invalid earlier input.

## Pipeline

### 1. Load configuration

The CLI selects an explicit publication profile and validates configuration before reading content. The profile defines base URL, base path, indexing behavior, canonical behavior, and output settings. Secrets are not publication configuration.

### 2. Read governed sources

Adapters read content, metadata, taxonomy, schemas, templates, assets, and relevant platform configuration using explicit UTF-8 handling. Filesystem traversal order is never publication order.

### 3. Validate and normalize

Validation converts external representations into domain models and reports actionable diagnostics with source location, rule, observed value, and remediation. Invalid required input stops publication. The publisher must not guess missing routes, dates, states, provenance, or relationships.

### 4. Plan publication

The application layer determines eligible content, route ownership, output mapping, navigation, relationships, module baselines, public resources, and profile-specific exclusions. Planning detects collisions before writing output.

### 5. Render

Templates receive validated, immutable view models. Rendering produces complete semantic HTML and machine resources. Templates cannot read repository state, select unpublished content, or resolve deployment paths independently.

### 6. Assemble artifact

The publisher writes to a clean staging directory, copies or transforms approved assets, and creates all required routes and public resources. It must not merge with stale output. Final placement uses a controlled replace or CI artifact upload only after successful validation.

### 7. Validate artifact

Artifact validation inspects the generated result rather than assuming successful rendering means publishable output. Checks include required files, internal references, route uniqueness, metadata, forbidden paths, profile policy, JSON or feed validity, baseline content, and deterministic expectations.

### 8. Deploy

Deployment consumes only a successfully validated artifact. GitHub Actions orchestration must invoke supported Python commands rather than reimplement publication rules. Pull requests may build and validate preview artifacts, but production publication authority remains separately controlled.

## Source and output ownership

Source-controlled inputs include content, configuration, taxonomy, schemas, templates, assets, code, tests, and Markdown documentation. Generated HTML, publication JSON, optimized or fingerprinted assets, indexes, feeds, sitemap, and composed deployment trees are build output.

Generated output:

- is never manually edited;
- is safe to delete and recreate;
- is not evidence of source completeness;
- is not committed unless a future accepted decision establishes a narrowly defined exception;
- must not be used as an input to the next normal build.

## Determinism

Publication order, serialization, route planning, resource ordering, and diagnostics are stable. Current time is excluded unless supplied as a governed build input. File modification times, directory enumeration order, locale, machine paths, and CI run identifiers must not alter semantic output unless the specification explicitly includes them.

## Failure semantics

The build fails before deployable output is published when required input is invalid, routes collide, rendering fails, artifact validation fails, or a required resource cannot be produced. Partial staging output may be retained for diagnostics locally but must not be promoted or described as a valid artifact.

## Extensibility

Search indexes, feeds, publication manifests, browser policies, and personalization resources are later pipeline products. They must consume the same validated publication plan rather than independently rediscovering content or routes.
