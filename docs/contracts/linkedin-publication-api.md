# LinkedIn publication dry-run API contract

## Status and evidence classes

This is a Phase 0 candidate contract. Accepted architecture decisions, repository facts, collaboration-established account facts, current external platform facts, provisional implementation defaults, unresolved external evidence, and future implementation work are identified separately. Appearance in this document does not itself make a proposal accepted repository authority.

## Accepted boundary

The initial publication identity is the personal LinkedIn member **Ulf Bourelius**. The Worker controls that identity; callers cannot select an actor, author URN, organization, or publication identity. Organization publication is outside the current scope.

The first increment is dry-run only. It implements `GET /health` and `POST /v1/linkedin/publications/dry-run`. It contains no LinkedIn credential, OAuth flow, LinkedIn API request, live-publication endpoint, browser authorization, or arbitrary outbound proxy. Existing reader-initiated LinkedIn sharing and governed Open Graph/PNG previews remain unchanged.

## Endpoint and provisional limits

`POST /v1/linkedin/publications/dry-run` accepts JSON encoded as UTF-8. The maximum exact request body is provisionally **16 KiB**. Unknown fields, duplicate JSON keys, invalid UTF-8, non-object roots, unsupported content encoding, unsupported schema versions, and non-canonical value types are rejected. No browser CORS headers are emitted.

Processing order is fixed:

1. Reject an unsupported method, path, content type, or content encoding.
2. Enforce the exact-body size limit while reading bounded bytes.
3. Validate required authentication-header syntax, timestamp format and provisional ±300-second skew, request-ID syntax, signature-version syntax, and the exact-body SHA-256/HMAC.
4. If authentication fails, return a bounded response without parsing JSON or creating a ledger row.
5. Decode UTF-8 and parse JSON with duplicate-key rejection.
6. Validate the closed schema and field bounds.
7. Validate and normalize the canonical URL.
8. Derive transport and semantic identities.
9. Check the kill switch and D1 availability.
10. Mutate the ledger atomically. No external publication call exists in this increment.

D1 failure is fail-closed and returns `503 ledger_unavailable`. An authenticated request that cannot obtain durable ledger state is not treated as accepted.

## Request schema version 1

Required fields are:

- `schemaVersion`: `"1.0"`
- `channel`: `"linkedin"`
- `canonicalUrl`
- `articleRevision`
- `sourceRepository`: `"ulfbou/ulfbou.github.io"`
- `sourceCommit`: 40 lowercase hexadecimal characters
- `sourceWorkflowRunId`: decimal string
- `publicationIntent`: `"governed-article-publication"`

Optional bounded fields are `articleSourceId`, matching an existing governed article ID, and `requestedAt`, an RFC 3339 UTC timestamp for audit context.

The request carries references and bounded metadata only. It cannot contain an actor URN, arbitrary LinkedIn payload or API path, article body, article HTML, image bytes, OAuth material, access token, refresh token, or caller-selected publication commentary.

## Canonical URL

Parse as a URL and require:

- scheme `https`;
- exact host `ulfbou.github.io` after ASCII case normalization;
- no user information, explicit port, query, or fragment;
- an exact governed article route beneath `/articles/`.

Reject control characters, backslashes, duplicate separators, dot segments, malformed percent escapes, percent-encoded separators or dot segments, Unicode ambiguity, user-info tricks, and unexpected ports. Percent-decode only unreserved ASCII characters before comparison.

The Phase 0 API does **not** impose a new slug regular expression. The route must equal a canonical article URL emitted by the governed repository publisher. This avoids making the backend stricter than the publisher. A later implementation must derive focused route fixtures from actual generated article routes before code is accepted.

## Immutable article revision

For the current repository architecture, `articleRevision` is provisionally the lowercase 64-hex `contentSha256` emitted in `data/generated/articles.json`. It covers the governed article body according to the current producer, but Phase 0 does not assume it covers every publication-relevant field. If title, description, social image, or future publication commentary can change without changing `contentSha256`, duplicate prevention may treat materially different publication intent as the same revision.

A later versioned contract may introduce a broader publication digest covering all publication-relevant fields. Phase 0 records that path but does not invent or implement it. Repository commit SHA alone is not semantic publication identity.

## HMAC authentication

Required headers:

- `X-Publication-Timestamp`
- `X-Publication-Request-Id`
- `X-Publication-Signature`

Timestamp is decimal Unix seconds in ASCII. Accepted skew is provisionally ±300 seconds against Worker time. Request ID is a lowercase canonical UUID v4. Body hash is lowercase hexadecimal SHA-256 of the exact received bytes before decoding, parsing, or reserialization.

Canonical signing input is UTF-8 with LF separators and no final LF:

```text
POST
/v1/linkedin/publications/dry-run
<timestamp>
<request-id>
<sha256-of-exact-request-body>
```

`X-Publication-Signature` uses `v1=<lowercase-hex HMAC-SHA-256>`. Comparison is constant-time. Complete signatures, signing secrets, authorization headers, and tokens are never logged or persisted.

Secret rotation uses opaque admitted version identifiers mapped server-side to current and next slots. Unknown, disabled, or malformed versions receive the same bounded authentication failure as an invalid MAC. The Worker performs bounded work for admitted formats and does not disclose whether a named secret version exists. Overlap is explicitly time-bounded.

Clock-skew failures return one stable code, such as `request_timestamp_outside_window`, and may log only the request ID, direction (`past` or `future`), and a bounded skew bucket. Responses do not expose Worker time, raw calculated skew, signature details, or secret-version validity.

## Replay and idempotency

Transport identity is the request ID. After successful authentication and validation, reuse with the same endpoint and exact body hash returns the established compatible result. Reuse with a different body hash or endpoint returns `409 replay_or_idempotency_conflict`. Unauthenticated or invalidly signed requests never create ledger rows.

Semantic publication identity is SHA-256 over length-prefixed UTF-8 components: schema marker `linkedin-publication-key-v1`, channel, configured personal publication identity, normalized canonical URL, and immutable article revision. Canonical URL alone and commit SHA alone are insufficient.

Concurrent inserts rely on D1 unique constraints. A losing compatible request reads and returns the established record rather than processing again. A conflicting identity returns a bounded conflict.

## Responses and statuses

- `202 accepted_dry_run`: authenticated, valid, ledgered dry run.
- `200 duplicate`: compatible transport or publication identity already has a conclusive result.
- `400 invalid_request`
- `401 invalid_signature`
- `409 replay_or_idempotency_conflict`
- `413 body_too_large`
- `415 unsupported_media_type_or_encoding`
- `422 invalid_publication_intent`
- `423 publication_disabled`
- `503 ledger_unavailable`

Responses contain stable bounded codes and the request ID when safely available, never secrets, signatures, tokens, stack traces, full bodies, or arbitrary upstream payloads.

Ledger statuses are `accepted_dry_run`, `completed`, `rejected`, `failed`, `uncertain`, and `disabled`. `disabled` describes a durable, authenticated intent rejected by an enabled ledger audit policy. The initial operational policy is simpler: the kill switch returns `423` **without ledger mutation**, so metrics record disablement but no `disabled` row is created. If durable disabled auditing is later accepted, API, operations, migration, and threat-model contracts must change together. `uncertain` is terminal for automatic retry until reconciliation.

## D1 ledger contract

A migration-managed table stores bounded values for request ID, publication key, channel, configured publication identity, canonical URL, article revision, mode, status, request-body hash, source repository, source commit SHA, workflow run ID, creation/update timestamps, nullable future external post ID, and bounded error code. Unique constraints cover request ID and publication key.

Never persist signing secrets, complete signatures, GitHub tokens, LinkedIn tokens, article bodies, image bytes, or arbitrary upstream responses.

Dry-run ledger retention is provisionally 90 days. Deletion is reviewed. Future live identities, if separately admitted, retain enough identity to prevent duplication or require explicit operator override.

## Interoperability test vectors

The Worker-foundation stage must commit canonical HMAC vectors containing exact body bytes (including a non-ASCII UTF-8 case), body SHA-256, timestamp, UUID, canonical input bytes or escaped representation, secret identifier, test-only secret, expected HMAC, and negative variants for changed byte, path, timestamp, request ID, version, duplicate JSON key, and unsupported encoding. Production secrets must never appear in vectors.
