---
title: ADR 0019 - Preserve logical-route equivalence across publication profiles
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: local, preview, and production route eligibility
supersedes:
superseded-by:
related-specifications: [publication-profiles.md, base-path-resolution.md]
---

# ADR 0019: Preserve logical-route equivalence across publication profiles

## Context

The reconstruction must build under `/new/` for preview and later under `/` for production without duplicating content or changing logical route identity. Publication profiles legitimately differ in base URL, base path, indexing, canonical metadata, visible environment labeling, and explicitly governed capabilities.

Without a route-equivalence rule, profile configuration could gradually become a second content-selection system. Preview and production might then expose different public structures because of profile names, deployment prefixes, source copies, template conditions, or accidental configuration drift.

ADR 0015 establishes publication profiles, while ADRs 0004 and 0005 separate deployment paths and origins from logical routes. A separate decision is needed to constrain how far profile differences may influence the set of published logical routes.

## Decision drivers

- Make preview representative of the production information architecture.
- Preserve stable route identity through preview and cutover.
- Prevent publication profiles from becoming content forks.
- Detect profile drift before deployment.
- Keep environment-specific differences narrow and explainable.
- Allow explicitly operational routes without weakening content-route equivalence.

## Options considered

### Maintain separate content trees for preview and production

This could permit independent experimentation, but it would duplicate governed source, create migration drift, and weaken preview as evidence of production readiness.

### Allow unrestricted profile-specific content selection

This would be flexible, but route differences could become accidental and difficult to review. A successful preview would no longer demonstrate that production could publish the same logical structure.

### Require identical artifacts across profiles

This would be too strict because base paths, absolute URLs, indexing metadata, labels, and certain operational resources must differ.

### Require equivalent logical routes with explicitly bounded profile differences

This preserves shared public identity while allowing deployment-specific resolution and policy.

## Decision

Given identical governed source and equivalent publication eligibility inputs, preview and production builds MUST produce equivalent sets of public logical content routes.

Differences between preview and production MUST be limited to:

- base-URL and base-path resolution;
- canonical and indexing policy;
- visible non-production labeling;
- profile-governed artifact behavior;
- explicitly accepted feature-policy differences; and
- explicitly specified operational routes that do not represent divergent public content.

A publication profile MUST NOT make `draft`, `candidate`, private, withdrawn, or otherwise ineligible content public unless a separate accepted decision and specification define protected preview behavior.

Content MUST NOT branch on publication-profile name. Source directories, templates, Git branches, and deployment prefixes MUST NOT define separate preview and production content identities.

Profile-specific operational routes MUST be explicitly declared and MUST NOT be treated as evidence that ordinary content-route divergence is permitted.

Cutover from `/new/` to `/` MUST preserve logical route identity. It changes deployment resolution and profile policy, not the underlying content routes.

## Consequences

### Positive

- Public preview provides meaningful evidence of the production structure.
- Cutover can be performed through configuration and a complete rebuild.
- Route drift between environments becomes detectable.
- Content identity remains independent of deployment.
- Preview cannot silently become a separate publication product.

### Negative or limiting

- Experiments that require different public content need a separately governed mechanism.
- Route-set comparison must distinguish content routes from explicitly operational routes.
- Scheduling and eligibility inputs must be controlled to make profile comparisons meaningful.
- Preview cannot expose candidate content merely for convenience.

### Follow-up obligations

- Define how route sets are exported or inspected during validation.
- Classify any profile-only operational routes explicitly.
- Add tests comparing logical route sets across preview and production.
- Ensure migration and cutover tests compare route identity independently of base path.
- Require a separate ADR and specification before introducing protected candidate-content preview.

## Validation

The decision is satisfied when:

- the same eligible governed content produces equivalent logical content-route sets for preview and production;
- `/new/` appears only in resolved preview locations, not in logical routes;
- differences are restricted to accepted profile-owned metadata, labels, policies, and operational routes;
- preview cannot publish content that production-state rules classify as ineligible; and
- cutover to `/` requires no content-path rewrite or route-identity migration.

## Notes

This ADR was reconstructed from the accepted publication-profile, routing, and base-path documentation after its reference was found without a corresponding transported ADR file. It records an existing constraint and does not authorize protected draft or candidate previews.