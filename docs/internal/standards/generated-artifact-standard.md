---
title: Generated artifact standard
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: publication output and derived files
related-adrs: [0013, 0014]
---

# Generated artifact standard

## Definition

A generated artifact is any file or directory produced from governed source by a build, transformation, validation, reporting, or deployment process. Examples include HTML, publication JSON, feeds, sitemap, search indexes, fingerprinted assets, copied site assets, coverage reports, caches, and composed GitHub Pages trees.

## Ownership

Generated artifacts are owned by the process that creates them. They are not authoring surfaces and MUST NOT become authoritative inputs to a normal subsequent build.

Every generator MUST identify:

- its governed inputs;
- its output root;
- whether the output is deployable, diagnostic, or temporary;
- how stale output is removed;
- the validation required before use.

## Repository policy

Generated site output MUST NOT be committed. Build directories, caches, coverage output, virtual environments, tool caches, and local preview output MUST be ignored by Git.

A generated-file exception requires an accepted ADR that identifies the consumer, explains why regeneration at consumption time is unsuitable, defines review and update controls, and keeps generated files distinct from authored source.

## Build behavior

A generator MUST write to a clean staging location and MUST NOT merge with stale output. Promotion to the requested output location occurs only after generation and required validation succeed. A failed build MUST NOT replace the last known valid output.

Generated output MUST be safe to delete. Recreating it from the same governed inputs and locked tools MUST produce semantically equivalent output.

## Manual changes

Manual edits to generated output are prohibited. A correction MUST be made to source, configuration, templates, schemas, or generator code and then regenerated. Pull-request review MUST reject fixes that modify only generated output.

## Identification

Generated text files MAY contain a concise generated-file notice when that notice does not alter a public format or user experience. The notice MUST name the owning command or source contract and MUST NOT include machine-specific paths or volatile timestamps.

## Deployment

CI MUST deploy only validated artifacts created in the current workflow. It MUST NOT depend on generated output committed by a developer or left by a previous job. Deployment composition MUST fail if required inputs are absent rather than silently preserving stale files.

## Review evidence

A pull request that changes generation behavior SHOULD provide focused evidence such as affected route lists, structural diffs, representative rendered pages, or artifact-validation results. Evidence is diagnostic output and MUST not be committed unless it is governed documentation.
