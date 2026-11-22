---
title: ADR 0002 - Rebuild as a Python static publication platform
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: platform reconstruction
supersedes:
superseded-by:
related-specifications: [build-interface.md, output-artifact.md]
---

# ADR 0002: Rebuild as a Python static publication platform

## Context

The existing site uses .NET and Blazor structures that do not match the intended visitor-first publication architecture. The target platform needs governed authored content, deterministic static generation, configurable deployment, optional browser enhancement, and no required application server.

## Decision drivers

- Use Python 3 as the sole build and publication implementation language.
- Publish static output compatible with GitHub Pages.
- Separate authored semantics from a framework-specific component tree.
- Support deterministic validation, generation, and artifact inspection.
- Avoid preserving obsolete structure merely to reduce short-term migration effort.

## Options considered

### Incrementally reshape the .NET and Blazor implementation

This could reuse code, but it would retain framework assumptions and complicate the Python-only target.

### Translate the existing implementation directly into Python

This would change language while preserving the wrong architecture and coupling.

### Rebuild from governed semantics

This requires explicit migration but lets the new architecture reflect the intended platform rather than the legacy implementation.

## Decision

The site MUST be rebuilt as a Python 3 static publication platform. Python owns validation, publication planning, rendering, resource generation, artifact assembly, and supported command-line interfaces.

The public artifact MUST consist of static files deployable to GitHub Pages. JavaScript MAY progressively enhance generated pages but MUST NOT become the primary publication engine.

The migration MUST preserve approved content and verified public behavior where appropriate. It MUST NOT preserve .NET, Blazor, source-folder, component, or deployment structures solely for implementation similarity.

## Consequences

### Positive

- Repository and runtime boundaries can match the target publication model.
- Static hosting remains simple, portable, and secure.
- Legacy implementation constraints do not become new platform contracts.

### Negative or limiting

- Existing application code is not a reusable foundation.
- Content and public behavior require explicit inventory and semantic migration.
- Python tooling and package boundaries must be established before implementation.

### Follow-up obligations

- Specify the build interface and repository structure.
- Inventory and classify existing public content and routes.
- Keep the legacy implementation available only as migration evidence until cutover.

## Validation

A clean checkout can install locked Python tooling, build a complete static artifact, and serve it without .NET or a server-side application runtime.
