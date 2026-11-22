---
title: ADR 0007 - Use progressive enhancement
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: browser behavior
supersedes:
superseded-by:
related-specifications: [page-model.md]
---

# ADR 0007: Use progressive enhancement

## Context

The platform should support interactive learning, filtering, search, modules, and visitor-controlled adaptation while preserving the complete static baseline. A clear enhancement principle is needed so browser features do not gradually turn the site into a client-rendered application.

## Decision drivers

- Add interaction without weakening baseline access.
- Isolate failures and unsupported browser capabilities.
- Preserve semantic HTML and native behavior.
- Introduce advanced features incrementally.

## Options considered

### Client-side application architecture

This offers unified runtime state but conflicts with the static-baseline requirement.

### No JavaScript

This maximizes simplicity but unnecessarily excludes useful search, filtering, continuity, and educational interaction.

### Progressive enhancement

This keeps HTML authoritative while allowing bounded browser behavior.

## Decision

Browser functionality MUST progressively enhance generated semantic HTML. Native HTML behavior MUST be preferred where it satisfies the requirement. Scripts MUST initialize within bounded regions, tolerate missing optional resources, and leave the original page usable after failure.

Enhancement MUST NOT hide essential content pending initialization, require storage for access, synthesize primary navigation without fallback, or reinterpret publication state. Feature detection MUST be used instead of browser identification.

## Consequences

### Positive

- Interactive features can evolve without replacing the publication foundation.
- Accessibility and resilience remain architectural properties.
- Modules can be tested independently against explicit fallbacks.

### Negative or limiting

- Both baseline and enhanced behavior require design and testing.
- Some application-style libraries will be unsuitable.

### Follow-up obligations

- Define a browser coding standard before the first module implementation.
- Test disabled JavaScript, failed resources, and unavailable storage.

## Validation

Each browser feature has an identified baseline, bounded DOM ownership, and a test proving usable fallback when enhancement is unavailable.
