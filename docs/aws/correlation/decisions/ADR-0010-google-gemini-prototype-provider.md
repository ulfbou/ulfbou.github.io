# Google Gemini prototype provider

**Status:** Accepted

## Context

It supplies the needed bounded external capability while remaining replaceable.

## Decision

Use one Gemini adapter for reviewed public scenario classification, behind versioned provider contracts.

## Consequences

Deployment pins the model; quota exhaustion fails closed; no sensitive or visitor-authored data is sent.
