# Provenance and evidence

## Identity domains

- **COR-PROV-001:** Git commit, workflow run, OIDC subject, assumed-role session, Lambda version, alias, invocation request ID, S3 key, S3 version identity, state revision, canonical state digest, manifest, and receipt are separate identities.
- **COR-PROV-002:** Deployment identity SHALL NOT be conflated with invocation identity.
- **COR-PROV-003:** Caller-provided claims SHALL be distinguished from independently observed values.

## Canonical active-state representation

`correlation-state-canonical-json-v1` is UTF-8 without BOM. The root is an object. Duplicate properties are rejected before domain validation. Properties are ordered by Unicode code point. Contract-order arrays preserve order; set-like arrays are sorted by their owning contract. No whitespace occurs outside strings. Non-ASCII characters are emitted directly. Control characters use shortest valid JSON escapes. No implicit Unicode normalization occurs.

Active state admits integers only. Plus signs, leading zeros, negative zero, fractions, exponents, and non-finite values are prohibited. Timestamps are UTC RFC 3339 with exactly millisecond precision: `YYYY-MM-DDTHH:MM:SS.fffZ`. Null is prohibited unless a schema explicitly admits it. Canonical bytes have no final newline.

The identity is `sha256:` followed by lowercase SHA-256 over the complete canonical UTF-8 bytes. S3 transport may have one final LF, but it is outside the digest domain. Readers reparse, canonicalize, and verify the digest before use.

## Provenance chain

```text
protected master commit
-> workflow run and OIDC subject
-> AWS assumed-role session
-> qualified Lambda alias and version
-> invocation request ID
-> derived S3 key and observed version identity
-> canonical active-state digest and revision
-> runtime tombstone path and blob digest
-> deletion manifest commit, path, and digest
-> cleanup receipt and observed outcome
-> reconciled status or unresolved conflict
-> transformed public replay
-> optional reviewed portfolio claim
```

- **COR-PROV-004:** A manifest SHALL bind repository, runtime branch, full commit, tombstone path, tombstone SHA-256, policy, creation, expiry, and expected active-state identity.
- **COR-PROV-005:** A receipt SHALL bind one outcome to every requested entry and SHALL reject unknown entries.
- **COR-PROV-006:** Evidence supports conclusions but does not create approved portfolio meaning.

## Traceability record

Every normative requirement maps to one owning document, architecture component, schema or registry, implementation boundary, test, DevOps gate, runtime evidence, and optional public claim. Contradictions fail validation; duplicate ownership is rejected.
