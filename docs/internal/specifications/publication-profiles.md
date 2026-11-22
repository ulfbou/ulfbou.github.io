---
title: Publication profiles specification
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: local, preview, and production publication
related-adrs: [0004, 0005, 0015, 0018, 0019]
---

# Publication profiles specification

## Purpose

A publication profile defines deployment-dependent policy without changing content, templates, logical routes, or module identity.

## Required fields

Each profile MUST define:

- `name`: stable identifier;
- `base_url`: validated absolute HTTPS origin for externally visible URLs, except that a local profile MAY use an explicitly allowed local HTTP origin;
- `base_path`: normalized deployment path;
- `indexing`: `allow` or `deny`;
- `canonical_mode`: `profile` or another later specified mode;
- `environment_label`: optional visible non-production label;
- `output_policy`: named artifact policy;
- `feature_policy`: named set of enabled publication capabilities.

Profiles MUST NOT contain credentials or visitor data.

## Initial profiles

### `local`

- intended for maintainer verification;
- base path defaults to `/` but MAY be overridden explicitly for path testing;
- indexing MUST be denied;
- canonical output MAY be omitted only when the local specification explicitly allows it;
- no deployment authority.

### `preview`

- public origin: the configured GitHub Pages origin;
- base path: `/new/` until cutover;
- indexing MUST be denied;
- canonical URLs MUST be self-consistent preview URLs unless superseded by an accepted ADR;
- sitemap and feeds MUST exclude preview URLs from production discovery;
- pages SHOULD display a clear preview label without changing content meaning.

### `production`

- public origin: the production GitHub Pages origin;
- base path: `/`;
- indexing MAY be allowed only for eligible published routes;
- canonical URLs MUST use production locations;
- sitemap and feeds MUST contain only production-eligible routes;
- preview labels MUST be absent.

## Profile invariants

Given identical governed content, preview and production builds MUST have equivalent logical route sets except for explicitly profile-only operational routes. Differences MUST be limited to resolved locations, indexing and canonical policy, visible environment labeling, and explicitly governed feature policy.

Content MUST NOT branch on profile name. Templates MUST NOT contain `/new/` literals. A profile MUST NOT make draft, candidate, private, or withdrawn content public unless a separate accepted publication-state rule explicitly permits a protected preview behavior.

## Validation

Profile validation MUST reject:

- duplicate names;
- unsupported schemes;
- base URLs containing query or fragment components;
- invalid or traversal-containing base paths;
- production profiles that deny required production metadata;
- preview profiles that allow indexing;
- unknown feature or artifact policies;
- secret-like fields.
