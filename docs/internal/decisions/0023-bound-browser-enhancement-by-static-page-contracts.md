---
title: ADR 0023 - Bound browser enhancement by static page contracts
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: public pages and browser-enhanced modules
supersedes:
superseded-by:
related-specifications: [page-model.md, output-artifact.md]
---

# ADR 0023: Bound browser enhancement by static page contracts

## Context

ADRs 0006 and 0007 require complete static baselines and progressive enhancement. The planned platform may later add search, page modules, learning interactions, local continuity, and personalization.

Those broad principles are insufficient by themselves to prevent gradual erosion of the baseline. A browser feature could technically be described as an enhancement while still owning essential navigation, fetching the only copy of important content, hiding the page until initialization completes, or corrupting unrelated regions when it fails.

A more precise ownership decision is required for how optional browser behavior interacts with generated pages.

## Decision drivers

- Preserve complete visitor journeys when JavaScript or browser storage fails.
- Prevent browser features from becoming an undeclared publication engine.
- Contain enhancement failures to bounded page regions.
- Prefer semantic and native HTML behavior.
- Make every enhanced capability testable against an explicit fallback.
- Preserve accessibility, indexing, metadata, and publication truth in generated output.

## Options considered

### Allow browser modules to control the complete page

This provides maximum runtime flexibility, but a module failure could remove content, navigation, recovery, or metadata from the visitor experience.

### Require no JavaScript

This preserves simplicity and resilience but excludes useful search, filtering, continuity, learning interaction, and bounded personalization.

### Permit enhancement without explicit module boundaries

This supports incremental scripting but permits implicit DOM ownership, initialization-order coupling, and failure propagation.

### Require each browser capability to operate within a declared static page contract

This preserves static authority while allowing bounded, independently testable improvements.

## Decision

Every optional browser capability MUST operate within an explicit static page or module contract.

Before browser enhancement runs, the generated HTML MUST contain the essential content, primary and contextual navigation, required metadata, provenance presentation, and recovery paths for the route.

Each enhanced module MUST declare:

- a stable module identifier and governed type;
- its bounded DOM region;
- its validated publication-owned inputs;
- its complete static baseline or explicitly approved nonessential fallback;
- whether enhancement is optional or unavailable;
- its permitted browser capabilities;
- its accessibility requirements;
- its failure and compatibility behavior; and
- whether it may read or write approved local state.

Browser modules MUST initialize idempotently within their declared regions. A module MUST NOT remove, reinterpret, or corrupt content outside its owned region.

Browser code MUST NOT:

- fetch the primary page body as its only published copy;
- create primary navigation without a generated fallback;
- hide essential content until initialization completes;
- require cookies, local storage, IndexedDB, telemetry, or another optional API for access;
- infer publication truth from browser-local state;
- expose content excluded by publication state or profile;
- derive private contracts by scraping arbitrary page markup;
- make canonical metadata, indexing policy, provenance, or route identity depend on runtime state; or
- replace usable page content with a generic application error when enhancement fails.

Published resources consumed by browser modules MUST be versioned when compatibility requires it. Missing, malformed, incompatible, or stale optional resources MUST leave the baseline usable.

Native HTML behavior MUST be preferred when it satisfies the requirement. Feature detection MUST be used instead of browser identification.

## Consequences

### Positive

- Browser capabilities can evolve without replacing the publication foundation.
- Failures remain local to the responsible module.
- Accessibility and no-JavaScript behavior are deliberate contracts.
- Generated HTML remains independently inspectable and indexable.
- Later personalization cannot silently take ownership of publication truth.

### Negative or limiting

- Every enhanced module requires a baseline, ownership boundary, and failure tests.
- Some application frameworks and page-wide hydration models will be unsuitable.
- Baseline and enhanced behavior may duplicate limited data or controls.
- Cross-module coordination must use explicit published contracts rather than shared hidden state.

### Follow-up obligations

- Define module architecture, module models, runtime behavior, feature policy, and browser coding standards before implementing the first module.
- Add no-JavaScript artifact assertions for representative routes.
- Add browser tests for initialization failure, missing resources, denied storage, repeated initialization, keyboard use, and focus behavior.
- Define version and compatibility behavior for browser-consumed publication resources.
- Require governance review before a module gains greater influence over generated or client-side presentation.
- Complete the privacy prerequisites before any module uses visitor-derived local state.

## Validation

The decision is satisfied when:

- representative pages remain coherent and navigable with JavaScript disabled;
- each browser module has a declared baseline and bounded DOM owner;
- failed or repeated initialization does not remove content or corrupt another module;
- denied storage and unavailable optional APIs preserve access;
- essential content, navigation, metadata, and provenance are present in generated HTML;
- browser code cannot publish content excluded by the publication plan; and
- every enhanced interaction has accessibility and fallback evidence appropriate to its behavior.

## Notes

This ADR was reconstructed from the accepted static-first architecture, page-model specification, and progressive-enhancement decisions after its reference was found without a corresponding transported ADR file. It does not authorize browser modules, search, IndexedDB, or personalization before their just-in-time documentation gates are satisfied.