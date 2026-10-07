# Initial PR plan

Protect `main` through reviewed PRs and use Conventional Commits.

## PR 1 - Documentation foundation

Branch `docs/portfolio-foundation`; title `docs: establish one-page portfolio foundation`.

Scope: all governing docs in this carrier.

Acceptance Ready: each doc has one responsibility; terminology is consistently one-page portfolio; phases have outcomes, Acceptance Ready, and DoD; authority/non-regression are explicit; brainstorm ideas are classified; no unsupported implementation claim exists.

Done: docs are accepted; links resolve; no competing roadmap remains active; Phase 1 can be evaluated without reinterpretation; these docs become controlling state.

## PR 2 - Prototype vertical slice

Branch `feat/portfolio-vertical-slice`; title `feat: solidify dynamic one-page portfolio`.

Scope: current `.github/` and `src/`, remove template debris, add model/interaction tests, deployment, loading/failure/empty states, responsive/accessibility corrections. Do not start Phase 2.

Acceptance Ready: every Phase 1 AR criterion has named evidence.

Done: Phase 1 DoD passes except explicit final content approval reserved for PR 3; that exception blocks public release.

## PR 3 - Approved initial content

Branch `content/initial-portfolio`; title `content: publish approved initial portfolio stories`.

Scope: replace/approve representative wording; verify status, contribution, reflection, technologies, artifacts, evidence, links, limitations, metadata, visual/accessibility/deployment review; remove prototype markers.

Acceptance Ready: each public statement has authority; no placeholder, private data, invented metric, unsupported strengthening, or dead link; project selection forms a trajectory; deployment matches reviewed revision.

Done: Ulf approves content; Phase 1 passes without exception; the site is fit for `ulfbou.github.io`; remaining ideas sit in later roadmap phases.

Each PR remains scoped, records evidence, updates docs with behavior, blocks on unknown mandatory results, and preserves semantic non-regression.
