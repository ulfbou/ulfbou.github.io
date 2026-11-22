---
title: ADR 0020 - Centralize public reference resolution
status: accepted
owner: Ulf Bourelius
decision-date: 2026-11-22
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: generated routes, links, assets, metadata, feeds, and browser resources
supersedes:
superseded-by:
related-specifications: [base-path-resolution.md, publication-profiles.md]
---

# ADR 0020: Centralize public reference resolution

## Context

The platform must publish the same logical routes beneath `/new/` during preview and `/` in production. Generated output includes navigational links, assets, canonical URLs, social metadata, structured-data identifiers, sitemap entries, feed links, and browser-resource locations.

If each template, serializer, resource generator, or browser module constructs URLs independently, base-path handling will drift. Common failures include duplicated prefixes, missing prefixes, incorrect absolute URLs, preview-path leakage, inconsistent encoding, and invalid links from nested routes.

ADRs 0004 and 0005 establish configurable base paths and separate base URLs from base paths. A decision is required to assign ownership of all public-reference resolution to one platform boundary.

## Decision drivers

- Apply one base-path rule to every generated reference.
- Prevent templates and serializers from concatenating URLs.
- Preserve logical routes independently of deployment.
- Produce consistent relative, root-relative, and absolute locations.
- Detect prefix leakage and invalid paths before output is published.
- Make route and reference behavior testable without rendering every page.

## Options considered

### Construct paths directly in templates and serializers

This is locally simple, but each output producer could interpret base paths, encoding, and trailing slashes differently.

### Use relative URLs throughout

Relative URLs avoid some root-prefix handling but become fragile across nested routes, copied fragments, feeds, canonical metadata, structured data, and browser requests.

### Depend on the HTML `base` element

A `base` element changes document-wide resolution behavior and can create unintended effects for fragments, forms, scripts, copied markup, and third-party content. It also does not solve non-HTML resources.

### Use one validated resolver for all generated references

This creates an explicit ownership boundary and lets all output producers consume resolved locations without implementing path policy.

## Decision

One shared public-reference resolver MUST own the transformation of validated logical routes and governed resource locations into profile-resolved public paths and absolute public URLs.

The resolver MUST:

- distinguish logical routes, artifact output paths, base paths, base URLs, fragments, and governed external URLs;
- normalize and validate base paths before resolution;
- reject traversal, schemes or authorities in path-only inputs, query or fragment contamination, duplicate separators, encoded path separators, and likely deployment-prefix leakage;
- resolve navigational links, authored assets, generated resources, browser fetch locations, canonical URLs, social URLs, structured-data identifiers, sitemap locations, and feed links through the same policy;
- apply consistent Unicode normalization and UTF-8 URL serialization;
- preserve fragments without incorrectly prefixing fragment-only references;
- reject ambiguous or invalid inputs rather than guessing intended paths; and
- produce deterministic results independent of the current page's filesystem location or host operating system.

Templates, content, resource serializers, browser source, and workflow configuration MUST NOT concatenate deployment URLs or independently add `/new/`.

Logical routes and governed resource identifiers MUST remain deployment-independent. Artifact output-path mapping MAY use the same validated route information but MUST remain a distinct operation because filesystem output paths and public URLs have different security and normalization requirements.

The HTML `base` element MUST NOT be used as the platform's primary routing mechanism. Adopting it as such requires a superseding ADR.

## Consequences

### Positive

- Root and nested deployments use one testable policy.
- Preview-prefix leakage can be detected consistently.
- Templates remain presentation-focused.
- Metadata, feeds, HTML, and browser resources agree on public identity.
- Cutover does not require rewriting authored references.

### Negative or limiting

- Every output producer must consume the resolver instead of using convenient string operations.
- Third-party libraries that insist on owning URL construction may require adapters or rejection.
- Logical route resolution and artifact output mapping must remain separate even when their inputs overlap.
- The resolver becomes a critical platform contract requiring extensive boundary tests.

### Follow-up obligations

- Implement dedicated validated types for logical routes, base paths, base URLs, governed external URLs, and artifact-relative paths.
- Use the resolver in all HTML, metadata, sitemap, feed, structured-data, asset, and browser-resource generation.
- Add table-driven tests for root paths, nested paths, Unicode, fragments, explicit files, traversal, encoding, duplicate separators, and prefix leakage.
- Add artifact checks for unresolved internal references and profile-inconsistent locations.
- Prevent templates and browser source from containing hard-coded `/new/` paths.

## Validation

The decision is satisfied when:

- one resolver produces all profile-dependent public references;
- the same logical route resolves correctly under `/` and `/new/`;
- generated HTML, metadata, feeds, sitemap entries, assets, and browser resources use mutually consistent locations;
- templates and serializers contain no independent deployment-path concatenation;
- invalid or ambiguous input is rejected deterministically; and
- a production artifact contains no unintended preview-prefix references.

## Notes

This ADR was reconstructed from the accepted URL-and-routing architecture and base-path-resolution specification after its reference was found without a corresponding transported ADR file. It records the already documented ownership boundary.