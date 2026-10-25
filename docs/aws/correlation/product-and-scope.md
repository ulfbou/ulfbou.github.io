# Product and scope

## Product intent

The Correlation Capability Prototype coordinates a browser-hosted portfolio interaction, bounded AWS execution, minimal active S3 state, one replaceable external capability, public-safe GitHub terminal records, and revision-bound cleanup.

- **COR-PROD-001:** The static portfolio SHALL remain useful with AWS and the provider disabled.
- **COR-PROD-002:** The prototype SHALL classify one reviewed public architecture scenario through one versioned provider adapter.
- **COR-PROD-003:** Provider output SHALL be presented only as provider-generated observation and candidate analysis, never accepted architectural truth.
- **COR-PROD-004:** S3 SHALL store only small, private, time-bounded active correlation state.
- **COR-PROD-005:** GitHub SHALL retain public-safe terminal tombstones and cleanup evidence.
- **COR-PROD-006:** `CorrelationId` SHALL be opaque and SHALL NOT authenticate a caller.

## Selected prototype capability

`architecture-scenario-classification-v1` evaluates the reviewed fixture `git-backed-correlation-cleanup` against a fixed architecture-question contract. The first adapter targets the Google Gemini API. The admitted deployment model is pinned in `providers-v1.json`; provider substitution is a reviewed contract change.

## Result target

The normalized result is returned synchronously to the browser during the active interaction. GitHub receives only the public-safe tombstone and cleanup evidence. S3 is not a result archive.

## Non-goals

No user accounts, visitor free text, callback provider, queue, workflow orchestrator, realtime channel, result archive, multi-provider failover, cross-repository correlation, production SLA, autonomous portfolio publication, or general-purpose correlation SDK is admitted in v1.

## Cost boundary

Required services are Lambda, API Gateway HTTP API, S3 Standard, IAM, CloudWatch, Secrets Manager, AWS Budgets, GitHub Actions, GitHub OIDC, and a GitHub App. DynamoDB, SQS, Step Functions, WebSocket API, Cognito, RDS, OpenSearch, provisioned concurrency, VPC attachment, and NAT Gateway are prohibited in v1. No guaranteed-zero-cost statement is permitted.
