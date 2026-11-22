---
title: Base-path resolution specification
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: generated URLs, assets, metadata, and browser resources
related-adrs: [0004, 0005, 0020]
---

# Base-path resolution specification

## Input model

A base path is a URL path prefix, not a filesystem path, route, or origin. Resolution MUST be implemented by one tested service shared by all publishers and serializers.

## Normalization

The normalizer MUST:

1. require a string value;
2. reject control characters, backslashes, query strings, fragments, schemes, authorities, and percent-encoded path separators;
3. reject `.` and `..` segments before and after percent-decoding validation;
4. add one leading slash when omitted;
5. collapse a missing trailing slash by adding one;
6. preserve `/` as the root base path;
7. reject duplicate separators rather than silently changing ambiguous input;
8. produce a normalized value with one leading and one trailing slash.

Examples:

| Input | Result |
|---|---|
| `/` | `/` |
| `new` | `/new/` |
| `/new` | `/new/` |
| `/new/` | `/new/` |
| `https://example.com/new/` | rejected |
| `/new?x=1` | rejected |
| `/../new/` | rejected |

## Resolving logical routes

A logical route MUST begin with `/` and MUST NOT already contain the base path as a deployment prefix. Resolution joins the normalized base path and logical route without duplicate separators.

Examples:

| Base path | Logical route | Public path |
|---|---|---|
| `/` | `/articles/example/` | `/articles/example/` |
| `/new/` | `/articles/example/` | `/new/articles/example/` |
| `/new/` | `/` | `/new/` |

Passing `/new/articles/example/` as a logical route while the base path is `/new/` MUST fail as likely prefix leakage.

## Resolving assets and resources

Assets and generated resources MUST use governed root-relative logical locations before base-path resolution. Templates and browser source MUST NOT build paths through string concatenation. Fragment-only references remain fragment-only. External absolute URLs are validated separately and MUST NOT be prefixed.

## Absolute URLs

Absolute public URLs are formed by combining the validated `base_url` and resolved public path. The combination MUST preserve the origin, MUST NOT introduce duplicate slashes after the authority, and MUST reject a base URL whose path conflicts with the profile model.

## Encoding

Resolution operates on normalized Unicode route segments before URL serialization. The serializer MUST apply consistent UTF-8 percent encoding and MUST NOT double-encode existing data. Route comparison and collision detection occur on the canonical normalized representation.

## Required tests

Tests MUST cover root and nested base paths, root routes, directory and file routes, Unicode segments, fragments, traversal attempts, encoded separators, duplicate separators, prefix leakage, and absolute URL composition.
