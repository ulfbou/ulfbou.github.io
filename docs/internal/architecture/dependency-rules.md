---
title: Dependency rules
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: Python publication platform and browser enhancement
related-adrs:
  - 0002
  - 0013
  - 0014
---

# Dependency rules

## Direction of dependency

Dependencies point from delivery mechanisms toward stable platform contracts, never from domain contracts toward templates, filesystems, CI, or deployment providers.

The intended Python dependency direction is:

```text
CLI and orchestration
        â†“
application services
        â†“
domain models and policies
```

Adapters for files, Markdown, schemas, templates, artifacts, and clocks implement interfaces required by the application layer. They do not become dependencies of the domain layer.

## Layer responsibilities

### Domain

Owns validated value objects and policies such as logical routes, content identity, publication state, base paths, profile semantics, relationships, and provenance references.

The domain layer must not import:

- template engines;
- Markdown parsers;
- filesystem or Git APIs;
- GitHub-specific code;
- command-line frameworks;
- browser-resource serialization adapters;
- system time directly.

### Application

Coordinates validation, content loading, route planning, rendering requests, resource generation, and artifact assembly through explicit interfaces. It may depend on domain contracts but not on CLI parsing or GitHub Actions.

### Adapters

Read and write specific formats: Markdown, YAML or JSON configuration, JSON Schema, templates, static assets, generated resources, and the output filesystem. Adapters translate external data into validated domain or application inputs.

### Delivery

The command-line interface validates arguments, selects a publication profile, invokes application services, reports diagnostics, and returns defined exit codes. It contains no publication policy.

### Browser modules

Browser modules consume generated public contracts. They must not share source code with the Python domain through generated assumptions or inspect repository structures. Compatibility is defined by versioned public resources.

## Cross-cutting rules

- Route resolution has one owner and is reused by HTML, assets, metadata, feeds, sitemap, and browser resources.
- Publication-state filtering occurs before rendering and indexing.
- Templates receive validated view models and cannot discover arbitrary content.
- Time, locale, environment variables, and filesystem ordering enter through controlled adapters.
- Generated-resource serializers produce stable ordering and explicit schema versions.
- Logging and diagnostics may observe all layers but cannot alter domain outcomes.
- Tests may use adapters and fixtures; production code must never depend on test packages.

## Third-party dependencies

A third-party package is admitted only when it provides material value over the Python standard library, has compatible licensing, supports the selected Python version, is actively maintained, and can be pinned reproducibly. Framework convenience is not sufficient reason to hand architectural ownership to a library.

## Enforcement

The test strategy must include import-boundary tests or equivalent static checks once package names exist. Circular imports are prohibited. A proposed dependency that violates the direction above requires redesign or an accepted ADR.
