---
title: ADR 0006 - Generate complete static baselines
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: all public pages
supersedes:
superseded-by:
related-specifications: [page-model.md, output-artifact.md]
---

# ADR 0006: Generate complete static baselines

## Context

The planned platform may later use browser modules, search, and local personalization. If foundational content or navigation is fetched or assembled only in the browser, the site becomes fragile, less accessible, harder to index, and dependent on runtime resources that GitHub Pages cannot recover server-side.

## Decision drivers

- Preserve access without JavaScript or browser storage.
- Provide reliable search-engine and social-client content.
- Minimize blank, shifting, or failure-prone initial rendering.
- Keep publication truth in the governed build.

## Options considered

### Browser-rendered application shell

This maximizes runtime flexibility but makes essential behavior dependent on JavaScript and fetched resources.

### Minimal static shell with most content fetched later

This improves initial metadata but still weakens content availability and resilience.

### Complete static page with optional enhancement

This generates the authoritative experience and lets runtime features add bounded value.

## Decision

Every public route MUST be generated as a complete static HTML document containing its essential content, navigation, metadata, provenance presentation, and recovery paths.

A page MUST remain coherent when scripts do not load, publication resources are unavailable, or browser storage cannot be used. Browser modules MAY add approved related material, filtering, continuity, or personalization, but MUST NOT become the only source of primary content or publication truth.

## Consequences

### Positive

- Strong accessibility, resilience, indexing, and direct-link behavior.
- Public pages can be inspected and validated as artifacts.
- Browser enhancement failures are isolated.

### Negative or limiting

- Some content is repeated between HTML and optional indexes or resources.
- Templates and page models must represent complete baselines.
- Runtime-only experiences require explicit nonessential classification.

### Follow-up obligations

- Test representative routes with JavaScript disabled.
- Define module baselines before implementing browser modules.

## Validation

Artifact tests confirm essential content and navigation in generated HTML, and browser tests confirm that failed enhancement does not remove them.
