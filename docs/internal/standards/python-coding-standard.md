---
title: Python coding standard
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: Python publication platform
related-adrs: [0002, 0014]
---

# Python coding standard

## Runtime and packaging

The platform MUST use Python 3 only. The first implementation PR MUST select and pin one supported minor version in `pyproject.toml`, CI, and local setup documentation. Support for another minor version requires passing the full test suite on that version.

The project MUST use a `src` layout with an installable package named `ulfbou_site`. Build and test commands MUST run against the installed package, not an accidental import from the repository root.

`pyproject.toml` is the authoritative project configuration. Runtime and development dependencies MUST be declared and locked reproducibly. Dependency groups MUST separate runtime, test, lint, typing, and documentation needs.

## Required tools

The initial toolchain MUST use:

- `ruff` for linting and formatting;
- `mypy` for static type checking;
- `pytest` for automated Python tests;
- `coverage.py` through `pytest-cov` for coverage evidence;
- `uv` for environment and lock-file management unless superseded by an ADR.

Tool versions MUST be locked. CI MUST invoke repository-defined commands rather than unpinned global installations.

## Formatting and source conventions

- Source encoding MUST be UTF-8.
- Line endings MUST be LF in governed text files.
- Formatting MUST be produced by `ruff format` without manual exceptions.
- Lint suppressions MUST be narrow, include the rule identifier, and include a reason when the code does not make the reason self-evident.
- Wildcard imports are prohibited.
- Import-time filesystem access, environment mutation, network access, and publication work are prohibited.
- Module-level mutable state is prohibited except for immutable constants.

## Typing

All production functions and public test helpers MUST have type annotations. `mypy` MUST run in strict mode for the platform package. Use precise domain types rather than passing unvalidated strings for routes, identifiers, states, base paths, or output paths.

`Any` is prohibited in domain and application code unless isolated at an external-library boundary and immediately validated or narrowed. Unchecked `cast` calls require a comment explaining the invariant.

## Domain modeling

- Prefer frozen dataclasses or equivalent immutable value objects for validated domain data.
- Validate external representations at adapters and construct valid domain values explicitly.
- Do not use dictionaries as long-lived substitutes for defined content, profile, route, page, or provenance models.
- Use enums or constrained literal types for closed state sets.
- Keep route resolution, publication eligibility, and output mapping in dedicated domain or application services.

## Functions and modules

Functions SHOULD have one clear responsibility and SHOULD return values rather than mutate distant state. Side effects MUST be explicit at adapter boundaries. Modules MUST follow the accepted dependency rules and MUST NOT create circular imports.

Public functions, classes, modules, and non-obvious policies MUST have docstrings explaining contracts and failure behavior. Docstrings MUST not restate type signatures.

## Exceptions and diagnostics

Expected validation and publication failures MUST use defined exception or result types and MUST be converted into stable diagnostics at the CLI boundary. Do not catch `Exception` unless adding context at the outer application boundary before re-raising or returning the documented internal-failure exit status.

Exceptions MUST NOT be used for ordinary branching. Error messages MUST identify the violated contract and relevant source location without leaking secrets or machine-specific paths.

## Filesystem and text handling

- Use `pathlib.Path` for filesystem paths.
- Resolve and validate output containment before writing.
- Specify UTF-8 explicitly for text I/O.
- Preserve authored newlines according to the relevant content specification.
- Sort discovered inputs by normalized repository-relative path before processing.
- Never depend on filesystem enumeration order, modification times, local timezone, locale, or current working directory.

## Logging

Normal successful operation SHOULD be quiet and deterministic. Diagnostics go to standard error; machine output requested by an option goes to standard output. Logging MUST NOT alter artifacts or include secrets.

## Tests

Each behavior change MUST include tests at the lowest effective level. Bug fixes MUST include a test that fails without the correction. Tests MUST not access the network, depend on execution order, sleep for synchronization, or write outside pytest-managed temporary directories.

## Security

Shell execution is prohibited in publication code unless an accepted ADR and specification define the command, validation, and threat controls. Unsafe deserialization, dynamic code execution, and templating of untrusted executable expressions are prohibited.
