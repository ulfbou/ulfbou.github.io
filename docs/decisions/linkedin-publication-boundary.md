# LinkedIn publication privileged boundary

## Status and evidence classes

Status: Phase 0 candidate.

- **Accepted decisions:** GitHub Pages remains the public static host; reader sharing remains credential-free; Cloudflare Workers is the privileged server-to-server boundary; the initial identity is personal member Ulf Bourelius; zero monetary cost is mandatory; the first Worker increment is dry-run only.
- **Repository facts:** facts verifiable from repository content or the supplied repository-state capture.
- **Collaboration-established account facts:** Cloudflare access, MFA completion, Workers & Pages availability, restricted GitHub App repository access, and absence of created Worker/D1/secrets.
- **Current external platform facts:** time-sensitive provider documentation, dated and sourced in the feasibility record.
- **Provisional contract values:** safe defaults still subject to review and validation.
- **Unresolved external evidence:** LinkedIn application eligibility, granted product/permission, token behavior, and current zero-cost access.
- **Future work:** implementation, provisioning, OAuth, and live publication.

A candidate proposal is not accepted repository authority merely because it appears here.

## Accepted decisions

- GitHub Pages remains the website host. The site is not migrated to Cloudflare Pages.
- Existing reader-initiated LinkedIn sharing and governed Open Graph/PNG previews remain unchanged.
- Cloudflare Workers is the privileged server-to-server publication boundary. The endpoint is not a browser endpoint, emits no browser CORS permission, and does not treat CORS as authorization.
- Initial publication identity: **personal LinkedIn member, Ulf Bourelius**. The server controls it. Organization publication is future out-of-scope work requiring a new decision.
- The complete design must remain within a zero-cost budget. Mandatory paid dependencies are outside current scope.
- First implementation is dry-run only: `GET /health` and `POST /v1/linkedin/publications/dry-run`. It has no LinkedIn credential, OAuth implementation, API call, live endpoint, or proxy.
- Cloudflare Workers Builds owns Worker deployment. A protected GitHub Actions job may sign a dry-run invocation only after successful GitHub Pages deployment. GitHub Actions is not a second Worker deployment owner.
- Production, Preview, and Local do not share secret values, D1 databases, routes, LinkedIn credentials, or live capability. Preview and Local are structurally incapable of live publication.

## Repository facts at Phase 0 capture

The default branch is `master`. The candidate branch is `docs/linkedin-publication-phase-0`, initially at the same commit as `master`, with the five Phase 0 files untracked. Governed articles use canonical routes beneath `/articles/`, generated records include `contentSha256`, and public article previews use governed metadata and PNG social images. These facts do not prove deployment or LinkedIn capability.

## Collaboration-established account facts

Cloudflare account access is available; Cloudflare MFA is **completed**; Workers & Pages is available; the Cloudflare GitHub App is installed with access only to `ulfbou/ulfbou.github.io`; repository discovery is confirmed. No Worker, deployment, D1 database, production Worker secret, or relevant LinkedIn developer application currently exists.

Recovery-code storage, recovery readiness, credential rotation, and account-access review remain operational recommendations. They are not evidence that MFA is incomplete and are not Phase 0 blockers.

## Provisional implementation defaults

The following are reviewable Phase 0 defaults rather than pre-existing accepted repository authority: ±300-second HMAC tolerance; 16 KiB exact body; 90-day dry-run ledger retention; 30-day operational-log target where provider controls permit it; dry-run endpoint and mode; `contentSha256` as current article revision; Workers Builds as deployment owner; protected post-Pages GitHub Actions as invocation owner.

These defaults are internally practical for the low-volume threat model. The two responsibility assignments and dry-run scope align with accepted architecture. Numeric limits remain versioned contract values and may be revised through governance without weakening security.

## Proposed privileged lifecycle

1. The governed producer exposes `contentSha256`, with its known publication-digest limitation documented.
2. Following successful Pages deployment, a protected GitHub job signs a bounded dry-run request.
3. The Worker authenticates before parsing or ledger mutation, validates the closed contract and URL, and records an idempotent D1 result.
4. A later feasibility-approved, separately governed live adapter may call LinkedIn. Phase 0 does not authorize it.

## Deployment-race rule

For the first dry-run increment, do not automatically invoke publication for a deployment that also changes `src/cloudflare/linkedin-publication-worker/**` unless the expected production Worker revision is independently confirmed. Pages success alone does not prove the new Worker revision is active.

## Future work and entry gate

The Worker foundation may later be created at `src/cloudflare/linkedin-publication-worker/` using Python, `uv`, `pywrangler`, and required Wrangler/Node tooling. It may implement the dry-run API, D1 migrations, tests, and isolated non-production configuration only after the five Phase 0 documents pass validation, are committed, complete their PR review, merge into `master`, and all entry-gate conditions are met.
