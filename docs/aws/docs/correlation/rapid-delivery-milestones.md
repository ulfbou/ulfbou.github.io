# Rapid delivery milestones

## M0 â€” Contract gate

**Outcome:** all implementation boundaries are executable and unambiguous.

**Scope:** schemas, registries, canonical vectors, semantic fixtures, dependency-free validation, traceability.

**Exit:** every schema parses; valid fixtures pass; invalid fixtures fail for the intended code; duplicate properties and byte overflow fail before semantic use; canonical hashes match; registries resolve; transitions are complete; terminal states have no outgoing transitions; deletion cases match expected decisions.

## M1 â€” Offline lifecycle proof

**Outcome:** the domain lifecycle is mechanically proven without AWS.

**Scope:** canonicalizer, state machine, deletion authorizer, fake clock/IDs/storage, focused tests.

**Exit:** normal and cancellation paths pass; retries are deterministic; late result is rejected; stale revision/digest cannot authorize deletion; no AWS SDK dependency exists in the domain.

## M2 â€” Local vertical slice

**Outcome:** browser contracts run through both brokers using fake storage and provider adapters.

**Scope:** create/cancel endpoints at application level, fake Gemini adapter, normalized result, tombstone event generation.

**Exit:** bounded scenario returns candidate analysis; cancellation terminates immediately; all required provider failures map to stable errors; static portfolio remains unchanged and independently buildable.

## M3 â€” AWS sandbox slice

**Outcome:** the same bounded flow runs in a protected prototype environment.

**Scope:** IaC, HTTP API, three Lambdas, private versioned S3, roles, aliases, logs, alarms, budget, secrets, OIDC deployment.

**Exit:** infrastructure validation passes; least-privilege policies pass; smoke test creates and completes one correlation; kill switch works; no long-lived AWS key exists.

## M4 â€” Durable terminal cleanup

**Outcome:** GitHub owns public-safe terminal decisions and exact-state cleanup evidence.

**Scope:** GitHub App dispatch, runtime branch, tombstone workflow, manifest compiler, Cleanup Lambda, receipt semantics, Reconcile.

**Exit:** Compile/Delete/Reconcile privilege separation passes; deletion succeeds only for exact revision and digest; stale state is retained as unresolved; reconciliation resumes without repeating deletion; branch races fail safely.

## M5 â€” Portfolio-ready prototype

**Outcome:** one honest, accessible, degraded-safe public demonstration is reviewable.

**Scope:** reviewed scenario UI, candidate-analysis result, lifecycle replay, unavailable state, end-to-end acceptance evidence.

**Exit:** success and cancellation scenarios pass; every required failure path has named evidence; AWS-disabled static site passes; public records contain only permanently safe data; exact-candidate gate passes; portfolio claims remain bounded to demonstrated behavior.

## Delivery priority

M0â€“M2 are the fastest path to visible functionality and must complete before cloud mutation. M3 proves AWS judgment. M4 proves the distinctive correlation and cleanup architecture. M5 makes the result honestly presentable. Production hardening, scale, multiple providers, callbacks, user accounts, and realtime messaging remain deferred.
