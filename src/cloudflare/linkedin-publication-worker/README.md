# LinkedIn publication Worker foundation

Dry-run-only Cloudflare Python Worker for governed article-publication intent. It implements:

- `GET /health`;
- `POST /v1/linkedin/publications/dry-run`;
- exact-byte HMAC-SHA-256 authentication;
- closed request-schema and canonical article URL validation;
- kill-switch behavior;
- transport and semantic idempotency through a ledger abstraction;
- a local in-memory ledger for tests and local development.

It contains no LinkedIn credential, OAuth flow, LinkedIn API call, live-publication endpoint, arbitrary proxy, D1 provisioning, route provisioning, or deployment automation. Preview and Production fail closed with `503 ledger_unavailable` until a separately reviewed D1 adapter and binding are implemented. `PUBLICATION_ENABLED` defaults to false.

## Local validation

```bash
uv sync
uv run python -m unittest discover -s tests -v
```

## Local Worker

```bash
uv run pywrangler dev
```

Use only test secrets locally. Do not provision or deploy from this increment.
