# Development specification

## Placement

The implementation remains an isolated subsystem in `ulfbou.github.io`:

```text
src/correlation/{contracts,domain,application,aws,providers,public_projection}
tests/correlation/{unit,contracts,workflows,infrastructure,integration,security}
infrastructure/correlation/
tools/correlation/
correlation/{schemas,registries,fixtures}
```

The existing static portfolio projects do not reference AWS SDK packages. Static compilation requires no AWS, provider, runtime branch, or network access. The domain layer does not depend on AWS SDK types.

## First boundary

- **COR-DEV-001:** First implementation covers canonical state identity, state-machine domain model, strict schemas, registries, fixtures, and focused tests only.
- **COR-DEV-002:** No live AWS, provider, GitHub mutation, or deployment is admitted in the first increment.
- **COR-DEV-003:** Golden canonical bytes and SHA-256 vectors SHALL be cross-runtime compatible.
- **COR-DEV-004:** Duplicate properties, unknown properties, invalid values, oversized bytes, stale revision, stale digest, terminal mutation, late result, and invalid registry references SHALL fail deterministically.

## Canonical validation sequence

1. formatting and UTF-8/LF policy;
2. compilation;
3. schema validation;
4. registry consistency;
5. valid fixture acceptance;
6. invalid fixture rejection;
7. focused domain tests;
8. state-machine completeness;
9. workflow security policy;
10. IAM policy validation;
11. infrastructure synthesis/validation;
12. repository mutation scope;
13. prototype acceptance scenarios.

Unknown mandatory results fail. The validator produces machine-readable evidence bound to exact commit and tree identities.

## Test obligations

Tests cover every state and transition, revision increments, idempotency, retry safety, terminal immutability, cancellation/late result, expiry, provider failure classes, key derivation, create-only state, conditional advancement, canonical digest verification, stale deletion prevention, manifest/receipt bijection, summary counts, branch-race safety, action pinning, privilege separation, IAM scope, disabled dynamic capability, and public replay sanitization.
