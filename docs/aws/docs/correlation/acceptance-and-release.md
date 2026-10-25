# Acceptance and release

## Definition of Ready

Prototype development is ready when scope, scenario, provider boundary, result target, state machine, AWS/GitHub components, trust boundaries, authorities, branch model, tombstone producer, public classification, canonical representation, IAM, workflows, source structure, contracts, tests, cost controls, degraded modes, and acceptance paths are explicit and mutually traceable.

This package establishes that design state. Readiness does not claim implementation or deployment.

## Release gates

- `correlation-contracts`
- `correlation-domain`
- `correlation-schema-fixtures`
- `correlation-workflow-security`
- `correlation-iam`
- `correlation-infrastructure`
- `correlation-static-degraded`
- `correlation-live-smoke`
- `correlation-cleanup-acceptance`
- `correlation-exact-candidate`

- **COR-REL-001:** Every gate result SHALL bind exact commit and tree identities, validator identity, inputs, outcome, and limitations.
- **COR-REL-002:** Unknown mandatory results fail.
- **COR-REL-003:** Successful and cancellation paths, stale-revision prevention, delayed cleanup, reconciliation recovery, provider quota failure, and AWS-disabled static behavior are mandatory.
- **COR-REL-004:** Public claims SHALL be limited to mechanically established behavior and explicit limitations.

## Traceability exemplar

`COR-SEC-001` is owned by the security document; implemented by cleanup job separation; verified by workflow policy tests; gated by `correlation-workflow-security`; evidenced by exact-candidate validation; and may support a bounded claim that no cleanup job simultaneously holds repository-write and AWS OIDC authority.

## Allowed prototype claim

After implementation and acceptance only:

> The prototype demonstrates reviewed GitHub governance, bounded AWS execution, minimal revisioned active state, non-authoritative provider mediation, terminal tombstones, and revision-bound eventual cleanup while preserving an independently useful static portfolio.

No production-readiness, zero-cost, availability, scale, privacy, or provider-quality claim follows automatically.
