---
title: System context
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
related-adrs:
  - 0002
  - 0004
  - 0006
  - 0007
---

# System context

## Purpose

The platform publishes complete, trustworthy, accessible static pages from governed repository sources. It may progressively enhance those pages in the visitor's browser, but its essential public behavior does not depend on a server application, client-side JavaScript, or browser storage.

## System boundary

The system includes:

- authored content, metadata, configuration, taxonomy, schemas, templates, source code, tests, and documentation in the repository;
- the Python 3 validation and publication toolchain;
- generated static artifacts and publication resources;
- CI validation and GitHub Pages deployment workflows;
- optional browser modules operating within published contracts.

The system does not include GitHub itself, GitHub Pages infrastructure, search-engine crawlers, visitor browsers, external source systems, or third-party link targets.

## Actors and external systems

### Author and maintainer

Ulf Bourelius authors and reviews content, configuration, architecture, specifications, and code. The maintainer approves publication state changes and architectural decisions. Roles remain logically separate even when held by the same person.

### Contributor

A contributor proposes bounded changes through repository workflows. Contributions must conform to accepted contracts and cannot redefine architecture implicitly through implementation.

### Visitor

A visitor accesses public pages through a browser. The visitor must receive a coherent baseline without JavaScript and may choose whether to use later local personalization controls.

### GitHub repository

GitHub stores governed source history and runs protected collaboration workflows. Repository contents are not themselves the public site artifact.

### GitHub Actions

GitHub Actions validates source, builds the selected publication profile, validates the resulting artifact, and coordinates deployment. Workflow execution must use least privilege and must not make generated output authored source.

### GitHub Pages

GitHub Pages serves the validated static artifact. It provides static hosting, not application-server behavior or arbitrary server redirects.

### Browser runtime

The browser renders generated HTML and may execute compatible publication clients and modules. Browser code consumes only published resources and must tolerate unavailable storage, missing optional resources, and version incompatibility.

### Search engines and link clients

Crawlers and social clients consume public HTML, canonical metadata, robots policy, structured data, sitemap, feeds, and social-preview metadata. Preview and production indexing behavior is controlled by publication profile.

### External evidence and source systems

External sources may support articles, learning content, portfolio evidence, and attribution. Their data enters the platform only through governed authored records; it is never silently fetched into a production build unless a later accepted specification explicitly permits that behavior.

## Primary flows

1. The maintainer changes governed repository sources.
2. Validation checks structure, schemas, policy, and content contracts.
3. The Python publisher resolves logical routes and assets against a publication profile.
4. Generation writes a complete static artifact to a configured output directory.
5. Artifact validation checks routes, links, resources, metadata, accessibility rules, and forbidden output.
6. CI deploys the validated artifact through the authorized GitHub Pages workflow.
7. Visitors and crawlers request static resources from the configured base URL and base path.
8. Optional browser modules enhance the baseline without assuming private server state.

## Trust boundaries

- Repository input is untrusted until validated, including Markdown and configured URLs.
- Generated HTML must escape or sanitize authored input according to the content-security specification.
- CI credentials and GitHub tokens never enter generated artifacts or browser resources.
- Browser-local state is untrusted, versioned input and must not determine publication truth.
- External links and evidence are references, not trusted executable content.

## Architectural constraints

- Python 3 is the only implementation language for the build and publication toolchain.
- Public output is static and deployable to GitHub Pages.
- `/new/` is a configurable preview base path, never a source route prefix.
- Generated output is replaceable and must be reproducible from governed inputs.
- Essential content, navigation, and recovery remain available without enhancement.
