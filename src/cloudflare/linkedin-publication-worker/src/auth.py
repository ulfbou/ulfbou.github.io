from __future__ import annotations

import hashlib
import hmac
import re
import time
import uuid
from dataclasses import dataclass

_SIGNATURE_RE = re.compile(r"^v1=([0-9a-f]{64})$")


@dataclass(frozen=True)
class AuthResult:
    ok: bool
    code: str
    request_id: str | None = None
    body_sha256: str | None = None


def canonical_input(timestamp: str, request_id: str, body_sha256: str) -> bytes:
    return (
        "POST\n/v1/linkedin/publications/dry-run\n"
        f"{timestamp}\n{request_id}\n{body_sha256}"
    ).encode("utf-8")


def sign(secret: bytes, timestamp: str, request_id: str, body: bytes) -> str:
    body_sha256 = hashlib.sha256(body).hexdigest()
    digest = hmac.new(secret, canonical_input(timestamp, request_id, body_sha256), hashlib.sha256)
    return f"v1={digest.hexdigest()}"


def authenticate(
    *,
    body: bytes,
    timestamp: str | None,
    request_id: str | None,
    signature: str | None,
    secret: bytes,
    now: int | None = None,
    skew_seconds: int = 300,
) -> AuthResult:
    if not timestamp or not timestamp.isascii() or not timestamp.isdecimal():
        return AuthResult(False, "invalid_signature")
    if not request_id:
        return AuthResult(False, "invalid_signature")
    try:
        parsed_id = uuid.UUID(request_id)
    except ValueError:
        return AuthResult(False, "invalid_signature")
    if parsed_id.version != 4 or str(parsed_id) != request_id or request_id.lower() != request_id:
        return AuthResult(False, "invalid_signature")
    match = _SIGNATURE_RE.fullmatch(signature or "")
    if not match:
        return AuthResult(False, "invalid_signature", request_id=request_id)
    current = int(time.time()) if now is None else now
    if abs(current - int(timestamp)) > skew_seconds:
        return AuthResult(False, "request_timestamp_outside_window", request_id=request_id)
    body_sha256 = hashlib.sha256(body).hexdigest()
    expected = hmac.new(
        secret,
        canonical_input(timestamp, request_id, body_sha256),
        hashlib.sha256,
    ).hexdigest()
    if not hmac.compare_digest(expected, match.group(1)):
        return AuthResult(False, "invalid_signature", request_id=request_id)
    return AuthResult(True, "authenticated", request_id=request_id, body_sha256=body_sha256)
