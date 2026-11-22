---
title: Content governance
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: public and candidate content
related-adrs: [0022]
---

# Content governance

## Purpose

Content governance ensures that public material has an accountable owner, valid state, suitable evidence, correct metadata, and an explicit correction or withdrawal path. File presence does not grant publication authority.

## Roles

- **Author** creates or materially revises content and identifies sources and uncertainty.
- **Reviewer** checks factual support, clarity, originality, accessibility, metadata, and policy compliance.
- **Publisher** approves the state transition that makes content eligible for a public profile.
- **Platform maintainer** maintains schemas and publication behavior without silently changing editorial meaning.

One person may hold several roles, but the content record and review evidence MUST preserve which responsibility was exercised.

## States and transitions

The common states are `draft`, `candidate`, `approved`, `published`, `withdrawn`, and `archived`.

Permitted normal transitions are:

- `draft` to `candidate` after author self-check;
- `candidate` to `approved` after review;
- `approved` to `published` after publication authorization and profile eligibility;
- `published` to `withdrawn` when continued discovery is unsafe, misleading, unlawful, or no longer supportable;
- `published` to `archived` when it remains intentionally public as historical material;
- any reviewed state back to `draft` when material revision is required.

A transition MUST be explicit governed metadata or an approved publication action. Branch name, directory location, merge time, and build time MUST NOT infer it.

## Publication requirements

Before an item becomes `published`, it MUST have:

- a stable ID and valid logical route;
- responsible author information;
- required description and dates;
- provenance appropriate to its claims and media;
- review of factual claims and cited evidence;
- review for originality and required attribution;
- accessible structure, link text, media alternatives, and language metadata;
- valid taxonomy and relationships when used;
- canonical and social metadata required by its type;
- no secrets, private personal data, or candidate-only material.

## Corrections

Minor typographic corrections MAY update published content without changing the public revision date when they do not alter meaning. Material factual, interpretive, or instructional changes MUST update the governed revision metadata and SHOULD include a visible correction note when readers could have relied on the earlier version.

A correction MUST preserve relevant provenance and MUST NOT rewrite history to conceal a previous error.

## Withdrawal

Withdrawal removes the item from normal navigation, indexes, feeds, sitemap, recommendations, and related-content candidates. The route behavior MUST follow the accepted withdrawal specification. A withdrawal record MUST identify the reason category, decision date, owner, and whether replacement content exists. Sensitive details need not be public.

## AI-assisted work

AI assistance does not transfer accountability. The author or reviewer MUST verify factual claims, originality, source use, links, code, and alignment with the author's intended voice. Generated citations or provenance claims MUST never be accepted without checking the underlying source.

## Migration

Existing public content is not automatically approved for the reconstructed platform. Migration inventory and disposition decide whether each item is preserved, revised, replaced, withdrawn, archived, or excluded. Legacy generated HTML is evidence, not authoritative authored content.
