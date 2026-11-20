# Platform validation and evidence

## Purpose and authority

This document defines validation requirements for the `ulfbou.github.io` multi-area static-publication platform. It covers the professional landing page, portfolio, technical articles, AWS Cloud and AI learning experiences, publication units, route and output ownership, composition, artifact integrity, deployment, and representative public verification.

Portfolio validation remains a complete dedicated part of this platform model. Existing portfolio compiler, generated-data, story-block, evidence-package, public-evidence-index, interaction, accessibility, compatibility, and failure checks remain valid within their portfolio scope.

Validation demonstrates conformance to accepted contracts. It does not independently approve public meaning, grant route ownership, authorize publication, or authorize release. Ulf retains final authority over public meaning, publication eligibility, accepted limitations, route-policy changes, and release of the exact revision or artifact.

This document defines required outcomes but does not prove that every validator, command, test, route, publication unit, composition mechanism, or deployment check exists. Current implementation claims below are limited to repository evidence.

The governing contracts are:

- `docs/platform.md` for platform purpose, areas, journeys, shared identity, accessibility, privacy, and accepted route direction;
- `docs/publication.md` for semantic approval, publication gates, artifact integrity, correction, withdrawal, archival behavior, and release authority;
- `docs/architecture/system.md` for production, allocation, composition, validation, artifact, and deployment boundaries;
- `docs/architecture/static-composition.md` for route-neutral publication units and clean composition;
- `docs/contracts/routes-and-output-ownership.md` for canonical routes and normalized emitted-path ownership;
- `docs/product/landing-page.md` for landing-page meaning and experience;
- `docs/product/experience.md`, `docs/prototype/visual-system.md`, `docs/architecture/accountable-portfolio.md`, and `docs/contracts/story-blocks.md` for portfolio meaning, behavior, visual requirements, evidence boundaries, and story blocks;
- `docs/product/articles.md` for article meaning, lifecycle, citations, accessibility, and URL policy;
- `docs/product/learning.md` for learning meaning, educational accuracy, interaction, learner state, accessibility, and privacy;
- `docs/roadmap.md` as the sole active public roadmap.

Increment 1 aligns documentation only. It does not implement target routes, producers, publication units, composition, platform validators, articles, learning experiences, or deployment changes.

## Validation-status vocabulary

Every validation criterion or recorded result uses one of these statuses:

- **Implemented automated:** repository evidence establishes an executable check, test, workflow step, or validator for the stated scope.
- **Implemented manual:** the check has a defined human review procedure. A passing result must still be recorded for the tested revision or artifact.
- **Partially implemented:** some required behavior is automated or reviewable, but the complete accepted criterion is not established.
- **Required target:** the accepted platform contract requires the check, but repository evidence does not establish an implementation.
- **Not applicable:** the criterion does not apply to the admitted output, with the reason recorded.
- **Blocked or unresolved:** required evidence, ownership, implementation, approval, compatibility behavior, or a reliable result is unavailable. A mandatory blocked or unresolved result prevents release.

A requirement is not automated merely because it can be inspected, is described by a contract, or commonly exists in another static-site system.

## Verified current validation boundary

### Implemented automated

The supplied repository establishes automated checks for:

- pull-request diff validity;
- UTF-8, BOM-free, LF-only governed text files;
- admitted GitHub Actions identities;
- portfolio-evidence prototype tests;
- prototype fixture validation;
- retained public-evidence-index verification;
- byte-reproducible prototype projection compilation;
- .NET restore, build, and tests;
- approved portfolio source loading and validation;
- deterministic portfolio compilation;
- retained generated portfolio projection synchronization;
- semantic equivalence between the controlling legacy projection and retained generated projection;
- story-block payload and renderer coverage;
- print-style rules that expose filtered and collapsed project detail;
- static-site publication;
- current root `index.html`;
- current generated portfolio runtime data at `publish/wwwroot/data/generated/portfolio.json`;
- rejection of the superseded `publish/wwwroot/data/portfolio.json` path.

The pull-request workflow names the current read-only check `pr / repository-gate` and targets pull requests to `master`.

### Implemented manual

Existing manual portfolio evidence covers:

- opening comprehension;
- distinct project propositions;
- contribution and reflection separation;
- maturity qualification;
- content approval;
- keyboard operation and visible focus;
- landmarks, headings, accessible names, and state communication;
- narrow, medium, and wide layouts;
- 200% and 400% zoom;
- reduced motion;
- selectable artifacts;
- print presentation;
- deployed root loading when direct public evidence is available.

These are defined review obligations, not claims that a current passing result exists.

### Partially implemented

Current validation is partial for the wider platform. The repository has a portfolio-oriented host, portfolio validation, static AWS files, and one uploaded `wwwroot` artifact, but it does not establish complete automated validation for:

- the accepted landing page at `/`;
- portfolio allocation to `/work/`;
- `/articles/`;
- a complete `/aws/ai/` index and admitted learning experience;
- route-neutral publication-unit identity and inventories;
- platform-owned emitted-path allocation;
- clean multi-unit composition from an empty destination;
- complete collision, case, Unicode, traversal, ownership, and protected-path validation;
- one deterministic final-site inventory;
- a stable final-artifact digest;
- proof that deployment uses the exact artifact that passed complete platform validation;
- representative live public verification.

### Current and target routes

The current repository implementation maps the professional landing page to `/`. It maps the complete portfolio to `/work/`, where the portfolio implementation requests `data/generated/portfolio.json`. The repository also contains a static `/aws/` index linking toward `/aws/ai/`. Successful build, publication, direct loading, and hosted behavior require execution evidence for the exact candidate and artifact.

The accepted target allocates:

- `/` to the professional landing page;
- `/work/` to the portfolio;
- `/articles/` to technical articles;
- `/aws/ai/` to AWS Cloud and AI learning.

Repository evidence establishes `/work/index.html` as current output. It does not establish `/articles/index.html` or `src/Ulfbou.Site/wwwroot/aws/ai/index.html`; those later target-route checks remain **Required target** until implementation evidence exists.

## Validation evidence model

Every criterion must record, as applicable:

- stable criterion or check name;
- validation-status label;
- command, test, workflow step, or manual-review procedure;
- result;
- tested source revision;
- publication-unit identity;
- artifact identity or digest where supported;
- representative route or file inventory;
- evidence location;
- reviewer for manual checks;
- known limitations;
- unresolved failures;
- semantic and publication approval status.

Public evidence must not expose secrets, private URLs, temporary local paths, raw private logs, unpublished source, learner records, or private diagnostics.

Unknown mandatory results fail. Semantic comparison determines non-regression unless a governing contract additionally requires byte identity.

## 1. Source and semantic validation

Each public area must use its own approved semantic authority.

Validation must establish that:

- landing-page output derives from approved landing-page meaning;
- portfolio output derives from approved portfolio source;
- article output derives from an approved article source;
- learning output derives from approved learning source;
- generated projections remain replaceable outputs rather than semantic authorities;
- evidence supports approved portfolio claims only and cannot create, strengthen, approve, or publish them;
- landing-page summaries preserve the source area’s qualifications, limitations, maturity, and authority;
- article publication preserves approved argument, wording, attribution, status, and correction state;
- learning publication preserves approved educational meaning, scope, limitations, and review basis;
- infrastructure does not invent, strengthen, weaken, omit, summarize, or rephrase public meaning without authority;
- candidate, draft, private, unapproved, withdrawn, or otherwise ineligible material is excluded according to its governing state;
- correction, withdrawal, archival, and supersession states are respected;
- human approval remains separate from technical validation.

A technically valid output with unapproved meaning fails publication validation.

## 2. Producer validation

Each admitted producer must validate:

- its expected semantic source and input authority;
- deterministic or explicitly controlled generation where required;
- complete bounded output;
- absence of undeclared output;
- absence of private, candidate, debug, diagnostic, and temporary material;
- required area-specific assets;
- internal links;
- required metadata;
- direct loading;
- area-specific accessibility;
- declared implementation limitations;
- deterministic failure behavior;
- non-zero or otherwise unambiguous failure for mandatory defects;
- preservation of approved meaning and compatibility behavior.

Producers may use different frameworks, languages, commands, and content models. No producer gains route ownership through its source path, framework, project name, or build directory.

## 3. Publication-unit validation

Each admitted publication unit must be validated for:

- stable identity;
- producer identity;
- admitted scope;
- complete file inventory;
- traceability to approved input and source revision where supported;
- destination-independent contents where applicable;
- absence of absolute destination assumptions;
- absence of path traversal;
- absence of output escaping the unit;
- absence of unauthorized protected-path claims;
- absence of private, candidate, debug, diagnostic, temporary, or undeclared files;
- reproducibility according to its producer contract;
- stable unit identity or digest where supported;
- completeness without reliance on files remaining only in the producer workspace.

The accepted architecture requires these results. Repository evidence does not establish a publication-unit manifest or complete publication-unit validator, so the complete layer is **Required target**.

## 4. Route and output-ownership validation

### Accepted target routes

When the corresponding area is implemented and admitted, validation must establish that:

- `/index.html` represents the professional landing page;
- `/work/index.html` represents the complete portfolio;
- `/articles/index.html` represents the article index;
- each admitted article resolves beneath `/articles/`;
- `/aws/ai/index.html` represents the learning-area index;
- each admitted learning experience resolves beneath `/aws/ai/`.

These are target requirements, not claims about current emitted files.

### Ownership requirements

Validation must establish that:

- every normalized emitted file has exactly one authorized owner;
- route roots and descendants remain inside authorized boundaries;
- root and shared files are emitted only by explicit platform owners or bounded delegates;
- route allocation is not inferred from source folders, project boundaries, frameworks, generators, or `wwwroot`;
- normalized output paths are unique;
- collisions after separator, dot-segment, percent-encoding, Unicode, and case handling are rejected;
- file-versus-directory conflicts are rejected;
- absolute, drive-qualified, network, malformed, or unsafe paths are rejected;
- traversal is rejected;
- output escaping the composition destination is rejected;
- duplicate or ambiguous ownership is rejected;
- unauthorized protected-path claims are rejected;
- undeclared output is rejected;
- redirects and compatibility artifacts have explicit owners.

Copy order, producer priority, timestamp, or last-writer-wins behavior cannot resolve a collision.

Complete target route and ownership enforcement is **Required target**. Current checks establish only the current root index and generated portfolio-data shape.

## 5. Clean-composition validation

Clean composition must validate that:

1. the destination is newly created or proven empty;
2. only declared and admitted publication units are composed;
3. every output path is normalized and authorized;
4. no stale file survives from an earlier build;
5. no undeclared file appears;
6. no collision is resolved by copy order;
7. all required shared and area-specific assets are present;
8. the final file inventory is complete and deterministic;
9. mandatory area and platform validation passes before artifact approval;
10. composition failure prevents deployment.

A failure must not silently substitute:

- stale output;
- partial output;
- placeholder output;
- a previous artifact;
- incomplete content;
- an unvalidated artifact.

The current workflow publishes one `wwwroot` tree. It does not prove clean multi-unit composition from an empty destination. Complete clean-composition validation is **Required target**.

## 6. Landing-page validation

The accepted landing page at `/` requires validation of:

- concise professional positioning;
- clear orientation across software, architecture, education, AWS Cloud, and AI;
- ordinary links to `/work/`, `/articles/`, `/aws/ai/`, and the approved GitHub destination where those destinations are admitted;
- clear distinction between featured work and the complete portfolio;
- preservation of portfolio, article, and learning qualifications in summaries;
- no invented or strengthened professional claims;
- accurate title, description, canonical URL, and language declaration;
- one meaningful primary heading and logical heading order;
- semantic landmarks;
- meaningful link text;
- keyboard operation and visible focus;
- sufficient contrast;
- zoom and reflow;
- reduced-motion behavior;
- meaningful image alternatives;
- direct loading;
- useful orientation and essential navigation without JavaScript;
- responsive and touch behavior;
- exclusion of private or candidate material.

These checks apply to the landing page at `/`. Repository evidence establishes the landing-page implementation and its route, metadata, no-JavaScript orientation, and navigation structure. Passing runtime, accessibility, responsive, and hosted results require execution or manual evidence for the exact candidate and artifact.

## 7. Portfolio validation

The implemented portfolio allocation is `/work/`. Automated and manual validation must target the portfolio application at `/work/`, preserve every applicable portfolio requirement, and cover route, asset, compatibility, direct-loading, and no-JavaScript behavior. The root landing page is validated separately.

### Canonical baseline

**Implemented automated** for the current solution:

```bash
dotnet restore Ulfbou.Portfolio.slnx
dotnet build Ulfbou.Portfolio.slnx --no-restore -c Release
dotnet test Ulfbou.Portfolio.slnx --no-build -c Release
```

The baseline covers restore, build, and repository tests. It must not be described as complete platform validation.

### Approved portfolio source

Approved-source validation covers:

- explicit-path source loading;
- required profile, facet, technology, and project documents;
- malformed or null document rejection;
- stable diagnostic codes and source paths;
- stable unique IDs;
- unique non-negative project order;
- known facet and technology references;
- duplicate-reference rejection;
- defined publication, visibility, and reading-depth values;
- published records having public visibility;
- published public profile requirements;
- complete required public fields;
- required portfolio limitations;
- required proof pointers;
- absolute HTTP or HTTPS profile links;
- public definition references for public projects;
- candidate and private exclusion;
- provenance;
- traceability;
- deterministic publication filtering;
- reference resolution;
- valid and invalid fixture behavior.

Repository-backed tests receive approved source and both public projections through test-output content resolved from `AppContext.BaseDirectory`; they do not depend on the process working directory.

### Portfolio compilation and runtime data

Compile approved portfolio source:

```bash
dotnet run --project src/Ulfbou.Portfolio.Generator/Ulfbou.Portfolio.Generator.csproj -- \
  src/content \
  src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

The generator loads and compiles approved source twice, rejects differing bytes, writes through a verified temporary file, and replaces the requested output only after successful verification. Mandatory validation or I/O failures return non-zero and must not promote partial output.

Verify synchronization of approved source and retained projection:

```bash
git diff --exit-code -- src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

Runtime reads:

```text
data/generated/portfolio.json
```

The retained generated projection is:

```text
src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

Pull-request and deployment checks require the generated deployment artifact and reject the superseded `data/portfolio.json` path.

Generated portfolio data is a projection, not semantic authority. The portfolio runtime path does not allocate the final portfolio route.

### Semantic and byte equivalence

Automated semantic equivalence independently deserializes the controlling legacy projection and retained generated projection as `PortfolioData` and compares the complete ordered object graph.

Automated reproducibility independently reloads approved source twice, compiles bytes A and B, compares A with B, and compares A with the retained generated bytes.

Semantic equivalence governs public meaning. Byte identity is additionally required for the retained generated projection.

### Story-block validation

Implemented automated story-block validation covers the content and renderer contracts below. One source-inspection test, `Home_preserves_filtering_while_retaining_printable_details`, still reads `Pages/Home.razor` although the portfolio moved to `Pages/Work.razor`. The supplied source therefore does not establish that this test currently proves portfolio filtering or disclosure structure.

The aligned coverage includes:

- stable non-empty IDs;
- unique non-negative order;
- admitted type values;
- accessible labels;
- reading depth;
- provenance;
- publication metadata;
- object payload requirements;
- type-specific payload requirements;
- absolute HTTP or HTTPS link URLs;
- renderer coverage;
- deterministic ordering;
- at least five coherent block types on one approved project;
- separation of reflection and evidence;
- missing image-alternative rejection;
- missing quote-attribution rejection;
- unknown-type rejection;
- malformed-payload rejection;
- print stylesheet structure for exposing filtered and collapsed details;
- loading story-block styles after base and accessibility styles.

Published unknown or unsupported block types fail. A development fallback cannot authorize publication.

Manual acceptance covers story-block presentation on desktop and mobile, keyboard use, zoom, reduced motion, selectable artifacts, print, and final presentation approval.

### Portfolio interaction and experience

Portfolio validation must preserve:

- loading, failure, and empty states;
- project discovery;
- facet filtering;
- in-place expansion;
- progressive disclosure;
- stable project ordering;
- contribution and reflection distinction;
- status and maturity qualification;
- evidence and approved-claim separation;
- visible limitations;
- external-link meaning;
- native controls;
- `aria-pressed` and expanded-state communication;
- visible focus;
- predictable reading and tab order;
- reduced motion;
- narrow, medium, and wide layouts;
- 200% and 400% zoom;
- reflow and touch use;
- selectable code and terminal material;
- meaningful image alternatives;
- quotation attribution;
- no color-only meaning;
- print exposure of all projects and details, including filtered and collapsed content;
- understandable unavailable and retry behavior;
- no invented content after runtime-data failure;
- compatibility presentation for established project fields.

No-JavaScript portfolio usefulness at the accepted `/work/` route is **Required target**. Repository evidence does not establish access to all current interactive portfolio detail without JavaScript.

### Portfolio evidence prototype

Run:

```bash
python tools/portfolio-data/tests.py
```

The prototype must reject:

- admitted inference adapters;
- unresolved evidence-package references;
- unresolved observation references;
- non-public approved references;
- non-public observations;
- symbolic revisions;
- absolute or traversing evidence paths;
- duplicate identities;
- stale package identities;
- package identity that omits canonical scope, policy, or observations;
- non-reproducible projections;
- extra or changed projection content.

Validate the admitted fixture:

```bash
ROOT=tools/portfolio-data/fixtures/valid/one-approved-claim
python tools/portfolio-data/cli.py validate \
  --governance "$ROOT/governance.json" \
  --approved "$ROOT/approved.json" \
  --evidence "$ROOT/evidence.json"
```

Verify the retained projection:

```bash
ROOT=tools/portfolio-data/fixtures/valid/one-approved-claim
python tools/portfolio-data/cli.py verify \
  --governance "$ROOT/governance.json" \
  --approved "$ROOT/approved.json" \
  --evidence "$ROOT/evidence.json" \
  --projection "$ROOT/evidence-index.json"
```

Compile a projection when an explicit output is required:

```bash
ROOT=tools/portfolio-data/fixtures/valid/one-approved-claim
python tools/portfolio-data/cli.py compile \
  --governance "$ROOT/governance.json" \
  --approved "$ROOT/approved.json" \
  --evidence "$ROOT/evidence.json" \
  --output "$ROOT/evidence-index.json"
```

The prototype is standard-library-only. Its fixture is synthetic contract data, not approved portfolio content. It adds no Python or NuGet dependency, repository secret, deployment input, or runtime dependency and does not alter approved-source publication.

Portfolio evidence supports approved portfolio claims only. It does not create claims, allocate routes, govern landing-page, article, or learning meaning, control composition, authorize publication, or authorize deployment.

### Current portfolio publication shape

Publish the current site:

```bash
dotnet publish src/Ulfbou.Site/Ulfbou.Site.csproj --no-restore -c Release -o publish
```

The current deployment-shape checks are:

```bash
test -f publish/wwwroot/index.html
test -f publish/wwwroot/data/generated/portfolio.json
test ! -e publish/wwwroot/data/portfolio.json
```

These direct shell checks establish part of the output shape. The configured publication-baseline verifier additionally requires `work/index.html`, computes the complete produced-artifact inventory and SHA-256, inspects metadata and local references, applies bounded privacy-path rules, and classifies `404.html`. Together they validate the intended root/work artifact shape when successfully executed, but repository files alone do not prove execution, generalized ownership, clean multi-unit composition, or identity preservation across deployment.

## 8. Article validation

Article validation is **Required target** because repository evidence does not establish an article index, approved article source, article output, or article validator.

When article publication is admitted, validate:

- approved article status and semantic authority;
- title and description;
- author;
- publication and updated dates where applicable;
- stable lowercase slug;
- canonical URL;
- series metadata where applicable;
- factual and version-sensitive claims;
- citations and references;
- quotation attribution;
- source links;
- licensed and bounded code excerpts;
- images, attribution, and alternatives;
- correction, withdrawal, archival, or supersession notices;
- logical headings and semantic structure;
- readable line length;
- keyboard access and visible focus;
- contrast;
- zoom and reflow;
- responsive code and tables;
- direct loading;
- internal and external links;
- accurate article-index entry;
- absence of unapproved portfolio claims;
- absence of private notes, prompts, critique, production instructions, and unpublished editorial material.

An article must pass the article-value test in `docs/product/articles.md`. Technical validity cannot approve article meaning.

## 9. Learning validation

Learning validation is partly represented by current static AWS files but remains primarily **Required target**.

For `/aws/ai/` and each admitted experience, validate:

- approved learning purpose;
- intended audience or assumed background;
- learner-centred framing;
- comprehension-led rather than assessment-first structure;
- scope and exclusions;
- source or factual-review basis;
- review date where needed;
- current handling of version-sensitive AWS service and certification information;
- no implication of AWS affiliation or endorsement;
- no claim of access to actual examination content;
- no guarantee of certification success;
- known limitations;
- stable descriptive route;
- direct loading;
- ordinary-link navigation;
- semantic structure;
- keyboard access and visible focus;
- sufficient contrast;
- alternatives for diagrams and visual models;
- no color-only meaning;
- reduced-motion behavior;
- zoom and reflow;
- narrow-screen and touch behavior;
- understandable interaction feedback;
- preserved essential learning meaning when interaction is unavailable;
- optional, understandable, removable, non-authoritative browser-local state;
- absence of learner identity, scores, answers, performance, or study history from public URLs and artifacts;
- operation of essential meaning without authentication, a database, server-side runtime, scheduled work, or live AI.

The current `/aws/` index, shared AWS styles, and namespaced local-storage helper are implementation evidence only. They do not prove a complete `/aws/ai/` experience or full learning-contract conformance.

## 10. Asset validation

Asset validation must establish that:

- shared assets resolve from `/`;
- portfolio assets resolve from `/work/` after transition and from the verified current portfolio route before transition;
- article assets resolve from representative article routes;
- learning assets resolve from `/aws/ai/` and admitted descendant routes;
- relative and root-relative references behave according to the validated hosting model;
- filename casing matches the emitted artifact;
- no public asset points to a private, temporary, or local filesystem path;
- no required asset remains only in a producer workspace;
- missing required assets fail validation;
- duplicate assets do not create ownership conflicts;
- cache or fingerprint behavior preserves admitted URL compatibility where applicable;
- nested direct loading does not break area or shared assets.

No unsupported asset pipeline is prescribed. Complete cross-area asset validation is **Required target**.

## 11. Metadata and discoverability validation

Validate, where governed and admitted:

- unique and accurate page titles;
- descriptions;
- language declarations;
- canonical URLs;
- crawl and indexing directives;
- social metadata;
- sitemap entries;
- feed entries;
- absence of duplicate canonical identities;
- correct ownership of platform-wide metadata;
- exclusion of private, candidate, draft, withdrawn, or unapproved content from indexes, feeds, and sitemaps;
- intentional treatment of corrected, withdrawn, archived, or moved content.

The existence of a requirement does not establish automation. Repository evidence does not establish complete platform metadata, sitemap, feed, or canonical validators.

## 12. Cross-area navigation validation

For every admitted area, validate that:

- the area can reach `/`;
- `/` can reach every admitted area;
- cross-area link labels remain understandable outside their immediate context;
- ordinary links work without client-side routing;
- keyboard users can operate shared navigation;
- visible focus remains usable;
- direct loading does not depend on prior navigation;
- cross-area navigation does not transfer semantic authority;
- unavailable areas are not represented misleadingly as published;
- links do not depend on source-directory structure;
- current-area indication does not rely on color alone.

Complete cross-area navigation validation is **Required target**.

## 13. Accessibility validation

Accessibility is release-blocking where the applicable contract makes it mandatory.

Representative validation must cover:

- semantic landmarks;
- logical heading order;
- keyboard operation;
- visible focus;
- predictable focus order;
- sufficient contrast;
- meaningful alternatives for images and diagrams;
- form and control labels;
- accessible names;
- selected, expanded, loading, error, unavailable, and completion-state communication;
- reduced motion;
- browser zoom;
- reflow;
- narrow-screen layouts;
- touch targets;
- table headers and responsive table behavior;
- readable and selectable code and terminal blocks;
- print behavior where governed;
- interaction fallback;
- no-JavaScript usefulness where required;
- direct loading.

Existing detailed portfolio accessibility checks remain valid. Representative landing-page, article, learning, and cross-area results are **Required target**. No conformance level or passing result is inferred beyond recorded evidence.

## 14. Privacy-boundary validation

The composed artifact must exclude:

- internal documents not approved for publication;
- `ulfbou-docs.md`;
- DX carriers;
- continuation and handoff reports;
- prompts and chat transcripts;
- AI-human collaboration instructions;
- raw diagnostics and private logs;
- temporary and local paths;
- temporary files;
- credentials and secrets;
- private repository information;
- personal learner state;
- examination performance;
- private study history;
- private instructor feedback;
- unpublished editorial material;
- private critique;
- unapproved drafts;
- candidate generated content;
- debug artifacts and source maps when they expose non-public material;
- development-session and coordination material.

Legitimate public repository paths needed to explain implementation or evidence are not prohibited merely because they are repository paths.

Privacy validation is mandatory and release-blocking. Presence in a repository, build directory, generated output, unlinked route, or crawl-excluded route does not make material public-safe.

## 15. Artifact-integrity validation

The accepted target requires validation that:

- the final artifact has a stable identity or digest;
- the artifact has a complete deterministic file inventory;
- included files trace to admitted publication units and authorized owners;
- source revisions and admitted units are traceable where supported;
- no unauthorized extra output exists;
- the artifact selected for deployment is the artifact that passed validation;
- no post-validation step silently mutates the artifact;
- deployment cannot substitute an older, newer, partial, or different artifact;
- a failed mandatory check prevents release;
- artifact evidence does not itself authorize publication.

The current workflow uploads one published `wwwroot` tree but does not establish the complete target inventory, digest, multi-unit traceability, or post-validation immutability proof. Complete artifact-integrity verification is **Required target**.

## 16. Repository, artifact, deployment, and public verification

These boundaries must remain separate.

### Repository validation

Repository validation checks source, contracts, tests, generation, and configuration before artifact formation. Current automated repository validation includes the `pr / repository-gate`.

### Artifact validation

Artifact validation checks the complete composed files, ownership, inventory, metadata, privacy, routes, assets, accessibility-relevant markup, compatibility output, and stable identity before deployment. Complete platform artifact validation is **Required target**.

### Deployment validation

Deployment validation must establish that the selected artifact is the validated artifact and that the deployment stage does not mutate or broaden it. Workflow configuration alone is not evidence of successful deployment.

### Representative public verification

After deployment, representative verification must cover, when implemented and admitted:

- `/`;
- `/work/`;
- `/articles/`;
- one admitted article;
- `/aws/ai/`;
- one admitted learning experience;
- representative shared and area-specific assets;
- titles, descriptions, language, canonical behavior, and crawl directives;
- cross-area navigation;
- compatibility routes or redirects;
- `404.html`;
- direct loading;
- expected status and fallback behavior;
- deployed-artifact identity.

No live public verification is claimed without direct evidence.

## Pull-request gate

Every pull request targeting `master` runs the read-only `pr / repository-gate` check.

The current gate validates:

- the pull-request diff;
- governed text encoding;
- admitted action identities;
- portfolio-data prototype tests;
- the valid prototype fixture;
- retained prototype projection verification;
- byte-reproducible prototype compilation;
- .NET restore;
- approved portfolio projection compilation;
- retained generated projection synchronization;
- .NET build;
- .NET tests;
- static-site publication;
- the current root index and runtime-data deployment shape.

Pull-request validation uses `actions/setup-python@v7` with Python 3.13. `tools/portfolio-data` remains standard-library-only and has no package-installation step.

Pull-request validation admits `actions/checkout`, `actions/setup-dotnet`, and `actions/setup-python`. Repository workflows as a whole may additionally use `actions/configure-pages`, `actions/upload-pages-artifact`, and `actions/deploy-pages`. No external service, repository secret, paid dependency, or mutation-capable pull-request workflow is admitted by the current contract.

The gate does not yet establish complete platform route allocation, publication-unit ownership, clean composition, artifact digest, or representative public verification.

## Release-blocking failures

Release must stop for any applicable mandatory failure or unknown result, including:

- failed restore, build, test, generation, or required validation;
- failed semantic-authority or semantic-preservation check;
- unapproved public meaning;
- candidate, draft, private, or withdrawn material in active output;
- missing required route;
- missing required asset;
- broken representative navigation;
- unresolved ownership;
- duplicate ownership;
- normalized path collision;
- file-versus-directory collision;
- unsafe, absolute, malformed, or traversing path;
- output escaping the destination;
- unauthorized protected-path claim;
- undeclared or stale output;
- non-empty unknown composition destination;
- failed required accessibility result;
- private material in the artifact;
- unresolved URL or compatibility obligation;
- incomplete or non-reproducible required output;
- artifact mismatch;
- post-validation mutation;
- deployment of an unvalidated artifact;
- missing final release authorization.

Warnings and advisory improvements must remain distinguishable from mandatory failures. A warning cannot suppress or replace a failed requirement. Only the authority responsible for the affected meaning or release may accept a bounded limitation, and the limitation must be recorded.

Failure must not substitute stale output, a previous artifact, partial output, placeholders, incomplete content, or generated guesses.

## Validation non-goals

Platform validation does not:

- independently approve public meaning;
- replace Ulf’s publication or release authority;
- prove that every target feature is implemented;
- require one testing framework;
- require one producer technology;
- require every area to use .NET or Blazor;
- infer route ownership from source layout;
- infer public state from workflow configuration;
- permit warning suppression to replace a failed requirement;
- convert evidence, runtime data, generated output, or metrics into semantic authority;
- authorize publication merely because generation is deterministic;
- authorize deployment of a failing, unknown, partial, stale, or mismatched artifact.

## Implemented root and work allocation

The current site requires two distinct public entry points in the produced artifact:

- `/index.html` is the professional landing page;
- `/work/index.html` is the complete portfolio host.

Validation must confirm distinct titles, descriptions, and canonical URLs for these entry points. The root host must retain useful no-JavaScript orientation and a direct link to `/work/`. The work host must use root-relative framework and stylesheet references so direct loading at `/work/` resolves the same application and generated portfolio data as in-app navigation.

The publication-baseline verifier treats `work/index.html` as a required current path. Its focused tests cover presence, absence, and distinct metadata for the root and work hosts. Artifact-level verification does not replace browser checks for direct loading, keyboard interaction, focus, responsive layout, zoom, reduced motion, or printing.
## Implemented article checks
Article validation covers registry schema versions, closed article properties, bounded lifecycle states, unique IDs and canonical routes, `/articles/` route containment, safe body paths, series references and statuses, exact ordered dates, required social assets, and editorial approval for published records. Producer tests exercise empty publication, nonpublished exclusion, approval failures, unknown properties, escaping paths, missing assets, publication, withdrawal, and rollback after an injected replacement failure. Artifact verification requires the article index, canonical article page, Atom feed, article projection, and combined sitemap; checks canonical and structured metadata; rejects client-startup-dependent article bodies; and verifies that only published projection records enter feed and sitemap discovery. Manual observation remains required before hosted behavior is claimed.
