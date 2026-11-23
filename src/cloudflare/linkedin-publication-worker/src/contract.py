from __future__ import annotations

import hashlib
import json
import re
from dataclasses import dataclass
from datetime import datetime
from urllib.parse import unquote, urlsplit

MAX_BODY_BYTES = 16 * 1024
_ALLOWED_KEYS = {
    "schemaVersion", "channel", "canonicalUrl", "articleRevision",
    "sourceRepository", "sourceCommit", "sourceWorkflowRunId",
    "publicationIntent", "articleSourceId", "requestedAt",
}
_REQUIRED_KEYS = _ALLOWED_KEYS - {"articleSourceId", "requestedAt"}
_SHA256_RE = re.compile(r"^[0-9a-f]{64}$")
_COMMIT_RE = re.compile(r"^[0-9a-f]{40}$")
_RUN_ID_RE = re.compile(r"^[0-9]+$")
_SOURCE_ID_RE = re.compile(r"^[a-z0-9][a-z0-9-]{0,127}$")
_BAD_PERCENT_RE = re.compile(r"%(?![0-9A-Fa-f]{2})")


class ContractError(ValueError):
    def __init__(self, code: str):
        super().__init__(code)
        self.code = code


@dataclass(frozen=True)
class PublicationIntent:
    canonical_url: str
    article_revision: str
    source_repository: str
    source_commit: str
    workflow_run_id: str
    article_source_id: str | None


def _pairs_no_duplicates(pairs: list[tuple[str, object]]) -> dict[str, object]:
    result: dict[str, object] = {}
    for key, value in pairs:
        if key in result:
            raise ContractError("invalid_request")
        result[key] = value
    return result


def _require_string(data: dict[str, object], key: str, max_length: int) -> str:
    value = data.get(key)
    if not isinstance(value, str) or not value or len(value) > max_length:
        raise ContractError("invalid_publication_intent")
    return value


def normalize_canonical_url(value: str, admitted_urls: frozenset[str]) -> str:
    if any(ord(char) < 32 or char == "\\" for char in value):
        raise ContractError("invalid_publication_intent")
    if _BAD_PERCENT_RE.search(value):
        raise ContractError("invalid_publication_intent")
    parts = urlsplit(value)
    if parts.scheme.lower() != "https" or parts.hostname != "ulfbou.github.io":
        raise ContractError("invalid_publication_intent")
    if parts.username or parts.password or parts.port or parts.query or parts.fragment:
        raise ContractError("invalid_publication_intent")
    if "//" in parts.path or "/./" in parts.path or "/../" in parts.path:
        raise ContractError("invalid_publication_intent")
    decoded = unquote(parts.path)
    if decoded != parts.path or not decoded.startswith("/articles/"):
        raise ContractError("invalid_publication_intent")
    normalized = f"https://ulfbou.github.io{parts.path}"
    if normalized not in admitted_urls:
        raise ContractError("invalid_publication_intent")
    return normalized


def parse_intent(body: bytes, admitted_urls: frozenset[str]) -> PublicationIntent:
    try:
        text = body.decode("utf-8")
        data = json.loads(text, object_pairs_hook=_pairs_no_duplicates)
    except (UnicodeDecodeError, json.JSONDecodeError, ContractError):
        raise ContractError("invalid_request") from None
    if not isinstance(data, dict) or set(data) - _ALLOWED_KEYS or not _REQUIRED_KEYS <= set(data):
        raise ContractError("invalid_request")
    if data.get("schemaVersion") != "1.0" or data.get("channel") != "linkedin":
        raise ContractError("invalid_publication_intent")
    if data.get("sourceRepository") != "ulfbou/ulfbou.github.io":
        raise ContractError("invalid_publication_intent")
    if data.get("publicationIntent") != "governed-article-publication":
        raise ContractError("invalid_publication_intent")
    canonical_url = normalize_canonical_url(
        _require_string(data, "canonicalUrl", 512), admitted_urls
    )
    revision = _require_string(data, "articleRevision", 64)
    commit = _require_string(data, "sourceCommit", 40)
    run_id = _require_string(data, "sourceWorkflowRunId", 32)
    if not _SHA256_RE.fullmatch(revision) or not _COMMIT_RE.fullmatch(commit) or not _RUN_ID_RE.fullmatch(run_id):
        raise ContractError("invalid_publication_intent")
    source_id = data.get("articleSourceId")
    if source_id is not None and (not isinstance(source_id, str) or not _SOURCE_ID_RE.fullmatch(source_id)):
        raise ContractError("invalid_publication_intent")
    requested_at = data.get("requestedAt")
    if requested_at is not None:
        if not isinstance(requested_at, str) or not requested_at.endswith("Z"):
            raise ContractError("invalid_publication_intent")
        try:
            datetime.fromisoformat(requested_at[:-1] + "+00:00")
        except ValueError:
            raise ContractError("invalid_publication_intent") from None
    return PublicationIntent(canonical_url, revision, "ulfbou/ulfbou.github.io", commit, run_id, source_id)


def publication_key(intent: PublicationIntent, identity: str) -> str:
    components = [
        "linkedin-publication-key-v1", "linkedin", identity,
        intent.canonical_url, intent.article_revision,
    ]
    framed = b"".join(len(part.encode()).to_bytes(4, "big") + part.encode() for part in components)
    return hashlib.sha256(framed).hexdigest()
