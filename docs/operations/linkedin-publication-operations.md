# LinkedIn publication operations

## Current scope and facts

This Phase 0 candidate governs future operations. It does not claim that a Worker, D1 database, secret, LinkedIn application, or deployment exists. Cloudflare account access and MFA are established; **Cloudflare MFA is completed**. Recovery readiness remains recommended but is not a provisioning blocker caused by incomplete MFA.

## Environment isolation

Production, Preview, and Local do not share signing-secret values, D1 databases, routes, LinkedIn credentials, or live-publication capability. Preview and Local are structurally incapable of live publication. Publicly reachable Preview endpoints require the same authentication properties as Production and never depend on obscurity. Production credentials never enter Preview configuration.

Cloudflare Workers Builds is the sole Worker deployment owner. Its future project root is `src/cloudflare/linkedin-publication-worker/`. GitHub Actions does not deploy the Worker. After successful Pages deployment, a separate protected production job may invoke an already deployed Worker.

## Initial deployment-race policy

Do not automatically invoke publication for a deployment that also changes `src/cloudflare/linkedin-publication-worker/**` unless the expected production Worker revision is independently confirmed. A successful Pages deployment proves neither Worker build success nor active Worker revision. Record Pages run ID, expected Worker version, and confirmation evidence before invocation.

## Authentication and ledger ordering

Reject unsupported encoding and oversized bodies before expensive work. Authenticate the exact bytes before JSON parsing and before any ledger mutation. Invalidly signed or unauthenticated requests create no ledger rows. After authentication, reject duplicate JSON keys, validate schema and URL, derive identities, assess disablement and D1 availability, then perform atomic ledger work. D1 unavailable means fail closed with no external call.

## Disablement

The production Worker has a future `PUBLICATION_ENABLED=false` kill switch. In the initial contract it returns `423 publication_disabled` after successful authentication and validation but before ledger mutation. Disablement is counted through bounded metrics, not a D1 row. Route removal is secondary containment. Future LinkedIn credential revocation is a later containment control.

The ledger retains `disabled` as a future status vocabulary, but Phase 0 does not create disabled rows. Changing that policy requires coordinated API, migration, operations, and threat-model governance.

## Secret rotation

Maintain current and next HMAC slots with opaque admitted version identifiers and a bounded overlap. Provision the next value independently to the Worker and protected GitHub environment, validate dry-run vectors and invocation, switch the signer version, observe bounded metrics, then remove the prior value. Unknown versions must not create a useful existence oracle. Never print or persist secret values or complete signatures.

## Replay, retries, and reconciliation

A compatible exact transport replay returns its established result. Conflicting reuse returns `409`. Compatible semantic duplicates return the established result. `uncertain` future live results are never retried automatically. Reconcile by request ID, publication key, canonical URL, article revision, configured personal identity, workflow run, and future external post ID. Record the conclusion before a reviewed status transition.

## Recovery

- D1 failure: fail closed; do not accept intent and do not call an external API.
- Migration failure: roll forward with a reviewed migration; never edit production schema ad hoc.
- Worker rollback: deploy a previously reviewed Worker revision without rolling the ledger backward.
- Lost signing secret: disable, rotate, inspect allowlisted metrics and ledger, then re-enable dry-run only.
- Pages/Worker race: stop invocation until canonical article availability and expected Worker revision are independently confirmed.

## Logging and retention

Allowlist request ID, truncated publication-key fingerprint, mode, status, stable error code, source repository, source commit, workflow run ID, Worker version, bounded timing, and bounded clock-skew direction/bucket. Log canonical URL only after validation. Never log bodies, authorization headers, secrets, complete signatures, GitHub or LinkedIn tokens, refresh tokens, or arbitrary upstream payloads.

Operational-log retention target is provisionally 30 days where provider controls permit it. Provider-fixed retention may be shorter or longer and must be recorded rather than misrepresented. Dry-run D1 rows are provisionally retained 90 days, then deleted by reviewed maintenance. These are reviewable defaults, not permanent provider guarantees.

## Account recovery and access review

Because MFA is completed, remaining recommendations are to store recovery codes safely and separately, verify account email and a second recovery path, periodically review account and GitHub App access, and rotate credentials after suspected exposure. Do not disable the current factor merely to test recovery.

## Zero-cost operations

Before provisioning and periodically afterwards, verify the Workers plan, Workers Builds allowance, D1 allowance, log/observability behavior, GitHub Actions usage, secret storage, LinkedIn access, OAuth/token renewal, and any external service. Keep previews, builds, checks, retention, and D1 scans bounded. Any mandatory paid dependency is outside current zero-cost scope.
