---
title: Test strategy
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: reconstruction development and publication
---

# Test strategy

## Objectives

Testing provides evidence that accepted architecture and specifications are implemented, that generated artifacts are publishable, and that visitor-critical behavior survives optional-feature failure. Tests are selected by risk and contract, not by an arbitrary desire to maximize test count.

## Test levels

### Unit tests

Use pytest unit tests for pure domain and policy behavior, including identifiers, publication states, base-path normalization, route resolution, output mapping, profile validation, deterministic ordering, and eligibility rules.

Unit tests MUST be fast, isolated, table-driven where useful, and free from filesystem or environment dependence unless the adapter itself is under test.

### Schema and model tests

Validate accepted and rejected examples for content, configuration, taxonomy, provenance, and generated public resources. Every required field, closed value set, unsafe value, and compatibility boundary MUST have representative fixtures.

### Component tests

Exercise adapters and application services with pytest temporary directories. Cover Markdown loading, template rendering, asset handling, artifact assembly, diagnostics, and failure cleanup without invoking GitHub or a network service.

### End-to-end build tests

Run the supported CLI against a minimal representative fixture site. Assert exit status, artifact paths, route equivalence across `/` and `/new/`, complete HTML structure, and expected profile-specific differences.

### Artifact contract tests

Inspect the generated tree directly. Check internal links, assets, canonical and robots metadata, structured resources, forbidden files, local path leakage, route collisions, UTF-8, 404 behavior, and no-JavaScript baseline content.

### Browser tests

Introduce Playwright only when browser behavior exists. Browser tests cover direct navigation, keyboard use, enhancement failure, storage denial, responsive layouts, and browser-consumed resource compatibility. They MUST NOT duplicate behavior already proven cheaply below the browser layer.

### Accessibility tests

Use automated checks such as axe-core through Playwright plus manual keyboard, zoom, reflow, reduced-motion, and screen-reader checks for representative templates. Automated success does not establish full WCAG conformance.

### Performance tests

Measure representative generated routes against accepted budgets in a controlled environment. Performance checks become release gates only after budgets, routes, and measurement variance are specified.

### Security tests

Cover unsafe URLs, raw HTML or script injection, path traversal, output containment, malformed content, hostile browser-local data, workflow permissions, and dependency vulnerabilities according to the security specification.

## Test data

Fixtures MUST be minimal but representative. Include normal, boundary, malformed, Unicode, route-collision, withdrawn, missing-provenance, and base-path cases. Synthetic fixtures MUST be clearly identified and MUST not be mistaken for publishable content.

## Determinism and isolation

Tests MUST not use the network, depend on execution order, mutate repository sources, use real user home directories, or rely on current time. Time and environment values are injected. File comparisons normalize only differences explicitly permitted by a specification.

## Coverage

Coverage is diagnostic, not a substitute for contract testing. New domain and application code SHOULD maintain at least 90% statement coverage, but pull requests are evaluated primarily on whether meaningful branches, failures, and public contracts are exercised. Generated boilerplate or unreachable defensive code must not be added to manipulate coverage.

## Defect rule

A defect correction MUST add the lowest-level regression test that reproduces the defect and SHOULD add a higher-level test only when the failure crossed an untested contract boundary.
