from __future__ import annotations

from workers import Response, WorkerEntrypoint

from ledger import MemoryLedger, UnavailableLedger
from service import handle_dry_run

_JSON_HEADERS = {"content-type": "application/json; charset=utf-8"}
_LOCAL_LEDGER = MemoryLedger()


def _json_response(response):
    return Response.json(response.payload(), status=response.status, headers=_JSON_HEADERS)


class Default(WorkerEntrypoint):
    async def fetch(self, request):
        url = request.url
        path = "/" + url.split("/", 3)[3].split("?", 1)[0] if url.count("/") >= 3 else "/"
        if request.method == "GET" and path == "/health":
            return Response.json({"code": "healthy", "mode": "dry-run"}, headers=_JSON_HEADERS)
        if request.method != "POST" or path != "/v1/linkedin/publications/dry-run":
            return Response.json({"code": "not_found"}, status=404, headers=_JSON_HEADERS)
        content_type = request.headers.get("content-type") or ""
        content_encoding = request.headers.get("content-encoding") or ""
        if content_type.split(";", 1)[0].strip().lower() != "application/json" or content_encoding:
            return Response.json(
                {"code": "unsupported_media_type_or_encoding"}, status=415, headers=_JSON_HEADERS
            )
        body = bytes(await request.bytes())
        environment = str(getattr(self.env, "ENVIRONMENT", "preview")).lower()
        admitted = frozenset(
            item.strip() for item in str(
                getattr(self.env, "CANONICAL_ARTICLE_URLS", "https://ulfbou.github.io/articles/bias/")
            ).split(",") if item.strip()
        )
        secret_value = str(getattr(self.env, "PUBLICATION_HMAC_SECRET", ""))
        ledger = _LOCAL_LEDGER if environment == "local" else UnavailableLedger()
        response = await handle_dry_run(
            body=body,
            headers={
                "x-publication-timestamp": request.headers.get("x-publication-timestamp"),
                "x-publication-request-id": request.headers.get("x-publication-request-id"),
                "x-publication-signature": request.headers.get("x-publication-signature"),
            },
            secret=secret_value.encode("utf-8"),
            ledger=ledger,
            enabled=str(getattr(self.env, "PUBLICATION_ENABLED", "false")).lower() == "true",
            admitted_urls=admitted,
            identity="personal-member:ulf-bourelius",
        )
        return _json_response(response)
