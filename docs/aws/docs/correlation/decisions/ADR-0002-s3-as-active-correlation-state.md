# S3 as active correlation state

**Status:** Accepted

## Context

S3 supplies low-cost bounded object storage without becoming an application database.

## Decision

Use one private versioned S3 Standard bucket only for bounded active state under derived `active/v1/` keys.

## Consequences

Conditional protocols, strict size limits, lifecycle cleanup, and distinct roles are mandatory.
