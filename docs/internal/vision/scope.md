---
title: Reconstruction scope
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
---

# Reconstruction scope

## In scope

The reconstruction includes:

- a Python 3 build, validation, and generation toolchain;
- structured authored content and validated metadata;
- generated static pages for the landing page, articles, portfolio material, learning content, and supporting routes;
- configurable publication profiles for local, preview, and production builds;
- correct operation at both `/new/` and `/` without source changes;
- semantic HTML, accessibility requirements, responsive presentation, and performance budgets;
- canonical metadata, social metadata, structured data, sitemap, feeds, and static 404 behavior;
- governed taxonomy, relationships, search resources, and related-content mechanisms;
- versioned publication resources for optional browser clients;
- progressively enhanced page modules;
- optional browser-local personalization with explicit controls and graceful fallback;
- deterministic artifact generation and automated validation;
- parallel preview deployment, semantic content migration, cutover, and rollback;
- architecture, governance, provenance, standards, specifications, ADRs, testing, security, privacy, accessibility, and operational documentation needed by each increment.

## Migration boundary

Approved public content and verified publication requirements may be migrated. Generated output and legacy implementation structure are evidence, not source architecture. Migration decisions must classify content as preserve, revise, replace, withdraw, archive, or exclude.

## Delivery boundary

The source repository owns authored inputs, schemas, templates, Python source, browser source, tests, configuration, and documentation. The build owns generated publication output. Deployment publishes a validated artifact rather than treating generated pages as manually maintained source.

## Scope changes

A proposed change belongs in this reconstruction only when it supports the platform vision and has a defined visitor or governance outcome. Material expansion requires an update to this document and, where architectural consequences exist, an ADR.
