from __future__ import annotations

from dataclasses import dataclass

from auth import authenticate
from contract import ContractError, MAX_BODY_BYTES, parse_intent, publication_key
from ledger import LedgerRecord, LedgerUnavailable


@dataclass(frozen=True)
class ServiceResponse:
    status: int
    code: str
    request_id: str | None = None

    def payload(self) -> dict[str, str]:
        value = {"code": self.code}
        if self.request_id:
            value["requestId"] = self.request_id
        return value


async def handle_dry_run(
    *, body: bytes, headers: dict[str, str], secret: bytes, ledger,
    enabled: bool, admitted_urls: frozenset[str], identity: str,
    now: int | None = None,
) -> ServiceResponse:
    if len(body) > MAX_BODY_BYTES:
        return ServiceResponse(413, "body_too_large")
    auth = authenticate(
        body=body,
        timestamp=headers.get("x-publication-timestamp"),
        request_id=headers.get("x-publication-request-id"),
        signature=headers.get("x-publication-signature"),
        secret=secret,
        now=now,
    )
    if not auth.ok:
        return ServiceResponse(401, auth.code, auth.request_id)
    try:
        intent = parse_intent(body, admitted_urls)
    except ContractError as error:
        status = 400 if error.code == "invalid_request" else 422
        return ServiceResponse(status, error.code, auth.request_id)
    if not enabled:
        return ServiceResponse(423, "publication_disabled", auth.request_id)
    record = LedgerRecord(
        request_id=auth.request_id or "",
        endpoint="/v1/linkedin/publications/dry-run",
        body_sha256=auth.body_sha256 or "",
        publication_key=publication_key(intent, identity),
        status="accepted_dry_run",
    )
    try:
        decision = await ledger.accept(record)
    except LedgerUnavailable:
        return ServiceResponse(503, "ledger_unavailable", auth.request_id)
    if decision.kind == "conflict":
        return ServiceResponse(409, "replay_or_idempotency_conflict", auth.request_id)
    if decision.kind == "duplicate":
        return ServiceResponse(200, "duplicate", auth.request_id)
    return ServiceResponse(202, "accepted_dry_run", auth.request_id)
