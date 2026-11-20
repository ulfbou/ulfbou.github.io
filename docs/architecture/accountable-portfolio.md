# Accountable portfolio architecture

## Purpose and scope

This document governs accountable portfolio content and architecture for the portfolio area accepted at `/work/`. The portfolio is one public area within the wider `ulfbou.github.io` multi-area professional static-publication platform.

This document does not govern:

- `/`, the professional landing page and platform launch point;
- `/articles/`, the technical-article area;
- `/aws/ai/`, the AWS Cloud and AI learning area;
- platform-wide route allocation;
- platform-wide static composition;
- article meaning;
- learning meaning.

Portfolio product documents and approved portfolio source remain authoritative for portfolio selection, claims, evidence, contribution, maturity, limitations, and corrections. Portfolio infrastructure does not acquire authority over landing-page, article, or learning meaning.

The portfolio produces one bounded publication input for the wider platform. Producing portfolio files, generated projections, or a portfolio publication unit does not independently allocate a final public route or emitted output path. Final allocation and emitted-path ownership are governed by `docs/architecture/static-composition.md` and `docs/contracts/routes-and-output-ownership.md`.

The accepted portfolio allocation is `/work/`. Repository evidence currently shows the portfolio application at `/`; it does not establish that the portfolio has moved to `/work/`. The current implementation and accepted target allocation must remain distinguishable during the transition.

## North star

The portfolio area is an accountable, evidence-backed static portfolio. Approved portfolio source remains the sole authority for public portfolio meaning. Evidence can support an approved portfolio claim but cannot create, strengthen, approve, or publish one. Generated projections are replaceable and reproducible outputs, not semantic authorities, even when consumed at runtime. The public portfolio remains useful without collection, scheduled work, an operational data branch, or inference.

Portfolio claims must preserve their approved wording, qualifications, limitations, provenance, attribution, maturity, visibility, and publication state. Evidence, observations, generated text, runtime data, build success, and deployment infrastructure cannot independently create or approve portfolio meaning. Content approval and correction remain human-authorized responsibilities, with Ulf retaining final authority over public portfolio meaning and publication.

## Authority and publication boundaries

The portfolio architecture distinguishes:

- **Approved portfolio source:** the authority for authored profile, facet, technology, project, claim, contribution, reflection, evidence reference, status, limitation, provenance, visibility, reading depth, and publication meaning.
- **Evidence and observations:** bounded support for already approved claims. Evidence may establish support or reveal a need for review, but cannot originate, strengthen, approve, or publish a claim.
- **Generated projections:** deterministic, replaceable outputs compiled from governed inputs. Generated output cannot become semantic authority merely because it is retained, validated, deployed, or consumed at runtime.
- **Runtime data:** the generated portfolio projection used by the current portfolio application. Runtime consumption grants no authority to revise portfolio meaning.
- **Public presentation:** the accessible rendering of approved portfolio meaning. Presentation may organize and progressively disclose approved content but must not invent, omit, strengthen, weaken, or silently requalify it.
- **Publication and release:** the authorized movement of approved public portfolio meaning into an allocated output path and identified release. Successful generation or validation is necessary where required but does not itself authorize publication.

Only approved public portfolio material may enter public portfolio output. Candidate, private, incomplete, or unapproved material must remain non-public. Unknown mandatory validation, unresolved authority, missing approval, missing ownership, or inability to preserve approved meaning is a publication-blocking failure.

Corrections must preserve relevant provenance, attribution, qualifications, limitations, and historical context. A correction must pass the applicable content, validation, publication, route-ownership, privacy, and release requirements. Failure must not silently substitute stale output, partial output, placeholders, generated guesses, or unapproved wording.

## Planes

```text
source repositories -> observations -> operational data plane
                                  -> human review
protected default branch -> approved source -> deterministic compiler -> portfolio publication input
```

The future `portfolio-data` orphan branch is an operational data plane for evidence, candidates, reviews, runs, and retained projections. It is introduced only when repeated collection would create material default-branch churn.

The operational data plane is not approved portfolio source. Evidence in that plane cannot independently create or approve portfolio claims, and retained projections do not acquire semantic authority through storage, validation, or runtime use.

The portfolio publication input is bounded to the portfolio area. Platform composition assigns admitted portfolio output to concrete emitted paths according to the platform publication plan. Source-folder ownership, build-directory layout, producer identity, and possession of generated files do not establish route ownership.

## Automation tiers

- Tier 0 is deterministic portfolio-publication work triggered by approved-source changes.
- Tier 1 is bounded deterministic maintenance. Failure can make evidence stale but cannot invalidate an already published portfolio.
- Tier 2 is optional probabilistic assistance. It is disabled in the prototype, candidate-only when admitted, never scheduled, and never part of publication.

No automation tier may create, strengthen, approve, or publish portfolio claims on its own authority. Probabilistic output remains candidate material until reviewed and approved. Deterministic output proves repeatability, not semantic correctness, content approval, route ownership, or release authorization.

## Generation, validation, and failure behavior

Portfolio generation must preserve approved source meaning and exclude private and candidate material. Equivalent governed inputs must produce semantically equivalent portfolio output and byte-identical retained output where the applicable contract requires byte reproducibility. Generated portfolio data must not be hand-edited as an alternative authority.

Portfolio validation must preserve and verify, where applicable:

- stable and unique identities;
- valid references and deterministic ordering;
- publication and visibility rules;
- provenance and attribution;
- evidence and approved-claim separation;
- required qualifications and limitations;
- renderer coverage and accessible presentation;
- reproducible generated projections;
- public privacy boundaries;
- compatibility with accepted portfolio fields and behavior;
- absence of invented or strengthened claims;
- the distinction between current implementation and accepted target routing.

Generation or validation must fail when required source is missing or malformed, references are unresolved, identities or ordering conflict, publication metadata is invalid, non-public material would enter public output, renderer coverage is absent, generated output is not reproducible, required accessibility behavior is unknown or failing, or approved meaning cannot be preserved.

A failed generation or validation must not promote evidence, observations, generated text, inferred values, stale projections, partial output, or placeholders as a replacement for approved portfolio source.

## Publication relationship to the platform

The portfolio producer creates one complete, bounded portfolio publication input from approved portfolio source. That input must preserve the portfolio’s semantic, evidence, accessibility, compatibility, and failure contracts, but it remains route-neutral until admitted by the platform publication plan.

`docs/architecture/static-composition.md` governs how independently produced publication inputs are admitted and composed into one static artifact. `docs/contracts/routes-and-output-ownership.md` governs canonical route allocation and ownership of normalized emitted paths.

The accepted target allocates the portfolio to `/work/`, while `/` is allocated to the professional landing page. The portfolio producer does not gain ownership of `/`, `/articles/`, `/aws/ai/`, shared root files, or another area’s outputs by writing to a source directory, generated-data directory, `wwwroot`, build directory, or publication-unit directory.

Portfolio publication must preserve approved claims, evidence relationships, provenance, attribution, limitations, accessibility, compatibility, and correction behavior after composition. Platform infrastructure may place and validate the admitted portfolio input but may not alter its public meaning.

## Current implementation and accepted target

### Verified current implementation

The supplied repository implements one .NET 10 Blazor WebAssembly portfolio application at `/`. Approved portfolio source under `src/content/` is compiled into the retained generated projection at `src/Ulfbou.Site/wwwroot/data/generated/portfolio.json`. The current application consumes that projection through `data/generated/portfolio.json`.

The current workflow compiles the portfolio projection, verifies reproducibility, tests and publishes the site, and checks for the generated portfolio data path. These facts establish current portfolio behavior only. They do not establish platform-wide composition, final emitted-path ownership across independent publication units, or implementation of the accepted `/work/` allocation.

### Accepted target allocation

The accepted target allocates the professional landing page to `/` and the complete portfolio to `/work/`. The portfolio must retain its approved content, evidence boundaries, interaction behavior, accessibility requirements, limitations, compatibility, and non-invention rules when moved.

This target is accepted direction, not a claim that the route transition, compatibility behavior, redirects, static composition, or final output ownership has already been implemented.

## Prototype boundary

The first prototype is fixture-backed because approved project records were not yet available when the prototype was introduced. It proves authority separation, two-layer visibility, content-addressed package identity, deterministic projection, stable diagnostics, and machine-readable AI optionality. It does not alter the site, deployment, approved source, or approved-source acceptance state.

The prototype remains portfolio-specific evidence and governance work. It does not allocate public routes, approve portfolio claims, govern other platform areas, or become a runtime prerequisite for the already published portfolio.
