# ulfbou.github.io platform

## Status of this document

This is the principal public product document for `ulfbou.github.io`. It defines the accepted platform direction and the boundaries that area-specific product documents must preserve.

The platform direction in this document is not, by itself, evidence that every described route, journey, navigation element, or publication capability is currently implemented. The section #accepted-direction-and-current-behavior records the distinction.

## Product purpose

`ulfbou.github.io` is a multi-area professional static-publication platform. It brings together a professional landing page, selected work, technical articles, and AWS Cloud and AI learning experiences under one public identity.

The platform exists to make Ulf Bourelius's work, reasoning, writing, and educational practice understandable and inspectable. It should help visitors move from a concise introduction to the area that best matches their purpose without forcing every area into one content model, one runtime, or one presentation pattern.

The unifying product principle is:

> One coherent public identity, composed from independently appropriate experiences.

## Intended audiences

The platform serves overlapping audiences with different needs:

- prospective employers, clients, and collaborators who need a concise professional overview and clear routes into relevant work;
- software developers and architects who want to inspect projects, decisions, constraints, evidence, and technical writing;
- educators and learning designers who want to understand the educational approach and inspect learning experiences;
- AWS learners who want focused, accessible, interactive material for AWS Cloud and AI learning;
- returning readers who want stable links to portfolio items, articles, and learning resources.

No visitor should need knowledge of the repository, build system, or authoring process to understand the public platform.

## Public areas

The public information architecture allocates these principal areas:

| Public route | Area | Distinct role | Semantic authority |
| --- | --- | --- | --- |
| `/` | Professional landing page | Introduces Ulf, communicates professional direction, and launches visitors into the other public areas. | The landing-page product and approved landing-page content. |
| `/work/` | Portfolio | Presents curated professional work, contributions, reasoning, evidence, maturity, and limitations. | The portfolio product documents and approved portfolio content. |
| `/articles/` | Technical articles | Publishes durable technical explanations, analyses, reflections, and authored arguments. | The article product and editorial documents, plus each approved article source. |
| `/aws/ai/` | AWS Cloud and AI learning | Provides learner-centred explanations and interactive learning experiences for AWS Cloud and AI. | The learning product documents and approved learning content. |

These areas are related, but they are not interchangeable. Platform documentation governs their relationship, shared public identity, navigation, routes, and publication boundaries. It does not replace area-specific authority over portfolio claims, article arguments, or learning meaning.

## Primary visitor journeys

The platform should support at least these journeys:

1. **Professional orientation:** arrive at `/`, understand Ulf's direction, then continue to selected work, writing, or learning resources.
2. **Work inspection:** enter `/work/`, scan curated projects, and inspect contribution, evidence, maturity, limitations, and deeper related material.
3. **Technical reading:** enter a stable article URL under `/articles/`, understand the article without requiring repository context, and optionally follow relevant links to work or learning experiences.
4. **Learning:** enter `/aws/ai/`, understand the learning area and available experiences, then use an experience without first navigating the portfolio.
5. **Cross-area exploration:** follow a contextual link between areas while keeping the destination's purpose clear. A portfolio link to an article offers deeper reasoning; an article link to learning offers optional exploration; a learning link to an article offers optional conceptual depth.
6. **Return by stable link:** revisit a bookmarked public URL and reach the same resource or an intentional redirect that preserves its meaning.

## Information architecture

The platform uses route ownership to keep public meaning clear:

- `/` owns orientation and launch, not the complete portfolio or an undifferentiated content feed.
- `/work/` owns portfolio-specific presentation and evidence.
- `/articles/` owns the article index and article routes.
- `/aws/ai/` owns the AWS Cloud and AI learning index and its learning applications.

Shared navigation may expose these top-level destinations, but each area may use local navigation appropriate to its own tasks. Cross-area links must state why the destination is useful rather than relying on vague labels such as "More" or "Related".

Platform-wide metadata, navigation, identity, sitemap, feed, and discoverability concerns may be composed across areas. Composition does not transfer semantic authority from an area to the platform infrastructure.

## Stable public URL policy

Public URLs are part of the product contract.

- Principal area roots are `/`, `/work/`, `/articles/`, and `/aws/ai/`.
- Published resources receive descriptive, durable, lowercase paths with stable path segments.
- A public URL must not expose implementation technology, build directories, generated filenames, private workflow state, or temporary editorial state.
- Changing a published URL requires an explicit route-policy decision and, where technically possible, a durable redirect from the previous URL.
- A moved resource must preserve destination meaning. Redirecting unrelated or withdrawn material to `/` is not an adequate substitute.
- Generated output must not create competing canonical URLs or duplicate aliases without explicit approval.
- Route ownership must have one declared owner. Two generators must not write the same public path.

## Shared identity and navigation principles

The areas should feel like one platform without being visually or technically identical.

Shared identity should provide:

- a consistent public name and professional tone;
- recognizable links to the principal areas;
- clear indication of the current area;
- contextual routes back to the professional landing page;
- coherent metadata and link language;
- truthful descriptions of maturity and purpose.

Area-specific visual systems and interactions are permitted when they serve the area's audience. Shared identity must not flatten a portfolio story, article, or learning experience into a generic site template.

Navigation must remain understandable without relying on hidden state, hover, animation, or prior knowledge. Essential destinations must be available as ordinary links.

## Accessibility expectations

Accessibility is a release expectation, not an optional enhancement. Public output should:

- use semantic landmarks, a logical heading hierarchy, meaningful link text, and native controls;
- support keyboard operation, visible focus, predictable reading and tab order, and skip navigation where appropriate;
- retain essential meaning without color, motion, hover, or complex interaction;
- respect reduced-motion preferences;
- provide text alternatives for meaningful images and attribution for quotations;
- remain usable with narrow viewports, text enlargement, browser zoom, and reflow;
- provide sufficient contrast across normal, hover, focus, selected, expanded, error, and disabled states;
- preserve readable, selectable text for code and terminal material;
- expose loading, empty, unavailable, and failure states in understandable language.

Area-specific acceptance criteria may be stricter. Publication must fail when a mandatory accessibility requirement for the affected area is unknown or unmet.

## Privacy boundary

Only deliberately approved public material may enter deployed output.

The public platform must exclude private or candidate content, unpublished editorial material, personal learning state, credentials, secrets, local paths, diagnostic output, private repository details, AI collaboration instructions, and transport mechanics. `robots.txt`, obscure URLs, client-side hiding, or an unlinked route are not privacy controls.

Public evidence and source references must be intentionally suitable for disclosure. Publication infrastructure must not infer that material is public merely because it is present in a repository or build context.

## Product constraints

The platform is constrained by the following accepted principles:

- It is a static-publication platform whose published output remains usable without a server-side application, authenticated session, database, scheduled job, or live AI service.
- Platform, portfolio, article, and learning meaning remain under separate semantic authorities.
- Ulf retains final authority over public meaning, claims, publication eligibility, route-policy changes, and release.
- Publication infrastructure may validate, transform, compose, and deploy approved output, but cannot create or revise public meaning on its own authority.
- Generated projections are replaceable outputs, not semantic authorities.
- Optional evidence, analytics, automation, or AI assistance must not become a prerequisite for understanding already published material.
- Public claims must preserve qualifications, limitations, provenance, and maturity.
- A shared platform concern must not force every area into the same framework, runtime, content schema, or interaction model.
- Failures must be visible to maintainers and must not silently publish partial, stale, private, or semantically changed output.

## Explicit non-goals

The platform is not intended to be:

- a single monolithic application or mandatory shared runtime for all areas;
- a conventional CV duplicated across multiple routes;
- a generic repository browser or automatically generated profile of all activity;
- a content-management system, social network, discussion platform, or authenticated learner portal;
- a publication channel in which AI, evidence collectors, generators, or deployment workflows can approve claims;
- a live dashboard whose correctness depends on runtime metrics, external APIs, scheduled collection, or inference;
- an excuse to publish private collaboration material, drafts, diagnostics, or local development state;
- a guarantee that every accepted product direction is already implemented.

## Relationship to area-specific product documents

This document governs the platform-wide product shape. Area-specific product documents remain authoritative for their own meaning:

- the landing-page product document governs the content and experience at `/`;
- the portfolio product and experience documents govern `/work/`, including project selection, claims, evidence, maturity, and limitations;
- the article product and editorial documents govern `/articles/`, including article purpose, argument, attribution, correction, and archival treatment;
- the learning product documents govern `/aws/ai/`, including learning goals, pedagogy, learner interaction, and educational accuracy;
- `docs/publication.md` governs the shared path from approved meaning to public output.

When documents overlap, the most specific semantic authority controls meaning and this platform document controls cross-area coherence. Infrastructure documents describe implementation only and cannot overrule either product authority.

## Accepted direction and current behavior

### Accepted product direction

The accepted direction is the multi-area allocation and contract stated above: `/` as the professional landing page and launch pad, `/work/` as the portfolio, `/articles/` as the technical-article area, and `/aws/ai/` as the AWS Cloud and AI learning area.

### Verified current behavior

The repository implements a .NET 10 Blazor WebAssembly professional landing page at `/` and the complete portfolio at `/work/`. Root and work have distinct host documents, titles, descriptions, and canonical URLs. `Pages/Home.razor` presents the approved headline, aspirational supporting line, Home and Work navigation, working principles, professional profile, and GitHub destination. `Pages/Work.razor` preserves the data-driven portfolio and its disclosure behavior.

Approved portfolio source under `src/content/` compiles to `src/Ulfbou.Site/wwwroot/data/generated/portfolio.json`, requested as `data/generated/portfolio.json`. The repository also contains `/aws/` static assets and a link toward `/aws/ai/`. The publication-baseline verifier requires the root and work hosts, AWS index, generated data, and `404.html`.

The repository does not establish article output, a complete `/aws/ai/` index or experience, generalized publication units, machine-enforced multi-unit composition, successful workflow execution, deployment, or live state. Shared navigation currently exposes only Home and Work.
