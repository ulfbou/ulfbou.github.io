# Security and threat model

## Trust boundaries

The browser is untrusted and credential-free. API ingress is public and bounded. AWS execution roles are distinct. S3 active state is private. The provider is untrusted for authority and output validity. GitHub `master` is reviewed authority. `correlation-runtime` is untrusted machine data until schema and provenance validation. Job handoffs are untrusted until size and digest verification.

## Public classification

Permanently public tombstone fields are restricted to the tombstone schema. Permanently public receipt fields are restricted to the receipt schema. Provider outcome, lifecycle replay, presentation timestamps, and diagnostics require transformation. Raw requests/responses, active S3 state, quotas, logs, role sessions, account topology, installation identifiers, and full exceptions are private operational data. Credentials, tokens, signatures, and authorization headers are secret.

Personal information, visitor free text, raw prompts, raw provider responses, arbitrary URLs, arbitrary S3 keys, private repository data, and data expected to be erased later are prohibited from public runtime records.

## Security requirements

- **COR-SEC-001:** No Actions job may hold both `contents: write` and `id-token: write`.
- **COR-SEC-002:** Runtime-branch content may not modify or execute workflow code.
- **COR-SEC-003:** Every action reference SHALL be a full commit SHA.
- **COR-SEC-004:** Privileged jobs SHALL NOT install uncontrolled network dependencies or use unnecessary caches.
- **COR-SEC-005:** Handoffs SHALL be byte-bounded and SHA-256 verified before parsing.
- **COR-SEC-006:** Manifest and receipt schemas SHALL be strict and bounded.
- **COR-SEC-007:** Tombstones cannot select arbitrary S3 keys.
- **COR-SEC-008:** A stale revision or digest cannot delete newer active state.
- **COR-SEC-009:** Provider output cannot write approved portfolio content.
- **COR-SEC-010:** Correlation identity cannot authenticate a caller.
- **COR-SEC-011:** Public records contain no data expected to be erased.
- **COR-SEC-012:** Provider quota exhaustion fails closed without paid or provider failover.
- **COR-SEC-013:** Delayed cleanup cannot restore validity.
- **COR-SEC-014:** Repository races fail safely without blind rebase or force push.
- **COR-SEC-015:** AWS roles are distinct and least privileged.
- **COR-SEC-016:** Logs, responses, dispatches, manifests, and receipts are bounded exfiltration channels.
- **COR-SEC-017:** Static portfolio operation is testable with AWS disabled.

## Threats and controls

Replay is controlled by operation identity, exact revision, digest, idempotent event identity, and terminal immutability. Confused deputy risk is controlled by derived keys, qualified aliases, path scopes, and separate roles. Injection is controlled by strict schemas and non-execution of runtime data. Data leakage is controlled by closed public schemas, transformations, byte limits, and prohibited fields. Cost abuse is controlled by HTTP throttling, low concurrency, bounded attempts/runtime, budget alerts, and kill switches. Supply-chain risk is controlled by full-SHA actions and no runtime package installation in privileged jobs.
