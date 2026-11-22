---
title: Common content model specification
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: governed public content items
related-adrs: [0013, 0022]
---

# Common content model specification

## Purpose

The common content model defines fields shared by pages, articles, portfolio entries, and learning resources. Type-specific specifications may add constraints but MUST NOT weaken common publication, provenance, or route rules.

## Required fields

Every content item MUST provide:

- `id`: stable, globally unique identifier independent of route and filename;
- `type`: governed content-type identifier;
- `title`: human-readable title;
- `description`: concise plain-text description;
- `language`: BCP 47 language tag;
- `route`: normalized logical route;
- `state`: governed publication state;
- `authors`: one or more governed author identifiers;
- `created`: ISO 8601 calendar date;
- `provenance`: one or more governed provenance references or an explicit original-authorship record;
- `body`: structured or Markdown source accepted by the type specification.

## Optional common fields

A content type MAY support:

- `updated`;
- `published`;
- `reviewed`;
- `summary` distinct from description;
- `tags` using governed taxonomy identifiers;
- `relationships` using governed relationship types;
- `image` using a governed media record;
- `canonical_route` when distinct from an alias or migration route;
- `replaces` or `replaced_by`;
- `withdrawal` metadata;
- type-specific social or structured-data fields.

## Identity and routes

`id` remains stable when a title, filename, or route changes. Routes MUST NOT be inferred solely from filenames. Route changes require migration review and may require a generated redirect. Two active items MUST NOT share an ID or normalized route.

## Publication states

The initial common states are:

- `draft`: incomplete and never public;
- `candidate`: under review and never included in public profiles by default;
- `approved`: content approved but not necessarily scheduled for public output;
- `published`: eligible for profiles whose date and policy conditions are satisfied;
- `withdrawn`: excluded from normal discovery and handled by the withdrawal policy;
- `archived`: retained as intentionally historical content when the type permits public archival.

State transitions are governed by content governance, not inferred from file location or Git branch.

## Dates

Dates MUST use ISO 8601. `created` describes the authored work, not repository file creation. `updated` records a material public revision. `published` records first public publication or a governed scheduled time. Build time MUST NOT overwrite content dates.

## Provenance

Provenance MUST distinguish original authorship, cited evidence, derived content, migrated content, and automated assistance where disclosure is required. A build transformation does not become the author of content. Missing required provenance is a validation error, not a field to infer.

## Body handling

Markdown interpretation MUST be deterministic and configured centrally. Unsupported raw HTML, executable content, unsafe URLs, and ambiguous directive syntax MUST be rejected. Conversion MUST preserve intended structure without silently rewriting meaning.

## Validation

Validation MUST report duplicate IDs or routes, unknown authors, invalid states, impossible date ordering, unknown taxonomy or relationship identifiers, missing required provenance, unsafe links, and fields not permitted by the declared content type.
