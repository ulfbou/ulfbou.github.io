---
title: ADR 0018 - Control publication capabilities through profile policy
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: publication capabilities and artifact policy
supersedes:
superseded-by:
related-specifications: [publication-profiles.md, output-artifact.md]
---

# ADR 0018: Control publication capabilities through profile policy

## Context

Local, preview, and production publication may require different generated resources, operational labels, indexing behavior, and artifact constraints. Later platform stages may also introduce search resources, browser modules, feeds, manifests, and other capabilities that are not valid for every publication target or development stage.

If content, templates, browser code, or scattered environment checks decide which capabilities are active, publication behavior becomes difficult to inspect and reproduce. The same governed source could then produce materially different results because of hidden environment state or implementation-specific conditionals.

ADR 0015 establishes named publication profiles. A separate decision is required for how optional publication capabilities and artifact behavior are selected within those profiles.

## Decision drivers

- Keep environment-dependent capability selection explicit and validated.
- Prevent content and templates from activating platform features.
- Support staged introduction of capabilities without source forks.
- Make the enabled publication surface reproducible and inspectable.
- Ensure artifact validation knows which resources are required, optional, or forbidden.
- Prevent undeclared environment variables from changing publication behavior.

## Options considered

### Let content or templates enable capabilities

This would permit local flexibility, but publication policy would become distributed across authored material and presentation code. Individual content items could accidentally activate unsupported resources or environment-specific behavior.

### Read environment variables throughout the publisher

This would make CI configuration convenient, but it would create hidden build inputs and weaken deterministic generation. It would also make local, preview, and production behavior harder to compare.

### Enable every capability in every profile

This would reduce configuration, but it would expose incomplete or unsuitable capabilities and make incremental development difficult. Local verification, public preview, and production do not always have identical operational requirements.

### Select named capability and artifact policies through the publication profile

This preserves one explicit configuration boundary. Profiles can select validated policies while content, templates, and publishing components remain independent of environment names.

## Decision

Every publication profile MUST select a named `feature_policy` and a named `output_policy`.

The `feature_policy` MUST identify the publication capabilities enabled for the build. A capability MUST NOT become active solely because implementation code, source files, templates, environment variables, or browser assets happen to exist.

The `output_policy` MUST identify the artifact requirements and exclusions that apply to the selected publication target. Artifact assembly and validation MUST use that policy when deciding which generated resources are required, permitted, or forbidden.

Feature and output policy identifiers MUST be validated before publication planning begins. Unknown identifiers MUST fail validation.

Content, templates, and browser modules MUST NOT branch on publication-profile names. They MAY consume validated capability decisions supplied through publication planning, but they MUST NOT independently infer those decisions from `local`, `preview`, `production`, `/new/`, the current Git branch, CI variables, or deployment URLs.

Policy values are governed build inputs and MUST participate in reproducibility evidence where they can affect generated output.

This ADR does not authorize any particular future capability. Search, browser modules, feeds, personalization, publication manifests, and similar capabilities remain unavailable until their own prerequisite contracts are accepted.

## Consequences

### Positive

- Active publication capabilities are explicit and reviewable.
- Artifact validation can distinguish required, optional, and forbidden output.
- New capabilities can be introduced without source branches or template conditionals.
- Local, preview, and production differences remain governed configuration.
- Hidden environment state cannot silently expand the public artifact.

### Negative or limiting

- Feature and output policies require schemas, validation, and tests.
- Policy combinations must be kept intentionally small to avoid accidental configuration complexity.
- A capability cannot be enabled experimentally by adding an ad hoc environment check.
- Policy changes that alter public contracts may require specification updates or a new ADR.

### Follow-up obligations

- Define the initial feature-policy and output-policy identifiers before implementation depends on them.
- Validate policy identifiers when loading publication profiles.
- Ensure publication planning receives resolved capability decisions rather than raw profile-name conditionals.
- Test that unsupported or unknown policies fail before artifact generation.
- Extend artifact validation when a newly accepted capability adds required or forbidden resources.

## Validation

The decision is satisfied when:

- every supported profile resolves to one validated feature policy and one validated output policy;
- unknown policies fail before publication planning;
- content and templates contain no environment-name or `/new/` capability branching;
- adding source files for a disabled capability does not place them in the artifact;
- artifact validation enforces the resources required and forbidden by the selected policies; and
- repeated builds with the same governed source, profile, policies, dependency lock, and tools produce equivalent output and diagnostics.

## Notes

This ADR was reconstructed from the accepted publication-profile, artifact, and publication-architecture documentation after its reference was found without a corresponding transported ADR file. It records the decision already represented by those documents and does not independently authorize later publication capabilities.