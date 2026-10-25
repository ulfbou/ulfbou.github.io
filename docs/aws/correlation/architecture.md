# Architecture

## Context

```text
GitHub Pages browser
  -> API Gateway HTTP API
  -> Request Broker Lambda
  -> Result Broker Lambda
  -> admitted provider
  -> browser normalized result

Request/Result Brokers <-> S3 private active state
Result Broker -> GitHub App dispatch -> protected workflow -> runtime tombstone
GitHub cleanup Compile -> Delete through OIDC -> Cleanup Lambda -> S3
GitHub cleanup Reconcile -> runtime receipt/status/conflict
```

## AWS resources

The bounded environment contains an HTTP API, Request Broker Lambda, Result Broker Lambda, Cleanup Lambda, one S3 active-state bucket, distinct execution roles, a GitHub cleanup-invocation role, a GitHub deployment role, GitHub OIDC provider, Secrets Manager secrets, CloudWatch log groups and alarms, an SNS budget target, an AWS Budget, published Lambda versions, and production aliases.

### Request Broker

- **Capability:** validate request, create correlation, dispatch provider operation.
- **Trigger:** `POST /correlations`; cancellation is `POST /correlations/{correlationId}/cancel`.
- **Input/output:** strict bounded browser contracts; no arbitrary prompt.
- **Memory/timeout/concurrency:** 256 MiB, 15 seconds, reserved concurrency 2.
- **Role:** create and conditionally advance active state; read provider secret; invoke qualified Result Broker alias; write bounded logs.
- **Prohibited:** delete S3, invoke Cleanup, modify GitHub, read GitHub App key, alter infrastructure.

### Result Broker

- **Capability:** validate provider response, normalize outcome, establish terminal state, dispatch tombstone event.
- **Trigger:** synchronous qualified invocation from Request Broker.
- **Memory/timeout/concurrency:** 256 MiB, 10 seconds, reserved concurrency 2.
- **Role:** read and conditionally advance exact active state; read GitHub App key; emit bounded dispatch; write bounded logs.
- **Prohibited:** delete active state, directly mutate GitHub contents, invoke Cleanup, alter infrastructure.

### Cleanup Lambda

- **Capability:** conditionally delete tombstone-authorized exact active revisions.
- **Trigger:** synchronous invocation of the production alias by the GitHub cleanup role.
- **Memory/timeout/concurrency:** 128 MiB, 30 seconds, reserved concurrency 1.
- **Limits:** 25 manifest entries; 128 KiB invocation body.
- **Role:** `s3:GetObject`, `s3:GetObjectVersion`, and exact conditional deletion under `active/v1/` only; bounded logs.
- **Prohibited:** caller-provided keys, provider/GitHub secrets, GitHub mutation, active-state writes, infrastructure mutation.

## HTTP ingress

API Gateway HTTP API owns CORS allow-list, admitted routes and methods, request-size checks, access logging, throttling at one request/second with burst two, and a future-safe boundary between request and cancellation. No result lookup endpoint is admitted.

## S3 active state

- **COR-ARCH-001:** Key derivation is `active/v1/{first-two-lowercase-correlation-id-characters}/{correlationId}.json`.
- **COR-ARCH-002:** Callers SHALL NOT provide an S3 key.
- **COR-ARCH-003:** Maximum transport object size is 16 KiB.
- **COR-ARCH-004:** Bucket public access is fully blocked; bucket-owner enforced ownership is used; TLS and server-side encryption are mandatory.
- **COR-ARCH-005:** Versioning is enabled; noncurrent versions and delete markers receive bounded lifecycle expiry.
- **COR-ARCH-006:** Create is conditional on absence. Advance is conditional on exact observed version identity and exact canonical state identity.
- **COR-ARCH-007:** Delete derives the key, performs a bounded read, validates schema and digest, compares correlation/revision/digest, and conditionally deletes the observed version.
- **COR-ARCH-008:** Missing, malformed, oversized, digest-invalid, or conflicting state fails closed with a distinct outcome.

## State model

```text
Created -> Dispatching -> AwaitingProvider -> ResultReceived -> Completed
AwaitingProvider -> Cancelled
nonterminal -> Expired
admitted terminal failures -> FailedTerminal
invalid exact state -> Invalid
replaced exact revision -> Superseded
```

Every successful transition increments a positive integer revision by exactly one. Terminal states have no outgoing transitions. Retry, duplication, delay, and partial failure are expected; exactly-once is not claimed.

## IAM

Roles are distinct. GitHub cleanup may invoke only the qualified Cleanup production alias and cannot read S3, secrets, IAM, or Lambda configuration. Cleanup accesses only derived active-state objects. Request and Result Brokers receive only their declared capabilities. Deployment uses a separate protected-environment OIDC role and no long-lived AWS key.
