# Canonical correlation state identity

**Status:** Accepted

## Context

Deletion requires a language-independent exact identity, not ambiguous object equivalence.

## Decision

Use `correlation-state-canonical-json-v1` and SHA-256 over exact canonical UTF-8 bytes without final newline.

## Consequences

Golden vectors and duplicate-property rejection are release requirements.
