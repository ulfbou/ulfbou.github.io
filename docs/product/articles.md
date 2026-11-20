# Technical articles

## Purpose and authority

The article area governs `/articles/` and publishes durable technical writing: explanations, architecture analyses, implementation retrospectives, design reasoning, technical comparisons, educational reflections, and carefully bounded opinion or argument.

Ulf approves article meaning and publication. Each approved article source is authoritative for its argument. This document governs the article product; `docs/platform.md` governs platform relationships and route allocation; `docs/publication.md` governs publication gates, integrity, release, correction, withdrawal, and archival behavior.

## Readers and value

The area serves developers, architects, educators, collaborators, prospective employers or clients seeking technical depth, and learners seeking optional conceptual context. A reader should gain reusable understanding rather than merely observe repository activity.

Each article must provide a clear benefit by doing one or more of the following:

- explain a difficult concept;
- document reusable reasoning;
- show how constraints shaped a decision;
- connect suitable implementation evidence to a broader lesson;
- clarify trade-offs;
- help readers avoid a known failure mode.

Repository activity, superficial progress reports, uncontextualized notes, or self-promotion without useful technical substance do not pass the article-value test.

## Routes and visitor journeys

`/articles/` is the article index. Durable, descriptive article paths live beneath it. Readers must be able to:

1. discover articles by meaningful title and summary;
2. open an article directly without first visiting `/`;
3. understand its scope, status, dates, sources, and relevant limitations;
4. follow contextual links to `/work/` or `/aws/ai/` without confusing their authorities;
5. return to `/` for platform orientation.

## Editorial identity and semantic authority

Article sources approved by Ulf determine article meaning. Opinions, interpretations, factual claims, and quotations must remain distinguishable. Evidence, repository data, generated summaries, and AI output may assist drafting but cannot approve or independently define an article's meaning.

Article claims and portfolio claims are different contracts. An article may analyze a project, but the article does not automatically approve a portfolio claim or become portfolio evidence. Portfolio claims remain governed by approved portfolio source under `/work/`.

## Supported forms

Supported forms include:

- standalone articles;
- ordered or loosely connected series;
- project-related articles that develop reasoning beyond a portfolio story;
- learning-related articles that offer optional conceptual depth;
- retrospectives and decision analyses.

The model must remain bounded but need not force every article into one template. Series membership must not prevent direct reading of an individual article. Cross-links must preserve the purpose and authority of both destinations.

## Required structure and metadata

Every published article must have:

- a title and useful description or summary;
- author identity;
- publication date and an updated date when a material revision occurs;
- a stable lowercase slug and canonical URL;
- publication status;
- citations and references where claims require them;
- correction, withdrawal, archival, or supersession notice where applicable.

Series metadata is required only for series articles. Topic or classification metadata may be used when it improves discovery. Metadata must be static-publication friendly and must not require a database, server process, authenticated editorial system, or live service.

## Lifecycle and publication boundary

Article states are:

- **Draft:** private or candidate work, not publishable.
- **Approved:** meaning accepted but not necessarily released.
- **Published:** approved for a canonical public route and authorized release.
- **Corrected or updated:** revised through review; material corrections must be visible when the previous wording could mislead.
- **Withdrawn:** removed from active indexes, feeds, and navigation; the route must communicate withdrawal or redirect to a genuinely equivalent replacement.
- **Archived or superseded:** retained with historical status and current limitations made clear.

Publication requires the applicable gates in `docs/publication.md`. Infrastructure may render, index, validate, and compose approved articles but may not invent titles, summaries, conclusions, claims, or corrections without semantic approval.

## Citations and source handling

Articles must:

- attribute factual claims whose basis is not common, durable knowledge;
- identify quotations and their sources;
- distinguish primary evidence from secondary interpretation;
- identify versions or dates for version-sensitive technical behavior;
- review external links before publication and material update;
- present code excerpts as readable, selectable text where practical and respect applicable licences;
- give screenshots and images meaningful alternatives, attribution, and context;
- avoid reproducing copyrighted material beyond permitted quotation, analysis, or licensed use;
- include only sources independently approved for public disclosure.

A broken or changed source must not be silently replaced if the replacement changes the claim.

## Relationship to portfolio, repositories, and other areas

Articles may provide deeper reasoning related to projects and may reference public repository implementation evidence when disclosure is approved and the evidence supports the stated point. Private repositories, private observations, speculative interpretations, and non-public evidence must not be exposed.

A portfolio link to an article should explain what deeper reasoning the article provides. An article link to `/work/` should identify the implementation context without converting the article into an approved portfolio claim. Links to `/aws/ai/` may offer optional exploration; learning experiences must remain understandable without reading an article.

## Reading experience and accessibility

Articles must provide readable line lengths, semantic headings, logical reading order, keyboard-accessible links and controls, visible focus, sufficient contrast, responsive presentation, zoom and reflow support, and reduced-motion behavior. Code and terminal material must remain selectable, labelled, and horizontally manageable. Images require meaningful alternatives. Tables need headers and must remain understandable on narrow screens. Notices and citations must be associated with the content they qualify. Print behavior should preserve article meaning where printing is supported.

## Metadata, discovery, and URL policy

The index and each article must provide accurate titles, descriptions, language declarations, and canonical URLs. Social metadata may be emitted where supported. Public articles may appear in platform search, sitemap, and the article feed when those features exist.

Article URLs must be durable, descriptive, lowercase paths beneath `/articles/`. They must not expose framework names, build paths, editorial status, or temporary identifiers. Changed URLs require an explicit owned redirect. Withdrawn routes must not redirect indiscriminately to `/`.

## Privacy boundary

The area excludes unpublished ideas, raw notes, private critique, private prompts, private production instructions, internal coordination, credentials, diagnostics, private repository information, and any material whose disclosure has not been independently approved. Presence in source control does not grant public status.

## Non-goals

The article area is not:

- a generic activity log;
- an automatic conversion of commits into articles;
- a private knowledge dump;
- a substitute for portfolio evidence;
- a discussion forum;
- a content-management application;
- an authority over the landing page, portfolio, or learning area.

## Product direction and implementation status

### Accepted direction

`/articles/` is the technical-article index with stable article routes beneath it. Articles are durable authored works with independent editorial authority.

### Verified current behavior

The supplied repository contains the governing platform and publication contracts but does not contain an implemented article index, approved article source, article route, or article publication tooling.

### Target behavior not yet implemented

This document defines the accepted article product contract. It does not claim that `/articles/`, an article feed, article metadata generation, redirects, or any article is currently deployed.
## Implemented source and output
Article meaning and publication state are held by explicit records in `content/articles/articles.json`; series declarations are held in `content/articles/series.json`; bodies are referenced explicitly beneath `content/articles/bodies/`. The bounded lifecycle is `draft`, `candidate`, `planned`, `published`, `withdrawn`, and `private`. Only `published` records with explicit editorial approval enter the generated index, article pages, Atom feed, projection, and sitemap.

The article producer emits complete static HTML beneath `/articles/`. When no record is published, it emits an empty index, an empty Atom feed whose contract-defined `<updated>` sentinel is `1970-01-01T00:00:00Z`, an empty projection, and no article body route. Correction changes authoritative source and regenerates all projections. Withdrawal changes the source state and regeneration removes the page and every discovery entry. Generation and validation do not grant editorial approval.

