---
title: Initial non-goals
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: initial reconstruction
---

# Initial non-goals

The following are intentionally excluded from the initial platform foundation. They may be introduced by later, separately specified increments.

## Not required for the first static generator

- visitor personalization;
- IndexedDB schemas or migrations;
- behavioral analysis or inferred interests;
- client-side search;
- advanced page-module orchestration;
- telemetry or analytics;
- a content-management user interface;
- server-side rendering or an application server;
- user accounts, authentication, comments, or server-hosted saved state;
- automatic translation;
- opaque AI-generated publication or ranking;
- reproducing the current .NET or Blazor project structure in Python;
- preserving source-folder-to-URL coupling;
- committing generated site output as authored source;
- hard-coding `/new/`, the production origin, or GitHub Pages paths in content.

## Not implied by later direction

Mentioning search, modules, taxonomy, personalization, or adaptive pages in architecture does not authorize implementation. Each capability requires its own accepted contracts, privacy and accessibility assessment where applicable, and increment-specific acceptance criteria.

## Quality is not deferred

The non-goals do not defer semantic HTML, accessibility foundations, secure content handling, deterministic generation, provenance preservation, URL correctness, or complete no-JavaScript baselines. Those are foundational constraints.
