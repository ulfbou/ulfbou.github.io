# One-page portfolio roadmap

## Goal

Build a distinctive one-page portfolio. Structured content drives one coherent editorial experience; project details unfold in place. The site must remain useful without collectors, LLM calls, authentication, databases, runtime metrics, carrier inspection, PDF generation, or authoring tools.

## Shared gates

Acceptance Ready means the bounded outcome is implemented, mandatory checks have named evidence, limitations are explicit, and no blocking defect or unknown result remains. Done means the result is accepted, docs match implementation, public wording is approved, semantic non-regression passes, and no undocumented workaround remains.

## Phase 1 - Solidify the rapid prototype

Outcome: deployable one-page vertical slice.

### Acceptance Ready

- .NET 10 solution restores, builds, and tests.
- `/` renders the complete experience and loads structured JSON through one service.
- facet controls filter without reload; projects expand and collapse in place.
- detail includes proposition, story, contribution, reflection, status, technologies, artifact, evidence, and limitations.
- at least three representative projects demonstrate distinct emphases.
- loading, failure, and empty states are intentional.
- GitHub Pages deployment and direct root loading work.
- mobile, desktop, keyboard, focus, headings, names, zoom, and reduced motion are usable.
- no later-phase tooling is required.

### Definition of Done

- deployed output matches the accepted local result.
- obsolete template content is removed.
- visible claims are approved or explicitly representative.
- experimental work is not presented as production-ready.
- no placeholder contact information, private data, or invented metric is public.
- accessibility and responsive reviews have named results.

## Phase 2 - Approved structured content

Outcome: approved authored source deterministically produces public data.

### Acceptance Ready

- source lives under `src/content/`; generated JSON under `src/Ulfbou.Site/wwwroot/data/generated/`.
- contracts cover profile, project, facet, technology, artifact, evidence, link, status, reflection, limitations, provenance, visibility, reading depth, and publication.
- IDs are stable and unique; references resolve.
- candidate/private content is excluded.
- duplicate IDs, unknown values, malformed metadata, and dangling references fail non-zero.
- equivalent approved source produces semantically equivalent output.
- projects render without project-specific Razor branches.

### Definition of Done

- each public fact has one authority and traceable approved source.
- valid and invalid fixtures cover compilation and validation.
- generated data is never hand-edited.
- Phase 1 remains passing.

## Phase 3 - Versatile story blocks

Outcome: reusable blocks compose richer project stories.

### Acceptance Ready

- support approved needs from prose, quote, reflection, code, terminal, image, links, evidence, callout, relationship, replay, and diagnostic comparison.
- each block has stable ID, type, accessible label, depth, provenance, order, and validated payload.
- unknown types fail; development fallback is contained; publication is blocked on rendering failure.
- code/terminal text is selectable; images have alternatives; quotes have attribution; reflection is distinct from evidence.
- at least one project coherently uses five block types.

### Definition of Done

- contracts and extension rules are documented.
- desktop, mobile, keyboard, zoom, reduced-motion, and print behavior pass.
- adding an ordinary project requires content, not a new page component.
- earlier phases remain passing.

## Phase 4 - First solid public portfolio

Outcome: polished portfolio independent of future automation.

### Acceptance Ready

- opening communicates Ulf's direction without interaction.
- three to five approved projects form a coherent trajectory.
- every project communicates purpose, contribution, competence, learning, maturity, evidence, and limitations.
- artifacts have human explanation; meaningful repository/source/test/docs links work.
- overview stays concise while detail and evidence unfold.
- metadata describes the portfolio accurately.
- no unsupported strengthening, dead links, private data, or quantity-as-quality scoring.

### Definition of Done

- Ulf approves content and visual result.
- desktop/mobile/accessibility/print are coherent.
- deployed output matches validated source.
- visitors can understand it without instruction.
- earlier phases remain passing.

## Phase 5 - Evidence package contract

Outcome: repository observations can enter review without becoming claims.

### Acceptance Ready

Versioned schemas cover repository/revision/scope, observations, candidates, evidence, technologies, warnings, exclusions, and provenance. Candidate, reviewed, approved, and published states remain distinct. Partial results identify missing evidence. Packages contain no layout and grant no publication authority.

### Definition of Done

Compatibility rules and normal/incomplete/conflicting/excluded fixtures pass. Portfolio works without packages. Earlier phases remain passing.

## Phase 6 - Deterministic collector

Outcome: a CLI produces one reviewable evidence package.

### Acceptance Ready

Repository, revision, project, sink, inclusion, and exclusion are explicit. Paths are normalized and bounded. Binary/generated/oversized/sensitive/missing/conflicting inputs have policies. Equivalent inputs produce semantically equivalent output. Mandatory failures return non-zero. The collector never judges quality, infers contribution, writes final prose, or changes approved content.

### Definition of Done

One real package is traceable; exclusions and partial behavior are proven; fixtures pass; public content remains unchanged pending review; earlier phases remain passing.

## Phase 7 - Optional bounded curation

Outcome: evidence may produce reviewable suggestions without becoming truth.

### Acceptance Ready

Suggestions retain candidate ID, task, evidence, revision, configuration, original value, review state, decision, and accepted edit. Accept/edit/reject work. Exclusions apply. Insufficient evidence is recorded. Direct publication is blocked. Core portfolio works without curation.

### Definition of Done

One evidence-to-approved-content example passes; rejected/unreviewed output is absent; unsupported claims and sensitive inputs are blocked; deterministic evidence remains distinct from probabilistic suggestion; earlier phases remain passing.

## Cross-phase non-regression

Block acceptance on semantic loss, contradiction, incorrect direction, overstated maturity, lost provenance, unreviewed publication, private exposure, broken build/deploy/root/interaction/accessibility/static behavior, later-tool dependency, unsupported promotion, or unknown mandatory validation.

## Overall Definition of Done

The one-page portfolio is strong without automation; approved structured content drives it; projects combine narrative and artifacts as needed; evidence and collection are bounded and traceable; suggestions remain optional; Ulf remains final authority; every accepted phase continues passing.

## Phase 5 prototype entry gate
Before automated collection, a repository-local fixture prototype must prove authority separation with one proposed claim fixture, one revision-addressed evidence package, one explicit public reference, and one deterministic evidence-index projection. The fixture is not approved portfolio content. The prototype does not alter the site, deployment, or Phase 2 scope.

Acceptance Ready additionally requires machine-readable inference disablement, two-layer public visibility, package identity over scope and policy, separate schema and contract versions, and stable diagnostics for invalid fixtures.

Definition of Done additionally requires no new Python or NuGet dependency, no new GitHub Action, no new repository secret, no cron, no orphan branch, no deployment change, and no Phase 2 implementation or acceptance claim.
