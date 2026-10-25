# GitHub OIDC for AWS access

**Status:** Accepted

## Context

Short-lived bound credentials reduce persistent secret exposure.

## Decision

GitHub Actions uses OIDC to assume exact deployment and cleanup roles; no long-lived AWS keys are stored in GitHub.

## Consequences

Trust policies bind repository, workflow/environment, branch, audience, and admitted operation.
