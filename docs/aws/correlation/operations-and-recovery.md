# Operations and recovery

## Limits

```text
active lifetime: 15 minutes
active object: 16 KiB
browser request: 8 KiB
normalized browser response: 16 KiB
provider attempts: 2 maximum
Request Broker: 15 s, concurrency 2
Result Broker: 10 s, concurrency 2
Cleanup: 30 s, concurrency 1
manifest: 25 entries, 128 KiB invocation
HTTP API: 1 request/s, burst 2
```

- **COR-OPS-001:** Limits are enforced before parsing or expensive work.
- **COR-OPS-002:** Provider quota exhaustion is non-retryable within the same operation and fails closed.
- **COR-OPS-003:** Cleanup delay affects storage hygiene only, never terminal validity.

## Telemetry

Allowed structured fields are `correlationId`, `operationId`, `capability`, `providerAlias`, `previousState`, `nextState`, `stateRevision`, `attempt`, `outcome`, `failureClass`, `durationMs`, `requestBytes`, `responseBytes`, `expired`, and `cancelled`.

Credentials, signatures, callback secrets, complete prompts or provider responses, personal information, raw S3 objects, private GitHub data, and unrestricted exception dumps are prohibited. Operational logs use short retention and are not portfolio evidence.

## Alerts

Alarms cover Lambda errors, throttles, duration near timeout, API 4xx/5xx anomalies, repeated provider failures, tombstone-dispatch failure, unresolved cleanup conflicts, cleanup temporary failures, and budget thresholds. Budget notification does not claim to prevent charges.

## Kill switches

1. Set Request Broker reserved concurrency to zero.
2. Disable the HTTP API stage.
3. Disable the provider in deployment configuration.
4. Withhold protected-environment deployment approval.
5. Set Cleanup concurrency to zero during incident containment without changing terminal validity.

## Recovery

A failed provider call advances according to its classified outcome and does not leak raw response. Tombstone dispatch retries the identical idempotent event. Cleanup `AlreadyAbsent` is successful evidence, not proof of an earlier exact deletion. Stale state becomes unresolved conflict and is never deleted. Failed GitHub reconciliation resumes from the validated receipt. Branch races require a fresh compile against the new tip; no blind integration occurs. Alias rollback returns to a previously verified immutable version.
