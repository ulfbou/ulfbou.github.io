---
title: ADR 0005 - Separate base URL and base path
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: publication profiles and URL generation
supersedes:
superseded-by:
related-specifications: [publication-profiles.md, base-path-resolution.md]
---

# ADR 0005: Separate base URL and base path

## Context

An origin such as `https://ulfbou.github.io` and a deployment prefix such as `/new/` have different validation, lifecycle, and resolution rules. Treating them as one `root_url` value encourages string concatenation and causes errors in canonical URLs, assets, browser requests, feeds, and sitemap entries.

## Decision drivers

- Represent origin and path semantics accurately.
- Support local, preview, and production profiles.
- Generate both root-relative public paths and absolute public URLs.
- Avoid ambiguous configuration ownership.

## Options considered

### One combined root URL

This is concise but conflates origin and path and makes root-relative resolution awkward.

### Derive the path from the base URL

This hides an important deployment contract and makes invalid combinations harder to detect.

### Model and validate both values independently

This is slightly more configuration but provides clear types and predictable composition.

## Decision

Publication profiles MUST define `base_url` and `base_path` as separate values.

`base_url` owns the externally visible origin used for absolute URLs. `base_path` owns the deployment prefix used for public paths within that origin. Each MUST be validated independently and combined only through the shared URL resolver.

Content, templates, and modules MUST NOT accept an ambiguous `root_url` substitute.

## Consequences

### Positive

- Canonical, social, sitemap, feed, and structured-data URLs can be formed correctly.
- Root-relative route resolution remains independent of the public origin.
- Configuration errors are diagnosed at the correct boundary.

### Negative or limiting

- Profile configuration contains two related values.
- Tests must cover invalid combinations and local-profile exceptions.

### Follow-up obligations

- Use distinct domain value types.
- Reject base URLs with conflicting path components unless a future specification explicitly permits them.

## Validation

Tests demonstrate correct production, preview, and local composition without duplicate paths, lost prefixes, or origin leakage.
