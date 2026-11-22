---
title: Output artifact specification
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: generated static publication artifact
related-adrs: [0013, 0014, 0015]
---

# Output artifact specification

## Artifact definition

An artifact is one self-contained directory tree ready for static deployment under the selected profile. Its root corresponds to the profile's deployment root. A preview artifact built specifically for `/new/` does not itself contain an additional top-level `new/` directory unless an artifact-composition specification explicitly requests a combined deployment tree.

## Initial required files

The initial artifact MUST contain:

```text
index.html
404.html
assets/
```

It MUST also contain an `index.html` for every eligible directory-style route. Additional required resources such as `robots.txt`, sitemap, feeds, publication manifest, or search index become mandatory when their specifications are accepted for the active increment.

## File requirements

- Text output MUST be valid UTF-8.
- HTML files MUST use `.html` and contain complete documents.
- Directory-style routes MUST map to `index.html`.
- File and directory names MUST be deterministic and portable to GitHub Pages.
- Output MUST NOT contain absolute local paths, credentials, source maps exposing private paths, repository metadata, temporary files, caches, test results, or authored source not intended for publication.
- Symlinks MUST NOT appear in the deployable artifact.

## Clean assembly

The publisher MUST assemble into a new staging directory. Existing output MUST NOT be used as an implicit input or merged into the staged tree. Promotion MUST occur only after artifact validation succeeds.

## Route completeness

For every eligible logical route, exactly one output target MUST exist. No two logical routes may map to the same target. Every generated internal link and required asset reference MUST resolve within the artifact or be an explicitly governed external URL.

## Static `404.html`

`404.html` MUST be a complete accessible page with a useful explanation and navigation back to known public entry points. It MUST use references valid from GitHub Pages 404 handling under the active base path. It MUST NOT depend on JavaScript to explain or recover from the error.

## Assets

Authored assets MAY be copied or transformed. Generated asset names and ordering MUST be deterministic. HTML MUST reference only assets present in the artifact. Unreferenced assets SHOULD be excluded unless they are required by a governed public contract.

## Artifact validation

Validation MUST detect at least:

- missing required files;
- unexpected generated files outside approved locations;
- broken internal links or assets;
- route collisions and case-confusable output paths;
- accidental preview prefixes in root builds;
- invalid UTF-8 text;
- forbidden secrets or local absolute paths;
- incomplete HTML document structure;
- profile-inconsistent indexing or canonical metadata;
- symlinks and traversal-derived output.

A failed artifact MUST NOT be deployed or described as valid.
