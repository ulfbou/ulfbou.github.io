---
title: Development roadmap
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
review-cycle: at each increment boundary
applies-to: ulfbou.github.io reconstruction
related-adrs:
  - 0002
  - 0004
  - 0005
  - 0006
  - 0007
  - 0013
  - 0014
  - 0015
related-specifications:
  - ../specifications/build-interface.md
  - ../specifications/publication-profiles.md
  - ../specifications/base-path-resolution.md
  - ../specifications/output-artifact.md
  - ../specifications/page-model.md
  - ../specifications/content-model.md
---

# Development roadmap

## Purpose

This roadmap sequences the reconstruction of `ulfbou.github.io` from the accepted pre-development contracts to production cutover. It is an internal planning document, not a release promise or fixed calendar.

Each increment must deliver a coherent, reviewable capability. An increment may begin only when the documents identified by `development-prerequisites.md` for that capability are accepted. Later roadmap stages do not authorize implementation before their just-in-time contracts exist.

## Governing delivery rules

1. Work proceeds on the dedicated reconstruction integration branch with short-lived, bounded feature branches.
2. Each pull request has one primary objective and preserves unrelated work.
3. Authored sources, tests, schemas, templates, and Markdown documentation are committed. Generated publication output is not committed.
4. The supported Python CLI owns validation, generation, and artifact checks. GitHub Actions orchestrates those commands without reimplementing publication logic.
5. Every generated page has a complete static baseline before browser enhancement.
6. Root and `/new/` behavior are produced from publication profiles, not source forks or hard-coded paths.
7. Documentation is written just in time before the increment that depends on it.
8. A roadmap stage is complete only when its exit criteria are demonstrated by tests or artifact evidence.
9. Dates are assigned only after the immediately preceding stage has met its exit criteria. Sequence and dependency are authoritative; calendar estimates are not.

## Stage 0: documentation and decision baseline

### Objective

Establish the minimum architecture, governance, standards, specifications, testing policy, and ADRs required to begin implementation without silently creating foundational contracts in code.

### Delivered scope

- vision, principles, scope, non-goals, and glossary;
- system context, repository boundaries, dependency rules, static-first architecture, routing, and publication architecture;
- build, profile, base-path, artifact, page, and content specifications;
- Python, generated-artifact, and deterministic-build standards;
- test strategy and quality gates;
- change and content governance;
- foundational ADRs;
- this roadmap and the staged prerequisite policy.

### Exit criteria

- every Gate A document exists and is accepted;
- the documentation set contains no unresolved contradiction that blocks Stage 1;
- the aggregate documentation carrier verifies as DX v2.0.0;
- later capabilities remain explicitly gated rather than accidentally authorized.

## Stage 1: Python repository foundation

### Objective

Create the installable Python platform skeleton and developer quality infrastructure without yet implementing publication behavior.

### Scope

- select and pin one supported Python 3 minor version;
- create `pyproject.toml` and a reproducible `uv.lock`;
- create the `src/ulfbou_site/` package and `tests/` structure;
- register the `ulfbou-site` command with placeholder subcommands matching the accepted build interface;
- configure Ruff formatting and linting, strict mypy, pytest, pytest-cov, and import-boundary enforcement;
- establish repository `.gitignore` rules for build output, virtual environments, caches, coverage, and previews;
- add focused CI for formatting, linting, typing, and tests;
- add contributor commands that run the existing relevant checks without unrelated repository-state gates.

### Exclusions

- no HTML generation;
- no content migration;
- no browser JavaScript;
- no GitHub Pages deployment changes;
- no generated output committed.

### Exit criteria

- a clean Termux or supported development environment can install from the lock file;
- `ulfbou-site --help` and each specified subcommand parse deterministically;
- Ruff, mypy, and pytest pass through repository-defined commands;
- import-boundary checks enforce the accepted dependency direction;
- CI invokes the same commands used locally.

## Stage 2: domain contracts and publication profiles

### Objective

Implement the validated domain values on which every later publisher component depends.

### Scope

- content identity and content-state types;
- logical route and artifact-relative output-path types;
- base URL and base-path value types;
- local, preview, and production publication-profile models;
- normalization and rejection rules from `base-path-resolution.md`;
- route-to-output mapping;
- stable diagnostics and documented CLI exit statuses;
- positive, boundary, Unicode, traversal, collision, and prefix-leakage tests.

### Exit criteria

- profile and path behavior is covered by table-driven unit tests;
- `/`, `/new/`, nested routes, explicit file routes, and invalid inputs behave according to specification;
- logical routes cannot contain deployment prefixes;
- domain and application code contain no template, filesystem, GitHub, or browser dependencies;
- diagnostics are deterministic and do not expose local absolute paths.

## Stage 3: minimal deterministic publisher

### Objective

Generate and validate the smallest complete static site through the supported CLI.

### Scope

- validated loading of the minimum page source and publication configuration;
- a page model for the root route;
- one semantic HTML template;
- deterministic asset copying for a minimal stylesheet;
- generation of `index.html`, `404.html`, and `assets/`;
- clean staging and atomic output replacement;
- `validate`, `build`, and `check-artifact` behavior;
- root and `/new/` profile builds;
- repeated-build artifact comparison.

### Acceptance example

The same source must build:

- `/` links for the local or production profile;
- `/new/` links for the preview profile;
- identical logical routes and baseline content;
- only the profile-governed URL, indexing, canonical, and environment-label differences.

### Exit criteria

- complete baseline HTML exists before JavaScript;
- direct loading of the root and 404 pages works in both base-path modes;
- artifact validation detects missing files, broken internal assets, path leakage, and profile-inconsistent metadata;
- two independent clean builds have identical deterministic file hashes;
- failed generation leaves any previously valid output unchanged.

## Stage 4: content and provenance foundation

### Documentation gate

Before implementation starts, accept:

- provenance model, source policy, authorship policy, and content-integrity policy;
- schema governance and taxonomy governance;
- article, portfolio, taxonomy, and relationship model specifications;
- migration content inventory, disposition, and metadata mapping.

### Objective

Introduce governed content types and make provenance survive validation and rendering.

### Scope

- Markdown and metadata loading through explicit schemas;
- common content-model validation;
- governed authorship and source records;
- page and article types first, followed by portfolio and learning-resource types as separately bounded increments;
- publication-state eligibility;
- deterministic Markdown rendering with unsafe HTML and URLs rejected;
- public provenance presentation appropriate to each type;
- correction, replacement, archive, and withdrawal metadata foundations.

### Exit criteria

- invalid state, date, route, author, provenance, and unsafe-link cases fail with actionable diagnostics;
- build transformations never become content authors;
- migration does not copy generated legacy HTML as authoritative source;
- only eligible content appears in public artifacts;
- representative content renders as complete, accessible static pages.

## Stage 5: information architecture and baseline page families

### Objective

Create the complete baseline experience for the platform's primary visitor journeys before adaptive behavior.

### Planned vertical increments

1. shared site shell, header, primary navigation, footer, breadcrumbs, and skip link;
2. landing page with editorially selected baseline modules;
3. article listing and article detail pages;
4. portfolio listing and portfolio detail pages;
5. learning-area landing and learning-resource pages;
6. governed related-resource and continuation links;
7. static tag or collection pages after taxonomy contracts are active.

### Constraints

- each page family is independently releasable to preview;
- source directories do not define public routes;
- native HTML disclosure and navigation are preferred;
- a page cannot depend on later search or personalization to be useful;
- visual alignment uses shared design tokens and semantic components rather than copied generated markup.

### Exit criteria

- all intended page families have complete static baselines;
- navigation and contextual relationships form coherent visitor journeys;
- representative pages pass keyboard, zoom, reflow, contrast, and no-JavaScript review;
- route and metadata contracts are validated in the artifact.

## Stage 6: preview deployment under `/new/`

### Documentation gate

Before deployment work starts, accept:

- deployment architecture, preview deployment, concurrency, and environment-profile documents;
- artifact-composition, canonical-metadata, SEO, structured-data, sitemap, feed, and 404 specifications needed by preview;
- GitHub workflow and supply-chain security policies;
- artifact-validation and parallel-development plans;
- ADRs for the integration branch, combined Pages artifact, single deployment coordinator, preview no-index policy, and preview canonical policy.

### Objective

Publish the reconstruction safely below `/new/` while preserving the current production root.

### Scope

- build current production and reconstruction preview as independently validated inputs;
- compose one GitHub Pages artifact with current production at `/` and reconstruction at `/new/`;
- use one deployment coordinator and one concurrency authority;
- permit pull requests to build and validate without Pages publication authority;
- deploy only from the authorized post-merge workflow;
- enforce preview `noindex` behavior and self-consistent preview canonical URLs;
- verify direct navigation, assets, nested routes, and 404 recovery under `/new/`.

### Exit criteria

- preview deployment cannot overwrite or distort current production root content;
- only one workflow can publish the composed Pages artifact;
- PR workflows perform the full build and validation but cannot push to GitHub Pages;
- preview URLs are excluded from production sitemap and feeds;
- generated output remains uncommitted;
- rollback to the prior Pages artifact is documented and tested.

## Stage 7: search, taxonomy, and versioned publication resources

### Documentation gate

Before implementation starts, accept the search architecture, publication-manifest, platform-resource, navigation-resource, content-index, search-index, browser-publication-client, taxonomy, and relationship specifications.

### Objective

Add governed discovery resources and browser search without making primary navigation or content depend on them.

### Scope

- controlled taxonomy identifiers, aliases, and deprecation;
- generated tag and collection pages;
- a versioned publication manifest;
- deterministic navigation, content-index, and search-index resources;
- client compatibility and graceful failure behavior;
- browser search with accessible controls and useful no-JavaScript alternatives;
- related-resource selection from the shared publication plan.

### Exit criteria

- malformed or incompatible resources do not damage baseline pages;
- search indexes contain only eligible public content;
- search behavior is keyboard accessible and tested at representative data sizes;
- resource versions and compatibility behavior are explicit;
- taxonomy changes follow governance and do not silently reclassify content.

## Stage 8: governed page modules

### Documentation gate

Before implementation starts, accept module architecture, page-module model, module-runtime, feature-policy, module-governance, and browser coding standards.

### Objective

Introduce page modules with controlled influence over generated pages and optional client behavior.

### Initial maturity levels

1. **Static marker:** metadata influences generation only.
2. **Generated module:** the publisher renders a complete baseline region.
3. **Enhanced module:** browser code improves behavior inside a bounded region.
4. **Adaptive module:** approved local signals may influence a limited part of the module under explicit policy.

### Scope

- stable module identifiers and validated inputs;
- placement and ordering contracts;
- static baseline and failure behavior for each module;
- publication-controlled feature policy;
- bounded DOM ownership and idempotent initialization;
- module-level accessibility and compatibility tests;
- initial landing-page modules with editorially selected baseline content.

### Exit criteria

- disabling every script leaves each page coherent;
- module failures do not affect content outside their owned region;
- modules cannot publish ineligible content or invent routes;
- maturity promotion requires governance review rather than a code-only change.

## Stage 9: local personalization and continuity

### Documentation gate

Before implementation starts, accept:

- personalization architecture;
- privacy model, data minimization, local-storage policy, controls, transparency, retention, deletion, and telemetry policy;
- privacy impact assessment;
- IndexedDB data model and migration specification;
- personalization-signal and recommendation-policy specifications;
- ADRs covering local-only state, IndexedDB, bounded influence, and explainable recommendations;
- public privacy and automation disclosures.

### Objective

Provide optional, explainable visitor continuity without server-side profiling or loss of editorial control.

### Planned sequence

1. explicit enable, disable, review, and reset controls;
2. saved resources and manual interests;
3. learning continuation from explicit visitor actions;
4. locally derived interests using only approved signals;
5. explainable recommendations inside bounded adaptive modules;
6. retention expiry and versioned IndexedDB migration.

### Constraints

- the default baseline is complete without personalization;
- no local signal changes publication eligibility or canonical truth;
- visitor data does not leave the browser unless a later independently approved architecture changes that rule;
- recommendations supplement rather than replace editorial baseline content;
- every recommendation can state a useful reason;
- storage denial, corruption, quota failure, and migration failure preserve access.

### Exit criteria

- visitors can inspect and delete all locally held personalization state;
- denied or unavailable IndexedDB leaves the baseline unchanged;
- recommendation tests enforce eligibility, diversity, maximum influence, and explanation rules;
- privacy documentation matches actual behavior.

## Stage 10: migration completion and production hardening

### Documentation gate

Before production-readiness work starts, accept the full security model and threat model, accessibility standard and test plan, production operations and incident runbooks, cutover and rollback plans, route mapping, final content disposition, and release-readiness criteria.

### Objective

Complete semantic migration, close release-critical risks, and prove the reconstruction can replace production safely.

### Scope

- finalize content inventory and disposition;
- migrate approved page, article, portfolio, and learning content;
- preserve required routes or create governed static redirects;
- verify provenance, authorship, corrections, media rights, and public metadata;
- complete WCAG 2.2 AA evidence for representative templates and critical journeys;
- enforce performance budgets for HTML, critical CSS, JavaScript, images, requests, and interaction latency;
- complete dependency, workflow-permission, content-injection, path-containment, and artifact-exposure security checks;
- verify production sitemap, feeds, structured data, canonical URLs, social previews, and 404 behavior;
- rehearse production build, cutover, and rollback from clean environments.

### Exit criteria

- every current public route has an explicit preserve, redirect, replace, archive, withdraw, or remove decision;
- no private, candidate, or unsupported content is in the production artifact;
- all release-blocking accessibility, security, privacy, provenance, and performance findings are resolved;
- the complete production artifact is deterministic and independently validated;
- the previous production artifact can be restored through the documented rollback path.

## Stage 11: production cutover

### Objective

Replace the current production root with the reconstructed platform through configuration and a full validated rebuild.

### Sequence

1. freeze migration-affecting changes for the cutover window;
2. verify the last preview artifact and release-readiness record;
3. switch the production profile to the final root behavior without changing logical routes;
4. build and validate the complete root artifact from clean source;
5. verify that no unintended `/new/` references remain;
6. deploy through the single authorized Pages workflow;
7. run public smoke checks for root, nested routes, assets, metadata, feeds, sitemap, direct navigation, and 404 behavior;
8. retain immediate rollback ability until the defined observation period ends.

### Exit criteria

- production uses the reconstructed root artifact;
- expected logical routes resolve correctly from `/`;
- production indexing and canonical metadata are correct;
- essential pages work without JavaScript and browser storage;
- no workflow can publish an unvalidated or competing Pages artifact;
- rollback remains available for the agreed observation period.

## Stage 12: post-cutover consolidation

### Objective

Remove temporary migration infrastructure and establish normal platform evolution.

### Scope

- remove `/new/` preview output or replace only governed known routes with temporary static redirects;
- retire legacy .NET, Blazor, and obsolete deployment workflows after rollback retention permits;
- remove temporary migration adapters and source snapshots that no longer serve an evidence purpose;
- update public and internal documentation to production ownership;
- review real visitor and maintainer outcomes without introducing telemetry contrary to policy;
- convert unresolved noncritical findings into bounded roadmap increments;
- review ADRs and specifications for supersession, not retrospective rewriting.

### Exit criteria

- one production platform and one authoritative publication pipeline remain;
- no indefinite duplicate maintenance of `/new/` and `/` exists;
- temporary redirects have owners and removal dates;
- repository documentation matches the post-cutover implementation;
- future work proceeds through the established change-control and just-in-time documentation gates.

## Cross-stage workstreams

These concerns are continuous and are not postponed to a final hardening phase:

### Accessibility

Semantic HTML, keyboard access, focus behavior, contrast, reflow, reduced motion, and clear content structure are included from the first generated template. Later audits deepen evidence but do not introduce the foundation retrospectively.

### Security

Input validation, path containment, safe Markdown handling, escaping, dependency locking, secret exclusion, and least-privilege workflows are implemented at the stage where their attack surface appears.

### Provenance

Content identity, authorship, source references, transformation history, corrections, and withdrawals remain intact from ingestion through generated output.

### Performance

Static-first delivery, bounded assets, deterministic resources, and limited browser code are architectural defaults. Formal budgets are introduced before page families or scripts can exceed them unnoticed.

### Documentation

Each increment updates only the documents whose accepted contracts or operational behavior changed. Accepted ADRs are superseded rather than rewritten.

## Roadmap change control

This roadmap may be refined as evidence emerges, but changes must preserve accepted architecture or explicitly propose the ADRs needed to alter it.

A roadmap update MUST identify:

- the changed stage, dependency, or exit criterion;
- the evidence motivating the change;
- affected contracts and risks;
- whether work is reordered, split, added, removed, or deferred;
- whether the change modifies scope or only delivery sequence.

Roadmap changes do not override normative specifications, governance policies, or ADRs.
