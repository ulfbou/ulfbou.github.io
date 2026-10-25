# Public runtime record classification

**Status:** Accepted

## Context

Public Git history is permanent and incompatible with later erasure expectations.

## Decision

Public runtime records use closed schemas; all other data is transformed, private, secret, or prohibited.

## Consequences

Unknown fields fail; raw prompts, provider responses, PII, credentials, and arbitrary paths are prohibited.
