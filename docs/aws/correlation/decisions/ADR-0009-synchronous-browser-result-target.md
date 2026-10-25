# Synchronous browser result target

**Status:** Accepted

## Context

This is the smallest meaningful path demonstrating provider mediation and lifecycle.

## Decision

Return normalized result during the active browser interaction; store no result archive and add no realtime service.

## Consequences

Timeout is explicit; a new operation is required after terminal failure.
