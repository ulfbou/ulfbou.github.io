---
title: Build interface specification
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: Python publication CLI
related-adrs: [0002, 0014, 0015]
---

# Build interface specification

## Normative language

The terms MUST, MUST NOT, SHOULD, SHOULD NOT, and MAY are normative.

## Supported entry point

The platform MUST expose one installed Python command named `ulfbou-site`. Normal local and CI builds MUST invoke this command rather than importing internal modules or duplicating build logic in shell or workflow YAML.

The initial command surface is:

```text
ulfbou-site validate --profile PROFILE
ulfbou-site build --profile PROFILE --output DIRECTORY
ulfbou-site check-artifact --profile PROFILE --artifact DIRECTORY
```

The implementation MAY also support `python -m ulfbou_site` as an equivalent developer entry point, but documentation and CI MUST use the installed command.

## Common behavior

All commands MUST:

- locate the repository root from an explicit `--root DIRECTORY` option or, when omitted, the current directory;
- read UTF-8 governed sources without lossy replacement;
- select exactly one named publication profile;
- emit deterministic diagnostics to standard error;
- avoid network access unless a later accepted specification explicitly permits it;
- avoid modifying authored sources;
- return a documented exit status;
- reject unknown options and unknown profiles.

## `validate`

`validate` MUST load and validate configuration, content, routes, schemas, templates, and governed relationships required by the selected profile. It MUST NOT create a deployable artifact. Temporary files, if any, MUST be isolated and removed on successful completion.

## `build`

`build` MUST perform source validation, publication planning, rendering, artifact assembly, and artifact validation. `--output` MUST be explicit and MUST NOT resolve to the repository root or to an authored-source directory.

The command MUST build in a clean staging location and replace the requested output only after the complete build passes. It MUST NOT merge with stale output. On failure, the previously valid output MUST remain unchanged.

## `check-artifact`

`check-artifact` MUST validate an existing artifact against the selected profile without rebuilding it or modifying it. It MUST inspect the actual output tree, generated HTML, public resources, and internal references.

## Diagnostics

A validation error SHOULD include:

- severity;
- stable rule identifier;
- source or artifact path;
- line or field when available;
- observed problem;
- required correction.

Diagnostics MUST NOT expose secrets, CI tokens, absolute home-directory paths, or private environment values.

## Exit statuses

- `0`: command completed successfully;
- `2`: command-line usage or unknown profile error;
- `3`: source, configuration, schema, or policy validation failed;
- `4`: publication planning or rendering failed;
- `5`: artifact validation failed;
- `6`: controlled filesystem or environment failure;
- `1`: unexpected internal failure.

A command MUST NOT return `0` when required checks were skipped or when only a partial artifact was produced.

## Reproducibility options

The CLI MUST accept governed time through `SOURCE_DATE_EPOCH` when a timestamp is required. Current wall-clock time MUST NOT enter semantic output implicitly. Locale, timezone, and filesystem enumeration order MUST NOT change semantic output.
