---
title: Deterministic build standard
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: validation, generation, and artifact assembly
related-adrs: [0014]
---

# Deterministic build standard

## Requirement

Equivalent governed source, configuration, dependency lock, tool version, and explicit build inputs MUST produce semantically equivalent artifacts and diagnostics.

Byte-for-byte identity is required for deterministic text and serialized resources. Binary optimization MAY differ only when the responsible tool cannot guarantee byte identity and an accepted specification defines semantic comparison and pins the tool version.

## Controlled inputs

The build MAY depend only on:

- repository-governed source;
- the selected publication profile;
- locked tool and dependency versions;
- explicit CLI arguments;
- explicitly supported reproducibility variables such as `SOURCE_DATE_EPOCH`.

The build MUST NOT implicitly depend on wall-clock time, local timezone, locale, username, home directory, absolute repository path, Git branch name, uncommitted status, filesystem enumeration order, file modification time, network responses, or CI run numbers.

## Ordering

All unordered inputs MUST be normalized before output. Files are processed by normalized repository-relative path. Content collections, navigation, taxonomy, relationships, diagnostics, JSON keys where contractually relevant, sitemap entries, and feed entries MUST use specified stable ordering with deterministic tie-breakers.

## Text and serialization

- Governed and generated text MUST use UTF-8.
- Generated text MUST use LF and end with one newline unless a public format specifies otherwise.
- JSON MUST use stable key and item ordering, consistent escaping, and no insignificant environment-dependent formatting.
- Dates and times MUST use ISO 8601 and UTC where time is required.
- Numeric and date formatting MUST not depend on process locale.
- HTML attribute or metadata ordering SHOULD remain stable to produce reviewable diffs.

## Time

Content dates remain authored metadata. Build time MUST not overwrite them. If a generated resource requires a build timestamp, it MUST derive from an explicit reproducible input, preferably `SOURCE_DATE_EPOCH`, and its contract MUST explain why it is necessary.

## Paths

Artifacts and diagnostics MUST use logical routes or repository-relative paths. Absolute local paths MUST not enter output. Path separators in public and serialized contracts MUST be `/` regardless of the host operating system.

## Randomness

Random values are prohibited in normal publication. If a later feature requires randomness, it MUST accept an explicit seed and demonstrate why deterministic selection cannot satisfy the requirement.

## Verification

CI MUST include a clean repeated-build test once the initial generator exists. It SHOULD build the same profile twice in independent temporary directories and compare normalized trees, file sets, and hashes. Any allowed difference MUST be explicitly named and tested.

A nondeterministic difference is a build defect even when public rendering appears unchanged, because it weakens review, caching, provenance, and rollback confidence.
