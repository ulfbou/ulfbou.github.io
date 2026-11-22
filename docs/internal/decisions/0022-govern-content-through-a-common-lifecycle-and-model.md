---
title: ADR 0022 - Govern content through a common lifecycle and model
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: public and candidate content
supersedes:
superseded-by:
related-specifications: [content-model.md]
---

# ADR 0022: Govern content through a common lifecycle and model

## Context

The platform will publish several content types, including pages, articles, portfolio entries, and learning resources. These types differ in presentation and type-specific metadata, but they share requirements for identity, routes, publication state, authorship, dates, provenance, review, correction, withdrawal, and migration.

If each content type defines these concepts independently, similar records may acquire incompatible meanings. File presence, directory location, Git history, merge time, or build time could then be mistaken for publication authority. Content migration could also copy public-looking legacy output without deciding whether its claims, provenance, ownership, and state remain acceptable.

A common governed lifecycle and model are required before real content is migrated.

## Decision drivers

- Give every publishable item stable identity independent of filename and route.
- Apply one publication-state model across content types.
- Preserve accountable authorship and provenance.
- Prevent repository mechanics from deciding publication eligibility.
- Support explicit correction, replacement, withdrawal, archival, and migration.
- Permit type-specific models without weakening common safeguards.

## Options considered

### Let each content type define its own lifecycle and shared fields

This provides local flexibility, but it risks incompatible identity, state, date, provenance, and route semantics.

### Infer publication state from repository location or Git events

This is easy to automate, but folder placement, branch name, merge time, and file existence do not establish editorial approval or public suitability.

### Treat all files under a public content directory as publishable

This reduces metadata, but it makes accidental publication likely and provides no reliable candidate, withdrawal, archival, or review states.

### Define a common governed model with type-specific extensions

This creates an explicit publication boundary while allowing articles, portfolio entries, learning resources, and pages to add stricter requirements.

## Decision

Every governed content item MUST conform to a common content model before a type-specific model is applied.

The common model MUST define, at minimum:

- a stable globally unique content identifier;
- a governed content-type identifier;
- title and description;
- language;
- a normalized logical route;
- an explicit publication state;
- one or more governed authors;
- authored dates;
- provenance or an explicit original-authorship record; and
- a validated body or structured payload.

Content identity MUST remain independent of filename, source directory, title, and route. Route changes MUST NOT silently create a new content identity.

Publication state MUST be explicit governed data. File presence, directory location, Git branch, merge status, commit date, filesystem timestamps, and build time MUST NOT infer or advance publication state.

The common lifecycle MUST support `draft`, `candidate`, `approved`, `published`, `withdrawn`, and `archived` unless a later accepted decision changes that state model. Type-specific specifications MAY restrict available transitions or public archival behavior, but MUST NOT make an earlier state publicly eligible without an explicit governing decision.

Before content becomes eligible for public publication, it MUST have the required identity, route, authorship, provenance, dates, metadata, review evidence, and accessibility properties for its type.

Corrections, replacements, withdrawal, and archival MUST preserve relevant history and provenance. A build transformation MUST NOT become the author of content and MUST NOT invent missing provenance, dates, routes, states, or relationships.

Existing public material MUST pass through migration inventory and disposition. Legacy generated HTML and current public visibility are evidence, not automatic approval for the reconstructed platform.

## Consequences

### Positive

- Shared content concepts have one meaning across page families.
- Publication eligibility is explicit and reviewable.
- Migration decisions can distinguish preservation from uncritical copying.
- Authorship, provenance, corrections, and withdrawal survive publication.
- Type-specific specifications can focus on meaningful differences.

### Negative or limiting

- Content requires more governed metadata than a simple Markdown-file collection.
- Migration cannot be completed by copying current files or generated HTML.
- Schemas, state transitions, author records, and provenance records require maintenance.
- Invalid or incomplete content may remain unpublished even when it renders successfully.

### Follow-up obligations

- Define schemas for the common model and each introduced content type.
- Establish governed author and provenance records before migrating real content.
- Define migration inventory, disposition, and metadata mapping.
- Test state transitions, duplicate identity, route collisions, dates, provenance, and unsafe body content.
- Define route behavior for withdrawn, archived, replaced, and migrated content before those states are publicly used.
- Preserve explicit responsibility for author, reviewer, publisher, and platform-maintainer actions even when one person holds several roles.

## Validation

The decision is satisfied when:

- every published item validates against the common model and its type-specific model;
- duplicate identifiers and normalized routes fail validation;
- ineligible states never appear in public profiles;
- missing authorship, provenance, required dates, or required review data fails before publication;
- a route or filename change does not silently change content identity;
- migrated content has an explicit disposition; and
- correction, withdrawal, replacement, and archival behavior preserve required history and provenance.

## Notes

This ADR was reconstructed from the accepted content-governance policy and common content-model specification after its reference was found without a corresponding transported ADR file. Detailed fields and validation rules remain owned by the specifications.