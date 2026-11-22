---
title: URL and routing architecture
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: all generated routes and references
related-adrs:
  - 0004
  - 0005
  - 0015
  - 0020
---

# URL and routing architecture

## Separation of concerns

The platform distinguishes four values:

- **logical route**: deployment-independent public identity, such as `/articles/example/`;
- **output path**: artifact location, such as `articles/example/index.html`;
- **base path**: deployment prefix, such as `/new/` or `/`;
- **base URL**: public origin used to create absolute URLs, such as `https://ulfbou.github.io`.

No one value may be substituted for another.

## Route ownership

Routes are declared by validated content or page configuration and resolved by one route service. Source folders, templates, generated filenames, and browser modules do not independently invent routes.

Logical routes:

- begin with `/`;
- represent directories with a trailing slash except for explicitly specified files such as feeds or machine resources;
- use normalized UTF-8 slugs permitted by the URL policy;
- contain no `.` or `..` path segments;
- contain no duplicate separators, query, fragment, origin, or deployment prefix;
- are unique after normalization;
- remain independent of `/new/`.

## Mapping to output

A directory-style logical route maps to an `index.html` file beneath the artifact root. The root route maps to `index.html`. Explicit machine-resource routes map to their specified filenames. Output mapping must reject traversal and collisions, including case-insensitive collisions where the deployment environment could confuse them.

## Reference generation

All internal HTML links, assets, browser fetches, canonical URLs, feeds, sitemap entries, and structured-data identifiers use the shared resolver.

- Navigational and asset references are resolved against the active base path.
- Absolute public identifiers are resolved from the validated base URL and base path.
- Authored content refers to logical routes or governed resource identifiers, not hard-coded preview URLs.
- Templates do not concatenate URL strings.
- Browser code receives resolved resource locations through generated configuration or the publication manifest.

## Base-path behavior

The base path is normalized to one leading and one trailing slash. `/` represents root publication. `/new`, `new/`, and `/new/` may normalize to `/new/` if the specification permits those input forms. Empty, absolute, traversal-containing, query-containing, or fragment-containing values are rejected.

A build for `/new/` and a build for `/` must expose equivalent logical routes. Only deployment-resolved references and profile-specific metadata may differ.

## Canonical behavior

Canonical URLs, social URLs, structured-data identifiers, sitemap locations, and feed links use the active profile's externally visible base URL and base path. Preview canonical and indexing policy is specified separately and must not be inferred from the route.

## HTML base element

The `<base>` element is not the primary routing mechanism. Explicit resolution avoids document-wide side effects for fragments, forms, copied markup, and third-party content. Its use requires a superseding ADR.

## Redirects and removed routes

Because GitHub Pages provides static hosting, required redirects are generated as explicit HTML pages only for governed, known route migrations. Unknown paths rely on the generated static 404 page. Redirect artifacts must not establish duplicate canonical content indefinitely.

## Validation

Artifact validation must detect unresolved internal references, accidental `/new/` literals in root builds, origin leakage, duplicate routes, path traversal, output collisions, missing assets, and profile-inconsistent canonical metadata.
