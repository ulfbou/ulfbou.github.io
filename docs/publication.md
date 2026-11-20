# Platform-wide publication contract

## Purpose and scope

This document defines the publication contract for all public areas of `ulfbou.github.io`: the professional landing page, portfolio, technical articles, and AWS Cloud and AI learning experiences.

It governs the movement from authorized source meaning to deployed static output. It does not define the substantive meaning of portfolio claims, article arguments, or learning content, and it does not claim that one shared implementation currently enforces every rule below.

## Governing principle

> Publication infrastructure may transform, compose, validate, and deploy authorized meaning, but it may not create, strengthen, weaken, approve, or otherwise alter public meaning on its own authority.

Ulf retains final authority over public meaning, claims, publication eligibility, route-policy changes, and release.

## Semantic authority

Semantic authority is the authority to determine what public material means.

- Platform product documents govern platform purpose, public-area allocations, shared identity, navigation principles, and route policy.
- Landing-page product and approved content govern the meaning published at `/`.
- Portfolio product documents and approved portfolio source govern portfolio selection, claims, contribution, evidence, maturity, and limitations.
- Article product and editorial documents, together with each approved article source, govern article arguments, wording, attribution, and corrections.
- Learning product documents and approved learning source govern learning goals, explanations, educational sequence, and learner-facing interaction.

Evidence, observations, generated projections, templates, compilers, build scripts, validators, and deployment workflows do not possess semantic authority. They may support or verify authorized meaning, but they cannot originate or approve it.

When authorities overlap, the most specific accepted product authority governs the meaning, while platform authority governs cross-area contracts. A conflict must be resolved by authorized review rather than by generator precedence.

## Publication authority

Publication authority is the authority to decide that approved material may become public at a specified route and release.

Ulf is the final publication authority. Delegated review or automation may provide evidence that gates have passed, but passing a gate does not itself grant authority to publish unapproved meaning.

A release authorization must identify, directly or through a traceable revision:

- the approved source to publish;
- the public routes and outputs covered;
- the applicable product and publication contracts;
- the validation evidence relied upon;
- any accepted limitations or known exceptions;
- the revision or artifact authorized for deployment.

## Visibility and publication states

Visibility and publication state are separate concerns.

### Visibility

- **Private:** must not enter public build inputs, previews shared outside the authorized review boundary, generated public artifacts, indexes, feeds, sitemaps, or deployments.
- **Candidate:** may be reviewed as a proposal but is not authorized for public output.
- **Public:** is eligible to be included in public output only when all publication gates also pass.

### Publication state

- **Draft:** incomplete or not yet approved; not publishable.
- **Approved:** accepted in meaning but not necessarily included in a released public artifact.
- **Published:** approved for a defined public route and included in an authorized release.
- **Withdrawn:** intentionally removed from active publication; must not remain in active indexes or feeds.
- **Archived:** retained at an intentional archival route or representation with its historical status made clear.

An implementation may use a bounded subset of these states, but it must preserve their operational distinctions. In the current portfolio implementation, `Draft`, `Approved`, and `Published` publication states coexist with `Private`, `Candidate`, and `Public` visibility. This contract does not claim that withdrawal and archival are already implemented as code.

No material becomes public merely because a file is tracked, generated, reachable by URL, or marked `Public`. Publication requires both public visibility and an authorized publication state.

## Approval and publication gates

Before material is published, the responsible process must establish:

1. **Semantic approval:** the applicable semantic authority has approved the complete meaning, including qualifications, limitations, labels, links, and metadata.
2. **Visibility approval:** every included source, reference, asset, and evidence item is explicitly suitable for public disclosure.
3. **Route approval:** the destination is owned by the publishing area and complies with the stable URL policy.
4. **Completeness:** required content and metadata are present; partial generation cannot silently omit mandatory meaning.
5. **Integrity:** generated output can be traced to the approved inputs and has not been changed outside the authorized process.
6. **Determinism:** equivalent governed inputs and configuration produce equivalent semantic output, and byte-identical output where the output contract requires it.
7. **Accessibility:** mandatory automated and manual accessibility checks for the affected output have known, acceptable results.
8. **Factual freshness:** time-sensitive claims and external references have been reviewed according to their freshness requirements.
9. **Privacy and safety:** prohibited private, secret, diagnostic, local, candidate, or unpublished material is absent.
10. **Non-regression:** previously accepted public meaning is preserved unless an authorized change explicitly replaces, corrects, withdraws, or archives it.
11. **Deployment authorization:** Ulf has authorized the exact revision or artifact for release.

An unknown mandatory result fails the gate. A warning may be accepted only by the authority responsible for the affected meaning or release, with the limitation recorded.

## Deterministic generation expectations

Where source is transformed into public output:

- source selection, ordering, filtering, serialization, and route mapping must be explicit;
- equivalent governed inputs must produce semantically equivalent outputs;
- retained generated artifacts must be reproducible when the governing implementation requires retention;
- output must not depend on timestamps, random values, machine-specific paths, locale-sensitive ordering, unstated network results, or ambient repository state unless the contract explicitly admits and records them;
- generated files must not be hand-edited as an alternative authority;
- probabilistic output must remain candidate material until separately reviewed and approved;
- a generator version or contract change that can alter meaning requires review as a semantic-risking change, not merely a tooling upgrade.

Determinism does not authorize publication. It proves repeatability of the transformation, not correctness or approval of the meaning.

## Route and output ownership

Every public route and generated artifact must have one declared owner.

- `/` is owned by the professional landing-page publication process.
- `/work/` is owned by the portfolio publication process.
- `/articles/` and its descendants are owned by the article publication process.
- `/aws/ai/` and its descendants are owned by the AWS Cloud and AI learning publication process.

A platform composition step may assemble these outputs into one deployable site, but it must not silently rewrite area-owned files or allow two producers to write the same path. Shared assets and indexes require an explicit owner and compatibility contract.

Generated output directories are deployment artifacts, not authoring locations. Route changes require platform-level approval because they affect public contracts beyond one build implementation.

## Artifact integrity

An authorized release must make it possible to establish:

- the source revision and approved inputs;
- the generator and relevant contract versions;
- the complete set of included public paths;
- the absence of unauthorized extra output;
- the integrity of the artifact through a stable digest or equivalent immutable identity;
- the relationship between the verified artifact and the deployed artifact.

Verification must occur after generation and before deployment. If packaging, upload, or hosting can alter output, the process must verify the final deployable boundary or use a mechanism that preserves the verified bytes.

## Factual freshness

Factual correctness includes time validity.

Each area must distinguish durable explanation from time-sensitive claims such as versions, certification objectives, service behavior, release status, external links, and availability.

- Time-sensitive claims require a source and a recorded review point appropriate to their volatility.
- AWS service and certification claims should be checked against current authoritative AWS sources when publication or review occurs.
- Portfolio maturity and release claims must be checked against the approved evidence for the stated project revision.
- Article corrections must preserve the history required to understand what changed.
- Broken or materially changed external references must not be silently replaced with a source that changes the claim.

Automated link or freshness checks may identify review candidates, but cannot determine new public meaning or approve an update.

## Deployment authorization

Build success is not release authorization.

Deployment is permitted only for an exact, reviewed revision or immutable artifact that has passed the required gates and received Ulf's release authorization. Workflows must not broaden scope from approved paths, substitute newer unreviewed generated output, or publish artifacts from an untrusted context.

Preview output must remain clearly non-public or clearly marked as a preview within an authorized review boundary. A preview must not create a discoverable canonical route that could be mistaken for released content.

## Correction, withdrawal, and archival behavior

### Correction

A correction must identify the affected public meaning, preserve relevant qualifications and provenance, pass the normal gates, and replace the released output through an authorized release. Material corrections should be visible to readers when the previous statement could materially mislead them.

### Withdrawal

Withdrawal removes material from active navigation, indexes, feeds, generated output, and sitemaps. The previous route must either return an intentional withdrawal response or redirect to a genuinely equivalent replacement. It must not redirect indiscriminately to `/`.

### Archival

Archival preserves historical material intentionally. Archived content must state that it is archived or superseded, retain necessary context, and avoid presenting obsolete instructions or claims as current. An archive is public publication and therefore still requires privacy, integrity, and route approval.

Deletion is not the default correction mechanism when provenance, public references, or historical understanding require retention.

## Failure behavior

Publication must fail closed when:

- required source, approval, validation evidence, or route ownership is missing;
- private, candidate, draft, or unapproved material would enter public output;
- multiple producers claim the same public path;
- deterministic generation or artifact verification fails;
- generated output is partial, stale relative to the authorized source, or contains unauthorized extras;
- mandatory accessibility, privacy, factual-freshness, or non-regression results are unknown or failing;
- a workflow would publish a different revision or artifact from the one authorized;
- infrastructure cannot preserve the approved meaning.

Failure diagnostics must identify the affected gate and artifact without exposing secrets or private content. A failed area build must not be replaced by stale or placeholder output without explicit authorization. Previously published unaffected areas may remain available when deployment design can prove they are unchanged and the release authority permits that behavior.

## Semantic non-regression

A publication change must preserve all previously accepted operational meaning unless an authorized decision explicitly changes it.

Semantic non-regression covers:

- claims, qualifications, limitations, attribution, provenance, and maturity;
- distinctions between private, candidate, approved, published, withdrawn, and archived material;
- route meaning and ownership;
- accessibility-required meaning and alternatives;
- the separation between evidence and claims;
- the separation between platform, portfolio, article, and learning authority;
- the fact that generated output and infrastructure are not semantic authorities.

Reformatting, deduplication, relocation, shorter wording, schema migration, or framework replacement is not sufficient justification for semantic loss. A transfer is complete only when the destination preserves operational meaning, constraints, exceptions, and authority.

Validation should compare meaning at the appropriate boundary. Byte identity is required only where the artifact contract requires byte reproducibility; semantic equivalence remains required in all cases.

## Protection against infrastructure-created meaning

Publication infrastructure is explicitly prohibited from:

- inventing, completing, summarizing, strengthening, weakening, or rephrasing public claims without approved source authority;
- treating repository observations, metadata, metrics, generated text, or AI output as approved claims;
- promoting candidate or private material because it passes syntax validation;
- selecting what is professionally important or educationally correct without the applicable semantic authority;
- silently changing qualifications, limitations, attribution, visibility, reading depth, or maturity;
- generating route titles, descriptions, article conclusions, portfolio statements, or learning explanations that become public meaning without review;
- resolving source conflicts by choosing whichever input is newest or easiest to process;
- interpreting successful transformation or deployment as approval.

If infrastructure cannot render authorized meaning without alteration, publication must stop and request an authorized source or contract change.

## Accepted contract and current implementation

This document is the accepted platform-wide publication contract for Increment 1.

At the start of Increment 1, the repository already implements a narrower portfolio publication model: approved source under `src/content/`, publication and visibility metadata, validation, deterministic compilation, a retained generated portfolio projection, and deployment checks. Existing documentation also defines portfolio-specific evidence and semantic non-regression rules.

Those mechanisms are evidence of current portfolio behavior only. They do not yet constitute a complete platform-wide implementation for `/`, `/work/`, `/articles/`, and `/aws/ai/`. Later increments must reconcile existing portfolio contracts with this platform contract without deleting or compressing their operational meaning in advance.
