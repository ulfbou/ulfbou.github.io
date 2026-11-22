---
title: ADR 0014 - Require deterministic builds
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: validation, generation, and artifact assembly
supersedes:
superseded-by:
related-specifications: [build-interface.md, output-artifact.md]
---

# ADR 0014: Require deterministic builds

## Context

Generated output is not committed, so confidence depends on being able to reproduce and meaningfully compare artifacts. Uncontrolled timestamps, ordering, paths, locale, or environment values would create noisy differences and weaken review, caching, provenance, and rollback.

## Decision drivers

- Make generated output reviewable and reproducible.
- Detect unintended semantic changes.
- Support reliable caching and artifact comparison.
- Avoid machine- and CI-specific output.

## Options considered

### Accept nondeterminism and review rendered results manually

This is simple initially but does not scale and hides subtle changes.

### Normalize artifacts only during comparison

This can conceal genuine nondeterminism and creates a second interpretation of output.

### Design deterministic generation

This constrains implementations but provides stable evidence throughout the pipeline.

## Decision

Equivalent governed inputs, publication profile, dependency lock, and tool versions MUST produce semantically equivalent artifacts and diagnostics. Deterministic text and serialized public resources MUST be byte-identical.

Builds MUST control ordering, encoding, line endings, serialization, locale, time, path representation, and environmental inputs. Required timestamps MUST derive from an explicit reproducible input such as `SOURCE_DATE_EPOCH`.

## Consequences

### Positive

- Artifact differences correspond to meaningful changes.
- Repeated builds can be verified automatically.
- Rollback and provenance evidence are stronger.

### Negative or limiting

- Some tools may be rejected or configured more strictly.
- Binary transformations may need pinned versions and semantic exceptions.

### Follow-up obligations

- Add a repeated clean-build comparison once the generator exists.
- Document any accepted non-byte-identical binary output explicitly.

## Validation

Two independent clean builds with the same governed inputs produce identical file sets and hashes for deterministic outputs.
