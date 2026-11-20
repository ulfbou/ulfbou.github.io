# Professional landing page

## Purpose and authority

The professional landing page governs `/`, the primary orientation and launch point for `ulfbou.github.io`. It presents a concise professional direction across software development, architecture, education, AWS Cloud, and AI, then leads visitors to deeper material. It is not the complete portfolio.

Ulf approves landing-page meaning and publication. This document is the semantic authority for the landing-page experience. `docs/platform.md` governs the wider platform, shared identity, route allocation, and cross-area relationships. `docs/publication.md` governs approval, visibility, integrity, release, correction, and withdrawal.

## Audiences and value

The page serves:

- prospective employers, who should quickly understand professional focus and reach selected evidence-backed work;
- prospective clients, who should understand relevant capabilities and reach material that supports an informed conversation;
- collaborators, who should find shared technical and educational interests and an appropriate GitHub destination;
- software developers and architects, who should reach portfolio work and technical articles;
- educators and learning designers, who should reach educational practice and AWS Cloud and AI learning experiences;
- visitors arriving from Ulf's GitHub profile or a direct link, who should receive immediate orientation without repository knowledge.

The first view must remain concise. Visitors must be able to move quickly to the complete portfolio at `/work/` or the approved GitHub destination. Incomplete platform areas are not exposed.

## Route and visitor journeys

`/` is owned by the landing-page publication process. Primary journeys are:

1. understand Ulf's professional direction;
2. follow the general work introduction to the complete portfolio at `/work/`;
3. continue to technical writing at `/articles/`;
4. continue to AWS Cloud and AI learning at `/aws/ai/`;
5. reach an approved GitHub profile or repository destination whose purpose is clear.

Cross-area links must describe their destination and value. Featured work must be labelled as selected or featured and must not imply that it is the complete portfolio.

## Experience requirements

The page must:

- provide concise professional positioning without duplicating a conventional CV;
- communicate the relationship among software, architecture, education, AWS Cloud, and AI;
- distinguish featured work from the complete portfolio;
- use ordinary links for all essential destinations;
- load directly at `/` and retain essential meaning without hidden client state;
- provide useful orientation and navigation when JavaScript is unavailable;
- support responsive layouts, narrow screens, text enlargement, and browser zoom;
- make unavailable, empty, loading, and failure states understandable when such states exist.

Interactive enhancement may improve presentation but must not be required to discover the principal areas or understand the professional focus.

## Content and semantic authority

Landing-page source approved by Ulf determines landing-page meaning. The page may summarize approved material from other areas, but each summary must preserve the source area's qualifications, maturity, limitations, and authority.

- Portfolio summaries must not invent, strengthen, or weaken portfolio claims.
- Article summaries must not change an article's conclusion or argument.
- Learning summaries must not redefine learning goals or educational claims.
- Featured material remains governed by its area-specific authority.
- Repository observations, generated metadata, metrics, evidence, and AI-generated text are candidates only and cannot independently become professional claims.

Platform infrastructure may select, transform, validate, compose, and deploy approved landing-page output. It may not decide what is professionally important or alter public meaning.

## Publication boundary

Only approved, public landing-page content may be published. Publication must satisfy `docs/publication.md`, including semantic approval, visibility, route ownership, completeness, accessibility, factual freshness, privacy, integrity, non-regression, and release authorization.

A valid build or generated summary is not publication approval. Ulf retains final authority over featured selection, professional claims, route changes, and release.

## Metadata and discoverability

The page must provide:

- an accurate, distinctive page title;
- a concise description of the professional platform;
- a correct language declaration;
- one canonical identity for `/`;
- social-sharing metadata where supported and approved;
- one meaningful primary heading and a logical heading hierarchy;
- meaningful link text that remains clear outside its surrounding layout;
- crawl and indexing behavior consistent with public publication.

Metadata must not create claims absent from approved page content. Alternate generated forms must not create duplicate canonical identities.

## Accessibility

The landing page must use semantic landmarks, logical headings, native controls, keyboard operation, visible focus, sufficient contrast, meaningful alternatives for images, and link text with a clear purpose. Essential meaning must not depend on color, hover, animation, or motion. Reduced-motion preferences must be respected. Content must reflow on narrow screens and remain usable with zoom and text enlargement.

## Stable URLs and relationships

`/` is the stable landing-page URL. Links to `/work/`, `/articles/`, and `/aws/ai/` must preserve those areas' distinct purposes. Route changes require platform approval and an owned redirect where needed. URLs must not expose framework names, build paths, private workflow state, or temporary editorial state.

The landing page may introduce the portfolio, articles, and learning area, but it is not their semantic authority. Those areas may link back to `/` for professional orientation.

## Privacy and public safety

The page must not publish private repository details, local paths, diagnostics, credentials, secrets, personal learning state, unpublished editorial material, AI collaboration instructions, transport mechanics, candidate professional claims, or material treated as public merely because it exists in a repository. An unlinked route, client-side hiding, or crawl instruction is not a privacy control.

## Non-goals

The landing page is not:

- the complete portfolio;
- a repository browser;
- a live activity dashboard;
- an automatically generated professional profile;
- an authenticated application;
- a content-management interface;
- an authority over portfolio, article, or learning meaning.

## Product direction and implementation status

### Accepted direction

`/` is the professional landing page and launch point. `/work/` is the complete portfolio. Articles and AWS Cloud and AI learning remain later platform allocations and are not visible first-release destinations.

### Verified current behavior

The repository implements `/` as the professional landing page with the approved headline, aspirational supporting line, primary routes to `/work/` and GitHub, four working principles, a compact professional profile, and only the complete Home and Work destinations.

The complete approved portfolio is implemented at `/work/`. Its approved structured source, deterministic generated projection, filtering, project expansion, story blocks, evidence, status, maturity, limitations, loading, empty, failure, keyboard, focus, responsive, reduced-motion, and print behavior remain in the existing portfolio implementation.

The produced site contains separate root and work host documents. `/index.html` owns landing-page metadata and no-JavaScript orientation. `/work/index.html` owns portfolio metadata and direct entry to the Blazor route. Their canonical URLs are `https://ulfbou.github.io/` and `https://ulfbou.github.io/work/` respectively. The GitHub Pages workflow continues to construct `404.html` from the root host for unknown routes; `/work/` does not depend on that fallback because it has a concrete host document.
