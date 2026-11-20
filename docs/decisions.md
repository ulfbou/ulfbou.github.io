# Platform and portfolio decisions

## Status and interpretation

This document records accepted durable public decisions for `ulfbou.github.io`. Decision scope is explicit:

- **Platform** decisions govern relationships among public areas, route allocation, shared publication boundaries, composition, and deployment.
- **Portfolio** decisions govern portfolio meaning and behavior under `/work/`.
- **Portfolio evidence** decisions govern the optional evidence-support model for approved portfolio claims.

A portfolio decision does not govern landing-page, article, or learning meaning. Infrastructure decisions do not acquire semantic authority over any public area. Accepted target decisions are not claims that the corresponding implementation is already complete.

## Platform decisions

- **D-026 — Multi-area platform:** `ulfbou.github.io` is a multi-area professional static-publication platform, not a portfolio-only product.
- **D-027 — Public route allocation:** `/` is the professional landing page and launch point, `/work/` is the portfolio, `/articles/` is the technical-article area, and `/aws/ai/` is the AWS Cloud and AI learning area.
- **D-028 — Surface-specific semantic authority:** Platform, landing-page, portfolio, article, and learning meaning remain under their applicable semantic authorities. Publication infrastructure may transform, validate, compose, and deploy approved output but must not create, strengthen, weaken, or approve public meaning.
- **D-029 — Platform-owned publication plan:** One platform-owned publication plan allocates admitted publication units to concrete public output paths. The plan grants technical output ownership, not semantic authority or release authorization.
- **D-030 — Emitted-path ownership:** Normalized emitted paths, rather than source directories, producer identities, framework conventions, or build-directory layout, form the enforceable output-ownership boundary. Every emitted file must have exactly one authorized owner.
- **D-031 — Clean composition:** Final composition begins with an empty destination and admits only declared publication units. It must reject unsafe or traversing paths, ownership collisions, unauthorized protected-path claims, stale files, undeclared output, and ambiguous normalization.
- **D-032 — One identified static artifact:** Composition produces one complete identified static artifact. Deployment publishes the artifact that passed required validation and must not substitute or mutate its public meaning or contents.
- **D-033 — Public URL compatibility:** Public URLs are product contracts. Route changes require an explicit route-policy decision and, where an equivalent destination exists and the static-host model supports it, an owned compatibility artifact or redirect that preserves meaning.
- **D-034 — Final publication authority:** Ulf retains final authority over public meaning, publication eligibility, route-policy changes, accepted limitations, and release of the exact revision or artifact.
- **D-035 — Active roadmap:** `docs/roadmap.md` is the single active public delivery roadmap. Earlier plans and exploratory sequences are historical or supporting evidence and must not compete with it.
- **D-036 — Area-appropriate implementation:** Public areas may use different frameworks, runtimes, content models, and interaction patterns. Shared platform identity and publication contracts must not force a universal application or component model.
- **D-037 — Accessibility and privacy are release boundaries:** Mandatory accessibility, public-safety, and privacy requirements must have known acceptable results before affected output is released. Private, candidate, diagnostic, local, or unpublished material must not enter public output.
- **D-038 — Current and target behavior remain distinct:** Accepted platform direction must be documented separately from verified current implementation. Documentation, generated output, build success, or target architecture does not prove that a route or capability is deployed.

## Portfolio decisions

The following decisions remain valid for the portfolio area implemented at `/work/`. The professional landing page is implemented separately at `/`, and the portfolio remains one area rather than the whole platform.

- **D-001 — Portfolio form:** The portfolio is one dynamic editorial portfolio page, not a conventional CV.
- **D-002 — Story before evidence:** Human story comes first; supporting evidence is progressively available.
- **D-003 — Structured portfolio meaning:** Structured approved portfolio content owns portfolio meaning; portfolio components own presentation.
- **D-004 — Current portfolio technology:** The verified current portfolio implementation uses .NET 10 Blazor WebAssembly and GitHub Pages. This is a current portfolio implementation decision, not a requirement that every platform area use the same runtime.
- **D-005 — In-place portfolio exploration:** Portfolio facets and expansion operate in place rather than fragmenting the portfolio into project routes.
- **D-006 — Human publication authority:** Ulf is final publishing authority; evidence and generated text are candidates unless separately approved.
- **D-007 — Deterministic validation first:** Deterministic portfolio validation precedes probabilistic assistance.
- **D-008 — Need-led tooling:** Portfolio tools emerge only from demonstrated content needs.
- **D-009 — No quantity-as-quality scoring:** Quantity metrics must never be used as quality or credibility scores.
- **D-010 — Accessible artifacts:** Portfolio code, terminal material, diagrams, and demonstrations are accessible explained content, not decoration.
- **D-011 — Semantic non-regression:** Reorganization is allowed; semantic loss, contradiction, overclaiming, lost authority, and unreviewed publication are not.
- **D-012 — Historical brainstorming:** The original brainstorm is an opportunity map, not a backlog, active roadmap, or governing contract.
- **D-013 — Accountable portfolio direction:** The long-term portfolio product is an accountable, evidence-backed static portfolio.
- **D-014 — Approved portfolio source:** Approved portfolio source on the protected default branch is the sole authority for public portfolio meaning.
- **D-015 — Evidence cannot create claims:** Evidence may support an approved portfolio claim but may never create, strengthen, approve, or publish one.
- **D-016 — Public evidence intersection:** Public portfolio evidence is the approved intersection of public claims, public approved references, and public observations.
- **D-017 — Generated projections are not authorities:** Generated portfolio projections are replaceable verification and runtime artifacts, not semantic authorities.
- **D-018 — Fixture-backed evidence prototype:** The first portfolio evidence prototype is fixture-backed, deterministic, offline-capable, and independent of approved-source publication.
- **D-019 — Conditional operational branch:** A portfolio operational orphan branch is introduced only if repeated evidence collection creates material default-branch churn.
- **D-020 — Scheduled maintenance is optional:** Scheduled portfolio evidence maintenance is not required for an already published portfolio to remain correct.
- **D-021 — Probabilistic assistance remains disabled:** Probabilistic portfolio assistance remains disabled until an adapter passes implementation, zero-charge verification, accepted-decision, and governance gates.
- **D-022 — Adapter substitution is contractual:** Replacing an admitted probabilistic adapter is a reviewed contract change, not ordinary provider configuration.
- **D-023 — Evidence-package identity:** Portfolio evidence-package identity covers repository, revision, collector contract, scope, policies, and observation digests.
- **D-024 — Independent version dimensions:** Portfolio evidence-projection schema version and compiler contract version evolve independently.
- **D-025 — Prototype CLI boundary:** The portfolio evidence prototype uses one dependency-free CLI with `validate`, `compile`, and `verify` subcommands.

## Consequences

The platform decisions supersede any interpretation of D-001 through D-025 that made the portfolio the complete website. They do not remove or weaken the portfolio decisions within their accepted scope.

Current implementation remains evidence of current behavior only. The repository contains one .NET site output with the professional landing page at `/`, the portfolio at `/work/`, a portfolio pipeline, bounded publication-baseline verification, and static AWS assets. Independent publication units, clean multi-unit composition, complete emitted-path ownership validation, article output, a complete `/aws/ai/` area, and post-validation immutable-artifact deployment remain target behavior.

Implementation and release must follow `docs/platform.md`, `docs/publication.md`, `docs/architecture/static-composition.md`, `docs/contracts/routes-and-output-ownership.md`, the applicable area product document, and the active `docs/roadmap.md`.
