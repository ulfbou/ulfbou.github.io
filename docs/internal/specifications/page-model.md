---
title: Page model specification
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: all generated HTML pages
related-adrs: [0006, 0007]
---

# Page model specification

## Purpose

The page model is the complete validated input to an HTML template. It combines page identity, profile-resolved references, content, navigation, metadata, provenance presentation, and module baselines. Templates MUST NOT discover missing data from the repository.

## Required fields

Every page model MUST provide:

- `page_id`: stable internal identifier;
- `route`: normalized logical route;
- `output_path`: validated artifact-relative path;
- `language`: BCP 47 language tag;
- `title`: visible primary title;
- `document_title`: complete browser and search title;
- `description`: concise plain-text summary;
- `page_type`: governed type identifier;
- `publication_state`: eligible public state;
- `canonical_url`: profile-resolved absolute URL when required;
- `robots`: explicit indexing and following policy;
- `body`: validated rendered-content model or safe rendered fragment;
- `primary_navigation`: ordered navigation items;
- `breadcrumbs`: ordered ancestors when applicable;
- `footer`: governed footer model;
- `assets`: resolved stylesheet, script, image, and icon references;
- `provenance_summary`: public provenance fields required by the content type;
- `modules`: ordered static module models;
- `structured_data`: validated structured-data models when applicable.

## Structural requirements

A rendered page MUST contain:

- one `html` element with the declared language;
- one `head` with character encoding, viewport, title, and required metadata;
- one `body` with semantic header, navigation, `main`, and footer landmarks as applicable;
- exactly one primary `h1` describing the route's main content;
- a stable skip link to the main content;
- no duplicate HTML IDs;
- no empty links or controls;
- no essential action represented only by a non-semantic clickable element.

## Navigation items

Each navigation item MUST provide a label, logical destination or governed external URL, relationship type, and current-state indication when applicable. Resolved URLs are produced before templating.

## Module models

Each module model MUST declare its identifier, type, static content, placement, maturity, enhancement policy, and fallback. A page remains complete if every optional module is left unenhanced.

## Metadata consistency

The visible title, document title, description, canonical URL, social metadata, structured data, and route MUST describe the same page identity. Templates MUST NOT invent fallback descriptions from arbitrary body truncation.

## Accessibility and safety

Rendered body and module content MUST follow the semantic HTML and content-security standards. Raw authored HTML is prohibited unless a later accepted content specification defines sanitization and permitted elements.
