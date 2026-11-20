# Multi-area system architecture

## Purpose and authority

This document defines the system architecture for the `ulfbou.github.io` multi-area professional static-publication platform. It governs production boundaries, route and emitted-path allocation, static composition, validation, artifact formation, and deployment.

Architecture does not determine landing-page, portfolio, article, or learning meaning and does not authorize publication. Semantic authority remains with `docs/platform.md`, each applicable area product document, and approved area source. `docs/publication.md` governs publication gates and release authority.

The architecture distinguishes:

- **Verified current architecture:** behavior directly supported by repository files.
- **Verified transitional behavior:** current implementation that differs from the accepted allocation.
- **Accepted target architecture:** approved direction that repository evidence does not yet prove is implemented.
- **Implementation gaps:** capabilities still required to reach the accepted target.

Documentation alignment does not implement architecture, move routes, create publication units, introduce a composer, or prove deployment behavior.

## Architectural constraints

The platform is a static-publication system compatible with one final GitHub Pages artifact.

Essential public meaning must not require:

- a server-side runtime;
- an authenticated session;
- a database;
- a scheduled job;
- a live AI service.

The architecture must preserve:

- area-specific semantic authority;
- deterministic generation where source is transformed;
- bounded producer responsibilities;
- direct loading at admitted public routes;
- useful no-JavaScript behavior where required by the applicable product contract;
- stable public URLs or explicit owned compatibility handling;
- accessible and public-safe output;
- one complete identified deployable static artifact.

Public areas may use different frameworks, runtimes, content models, generators, and interaction patterns. Shared publication boundaries do not require a universal application model.

## Accepted publication stages

```text
approved semantic sources
  -> bounded area production
  -> route-neutral publication units
  -> platform-owned route and emitted-path allocation
  -> clean static composition
  -> area and platform validation
  -> one identified static artifact
  -> GitHub Pages deployment
```

A technical component may perform more than one stage in the current implementation, but the responsibilities and authorities of the stages remain distinct.

### 1. Semantic source authority

Approved source and product contracts determine public meaning.

- `docs/platform.md` and accepted platform decisions govern platform purpose, area relationships, shared identity, and route policy.
- Approved landing-page source and `docs/product/landing-page.md` govern landing-page meaning.
- Approved portfolio source and portfolio product documents govern portfolio selection, claims, contributions, evidence, maturity, limitations, and presentation.
- Approved article source and `docs/product/articles.md` govern each article’s argument, wording, attribution, status, and correction.
- Approved learning source and `docs/product/learning.md` govern learning goals, explanations, sequence, and learner-facing interaction.

Evidence, observations, generated projections, compilers, templates, validators, build scripts, workflows, and deployment infrastructure do not possess semantic authority. They may support, transform, or verify approved meaning but cannot independently create, strengthen, weaken, summarize, approve, or publish it.

Portfolio evidence may support an approved portfolio claim but cannot create or approve one. Generated runtime data remains a projection even when retained, validated, deployed, and consumed by an application.

### 2. Publication-unit production

An authorized area producer transforms approved source into one complete bounded static publication unit.

A publication unit must:

- contain every file required for its admitted scope;
- preserve approved meaning and applicable accessibility behavior;
- exclude private, candidate, diagnostic, temporary, and undeclared material;
- expose sufficient identity, inventory, provenance, and validation information for admission;
- remain independent of final public route allocation;
- claim no emitted path outside its admitted allocation;
- be reproducible where its producer contract requires deterministic or byte-identical retained output.

A producer’s source path, project boundary, framework, generated directory, build output, or identity does not establish final route ownership. Production success does not grant semantic approval, publication approval, route ownership, or release authorization.

No single framework is required for all area producers.

### 3. Route and output-path allocation

A platform-owned publication plan maps admitted publication units to concrete artifact-relative output paths.

The plan must identify, directly or through traceable governed records:

- unit and producer identity;
- admitted source artifact;
- allocated destination path or prefix;
- protected-path permissions;
- compatibility and redirect obligations;
- required area and platform validation;
- deterministic ordering where ordering affects retained output;
- artifact identity or digest where available.

Concrete normalized emitted files form the enforceable ownership boundary. Every emitted file must have exactly one authorized owner.

Root and shared output, including `/index.html`, `/404.html`, shared identity assets, shared metadata, sitemap, feed, robots, redirect, compatibility, and manifest output where present, requires explicit platform ownership or bounded delegation.

Overlapping prefixes are permitted only when every resulting emitted file is proven non-conflicting. Copy order, producer priority, timestamp, source location, and last-writer-wins behavior must never resolve ownership.

Detailed route and ownership rules are defined in `docs/contracts/routes-and-output-ownership.md`.

### 4. Clean static-site composition

Composition begins with a new destination or proves that the destination is empty. It admits only units authorized by the publication plan and materializes them only at allocated paths.

Composition must reject:

- missing, unidentified, untraceable, or undeclared publication units;
- undeclared output;
- absent, duplicate, ambiguous, or unproven overlapping ownership;
- normalized path collisions;
- file-versus-directory collisions;
- absolute filesystem paths;
- drive-qualified or network paths;
- malformed paths;
- traversal and output escaping the destination;
- concealed or ambiguous separators and encodings;
- conflicting case or Unicode normalization;
- unauthorized protected-path claims;
- stale or unexpected files;
- a non-empty unknown composition destination;
- private, candidate, draft, diagnostic, temporary, or unpublished material;
- failed or unknown mandatory validation.

Composition must not overlay admitted output onto an unknown prior site tree. A producer cannot obtain ownership by writing first or last.

Failure must not silently substitute stale output, a prior artifact, partial output, placeholder pages, incomplete content, or an unvalidated artifact.

The detailed architecture is defined in `docs/architecture/static-composition.md`.

### 5. Validation

Area-specific and platform-wide validation occurs before deployment.

Validation must establish, as applicable:

- preservation of approved area meaning;
- public visibility and publication eligibility;
- deterministic generation and retained-output reproducibility;
- canonical route and compatibility behavior;
- representative direct loading and asset resolution;
- exclusive emitted-path ownership;
- protected-path authorization;
- metadata and canonical identity;
- accessibility and responsive behavior;
- public privacy boundaries;
- absence of stale, partial, undeclared, or unexpected output;
- compatibility with previously accepted public behavior;
- complete deterministic artifact inventory;
- integrity and identity of the final artifact.

An unknown mandatory result fails validation. The accepted target requires these checks, but repository evidence does not establish that complete platform validators currently exist.

Current portfolio validation is described below and in `docs/validation.md`.

### 6. Artifact formation

Successful composition produces one complete static site artifact.

The artifact must have:

- a complete deterministic inventory;
- a stable digest or equivalent immutable identity;
- traceability to admitted publication units and source revisions where available;
- no unauthorized extra output;
- no post-validation semantic or content mutation.

The artifact selected for deployment must be the artifact that passed required validation. Packaging and upload must preserve the verified deployment boundary or provide equivalent final-boundary verification.

Current repository workflows upload one published `wwwroot` directory as a Pages artifact. Repository evidence does not establish the complete target inventory, digest, multi-unit traceability, or immutable-artifact verification model.

### 7. Deployment

Deployment publishes the already validated artifact to GitHub Pages. It does not create, approve, alter, or summarize public meaning.

Deployment must not:

- substitute a newer or different unreviewed artifact;
- mutate the validated artifact silently;
- broaden admitted paths;
- resolve collisions by copy order;
- publish partial or stale output after failure;
- infer release authorization from build or validation success.

Ulf retains final authority over the exact revision or artifact released.

The repository contains GitHub Actions configuration for Pages deployment. That configuration is evidence of the current deployment process, not proof of a successful run, current public state, or implementation of the complete accepted target architecture.

## Area producers

### Landing-page production

- **Semantic authority:** `docs/product/landing-page.md` and approved landing-page source.
- **Producer responsibility:** create a complete bounded landing-page publication unit without redefining portfolio, article, or learning meaning.
- **Accepted allocation:** `/`, represented by `/index.html`.
- **Implementation status:** implemented inside the current `Ulfbou.Site` host through `Pages/Home.razor` and the root host document. It is not yet an independent route-neutral publication unit.

Landing-page summaries of other areas must preserve their qualifications, limitations, maturity, and authority.

### Portfolio production

- **Semantic authority:** approved portfolio source under `src/content/` and the portfolio product documents.
- **Producer responsibility:** validate and compile approved portfolio meaning into bounded portfolio output while preserving interactions, evidence boundaries, accessibility, compatibility, and failure behavior.
- **Accepted allocation:** `/work/`, represented by `/work/index.html` and admitted descendants.
- **Implementation status:** implemented inside the current `Ulfbou.Site` host through `Pages/Work.razor` and `wwwroot/work/index.html`. It is not yet an independent route-neutral publication unit.

Portfolio output is one publication input into the wider platform. The portfolio producer does not own the landing page, articles, learning, shared platform output, or platform route allocation.

### Article production

- **Semantic authority:** `docs/product/articles.md` and each approved article source.
- **Producer responsibility:** create bounded article index and article output with approved metadata, citations, status, corrections, and accessibility.
- **Accepted allocation:** `/articles/`.
- **Implementation status:** target behavior. The supplied repository does not establish an article index, article source, article producer, or article route.

The architecture does not prescribe an article framework, generator, manifest, command, or source layout.

### Learning-experience production

- **Semantic authority:** `docs/product/learning.md` and approved learning source.
- **Producer responsibility:** create bounded learner-centred static output while preserving educational accuracy, accessibility, optional-state limits, and privacy.
- **Accepted allocation:** `/aws/ai/`.
- **Implementation status:** transitional evidence exists. The repository contains a static `/aws/` index, shared AWS styles, and a browser runtime with namespaced local-storage helpers. It does not establish `src/Ulfbou.Site/wwwroot/aws/ai/index.html` or a complete admitted learning experience.

The architecture does not require one learning framework, a server-side runtime, authentication, a database, or live AI. Personal learner state must not enter public artifacts.

## Verified current host architecture

`src/Ulfbou.Site/Ulfbou.Site.csproj` is the current static-site host project. It uses the .NET 10 Blazor WebAssembly SDK and references `Ulfbou.Portfolio.Core`.

Its current `wwwroot` contains:

- the root Blazor host `index.html` for the professional landing page;
- the direct-entry portfolio host `work/index.html`;
- a source `404.html`;
- `.nojekyll`;
- portfolio CSS;
- the retained generated portfolio projection;
- a static `/aws/` index;
- shared AWS CSS and JavaScript assets.

The current `Home.razor` owns `@page "/"` and renders the professional landing page. `Work.razor` owns `@page "/work/"` and renders the portfolio. `PortfolioService` requests `data/generated/portfolio.json` relative to the application base address. UI state consists of the selected facet, expanded project, loading state, loaded data, and load error. Selection and expansion change disclosure, not source truth.

The current portfolio supports loading, failure, empty, filtering, in-place expansion, story-block presentation, compatibility presentation, responsive layout, visible focus, reduced motion, and print exposure of filtered or collapsed project details.

The current workflow publishes `src/Ulfbou.Site/Ulfbou.Site.csproj` to `publish`, verifies `publish/wwwroot/index.html` and `publish/wwwroot/data/generated/portfolio.json`, rejects `publish/wwwroot/data/portfolio.json`, copies the published root document to `publish/wwwroot/404.html`, creates `.nojekyll`, uploads `publish/wwwroot`, and invokes the GitHub Pages deployment action.

`Ulfbou.Site` and its `wwwroot` are the verified current host producer and static host tree. They are not permanent platform semantic authority, do not establish permanent repository architecture, and do not independently own every accepted route. Their directory layout is current implementation evidence rather than the final output-ownership model.

## Portfolio compiler architecture

### Approved source

Approved portfolio source under `src/content/` is the sole authority for authored profile, facet, technology, and project meaning.

`ApprovedContentLoader` reads:

- `src/content/profile.json`;
- `src/content/facets.json`;
- `src/content/technologies.json`;
- project JSON files directly under `src/content/projects/`, ordered by filename before project ordering is applied.

The loader rejects a missing content directory, missing required files, a missing projects directory, duplicate project filenames under case-insensitive comparison, malformed JSON, null documents, I/O failure, and unauthorized access.

### Validation

`ContentValidator` enforces:

- required profile, facet, technology, and project documents and collections;
- unique stable IDs;
- unique non-negative project order;
- valid references;
- no duplicate references;
- provenance;
- defined publication, visibility, and reading-depth values;
- published records being public;
- a published public profile;
- complete required strings for publishable projects;
- at least one limitation and proof pointer for each publishable project;
- absolute HTTP or HTTPS profile links;
- public facet and technology references for public projects;
- story-block validation.

Validation failures use stable codes and source paths through `ContentValidationException`.

### Deterministic compilation

`CompiledPortfolioGenerator`:

- validates approved source;
- filters projects and story blocks to `Published` and `Public`;
- resolves authored facet and technology IDs into public display values;
- filters proof pointers to public visibility;
- preserves project and story-block ordering;
- emits camel-case JSON;
- uses UTF-8 without BOM;
- uses two-space indentation;
- normalizes to LF-only newlines;
- writes exactly one final LF;
- excludes timestamps, machine paths, locale-sensitive values, random values, and environmental state.

`Ulfbou.Portfolio.Generator` loads and compiles approved source twice, rejects differing bytes, writes to a temporary file, verifies the temporary bytes, and atomically replaces the requested output. A validation, deterministic-generation, temporary-file, or I/O failure returns a non-zero result and must not promote partial output.

### Runtime projection and consumption

The retained generated portfolio projection is:

```text
src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

The current runtime request is:

```text
data/generated/portfolio.json
```

Pull-request and deployment workflows compile approved source, require the retained projection to remain unchanged after generation, require the generated deployment path, and reject the superseded `data/portfolio.json` path.

The retained generated projection is current runtime data, not semantic authority. It must not be hand-edited as an alternative source. Its current location does not allocate the final portfolio route.

### Semantic and byte equivalence

Repository tests independently deserialize the controlling legacy projection and retained generated projection as `PortfolioData` and compare their complete ordered object graphs. Separate reproducibility tests load and compile approved source twice and compare both generated bytes with the retained generated bytes.

Semantic equivalence governs meaning. Byte identity is additionally required for the retained generated projection.

### Story blocks and compatibility

Approved projects may contain ordered reusable story blocks. `StoryBlockValidator` validates:

- unique non-empty IDs;
- unique non-negative order;
- admitted type values;
- accessible labels;
- reading depth;
- provenance;
- publication metadata;
- object payloads;
- type-specific required values;
- absolute HTTP or HTTPS link URLs;
- renderer coverage for published public blocks.

`StoryBlockContract` defines admitted renderer coverage. `StoryBlockView` renders admitted types without project-specific branches.

Admitted types are prose, quote, reflection, code, terminal, image, links, evidence, callout, relationship, replay, and diagnostic comparison. Published unknown or unsupported types fail validation and publication. The development fallback is presentation-only and cannot authorize publication.

Existing approved project fields remain available during the additive story-block transition. Projects without blocks use established narrative, artifact, proof, and limitation presentation. Projects with blocks retain approved contribution compatibility presentation. Complete contract and compatibility rules are in `docs/contracts/story-blocks.md`.

### Evidence boundary

The fixture-backed prototype under `tools/portfolio-data/` proves portfolio authority separation before automated evidence collection. It uses one dependency-free Python CLI with `validate`, `compile`, and `verify` commands.

The prototype:

- admits no inference adapter;
- requires full lowercase revision identity;
- binds evidence-package identity to repository, revision, collector contract, scope, policies, and observation digests;
- requires normalized repository-relative evidence paths;
- requires both approved references and observations to be public for projection;
- produces a deterministic public evidence-index projection;
- rejects unresolved references, non-public references, non-public observations, symbolic revisions, path traversal, duplicate identities, stale package identity, and non-reproducible projection.

Its fixture is synthetic contract data, not approved portfolio content. It is not a deployment input or runtime dependency and does not alter approved-source publication.

Evidence cannot independently create, strengthen, approve, or publish a portfolio claim. Optional evidence failure must not break approved core portfolio stories.

## Output ownership and protected paths

Output ownership is evaluated at the normalized emitted-file level.

- Every emitted file has exactly one authorized owner.
- Source-directory ownership does not establish a public route.
- Framework conventions do not override route allocation.
- Producer identity does not override the publication plan.
- Build directories and `wwwroot` locations do not grant permanent ownership.
- Root and shared files require explicit platform ownership or delegation.
- Overlapping prefixes require proof that emitted files do not conflict.
- A file path cannot simultaneously be required as a directory.
- Producer copy order and last-writer-wins behavior are forbidden collision strategies.

The accepted allocations are `/` for the landing page, `/work/` for the portfolio, `/articles/` for articles, and `/aws/ai/` for learning. These allocations are target ownership, not proof that each route is currently emitted.

Normalization, case, trailing-slash, redirect, collision, compatibility, and protected-path requirements are defined in `docs/contracts/routes-and-output-ownership.md`.

## Accessibility, privacy, and semantic preservation

Architecture must preserve the accessible markup, interaction behavior, alternatives, labels, reading order, focus behavior, reduced-motion behavior, responsive behavior, and print meaning required by each area contract.

Representative routes and nested assets must be validated under the admitted static-host model. Infrastructure must not strip or conceal required meaning during generation, placement, composition, packaging, or deployment.

Only deliberately approved public material may enter the final artifact. Production and composition must exclude:

- private and candidate content;
- unpublished editorial material;
- credentials and secrets;
- personal learner state and performance;
- internal diagnostics and raw command output;
- local paths and temporary files;
- collaboration procedures and prompts;
- transport or carrier material;
- undeclared repository content.

Infrastructure must not invent, strengthen, weaken, omit, or summarize public claims without source authority. If approved meaning cannot be preserved through transformation or composition, publication stops.

Complete product accessibility and privacy expectations remain in `docs/platform.md` and the area product documents. Publication gates remain in `docs/publication.md`.

## Current, transitional, and target architecture

### Verified current behavior

The repository currently implements:

- one .NET 10 Blazor WebAssembly host project;
- a professional landing-page root route at `/`;
- a complete portfolio route at `/work/`;
- distinct root and work host documents, metadata, and canonical URLs;
- approved portfolio source under `src/content/`;
- deterministic source validation and compilation;
- one retained generated portfolio projection;
- runtime portfolio loading from `data/generated/portfolio.json`;
- reusable portfolio and story-block rendering;
- portfolio source and renderer tests;
- a fixture-backed portfolio-evidence prototype;
- a static `/aws/` index and shared AWS assets;
- pull-request validation targeting `master`;
- a current workflow that publishes one `wwwroot` tree and uploads it as one Pages artifact.

Current configuration does not prove live public-site state or successful workflow execution.

### Verified transitional behavior

The professional landing page currently occupies `/`, and the complete portfolio currently occupies `/work/`, matching the accepted allocation.

The repository contains a static `/aws/` index linking to `/aws/ai/`, but does not establish `src/Ulfbou.Site/wwwroot/aws/ai/index.html` or a complete admitted learning experience.

The source `404.html` redirects toward `/`, while the deployment workflow copies the published root `index.html` over `publish/wwwroot/404.html`. These are distinct repository and workflow facts. The supplied inputs do not establish the resulting live host behavior.

Documentation defines `/articles/`, but repository evidence does not establish article source, production, or output.

### Accepted target architecture

The accepted target has:

- area-specific semantic sources;
- bounded area producers;
- route-neutral publication units;
- one platform-owned publication plan;
- allocation to `/`, `/work/`, `/articles/`, and `/aws/ai/`;
- emitted-path ownership enforced after deterministic normalization;
- explicit protected-path permission;
- clean composition from an empty destination;
- rejection of unsafe, stale, partial, private, colliding, or unowned output;
- area-specific and platform-wide validation;
- one complete deterministic inventory;
- one stable artifact identity;
- deployment of the exact validated artifact to GitHub Pages.

This is accepted architecture, not a claim that the corresponding composer, publication plan, inventory, digest, or validators currently exist.

### Remaining implementation gaps after Increment 1

Repository evidence does not yet establish:

- route-neutral independent publication units for the implemented landing page and portfolio;
- an article producer, article index, approved article, or article route;
- a complete governed `/aws/ai/` index and admitted learning experience;
- complete shared identity and cross-area navigation;
- route-neutral publication units for all areas;
- a platform-owned machine-enforced publication plan;
- clean multi-unit composition from an empty destination;
- complete normalized emitted-path ownership validation;
- complete protected-path validation;
- a deterministic final-site inventory;
- a stable final-artifact digest or equivalent identity;
- verification that deployment selects the exact artifact that passed platform validation;
- an explicit compatibility artifact or accepted no-redirect decision for historical expectations that the portfolio occupied `/`;
- verified live route and deployment behavior.

The implemented root/work allocation closes the earlier route-transition gap. It does not close the remaining composition, article, learning, compatibility, artifact-integrity, or live-verification gaps.

## Need-led architecture

Do not add compilers, ahead-of-time compilation, service workers, graph, PDF, carrier, collector, or live LLM integration merely because the capability is possible.

A new architectural mechanism must be justified by an accepted product or publication need, preserve semantic authority and public safety, have bounded responsibilities, and provide proportionate validation. Optional tooling must not become a runtime prerequisite for already published meaning.

A future collector may produce bounded reviewable portfolio evidence only after a demonstrated need. It must not judge quality, infer contribution, write final public prose, change approved content, or obtain publication authority.

Probabilistic assistance remains candidate-only unless separately admitted through accepted implementation and governance. It must not write directly to public runtime JSON or bypass human approval.

## Failure and safe-stop behavior

Production, composition, validation, artifact formation, and deployment must fail closed when required authority, source, unit identity, ownership, validation, compatibility, integrity, or release authorization is missing or failing.

Failure must not:

- promote evidence or generated text into semantic authority;
- publish private or candidate material;
- substitute stale or partial output;
- retain an unknown prior composition as the new release;
- deploy placeholders for failed areas;
- use a prior artifact without explicit authorization;
- resolve ownership through copy order;
- deploy an artifact different from the validated artifact;
- conceal a mandatory unknown result.

Diagnostics should identify the failed contract, unit, or path without exposing private content.

## System architecture non-goals

The system architecture does not:

- centralize all semantic authority in one producer;
- require every area to use the same framework;
- require source folders to mirror public URLs;
- make generated output authoritative;
- permit one producer to write the whole site without explicit ownership;
- treat current repository structure as permanent product architecture;
- authorize publication or release;
- prove that accepted target composition infrastructure exists;
- require a server-side runtime for essential public meaning;
- require authentication, a database, scheduled work, or live AI;
- convert private working material into public output;
- infer live GitHub, Pages, route, workflow, or deployment state from repository configuration.
