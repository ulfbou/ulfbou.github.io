# LinkedIn publication threat model

## Evidence boundary

This Phase 0 candidate distinguishes accepted architecture, repository and collaboration-established facts, current provider facts, provisional defaults, unresolved LinkedIn evidence, and future implementation. It does not claim that cloud resources, secrets, OAuth, or live publication exist.

| Threat and trust boundary | Preventive control | Detective control | Recovery or disablement | Residual risk |
| --- | --- | --- | --- | --- |
| Signing secret leak between protected GitHub environment and production Worker | Separate production value, protected environment, opaque version slots, bounded overlap, no PR access | Version-class metrics and anomalous failures without existence disclosure | Disable, rotate both sides, review ledger | Valid forged requests before containment |
| Replayed transport request | Provisional ±300-second window, UUID v4 request ID, authenticated validation before atomic unique insert | Compatible duplicate and conflict counters | Return established result for exact compatible replay; reject conflict | D1 outage reduces availability |
| Semantic duplicate | Publication key includes channel, configured Ulf Bourelius member identity, normalized canonical URL, and article revision | Duplicate metrics and ledger audit | Return established result; reconcile conflict | `contentSha256` may omit future publication metadata |
| Stale or future request | Strict timestamp format and bounded skew | Direction and bounded skew bucket only | Correct signer clock; never widen silently | Clock-service fault |
| Body modified or reserialized | Exact-body SHA-256 inside HMAC; authenticate before parsing | Stable signature-failure metric | Reject without ledger mutation | Secret compromise |
| Authentication oracle | Uniform bounded invalid-signature response; opaque admitted version IDs; constant-time MAC comparison | Aggregate failure metrics | Rotate/disable after anomaly | Timing differences from platform/runtime |
| Oversized or encoded body abuse | Reject unsupported content encoding; bounded exact-body read; 16 KiB provisional limit | Size/encoding counters without payload | Reject | Edge/runtime pre-processing defects |
| Duplicate JSON keys | Parser configured to reject duplicates after authentication | Stable schema-rejection metric | Reject and regression-test | Parser/runtime differences |
| Arbitrary or ambiguous URL | Exact HTTPS host and publisher-equivalent governed article route; reject encoded separators, dot segments, backslashes, controls, malformed escapes, Unicode ambiguity, user info, ports, queries, fragments, duplicate separators | Bounded invalid-URL counters | Reject and add fixture | Canonicalization defect |
| Arbitrary LinkedIn proxying | Fixed dry-run endpoint, configured personal identity, closed schema, no outbound LinkedIn call | Route/schema rejection metrics | Disable endpoint | Future adapter scope creep |
| Invalid request pollutes ledger | No ledger mutation before authentication, parse/schema/URL validation, and identity derivation | Ledger invariant checks | Reject; migrate only through review | Implementation-order regression |
| D1 outage | Fail closed before acceptance; no external call without durable intent | Health/error metrics | Restore and reconcile; no blind replay | Availability loss |
| Concurrent submissions | Unique request/publication constraints and atomic handling | Conflict metrics | Read established compatible record | Migration or query defect |
| Production credentials leak into Preview/Local | Separate bindings, routes, databases and values; no live capability outside Production | Configuration tests and deployment review | Revoke, rotate, destroy contaminated state | Platform misconfiguration |
| Public Preview endpoint attacked | Same authentication strength; no obscurity assumption; isolated state | Preview-specific bounded metrics | Disable Preview route/secret | Denial of service within free limits |
| Sensitive logs | Allowlisted fields; complete-signature/body/token prohibition | Sampling review and secret scanning | Purge where supported; rotate exposure | Provider retention copies |
| Compromised workflow | Protected environment, trusted post-deploy context, reviewed workflow changes | GitHub audit and workflow diff | Disable environment/endpoint and rotate | Trusted maintainer compromise |
| Accidental live mode | Dry-run-only first increment; Preview/Local structurally incapable; no LinkedIn credential or adapter | Configuration assertion and mode metric | Kill switch, then route removal | Later governance failure |
| Disabled operation ambiguity | Initial kill switch returns `423` without D1 mutation; metrics only | Disabled counter | Reviewed re-enable | No per-request disabled ledger record |
| Uncertain future live result | Persist `uncertain`; prohibit automatic retry | Reconciliation report | Confirm externally before reviewed transition | LinkedIn may not expose definitive lookup |
| Worker/Pages deployment race | Do not auto-invoke when Worker source also changed unless expected production Worker revision is independently confirmed | Record Pages run and Worker version evidence | Stop invocation; resume only after confirmation | Propagation delay |
| Free-tier exhaustion or policy change | Bounded traffic/builds/logging/retention; indexed D1; periodic official-policy review; no paid auto-upgrade | Usage and allowance monitoring | Disable nonessential checks/previews; remain fail closed | Availability loss, manual effort |

## Trust boundaries

Untrusted browser and public internet; GitHub workflow runner; protected GitHub publication environment; Cloudflare Workers Builds; production Worker; Preview Worker; local runtime; D1 ledger; future LinkedIn API; and human account administration are distinct boundaries. Passing validation does not grant live-publication authority.

## Required security tests for Worker entry

The later Worker foundation must include canonical positive and negative HMAC vectors, exact-byte and non-ASCII cases, duplicate-key rejection, unsupported-encoding rejection, size-boundary tests, timestamp-boundary tests, version-oracle resistance checks, replay compatibility/conflict tests, concurrent semantic-idempotency tests, D1-unavailable fail-closed tests, kill-switch no-mutation tests, redaction tests, URL canonicalization fixtures derived from repository routes, and proof that Preview/Local cannot publish live.
