from __future__ import annotations

import asyncio
import json
import sys
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parents[1] / "src"))

from auth import canonical_input, sign
from contract import ContractError, parse_intent
from ledger import MemoryLedger
from service import handle_dry_run

SECRET = b"test-only-signing-secret"
TIMESTAMP = "1795430400"
REQUEST_ID = "123e4567-e89b-42d3-a456-426614174000"
URLS = frozenset({"https://ulfbou.github.io/articles/bias/"})


def body(**overrides):
    value = {
        "schemaVersion": "1.0",
        "channel": "linkedin",
        "canonicalUrl": "https://ulfbou.github.io/articles/bias/",
        "articleRevision": "a" * 64,
        "sourceRepository": "ulfbou/ulfbou.github.io",
        "sourceCommit": "b" * 40,
        "sourceWorkflowRunId": "123456789",
        "publicationIntent": "governed-article-publication",
        "articleSourceId": "bias",
        "requestedAt": "2026-11-23T12:00:00Z",
    }
    value.update(overrides)
    return json.dumps(value, ensure_ascii=False, separators=(",", ":")).encode()


def headers(raw):
    return {
        "x-publication-timestamp": TIMESTAMP,
        "x-publication-request-id": REQUEST_ID,
        "x-publication-signature": sign(SECRET, TIMESTAMP, REQUEST_ID, raw),
    }


class FoundationTests(unittest.TestCase):
    def test_canonical_vector(self):
        raw = body(articleSourceId="räka")
        signature = sign(SECRET, TIMESTAMP, REQUEST_ID, raw)
        self.assertTrue(signature.startswith("v1="))
        self.assertEqual(67, len(signature))
        self.assertEqual(5, len(canonical_input(TIMESTAMP, REQUEST_ID, "0" * 64).split(b"\n")))

    def test_duplicate_json_key_rejected(self):
        raw = b'{"schemaVersion":"1.0","schemaVersion":"1.0"}'
        with self.assertRaises(ContractError):
            parse_intent(raw, URLS)

    def test_unadmitted_url_rejected(self):
        with self.assertRaises(ContractError):
            parse_intent(body(canonicalUrl="https://ulfbou.github.io/articles/other/"), URLS)

    def test_kill_switch_precedes_ledger(self):
        raw = body()
        response = asyncio.run(handle_dry_run(
            body=raw, headers=headers(raw), secret=SECRET, ledger=MemoryLedger(),
            enabled=False, admitted_urls=URLS, identity="personal-member:ulf-bourelius",
            now=int(TIMESTAMP),
        ))
        self.assertEqual((423, "publication_disabled"), (response.status, response.code))

    def test_accept_duplicate_and_conflict(self):
        ledger = MemoryLedger()
        raw = body()
        first = asyncio.run(handle_dry_run(
            body=raw, headers=headers(raw), secret=SECRET, ledger=ledger,
            enabled=True, admitted_urls=URLS, identity="personal-member:ulf-bourelius",
            now=int(TIMESTAMP),
        ))
        duplicate = asyncio.run(handle_dry_run(
            body=raw, headers=headers(raw), secret=SECRET, ledger=ledger,
            enabled=True, admitted_urls=URLS, identity="personal-member:ulf-bourelius",
            now=int(TIMESTAMP),
        ))
        changed = body(sourceWorkflowRunId="987654321")
        conflict = asyncio.run(handle_dry_run(
            body=changed, headers=headers(changed), secret=SECRET, ledger=ledger,
            enabled=True, admitted_urls=URLS, identity="personal-member:ulf-bourelius",
            now=int(TIMESTAMP),
        ))
        self.assertEqual((202, "accepted_dry_run"), (first.status, first.code))
        self.assertEqual((200, "duplicate"), (duplicate.status, duplicate.code))
        self.assertEqual((409, "replay_or_idempotency_conflict"), (conflict.status, conflict.code))

    def test_changed_body_invalidates_signature(self):
        raw = body()
        changed = raw + b" "
        response = asyncio.run(handle_dry_run(
            body=changed, headers=headers(raw), secret=SECRET, ledger=MemoryLedger(),
            enabled=True, admitted_urls=URLS, identity="personal-member:ulf-bourelius",
            now=int(TIMESTAMP),
        ))
        self.assertEqual((401, "invalid_signature"), (response.status, response.code))


if __name__ == "__main__":
    unittest.main()

class InteroperabilityVectorTests(unittest.TestCase):
    def test_committed_hmac_vector(self):
        import hashlib
        fixture = json.loads((Path(__file__).parents[1] / "fixtures" / "hmac-v1.json").read_text(encoding="utf-8"))
        raw = fixture["exactBodyUtf8"].encode("utf-8")
        self.assertEqual(fixture["bodySha256"], hashlib.sha256(raw).hexdigest())
        self.assertEqual(
            fixture["expectedSignature"],
            sign(fixture["testOnlySecret"].encode(), fixture["timestamp"], fixture["requestId"], raw),
        )
        self.assertEqual(
            fixture["canonicalInputUtf8"].encode("utf-8"),
            canonical_input(fixture["timestamp"], fixture["requestId"], fixture["bodySha256"]),
        )
        non_ascii = fixture["nonAsciiExactBodyUtf8"].encode("utf-8")
        self.assertEqual(fixture["nonAsciiBodySha256"], hashlib.sha256(non_ascii).hexdigest())

    def test_oversized_body_rejected_before_authentication(self):
        response = asyncio.run(handle_dry_run(
            body=b"x" * (16 * 1024 + 1), headers={}, secret=SECRET,
            ledger=MemoryLedger(), enabled=True, admitted_urls=URLS,
            identity="personal-member:ulf-bourelius", now=int(TIMESTAMP),
        ))
        self.assertEqual((413, "body_too_large"), (response.status, response.code))

    def test_timestamp_outside_window_rejected(self):
        raw = body()
        response = asyncio.run(handle_dry_run(
            body=raw, headers=headers(raw), secret=SECRET, ledger=MemoryLedger(),
            enabled=True, admitted_urls=URLS, identity="personal-member:ulf-bourelius",
            now=int(TIMESTAMP) + 301,
        ))
        self.assertEqual((401, "request_timestamp_outside_window"), (response.status, response.code))
