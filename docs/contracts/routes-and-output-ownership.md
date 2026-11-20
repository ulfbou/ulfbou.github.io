# Routes and output ownership contract

## Purpose and scope

This contract allocates canonical public routes and defines enforceable ownership of normalized emitted files. Route allocation is a product decision. Output ownership is granted by the platform publication plan at the artifact boundary, not by semantic authority, source location, producer identity, framework, or repository layout.

This contract complements `docs/platform.md`, `docs/publication.md`, the area product documents, and `docs/architecture/static-composition.md`. It does not assign public meaning or authorize release.

## Canonical route allocations

### Professional landing page

- **Canonical route:** `/`
- **Purpose:** professional orientation and launch point.
- **Semantic authority:** `docs/product/landing-page.md` and approved landing-page source.
- **Output owner:** landing-page publication unit under platform permission.
- **Representative output:** `/index.html`.
- **Descendant scope:** only explicitly allocated landing-page or platform files; no implicit ownership of every root descendant.
- **Protected boundary:** the root index and platform-wide metadata are protected.
- **Relationship:** introduces `/work/`, `/articles/`, and `/aws/ai/` without taking their authority.

### Portfolio

- **Canonical route:** `/work/`
- **Purpose:** complete curated portfolio and its evidence-backed stories.
- **Semantic authority:** portfolio product documents and approved portfolio source.
- **Output owner:** portfolio publication unit.
- **Representative output:** `/work/index.html`.
- **Descendant scope:** admitted portfolio pages, data, and area-specific assets beneath `/work/`.
- **Protected boundary:** no write to `/`, `/articles/`, `/aws/ai/`, or shared root output without explicit permission.
- **Relationship:** may link to articles and learning experiences while preserving their authority.

### Technical articles

- **Canonical route:** `/articles/`
- **Purpose:** durable technical writing.
- **Semantic authority:** `docs/product/articles.md` and each approved article source.
- **Output owner:** article publication unit.
- **Representative output:** `/articles/index.html`.
- **Descendant scope:** approved article indexes, article paths, and area-specific assets beneath `/articles/`.
- **Protected boundary:** article output must not overwrite portfolio, learning, landing-page, or platform-owned files.
- **Relationship:** articles may provide deeper reasoning but are not portfolio claims by default.

### AWS Cloud and AI learning

- **Canonical route:** `/aws/ai/`
- **Purpose:** learner-centred AWS Cloud and AI experiences.
- **Semantic authority:** `docs/product/learning.md` and approved learning source.
- **Output owner:** learning publication unit.
- **Representative output:** `/aws/ai/index.html`.
- **Descendant scope:** admitted learning experiences and area-specific assets beneath `/aws/ai/`.
- **Protected boundary:** no ownership of `/aws/` generally or another area without explicit allocation.
- **Relationship:** may link to articles, portfolio context, and the landing page without changing their meaning.

These allocations are accepted target ownership. They are not evidence that every route is currently emitted or deployed.

## Representative emitted-path ownership

The publication plan must assign exactly one owner to each emitted file. Accepted target ownership includes:

- `/index.html`: landing-page owner with platform protection;
- `/work/index.html` and admitted `/work/` descendants: portfolio owner;
- `/articles/index.html` and admitted article descendants: article owner;
- `/aws/ai/index.html` and admitted learning-experience descendants: learning owner;
- `/404.html`: platform owner or an explicitly delegated producer;
- shared root assets, platform navigation assets, platform metadata, sitemaps, feeds, robots directives, redirects, and shared manifests: explicit platform owner or explicit bounded delegation.

The contract does not manufacture filenames for optional outputs. Any output that is introduced must receive explicit ownership before composition.

## Path model and normalization

Ownership claims use workspace-independent, artifact-relative paths. Before comparison or materialization, the composer must apply one documented deterministic normalization policy that:

- converts admitted separators to `/`;
- rejects absolute filesystem paths, network paths, URL-like authorities, and drive-qualified paths;
- rejects `..` traversal and any path that can escape the destination;
- removes or resolves `.` segments without changing path meaning;
- collapses duplicate separators consistently;
- rejects empty paths, empty ownership claims, and ambiguous file names;
- treats percent-encoding consistently and rejects encodings that conceal separators, traversal, control characters, or a conflicting normalized path;
- applies a declared Unicode normalization form before collision comparison;
- applies the declared case policy before collision comparison;
- detects file-versus-directory conflicts;
- rejects every collision revealed after normalization.

Normalization must not convert an unsafe claim into an accepted different route. If intent becomes ambiguous, validation fails.

## Case behavior

Canonical public paths normally use lowercase. An approved existing URL may retain other casing only with an explicit compatibility decision.

Ownership comparison must use one declared case-normalization policy. Paths that differ only by unsupported or unsafe case variation must be rejected rather than treated as separate compatible resources. Authors must not rely on a case-insensitive local filesystem, copy order, or hosting correction. Compatibility for an existing case variant requires explicit ownership and validation.

## Trailing-slash behavior

Area roots are canonical with a trailing slash: `/work/`, `/articles/`, and `/aws/ai/`. Their representative static file is the corresponding `index.html` within that route directory. `/area/` and `/area/index.html` must not become competing canonical identities.

Behavior for `/area` without the trailing slash depends on an explicitly validated host or redirect capability. When compatibility output is required, its owner must be declared. This contract does not assume that hosting automatically corrects or redirects every variant.

## Redirect ownership and compatibility

Redirects are publication output and require an explicit owner. Route changes require a platform decision and must preserve existing public links when an equivalent destination exists.

- A redirect target must preserve the source route's meaning.
- Unrelated or withdrawn material must not be redirected indiscriminately to `/`.
- Redirect loops, self-redirects, ambiguous aliases, and avoidable chains must fail validation.
- Withdrawn routes must have intentional documented behavior.
- When GitHub Pages cannot express a desired redirect mechanism, the publication decision must choose a supported compatibility artifact or an explicit non-redirect response without fabricating host behavior.

The owner of the changed route is responsible for compatibility content; the platform owns approval of the route-policy change and any protected root artifact.

## Collision behavior

Composition must reject:

- two producers writing the same normalized file;
- a file required by one owner where another requires a directory;
- overlapping prefixes whose emitted files cannot be proven non-conflicting;
- duplicate aliases that create competing canonical URLs;
- writes into another area's allocation without permission;
- writes to protected output without permission;
- collisions exposed by separator, dot-segment, percent-encoding, Unicode, or case normalization.

Producer priority, copy order, timestamp, source location, and last-writer-wins behavior must never resolve a collision.

## Protected paths

Explicit platform ownership or permission is required for:

- the root landing-page output;
- `404.html`;
- route indexes when ownership affects cross-area behavior;
- shared navigation and identity assets;
- sitemap and feed output;
- robots directives;
- redirect and compatibility artifacts;
- shared manifests and canonical metadata;
- any file whose modification affects more than one area.

The protected set may expand only through an accepted platform decision. Protection grants technical write authority, not semantic authority over area content.

## Validation requirements

Validation must establish that:

- every emitted file has exactly one authorized owner;
- every canonical route maps to its intended output;
- route roots and representative descendants support direct navigation under the validated static-host model;
- assets resolve from representative nested routes;
- normalization is deterministic and unsafe or ambiguous paths are rejected;
- protected paths are written only by authorized owners;
- redirects and compatibility aliases are intentional and non-conflicting;
- no stale, partial, unexpected, or undeclared file exists;
- no private, candidate, draft, diagnostic, or temporary content is emitted;
- canonical metadata creates no competing public identity.

These are accepted validation requirements. Repository evidence does not establish that a validator currently enforces the complete contract.

## Failure and safe stop

Missing ownership, ambiguous ownership, unsafe paths, collisions, unauthorized protected writes, stale output, unresolved compatibility, unknown mandatory validation, and artifact mismatch are release-blocking failures. The composer and deployer must stop without substituting a prior artifact, partial site, placeholder, or last-writer result.

## Current and target ownership

### Verified current behavior

The repository publishes one Blazor WebAssembly `wwwroot` output tree with two explicit public entry points. `/index.html` hosts the professional landing page, while `/work/index.html` hosts direct entry to the complete portfolio route. The Blazor router assigns `/` to the landing component and `/work/` to the preserved portfolio component. Both entry points use the GitHub Pages user-site root as their base path and have distinct titles, descriptions, and canonical URLs.

The workflow creates `404.html` by copying the published root index for unknown routes. `/work/` does not depend on that fallback because it has a concrete host document. Static AWS files remain under `/aws/` and are unchanged. The publication-baseline verifier requires the root index, work index, AWS index, generated portfolio data, and `404.html`; this remains bounded artifact validation rather than generalized emitted-path ownership across independent publication units.

Repository evidence does not establish `/articles/index.html` or `/aws/ai/index.html` as current emitted files. Documentation changes alone do not create those routes.

### Accepted target allocations

The target assigns `/` to the landing page, `/work/` to the portfolio, `/articles/` to articles, and `/aws/ai/` to learning. `/404.html` and cross-area shared output remain protected platform concerns.

### Transition obligations

The implemented allocation preserves approved portfolio meaning through the unchanged source and generated-data pipeline. Root has landing-page canonical identity, while `/work/` has portfolio canonical identity; direct entry is owned by `work/index.html`. Home and Work navigation explains the current allocation but does not provide compatibility for historical root fragments, bookmarks, search results, or external links that expected the portfolio at `/`. No redirect or compatibility artifact for those historical expectations is established by the current repository implementation; any accepted compatibility policy remains a separate route-policy decision and validation obligation. Existing `/aws/` behavior must be evaluated explicitly when `/aws/ai/` becomes the governed learning root. Article routes must not be claimed until admitted article output exists. Later implementation must validate direct navigation, nested assets, redirects or compatibility behavior, and final artifact ownership.

## What this contract does not govern

This contract does not:

- assign semantic authority from route ownership;
- require source directories to mirror URLs;
- require one producer or framework for every area;
- define a permanent repository layout;
- authorize publication or release;
- implement redirects;
- prove that target routes are live;
- permit infrastructure to alter area-owned meaning.
## Implemented article output ownership
The article producer exclusively owns `articles/index.html`, `articles/feed.xml`, and `articles/{slug}/index.html`. It also produces `data/generated/articles.json`. The platform owns the combined `sitemap.xml`; the current bounded article generator materializes it from explicit platform routes and the published article projection. Article generation is not authorized to modify `/`, `/work/`, `/aws/`, `/aws/ai/`, `404.html`, or sibling outputs.
