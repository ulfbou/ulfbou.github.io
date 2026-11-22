---
title: Repository structure
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
related-adrs:
  - 0002
  - 0013
  - 0014
---

# Repository structure

## Purpose

The repository structure separates authored truth, executable publication logic, validation contracts, tests, and replaceable generated artifacts. Directory names express ownership, not public URL structure.

## Target top-level structure

```text
.
â”œâ”€â”€ README.md
â”œâ”€â”€ CONTRIBUTING.md
â”œâ”€â”€ CHANGELOG.md
â”œâ”€â”€ LICENSE.md
â”œâ”€â”€ SECURITY.md
â”œâ”€â”€ pyproject.toml
â”œâ”€â”€ uv.lock
â”œâ”€â”€ content/
â”œâ”€â”€ config/
â”œâ”€â”€ taxonomy/
â”œâ”€â”€ schemas/
â”œâ”€â”€ templates/
â”œâ”€â”€ assets/
â”œâ”€â”€ src/
â”œâ”€â”€ tests/
â”œâ”€â”€ tools/
â”œâ”€â”€ docs/
â””â”€â”€ .github/
```

A generated output directory such as `build/` or `dist/` may exist locally or in CI but must be ignored by Git and must not be treated as an authored repository area.

## Directory ownership

### `content/`

Contains authored public content and its front matter or structured metadata. Content paths do not determine public routes unless an accepted specification explicitly maps them. Content must not contain deployment prefixes or generated asset fingerprints.

Initial subdirectories may include `pages/`, `articles/`, `portfolio/`, and `learning/`. They are authoring categories, not route promises.

### `config/`

Contains validated platform and publication-profile configuration. Environment names may select values, but configuration must not contain secrets. Preview and production differences belong here rather than in content or templates.

### `taxonomy/`

Contains governed terms, aliases, relationships, and deprecation metadata. Free-form classifications must not bypass this area once taxonomy support is introduced.

### `schemas/`

Contains versioned machine-readable schemas for content, configuration, publication resources, and modules. A schema change that breaks accepted inputs or clients requires an explicit migration and version decision.

### `templates/`

Contains presentation templates and shared fragments. Templates render validated models; they do not read arbitrary repository files, determine publication state, or encode deployment paths.

### `assets/`

Contains authored static assets such as styles, browser-source modules, images, icons, and fonts. Generated fingerprints, optimized derivatives, bundles, and copied assets belong only in build output.

### `src/`

Contains the installable Python package for domain models, validation, route resolution, rendering, publication resources, artifact assembly, and command-line entry points. Python packages must follow the dependency rules.

### `tests/`

Contains automated tests and explicit fixtures. Tests mirror behavior and contracts rather than copying the production package tree mechanically. Golden artifacts must be small, reviewed, and used only when structural assertions are insufficient.

### `tools/`

Contains bounded maintainer utilities that are not part of the supported publication interface. A tool that becomes required for normal builds must move into the Python package and receive tests and a specified interface.

### `docs/`

Contains all human-facing public and internal documentation in Markdown. Accepted ADRs, specifications, and policies are governed source.

### `.github/`

Contains GitHub Actions workflows, issue and pull-request templates, and repository automation configuration. Workflows orchestrate supported commands; they must not duplicate publication logic in shell or YAML.

## Prohibited ownership patterns

- Public routes must not be inferred solely from source folder locations.
- Templates must not own content selection, publication state, canonical policy, or taxonomy.
- Browser code must not parse repository source formats.
- CI YAML must not implement route resolution or artifact composition logic that belongs in tested Python code.
- Generated output must not be edited manually or committed as source merely to make GitHub Pages deployment convenient.
- Documentation must not be generated into non-Markdown formats as its only human-readable form.

## Package boundary

The Python distribution should use a `src` layout, for example `src/ulfbou_site/`. The final package name is established by the build-interface specification and `pyproject.toml`; imports must never depend on the repository being the current working directory.

## Change rule

A new top-level directory requires a documented owner and must not overlap an existing responsibility. Material changes to these boundaries require architecture review and may require an ADR.
