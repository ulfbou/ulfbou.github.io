# ulfbou.github.io

`ulfbou.github.io` is a multi-area professional static-publication platform for professional positioning, selected software work, durable technical articles, and interactive AWS Cloud and AI learning experiences.

The portfolio is one public area of the platform. It remains a dynamic, data-driven editorial experience for selected systems, implementation artifacts, reflections, limitations, and bounded evidence through progressive disclosure. It is not a conventional CV or repository profiler, and it does not define the complete platform.

## Public routes

- `/` presents the professional landing page and platform orientation.
- `/work/` presents the complete approved portfolio.
- `/aws/` retains its existing static behavior.

The root and work routes have distinct host documents, metadata, descriptions, and canonical URLs. The portfolio source under `src/content/` remains authoritative; `wwwroot/data/generated/portfolio.json` remains replaceable generated output.

## Implementation status

### Verified current implementation

The repository currently contains:

- a .NET 10 standalone Blazor WebAssembly portfolio application in `src/Ulfbou.Site`;
- the professional landing page at `/`;
- the complete portfolio page at `/work/`;
- approved portfolio source under `src/content/`;
- deterministic portfolio compilation through `src/Ulfbou.Portfolio.Generator`;
- the retained generated portfolio projection at `src/Ulfbou.Site/wwwroot/data/generated/portfolio.json`;
- a runtime portfolio request to `data/generated/portfolio.json`;
- portfolio core and generator projects plus their tests in `Ulfbou.Portfolio.slnx`;
- a fixture-backed, dependency-free portfolio-evidence prototype under `tools/portfolio-data/`;
- static AWS files under `src/Ulfbou.Site/wwwroot/aws/` and shared AWS assets under `src/Ulfbou.Site/wwwroot/css/aws/` and `src/Ulfbou.Site/wwwroot/js/aws/`;
- GitHub Actions workflows that validate pull requests and publish one `wwwroot` tree for GitHub Pages.

These facts describe repository implementation. They do not establish live GitHub Pages state, successful workflow runs, branch protection, deployment status, or completion of the accepted platform route allocation.

### Accepted product direction

The accepted public allocations are:

| Route | Area | Public purpose | Implementation status in the supplied repository |
| --- | --- | --- | --- |
| `/` | Professional landing page | Concise professional orientation and launch point for the platform. | Implemented by the root host and `Pages/Home.razor`. |
| `/work/` | Portfolio | Complete curated portfolio, including project stories, contributions, evidence, maturity, and limitations. | Implemented by `work/index.html` and `Pages/Work.razor`. |
| `/articles/` | Technical articles | Durable technical explanations, analyses, reasoning, and reflections. | Target allocation. The supplied repository does not establish an article index or article output. |
| `/aws/ai/` | AWS Cloud and AI learning | Learner-centred static explanations and interactive learning experiences. | Target allocation. The repository contains a static `/aws/` index linking to `/aws/ai/`, but does not establish `src/Ulfbou.Site/wwwroot/aws/ai/index.html`. |

The areas share a coherent public identity and navigation model while retaining separate semantic authority. Landing-page source governs landing-page meaning, approved portfolio source governs portfolio meaning, approved article source governs each article, and approved learning source governs learning meaning.

Source paths, project names, frameworks, generated directories, and producer identities do not independently allocate public routes. Route and emitted-path ownership are established through the platform publication model.

Documentation alignment in Increment 1 does not move routes, implement a landing page, publish articles, complete learning experiences, introduce platform composition infrastructure, or change deployment behavior.

## Repository structure

- `src/Ulfbou.Portfolio.Core/` contains portfolio content contracts, validation, compilation, runtime models, and story-block contracts.
- `src/Ulfbou.Portfolio.Generator/` contains the portfolio generator command-line entry point.
- `src/Ulfbou.Site/` contains the current .NET 10 Blazor WebAssembly host and portfolio presentation.
- `src/Ulfbou.Site/wwwroot/` contains the current static host files, portfolio styles, generated portfolio data, and static AWS assets.
- `src/content/` contains approved portfolio profile, facet, technology, and project source.
- `tests/Ulfbou.Portfolio.Core.Tests/` contains portfolio source, generation, compatibility, renderer, interaction-shape, and print-oriented tests.
- `tools/portfolio-data/` contains the fixture-backed portfolio-evidence prototype. Its fixture is not approved portfolio content and is not deployed.
- `.github/workflows/validate-pr.yml` defines the current pull-request repository gate.
- `.github/workflows/deploy-pages.yml` defines the current portfolio-oriented GitHub Pages publication workflow.
- `docs/` contains the public product, publication, architecture, contract, decision, roadmap, delivery, and validation documents.

This structure is current repository evidence, not a required permanent platform layout.

## Build, test, and local operation

The repository targets .NET 10. The current solution is `Ulfbou.Portfolio.slnx`.

Restore, build, and test the current .NET solution:

```bash
dotnet restore Ulfbou.Portfolio.slnx
dotnet build Ulfbou.Portfolio.slnx --no-restore -c Release
dotnet test Ulfbou.Portfolio.slnx --no-build -c Release
```

Compile approved portfolio source into the retained generated projection:

```bash
dotnet run --project src/Ulfbou.Portfolio.Generator/Ulfbou.Portfolio.Generator.csproj -- \
  src/content \
  src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

The generator requires exactly a content-directory argument and an output-file argument. It loads approved content twice, requires deterministic byte output, writes through a verified temporary file, and replaces the requested output only after successful validation and verification. Validation or generation failures return a non-zero result.

After generation, a clean repository diff for the retained projection confirms that approved source and retained generated data remain synchronized:

```bash
git diff --exit-code -- src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

Publish the current static host:

```bash
dotnet publish src/Ulfbou.Site/Ulfbou.Site.csproj --no-restore -c Release -o publish
```

The current deployment-shape checks require:

```bash
test -f publish/wwwroot/index.html
test -f publish/wwwroot/data/generated/portfolio.json
test ! -e publish/wwwroot/data/portfolio.json
```

For local development, run the current Blazor WebAssembly project:

```bash
dotnet run --project src/Ulfbou.Site/Ulfbou.Site.csproj
```

The project launch profile declares development URLs at `https://localhost:58892` and `http://localhost:58893`. A local environment may choose an available URL according to normal .NET launch behavior.

Run the portfolio-evidence prototype tests:

```bash
python tools/portfolio-data/tests.py
```

The prototype is standard-library-only. Its complete `validate`, `compile`, and `verify` examples are documented in `tools/portfolio-data/README.md`.

These commands describe the current repository. They do not implement or validate the complete accepted multi-area composition target.

## Portfolio production

Approved portfolio source under `src/content/` is the sole semantic authority for portfolio profile, facet, technology, project, claim, contribution, reflection, evidence reference, status, limitation, provenance, visibility, reading depth, and publication meaning.

`ApprovedContentLoader` loads explicit portfolio source paths. `ContentValidator` enforces identity, reference, publication, visibility, provenance, public completeness, URL, ordering, and story-block rules. `CompiledPortfolioGenerator` filters to published public content, resolves authored references into public display values, and produces deterministic portfolio runtime data.

The retained generated projection is:

```text
src/Ulfbou.Site/wwwroot/data/generated/portfolio.json
```

The current portfolio application loads it through:

```text
data/generated/portfolio.json
```

Generated portfolio data is a replaceable projection, not semantic authority. It must not be hand-edited as an alternative to approved source. Portfolio evidence can support an approved claim but cannot create, strengthen, approve, or publish one.

Portfolio story blocks support admitted prose, quote, reflection, code, terminal, image, links, evidence, callout, relationship, replay, and diagnostic-comparison content. Their contract, validation, renderer coverage, compatibility behavior, and failure rules are defined in `docs/contracts/story-blocks.md`.

Portfolio production is bounded to portfolio meaning. The portfolio compiler, generated projection, evidence prototype, current `wwwroot` layout, and `Ulfbou.Site` project do not govern landing-page, article, learning, platform-route, or final output-path meaning.

## Architecture summary

The accepted platform architecture separates:

1. **Semantic authorities:** approved platform and area sources determine public meaning.
2. **Area-specific producers:** authorized implementations produce complete bounded static publication units.
3. **Route-neutral publication output:** a producer does not acquire a public route from its source layout, framework, or internal output directory.
4. **Platform allocation:** a platform-owned publication plan assigns admitted output to concrete normalized emitted paths.
5. **Clean composition:** composition begins with an empty destination and rejects unsafe paths, traversal, collisions, duplicate ownership, stale files, and unauthorized protected-path claims.
6. **Validation:** area and platform checks establish semantic preservation, ownership, routes, assets, metadata, accessibility, privacy, compatibility, and artifact integrity.
7. **Artifact deployment:** composition forms one identified static artifact, and GitHub Pages receives the artifact that passed required validation without deployment changing its contents or meaning.

The repository does not currently prove implementation of the complete publication-plan, multi-unit composition, emitted-path ownership, deterministic inventory, or immutable-artifact verification model. Those capabilities remain accepted target architecture.

Detailed architecture is governed by `docs/architecture/system.md`, `docs/architecture/static-composition.md`, and `docs/contracts/routes-and-output-ownership.md`.

## Current workflows and deployment boundary

The current pull-request workflow targets `master` and performs:

- pull-request diff checking;
- governed text encoding validation;
- admitted GitHub Actions identity validation;
- portfolio-evidence prototype tests and fixture verification;
- retained prototype projection verification;
- .NET restore, portfolio compilation, build, and tests;
- static-site publishing;
- current deployment-shape checks.

The current deployment workflow is triggered for pushes to `master` and by manual dispatch. It restores the solution, compiles and verifies the retained portfolio projection, tests the solution, publishes `src/Ulfbou.Site`, verifies the generated portfolio path, creates `404.html` from the published root document, creates `.nojekyll`, uploads `publish/wwwroot` as one Pages artifact, and invokes the Pages deployment action.

This is repository workflow configuration, not evidence that a workflow ran successfully or that a particular artifact is currently public. The current deployment workflow also does not prove the accepted clean multi-unit composition or immutable final-artifact model.

## Contribution and validation expectations

Changes must preserve the applicable semantic authority and publication boundary. In particular:

- approved portfolio meaning must be changed in approved portfolio source rather than in generated runtime data;
- generated output must remain reproducible;
- candidate and private material must remain absent from public output;
- evidence must not create or approve portfolio claims;
- unknown or unsupported story-block types must fail governed publication validation;
- public routes and compatibility behavior must not be inferred from source layout;
- documentation must distinguish current implementation from accepted target behavior;
- mandatory validation failures must not be replaced with stale, partial, placeholder, or unvalidated output.

Use reviewed pull requests and Conventional Commits as required by the active delivery plan. Full validation evidence and manual review expectations are defined in `docs/validation.md`.

## Governing public documents

`README.md` is the repository entry point. It summarizes the repository but does not replace the detailed authorities below.

### Platform-wide product and publication

- `docs/platform.md` is the principal public product document.
- `docs/publication.md` is the platform-wide publication contract.
- `docs/product/landing-page.md` governs the professional landing page at `/`.
- `docs/product/articles.md` governs technical articles under `/articles/`.
- `docs/product/learning.md` governs AWS Cloud and AI learning under `/aws/ai/`.

### Architecture and route ownership

- `docs/architecture/system.md` describes current, transitional, and accepted target system architecture.
- `docs/architecture/static-composition.md` defines publication units, platform allocation, clean composition, artifact identity, and responsibility boundaries.
- `docs/contracts/routes-and-output-ownership.md` defines canonical routes and emitted-path ownership.
- `docs/architecture/accountable-portfolio.md` governs accountable portfolio architecture and evidence boundaries for `/work/`.

### Portfolio product and presentation

- `docs/product/experience.md` governs the portfolio experience under `/work/`.
- `docs/prototype/visual-system.md` governs the portfolio visual and accessibility system.
- `docs/contracts/story-blocks.md` defines the portfolio story-block contract.

### Decisions, delivery, and validation

- `docs/decisions.md` records durable platform, portfolio, and portfolio-evidence decisions.
- `docs/roadmap.md` is the sole active public delivery roadmap.
- `docs/delivery/pr-plan.md` defines the bounded platform-transition pull-request sequence.
- `docs/validation.md` defines current validation evidence and the platform validation direction.

The accepted product and architecture documents define direction. Repository evidence determines which capabilities are currently implemented.
