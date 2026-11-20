# AWS Cloud and AI learning

## Purpose and authority

The learning area governs `/aws/ai/`. It provides learner-centred AWS Cloud and AI exploration that helps learners understand concepts, relationships, boundaries, and practical consequences.

Ulf approves learning meaning and publication. Approved learning source is the semantic authority for each experience. `docs/platform.md` governs platform relationships and route allocation. `docs/publication.md` governs approval, visibility, integrity, factual freshness, release, correction, and withdrawal.

## Learning model and learner value

The area must prioritize comprehension rather than answer production. It should support multiple ways of approaching difficult material through conceptual maps, service relationships, comparisons, guided exploration, scenarios, layered explanations, visual or interactive models, worked examples, reflection prompts, and optional recall or practice activities.

Interactive static experiences may be used where interaction improves understanding. Essential learning meaning must remain useful without a live AI service, authenticated backend, database, scheduled job, or server-side application. No single framework or interaction model is required.

Assessment, recall checks, and practice questions may support learning, but the area is not defined primarily as a quiz system, exam simulator, score tracker, or answer-revealing assistant.

## Intended learners

The area serves:

- learners preparing for foundational or AI-focused AWS certification;
- learners seeking conceptual AWS Cloud and AI understanding beyond an exam outline;
- developers who need practical relationships among AWS services;
- educators or peers inspecting the learning approach.

The area must not claim official AWS affiliation, authorization, endorsement, or training-provider status, and must not promise certification success.

## Routes and visitor journeys

`/aws/ai/` is the learning-area index. Admitted experiences use stable descriptive paths beneath it. Learners must be able to:

1. understand an experience's learning purpose, audience, scope, and assumptions;
2. open an experience directly through an ordinary public URL;
3. explore concepts through an appropriate combination of explanation and interaction;
4. understand known limitations and source or review basis;
5. navigate to related articles for optional depth, `/work/` for professional project context, and `/` for platform orientation.

Navigation and completion expectations must be stated when an experience has a sequence. Essential navigation must use ordinary links and must not depend on hidden client state.

## Learning-experience contract

Each experience must state:

- learning purpose;
- intended audience or assumed background;
- scope and exclusions;
- source or review basis;
- known limitations;
- navigation and completion expectations where applicable.

Experiences may offer multiple perspectives on the same concept. Visual or interactive elements must clarify rather than decorate. When an interaction is unavailable, the experience must preserve the learning purpose through an understandable alternative or explanation.

## Certification-domain relationship

Certification objectives may orient coverage but must not be the sole product structure. Material must help learners build service and concept understanding that remains useful beyond memorizing an exam outline.

The area must not imply access to actual examination content, reproduce exam dumps, or promise examination success. Certification-domain and exam-version references are time-sensitive and must be reviewed for freshness. AWS names and marks must be used accurately without implying affiliation or endorsement.

## Semantic authority and source quality

Approved learning source determines educational meaning. Current authoritative AWS documentation should support time-sensitive service behavior and certification claims. Teaching interpretations, analogies, and sequencing must be distinguishable from factual AWS service behavior.

AI-generated explanations, summaries, quizzes, hints, and suggested relationships remain candidate material until Ulf approves them. Generated material, repository data, learner responses, or automated metrics cannot independently define what is educationally correct.

When approved course framing and current authoritative AWS information conflict, the conflict must be made explicit and resolved through authorized review rather than silently choosing or blending sources.

## Publication and approval boundary

Only approved public experiences may be published. Publication must satisfy `docs/publication.md`, including semantic approval, public visibility, route ownership, accessibility, factual freshness, privacy, integrity, non-regression, and release authorization.

Infrastructure may transform and compose approved learning output but may not invent explanations, answers, feedback, learning objectives, difficulty, or conclusions. Ulf retains final authority over learning meaning, eligibility, route changes, and release.

## Factual freshness

Learning material must distinguish durable cloud concepts from mutable product details.

- Version-sensitive service behavior and certification information must be reviewed against current authoritative AWS sources before publication or material update.
- Review dates should be recorded when useful for judging time-sensitive claims.
- Renamed, changed, deprecated, or newly released services must be handled explicitly.
- Corrections must preserve the intended learning purpose and make material changes visible when silent revision could mislead returning learners.
- External references must be reviewed for continued relevance.

Freshness checks may be performed during authoring and publication; the public experience does not require live runtime calls to AWS documentation.

## Learner state and privacy

Essential public learning meaning must not depend on personal learner data. Optional browser-local state may support progress or preferences when it is minimal, understandable, removable, and non-authoritative.

Personal learner state must not be published, embedded in public URLs, committed to the repository, or transmitted without an explicit need and consent. Authentication is not required for the accepted static model. Loss or removal of local state must not make published learning content inaccessible.

Public output must not expose personal knowledge gaps, test performance, study history, private instructor feedback, or other private learner records.

## Accessibility and device behavior

Experiences must support keyboard access, visible focus, logical reading order, semantic structure, sufficient contrast, zoom and reflow, narrow screens, and touch use. Meaning must not depend on color alone. Motion must be optional and respect reduced-motion preferences.

Visual models and diagrams require meaningful text alternatives or equivalent explanations. Diagrams must remain readable at supported sizes. Feedback must identify what happened and why without relying only on color or animation. Direct loading of an experience must provide its context and navigation. When an interaction cannot run, preserved explanatory content must maintain the core learning value.

## Metadata, discovery, and stable URLs

The index and each experience must provide an accurate title, description, language declaration, and canonical URL. Metadata should describe learning purpose and scope without implying AWS affiliation or guaranteed outcomes. Social metadata may be emitted where supported.

Public experience paths must be durable, descriptive, and lowercase beneath `/aws/ai/`. URLs must not expose framework names, build paths, temporary state, answer state, scores, or learner identity. Changed routes require an explicit owner and an appropriate redirect. Direct links must remain meaningful without prior navigation.

## Relationship to other public areas

The learning area shares platform identity and navigation but retains educational authority. `/` may introduce approved learning experiences without redefining them. `/work/` may present a learning experience as professional work while portfolio claims remain governed by portfolio source. `/articles/` may provide optional depth, but an experience must explain its basic purpose and controls without requiring a long article.

## Privacy and public safety

The area must exclude credentials, secrets, private repository information, local paths, diagnostics, private prompts, unpublished teaching material, private learner records, personal performance, instructor feedback, and content made public merely because it exists in a repository. Public examples must avoid exposing real confidential data.

## Non-goals

The learning area is not:

- primarily an assessment platform;
- an exam-dump or actual-exam-content repository;
- an official AWS training service;
- a guarantee of certification success;
- a personal learner-tracking backend;
- dependent on live AI tutoring;
- an authenticated learning-management system;
- an authority over landing-page, portfolio, or article meaning.

## Product direction and implementation status

### Accepted direction

`/aws/ai/` is the AWS Cloud and AI learning index with stable static learning experiences beneath it. The product is comprehension-led and may use interaction as a learning tool.

### Verified current behavior

The repository contains a static `/aws/` index that links to `/aws/ai/`, describes an AI Practitioner learning curriculum, and includes shared AWS styles and a browser runtime. The runtime provides namespaced local-storage helpers. The repository evidence does not establish an `/aws/ai/index.html` file or the complete learning experiences referenced by the `/aws/` index.

### Transitional and target behavior

The existing static AWS assets and index are implementation evidence, not proof that every learning contract is satisfied or publicly deployed. The target is a coherent `/aws/ai/` index and admitted experiences that meet this document. Any browser-local state must be reconciled with the learner-state boundary, remain optional, and never become semantic or publication authority.
