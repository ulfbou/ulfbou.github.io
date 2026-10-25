# Separated workflow privilege domains

**Status:** Accepted

## Context

Compromise of AWS authority must not directly grant repository mutation and vice versa.

## Decision

Compile, Delete, and Reconcile are distinct jobs; none holds both `contents: write` and `id-token: write`.

## Consequences

Handoffs are bounded and digest-bound; reconciliation can resume without repeating deletion.
