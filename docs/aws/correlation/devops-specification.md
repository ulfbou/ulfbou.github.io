# DevOps specification

## correlation-validate

Triggered by pull requests and manual dispatch. It has `contents: read`, no OIDC, no repository write, and no secrets. It runs the canonical validation sequence and retains exact-candidate validation evidence.

## correlation-deploy

Triggered manually from protected `master` after validation. It has `contents: read` and `id-token: write`, no repository write, uses a protected environment and exact deployment role, validates infrastructure, deploys immutable versions, moves production aliases, performs smoke verification, and rolls aliases back on failure. It uses no long-lived AWS key.

## correlation-cleanup

```text
Compile:   contents: read; no OIDC; no mutation
Delete:    contents: read; id-token: write; no repository write
Reconcile: contents: write; no OIDC; no AWS authority
```

- **COR-DEVOPS-001:** No job may have both repository-write and AWS OIDC authority.
- **COR-DEVOPS-002:** Compile checks out exact `correlation-runtime`, validates eligible tombstones, and emits a bounded manifest plus byte count and SHA-256.
- **COR-DEVOPS-003:** Delete verifies the handoff before assuming AWS authority, invokes only the qualified Cleanup alias, validates the bounded receipt, and emits receipt bytes plus digest.
- **COR-DEVOPS-004:** Reconcile verifies the receipt handoff, compares the exact runtime tip, writes only admitted paths, and fails safely on races.
- **COR-DEVOPS-005:** Failed reconciliation resumes from the retained receipt without repeating successful deletion.

## correlation-live-smoke

Triggered after deployment and manually. It uses a protected environment, creates one bounded synthetic correlation using the reviewed fixture, verifies public-safe outputs, and cleans it through the normal mechanism. It cannot approve portfolio content.

## Common workflow controls

All actions are pinned to full commit SHAs. Privileged jobs install no network dependencies and use no unnecessary caches. Artifacts have explicit maximum bytes and SHA-256 binding. Runtime data is never sourced, executed, interpolated into shell code, or used to choose arbitrary paths. Failure classes are `Validation`, `Identity`, `Authorization`, `Conflict`, `TransientExternal`, `PermanentExternal`, `CostBoundary`, and `Internal`.
