---
title: Product principles
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
---

# Product principles

## 1. Visitor value precedes implementation convenience

Architecture and workflow choices must improve relevance, clarity, trust, accessibility, discoverability, or performance for visitors. Internal convenience alone is insufficient justification for a visitor-facing compromise.

## 2. Complete static baseline

Every public route must provide its essential content, navigation, metadata, and recovery paths in generated HTML before JavaScript executes.

## 3. Progressive enhancement

Browser code may improve discovery, filtering, continuity, and presentation. Failure or absence of JavaScript, IndexedDB, or optional resources must leave a coherent page.

## 4. Explicit ownership and contracts

Authored content, schemas, templates, generators, publication resources, browser modules, and deployment artifacts must have distinct responsibilities. Generated output is not an authoring surface.

## 5. Configuration-driven publication

Deployment origin, base path, indexing policy, canonical behavior, and publication resources are controlled by validated publication profiles. Preview paths such as `/new/` must never be embedded in content or module identity.

## 6. Deterministic generation

Equivalent source, configuration, and tool versions must produce semantically equivalent artifacts. Ordering, serialization, locale, timestamps, and path handling must not introduce uncontrolled variation.

## 7. Provenance is first-class data

Relevant source, authorship, derivation, review, correction, and withdrawal information must survive the publication pipeline. Automation must not silently change meaning.

## 8. Privacy by design

Personalization begins with no server-side visitor profile. Local behavior must be bounded, understandable, controllable, deletable, and introduced only after a privacy impact assessment.

## 9. Accessibility is a release property

Semantic HTML, keyboard use, focus visibility, contrast, reflow, reduced motion, and clear language are implementation requirements, not retrospective improvements.

## 10. Governed evolution

Foundational decisions require ADRs. Public contracts and schemas require explicit versions and migration rules. Documentation is written when an increment needs it, not speculatively for distant features.

## 11. Preserve approved semantics, not legacy structure

Migration may reuse verified content and publication rules. It must not reproduce obsolete .NET, Blazor, folder, route, or deployment assumptions merely for implementation similarity.

## 12. Bounded adaptation

Editorially selected baseline content remains authoritative. Adaptive behavior may supplement or reorder only within an approved policy and must preserve visitor control and explainability.
