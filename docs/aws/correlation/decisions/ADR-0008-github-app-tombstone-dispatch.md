# GitHub App tombstone dispatch

**Status:** Accepted

## Context

The App provides replaceable narrow installation identity without placing repository credentials in browser or state.

## Decision

Result Broker uses a repository-scoped GitHub App to send a bounded dispatch; protected workflow code constructs the tombstone. PATs are excluded.

## Consequences

App key is in Secrets Manager; failures never restore validity; event identity is idempotent.
