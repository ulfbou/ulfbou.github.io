---
title: Static-first architecture
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: all public pages
related-adrs:
  - 0006
  - 0007
  - 0023
---

# Static-first architecture

## Principle

Each public route is generated as a complete document. Browser enhancement may improve the experience but cannot be the only source of essential content, navigation, meaning, metadata, or recovery.

## Required baseline

Before JavaScript executes, a public page must provide, as applicable:

- a unique document title and useful description;
- semantic landmarks and one clear primary heading;
- the complete primary content of the route;
- primary navigation and contextual navigation needed to continue;
- visible publication, authorship, and provenance information required by the content type;
- canonical, indexing, social, and structured metadata required by the active profile;
- accessible links to related or next-step material selected for the baseline;
- a usable footer and recovery path;
- forms or disclosures using native HTML where their baseline behavior is sufficient;
- meaningful failure content for unknown or withdrawn routes.

## Enhancement boundary

Browser code may:

- filter or sort an already represented collection;
- add approved related items within policy limits;
- preserve visitor choices locally;
- provide search over a generated index;
- enhance disclosure, navigation, or continuity behavior;
- explain why locally recommended material appears.

Browser code must not:

- fetch the primary article or page body as the only copy;
- construct primary navigation from an unavailable resource with no HTML fallback;
- hide essential content until initialization completes;
- require IndexedDB, local storage, cookies, or telemetry for access;
- replace editorial publication state with browser-local data;
- publish content excluded from the generated baseline or publication profile;
- make canonical, indexing, provenance, or accessibility truth depend on runtime state.

## Failure behavior

The following failures must preserve a usable baseline:

- JavaScript disabled or blocked;
- a script or module fails to load;
- publication resources are missing, malformed, incompatible, or stale;
- IndexedDB is unavailable, blocked, over quota, corrupt, or rejects a migration;
- personalization is disabled or reset;
- a browser lacks an optional API;
- the visitor uses keyboard navigation, high zoom, reduced motion, or high-contrast settings.

Enhancement failures may be reported unobtrusively when the visitor can act on them. They must not replace page content with a generic application error.

## Page modules

Every module declares:

- its static baseline;
- whether enhancement is optional or unavailable;
- inputs owned by publication rather than DOM scraping;
- a bounded DOM region;
- failure and compatibility behavior;
- accessibility requirements;
- whether it may read or write local state.

A module without a useful baseline cannot be placed on a primary public route unless an accepted specification classifies it as explicitly nonessential.

## Verification

Representative routes must be tested with JavaScript disabled. Artifact tests must confirm that critical content and navigation are present in generated HTML rather than only in browser resources. Browser tests must verify that failed enhancement does not remove or corrupt the baseline.
