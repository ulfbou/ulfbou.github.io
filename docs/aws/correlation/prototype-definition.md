# Prototype definition

## Name and use case

**Correlation Capability Prototype** lets a visitor select one reviewed public architecture scenario. Request Broker creates a correlation and asks one admitted provider to classify the scenario against a small versioned question contract. Result Broker validates and normalizes the response. The browser presents it as provider-generated candidate analysis.

## Reviewed scenario

```json
{
  "schemaVersion": "architecture-scenario-v1",
  "scenarioId": "git-backed-correlation-cleanup",
  "title": "Git-governed terminal cleanup",
  "facts": [
    "Active correlation state is stored in S3.",
    "Terminal decisions are stored as public-safe GitHub tombstones.",
    "GitHub Actions invokes Cleanup Lambda through OIDC.",
    "Cleanup Lambda conditionally deletes an expected state revision."
  ],
  "questions": [
    "Is the authority for terminal decisions explicit?",
    "Can a stale tombstone delete newer active state?",
    "Does the portfolio remain usable when AWS is unavailable?"
  ]
}
```

## Normal path

Browser loads the static fixture, submits bounded public content, Request Broker creates revision 1 and dispatches the provider, Result Broker validates and normalizes the response, conditionally advances to `Completed`, and returns the normalized result. A GitHub workflow produces a public-safe tombstone. Later cleanup compiles a manifest, invokes Cleanup through OIDC, conditionally deletes exact state, reconciles the receipt, and produces a sanitized replay.

## Cancellation path

`AwaitingProvider -> Cancelled` is terminal. A later result is rejected, a cancellation tombstone is produced, and exact active state is eventually deleted. Cancellation authority does not depend on cleanup timing.

## Required failures

- provider unavailable;
- quota exhausted;
- invalid provider response;
- duplicate result;
- result after cancellation;
- expired correlation;
- active state already absent;
- stale tombstone against newer revision;
- deletion succeeds but first reconciliation fails;
- cleanup schedule delayed;
- AWS disabled while static content remains available.

- **COR-PROT-001:** Each path SHALL have deterministic acceptance evidence.
- **COR-PROT-002:** No path may imply provider authority or exactly-once processing.
