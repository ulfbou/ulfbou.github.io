# Governance

## Authorities

- **COR-GOV-001:** Protected `master` is the authority for reviewed source, policies, schemas, registries, workflows, infrastructure, and approved portfolio meaning.
- **COR-GOV-002:** The orphan branch `correlation-runtime` is the authority for machine-generated public-safe tombstones, receipts, statuses, unresolved conflicts, and replay projections.
- **COR-GOV-003:** AWS is an execution and trust boundary, not an authority for portfolio meaning.
- **COR-GOV-004:** External providers supply replaceable capability and have no publication authority.
- **COR-GOV-005:** Ulf remains final human publishing authority.

## Branch model

`master` changes only through reviewed pull requests. `correlation-runtime` has no required shared history with `master`; runtime records are data only, never scripts or workflow definitions. Workflow code is loaded from protected `master`.

Runtime mutation is limited to:

```text
correlation-runtime/tombstones/v1/
correlation-runtime/receipts/v1/
correlation-runtime/status/v1/
correlation-runtime/unresolved/v1/
correlation-runtime/replay/v1/
```

- **COR-GOV-006:** Reconcile SHALL compare the exact observed runtime-branch tip before advancement.
- **COR-GOV-007:** A branch race SHALL fail safely without force push, blind rebase, or overwrite.
- **COR-GOV-008:** Runtime records SHALL NOT be merged wholesale into `master`.

## Tombstone producer

A repository-scoped GitHub App is the admitted AWS-to-GitHub identity. Result Broker uses a short-lived installation token to emit a bounded `repository_dispatch`. A workflow on protected `master` validates the event and independently constructs the tombstone. The event is not itself durable authority.

The App is installed only on `ulfbou/ulfbou.github.io`, has the minimum endpoint-required repository permissions, and has no administration, secret, environment, deployment, issue, pull-request, package, Pages, or organization-wide authority.

- **COR-GOV-009:** The GitHub App private key SHALL exist only in AWS Secrets Manager and SHALL be readable only by Result Broker.
- **COR-GOV-010:** PATs are excluded from v1.
- **COR-GOV-011:** Tombstone production SHALL be idempotent by `tombstoneEventId`.
- **COR-GOV-012:** Failure to produce a tombstone SHALL NOT restore validity; cleanup waits for a valid durable tombstone.

## Human and machine mutation

Humans review `master`. Machines may append or reconcile only schema-valid runtime records under admitted prefixes. Transformation of runtime evidence into approved portfolio meaning always crosses a reviewed `master` boundary.
