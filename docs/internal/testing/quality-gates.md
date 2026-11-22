---
title: Quality gates
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: pull requests, preview publication, and cutover
---

# Quality gates

## Principle

A gate exists only when it protects an accepted contract or material risk. Checks MUST be proportionate to the changed behavior and MUST not fail because of unrelated generated files, repository cleanliness, or checks that have no bearing on the deliverable.

## Documentation-only pull requests

A documentation-only change MUST pass:

- Markdown formatting or lint rules that exist in the repository;
- internal-link validation for changed documentation;
- front-matter validation for governed documents;
- DX carrier verification when a carrier is the delivery mechanism.

It MUST NOT run publication or browser suites unless the documentation changes an executable example or fixture.

## Python platform pull requests

A Python implementation change MUST pass, where relevant:

- `ruff format --check`;
- `ruff check`;
- `mypy` in the configured strict mode;
- focused pytest tests for the changed behavior;
- the existing broader unit or component suite affected by its dependency boundary;
- dependency lock consistency when dependencies change.

CI MUST use repository-defined commands or scripts. It MUST not rely on a clean `git diff --exit-code` as evidence that untracked generated output is correct.

## Generator and template pull requests

Changes affecting publication output MUST additionally pass:

- representative end-to-end builds for root and `/new/` base paths;
- artifact validation;
- deterministic repeated-build comparison;
- focused generated-output review for affected templates or routes;
- no-JavaScript baseline assertions.

Generated output remains uncommitted. CI validates the output it created.

## Browser-feature pull requests

Changes to browser modules MUST additionally pass:

- JavaScript linting and tests selected by the accepted browser standard;
- Playwright tests for changed interactions;
- keyboard and focus checks;
- disabled-script or failed-resource fallback tests;
- denied or unavailable storage tests when local storage is involved;
- axe-core checks for affected representative pages.

## Preview deployment gate

Before `/new/` deployment, CI MUST build the current governed preview source, validate the complete artifact, enforce no-index policy, verify resolved `/new/` references, and upload one immutable Pages artifact through the authorized deployment workflow. Pull-request workflows MUST NOT have production deployment authority.

## Production cutover gate

Cutover requires all preview gates plus accepted cutover and rollback checklists, route mapping, canonical and indexing verification, accessible representative-page review, content disposition completion, security and privacy readiness, and evidence that the previous production artifact can be redeployed.

## Failure policy

Required gate failures block promotion. Flaky tests are defects and MUST not be normalized through unconditional retries. A temporary waiver requires a documented owner, reason, bounded scope, expiry date, and follow-up issue; security, publication-state, path-containment, or invalid-artifact failures cannot be waived.
