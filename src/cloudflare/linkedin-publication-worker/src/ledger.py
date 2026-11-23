from __future__ import annotations

from dataclasses import dataclass


@dataclass(frozen=True)
class LedgerRecord:
    request_id: str
    endpoint: str
    body_sha256: str
    publication_key: str
    status: str


@dataclass(frozen=True)
class LedgerDecision:
    kind: str
    record: LedgerRecord


class LedgerUnavailable(RuntimeError):
    pass


class MemoryLedger:
    def __init__(self) -> None:
        self._requests: dict[str, LedgerRecord] = {}
        self._publications: dict[str, LedgerRecord] = {}

    async def accept(self, record: LedgerRecord) -> LedgerDecision:
        prior_request = self._requests.get(record.request_id)
        if prior_request:
            if prior_request.endpoint == record.endpoint and prior_request.body_sha256 == record.body_sha256:
                return LedgerDecision("duplicate", prior_request)
            return LedgerDecision("conflict", prior_request)
        prior_publication = self._publications.get(record.publication_key)
        if prior_publication:
            self._requests[record.request_id] = prior_publication
            return LedgerDecision("duplicate", prior_publication)
        self._requests[record.request_id] = record
        self._publications[record.publication_key] = record
        return LedgerDecision("accepted", record)


class UnavailableLedger:
    async def accept(self, record: LedgerRecord) -> LedgerDecision:
        raise LedgerUnavailable("ledger unavailable")
