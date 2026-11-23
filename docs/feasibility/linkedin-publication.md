# LinkedIn automatic publication feasibility gate

## Decision boundary

Initial publication identity: **personal LinkedIn member, Ulf Bourelius**. Organization publication is outside the initial path. No relevant LinkedIn developer application currently exists.

The automatic-publication feasibility result is distinct from existing reader-initiated sharing, which remains available without server credentials. This gate blocks live LinkedIn adapter and OAuth work, but not a later dry-run-only Worker foundation after Phase 0 merges.

## Evidence classes

- Accepted architecture and collaboration-established facts are recorded as such.
- Provider capabilities are current external facts and require dated official sources.
- Permission naming does not prove application eligibility, product approval, or granted access.
- Proposed limits and lifecycle details are provisional until repository governance accepts them.
- Application creation, authorization, API testing, and live publication are future work.

## Cloudflare infrastructure feasibility within free allowances

Reviewed 2026-11-23 against official Cloudflare documentation:

- Workers Free provides bounded daily Worker usage and CPU limits. The tiny privileged workload is expected to fit because normal readers never call the Worker and publication requests occur only after governed article deployments.
- Workers Builds has a Free-plan allowance, one concurrent build, a bounded build timeout, and retained build logs. Deliberate change frequency is expected to fit.
- D1 Free provides bounded daily row operations, total storage, and database count. Indexed point lookups and a very small ledger are expected to fit.
- Preview, health checks, logs, and tests must remain deliberate and bounded. Preview is not a load-test environment.

Sources:

- https://developers.cloudflare.com/workers/platform/pricing/
- https://developers.cloudflare.com/workers/ci-cd/builds/limits-and-pricing/
- https://developers.cloudflare.com/d1/platform/pricing/

These allowances are current evidence, not permanent guarantees. Before provisioning and periodically thereafter, verify provider policy, account plan, usage, storage, build minutes, and observability retention. Configure available usage alerts or hard limits where they do not introduce payment.

## Expected workload fit

Readers continue using the static GitHub Pages site and client-side LinkedIn share link. Article HTML and PNG social images remain on GitHub Pages. The privileged Worker receives only bounded post-deployment requests, controlled health checks, and deliberately limited Preview traffic. D1 operations are indexed single-row identity checks and bounded writes. This pattern is expected to remain far below current free allowances.

## Cost-triggering conditions

Potential triggers include enabling a paid Workers plan, exceeding or changing free allowances, excessive builds or previews, unbounded logging, large retention, inefficient D1 scans, excessive synthetic health checks, a new paid observability service, paid secret management, or a LinkedIn product/support requirement with mandatory payment. Nothing may silently upgrade or admit a paid dependency.

Any mandatory-payment feature is **OUTSIDE CURRENT ZERO-COST SCOPE**.

## LinkedIn capability and unresolved eligibility

Current official LinkedIn documentation describes the Posts API for organic article posts and identifies `w_member_social` for posting on behalf of an authenticated member. That proves an API capability and permission name only. It does **not** prove that a newly created application for this personal project can obtain the required product, approval, permission, or token.

Official documentation also describes programmatic refresh tokens for approved Marketing Developer Platform partners. Programmatic refresh is preferred if the actual application is eligible and the permission/product combination supports it. Otherwise, periodic manual reauthorization is accepted. The feasibility stage must determine the actual access-token lifetime, refresh eligibility and lifetime, revocation behavior, and reauthorization procedure for the granted application.

Sources reviewed 2026-11-23:

- https://learn.microsoft.com/en-us/linkedin/marketing/community-management/shares/posts-api
- https://learn.microsoft.com/en-us/linkedin/shared/authentication/programmatic-refresh-tokens

## Named evidence required

1. Current official route to create an appropriate personal-member LinkedIn application.
2. Required product and whether approval is needed.
3. Whether `w_member_social` is actually obtainable and granted to this application.
4. Authorization Code flow details, redirect requirements, and configured member identity.
5. Actual access-token lifetime.
6. Programmatic refresh-token eligibility and lifetime, or the periodic manual reauthorization procedure.
7. Current Posts API or official replacement, version headers, permitted article-post payload, and revocation behavior.
8. Link-preview behavior for the canonical GitHub Pages URL and whether existing Open Graph PNG metadata is used as intended.
9. Confirmation that LinkedIn developer access, OAuth, token handling, and any required supporting service remain zero monetary cost.
10. One later, separately authorized non-production feasibility test, without exposing tokens.

## Manual effort that remains zero monetary cost

Application setup, authorization, approval requests, periodic policy review, usage review, secret rotation, incident response, and periodic manual reauthorization can consume operator time while satisfying the zero-monetary-cost constraint. They must be documented as operational effort, not described as seamless automation.

## Current result

**UNRESOLVED PENDING SPECIFIC NAMED EVIDENCE**

Cloudflare infrastructure and the expected workload are credible within current free allowances, but personal-member LinkedIn application eligibility and token lifecycle are not yet proven. The result may later become one of the required feasibility outcomes, including **SUPPORTED WITHIN ZERO-COST BUDGET BUT REQUIRES PERIODIC MANUAL REAUTHORIZATION**, only after the named evidence exists.
