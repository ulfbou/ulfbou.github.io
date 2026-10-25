# Correlation prototype implementation plan

## Delivery rule

Implement one reviewable vertical boundary at a time. Each boundary must preserve the static portfolio, add deterministic tests before external mutation, and produce exact-candidate evidence. No later boundary may weaken accepted contract, authority, security, or degraded-mode behavior.

## Ordered increments

1. **Executable contracts:** add the dependency-free validator, strict API/provider/dispatch schemas, canonical vectors, state-transition registry, deletion cases, and complete traceability registry. No AWS or GitHub mutation.
2. **Domain kernel:** implement canonical JSON, identity, state transitions, terminality, late-result rejection, and deletion authorization behind AWS-independent interfaces.
3. **Storage adapter:** implement derived S3 keys, bounded reads, create-only writes, conditional revision advancement, version-aware deletion, and deterministic fake-storage tests.
4. **Broker slice with fake provider:** implement browser request/cancel contracts, Request Broker and Result Broker application services, normalized result, cancellation, expiry, and structured failures without live provider access.
5. **Gemini adapter:** implement the provider-specific raw boundary, model pinning, strict output normalization, timeout, quota classification, and no-sensitive-data tests.
6. **Tombstone boundary:** implement bounded GitHub App dispatch and protected workflow ingestion with idempotent event identity and path-scoped runtime mutation.
7. **Cleanup boundary:** implement manifest compiler, Cleanup Lambda core, exact conditional deletion, receipt semantic validation, and reconcile recovery.
8. **Infrastructure and security:** synthesize HTTP API, Lambdas, S3, secrets, aliases, logs, alarms, budget, OIDC trust, and separate least-privilege roles.
9. **Separated workflows:** implement validate, deploy, cleanup Compile/Delete/Reconcile, and live smoke using full-SHA actions and digest-bound handoffs.
10. **Portfolio demonstration:** add reviewed scenario UI, unavailable/degraded behavior, candidate-analysis presentation, sanitized replay, and no dynamic dependency for core content.
11. **End-to-end acceptance:** prove success, cancellation, late result, expiry, quota exhaustion, stale deletion prevention, already absent, failed reconciliation recovery, delayed cleanup, and AWS-disabled static behavior.

## Review boundary contract

Every increment must identify exact changed paths, pass focused tests and the established repository gate, update only its owning documentation, and avoid unrelated portfolio changes. Live mutation begins only after increments 1â€“4 pass locally.
