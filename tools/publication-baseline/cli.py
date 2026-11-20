#!/usr/bin/env python3
"""Inspect an already-produced publication artifact without creating it."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import sys
import tempfile
from html.parser import HTMLParser
from pathlib import Path, PurePosixPath
from urllib.parse import unquote, urlsplit

SCHEMA_VERSION = "1.0"
REQUIRED_CURRENT_PATHS = (
    "index.html",
    "404.html",
    "aws/index.html",
    "work/index.html",
    "data/generated/portfolio.json",
    "articles/index.html",
    "articles/feed.xml",
    "data/generated/articles.json",
    "sitemap.xml",
)
DISCOVERY_PATHS = ("robots.txt", "sitemap.xml", "feed.xml", "atom.xml", "rss.xml")
REFERENCE_ATTRIBUTES = {"a": "href", "img": "src", "link": "href", "script": "src", "source": "src"}
IGNORED_SCHEMES = {"data", "http", "https", "mailto", "tel", "javascript"}
HEX_REVISION = re.compile(r"^[0-9a-fA-F]{7,64}$")
WINDOWS_ABSOLUTE = re.compile(r"^[A-Za-z]:[\\/]")


class BaselineError(Exception):
    pass


class PageParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__(convert_charrefs=True)
        self.lang: str | None = None
        self.title_parts: list[str] = []
        self.in_title = False
        self.description: str | None = None
        self.canonical: str | None = None
        self.refresh: str | None = None
        self.references: list[tuple[str, str, str]] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        values = {name.lower(): value for name, value in attrs if value is not None}
        lowered = tag.lower()
        if lowered == "html" and self.lang is None:
            self.lang = values.get("lang")
        if lowered == "title":
            self.in_title = True
        if lowered == "meta":
            name = values.get("name", "").lower()
            equiv = values.get("http-equiv", "").lower()
            if name == "description" and self.description is None:
                self.description = values.get("content")
            if equiv == "refresh" and self.refresh is None:
                self.refresh = values.get("content")
        if lowered == "link" and values.get("rel", "").lower() == "canonical":
            self.canonical = values.get("href")
        attribute = REFERENCE_ATTRIBUTES.get(lowered)
        if attribute and attribute in values:
            self.references.append((lowered, attribute, values[attribute]))

    def handle_endtag(self, tag: str) -> None:
        if tag.lower() == "title":
            self.in_title = False

    def handle_data(self, data: str) -> None:
        if self.in_title:
            self.title_parts.append(data)

    @property
    def title(self) -> str | None:
        value = "".join(self.title_parts).strip()
        return value or None


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def canonical_json(value: object) -> bytes:
    return (json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")) + "\n").encode("utf-8")


def normalize_revision(value: str | None) -> str | None:
    if value is None:
        return None
    revision = value.strip()
    if not HEX_REVISION.fullmatch(revision):
        raise BaselineError("source revision must be an explicitly supplied hexadecimal revision of 7 to 64 characters")
    return revision.lower()


def artifact_files(root: Path) -> list[Path]:
    files = [path for path in root.rglob("*") if path.is_file()]
    return sorted(files, key=lambda path: path.relative_to(root).as_posix().encode("utf-8"))


def privacy_reason(relative: str) -> str | None:
    path = PurePosixPath(relative)
    lower = relative.lower()
    name = path.name.lower()
    parts = {part.lower() for part in path.parts}
    if lower.endswith((".dx", ".dx.txt")):
        return "transport carrier"
    if name in {".env", ".env.local", ".env.production", "id_rsa", "id_ed25519"}:
        return "credential or local environment file"
    if ".git" in parts or ".github" in parts:
        return "repository-control material"
    if name.endswith((".tmp", ".temp", ".bak", ".orig", ".rej", ".log")) or name.startswith("~"):
        return "temporary or diagnostic file"
    if name.endswith(".map"):
        return "source map requires explicit public admission"
    tokens = set(re.split(r"[^a-z0-9]+", lower))
    if tokens & {"prompt", "prompts", "handoff", "continuation", "transcript"}:
        return "collaboration material"
    diagnostic_components = {"diagnostic", "diagnostics"}
    if parts & diagnostic_components or re.match(r"^diagnostics?(?:[._-]|$)", name):
        return "diagnostic material"
    return None


def resolve_reference(page_path: str, reference: str) -> tuple[str | None, str | None]:
    value = reference.strip()
    if not value or value.startswith("#") or value.startswith("//"):
        return None, None
    split = urlsplit(value)
    if split.scheme.lower() in IGNORED_SCHEMES or split.netloc:
        return None, None
    decoded = unquote(split.path)
    if not decoded:
        return None, None
    if "\\" in decoded or WINDOWS_ABSOLUTE.match(decoded):
        return None, "unsafe local reference"
    if decoded.startswith("/"):
        candidate = PurePosixPath(decoded.lstrip("/"))
    else:
        candidate = PurePosixPath(page_path).parent / decoded
    if ".." in candidate.parts:
        return None, "traversing local reference"
    normalized = candidate.as_posix().lstrip("./")
    if decoded.endswith("/") or normalized == "":
        normalized = (PurePosixPath(normalized) / "index.html").as_posix()
    elif PurePosixPath(normalized).suffix == "":
        normalized = (PurePosixPath(normalized) / "index.html").as_posix()
    return normalized, None


def inspect_page(root: Path, relative: str, known_files: set[str]) -> dict[str, object]:
    parser = PageParser()
    parser.feed((root / relative).read_text(encoding="utf-8"))
    references: list[dict[str, object]] = []
    for tag, attribute, value in sorted(parser.references, key=lambda item: (item[2], item[0], item[1])):
        resolved, problem = resolve_reference(relative, value)
        entry: dict[str, object] = {"attribute": attribute, "tag": tag, "value": value}
        if resolved is not None:
            entry["artifactPath"] = resolved
            entry["present"] = resolved in known_files
        if problem is not None:
            entry["problem"] = problem
        references.append(entry)
    return {
        "path": relative,
        "language": parser.lang,
        "title": parser.title,
        "description": parser.description,
        "canonical": parser.canonical,
        "refresh": parser.refresh,
        "localReferences": references,
    }


def inspect(root: Path, source_revision: str | None = None) -> dict[str, object]:
    root = root.resolve()
    if not root.is_dir():
        raise BaselineError(f"artifact directory does not exist: {root}")
    files = artifact_files(root)
    if not files:
        raise BaselineError("artifact directory contains no files")
    inventory: list[dict[str, object]] = []
    privacy: list[dict[str, str]] = []
    known_files: set[str] = set()
    for path in files:
        relative = path.relative_to(root).as_posix()
        known_files.add(relative)
        data = path.read_bytes()
        inventory.append({"path": relative, "bytes": len(data), "sha256": sha256(data)})
        reason = privacy_reason(relative)
        if reason:
            privacy.append({"path": relative, "reason": reason})
    identity_input = b"".join(
        item["path"].encode("utf-8") + b"\0" + item["sha256"].encode("ascii") + b"\0"
        for item in inventory
    )
    pages = [inspect_page(root, relative, known_files) for relative in sorted(known_files) if relative.endswith((".html", ".htm"))]
    missing_references = [
        {"page": page["path"], "value": ref["value"], "artifactPath": ref.get("artifactPath"), "problem": ref.get("problem")}
        for page in pages
        for ref in page["localReferences"]
        if ref.get("problem") is not None or ("present" in ref and not ref["present"])
    ]
    required = {path: path in known_files for path in REQUIRED_CURRENT_PATHS}
    index_hash = next((item["sha256"] for item in inventory if item["path"] == "index.html"), None)
    not_found_hash = next((item["sha256"] for item in inventory if item["path"] == "404.html"), None)
    not_found_page = next((page for page in pages if page["path"] == "404.html"), None)
    if not_found_hash is None:
        not_found_behavior = "missing"
    elif not_found_hash == index_hash:
        not_found_behavior = "byte-identical-to-root-index"
    elif not_found_page and not_found_page["refresh"]:
        not_found_behavior = "html-refresh"
    else:
        not_found_behavior = "standalone-or-runtime-defined"
    failures: list[str] = []
    failures.extend(f"missing required current path: {path}" for path, present in required.items() if not present)
    asset_reference_keys = {
        (page["path"], ref["value"])
        for page in pages
        for ref in page["localReferences"]
        if ref["tag"] != "a" and (ref.get("problem") is not None or ("present" in ref and not ref["present"]))
    }
    failures.extend(
        f"missing or unsafe local asset reference: {item['page']} -> {item.get('artifactPath') or item['value']}"
        for item in missing_references
        if (item["page"], item["value"]) in asset_reference_keys
    )
    failures.extend(f"prohibited artifact path: {item['path']} ({item['reason']})" for item in privacy)
    if "data/generated/articles.json" in known_files:
        try:
            projection = json.loads((root / "data/generated/articles.json").read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError) as error:
            failures.append(f"invalid article projection: {error}")
            projection = {"articles": []}
        projected = projection.get("articles", [])
        if not isinstance(projected, list):
            failures.append("article projection articles must be an array")
            projected = []
        if any(not isinstance(item, dict) or item.get("status") != "published" for item in projected):
            failures.append("article projection contains nonpublished or malformed records")
        feed = (root / "articles/feed.xml").read_text(encoding="utf-8") if "articles/feed.xml" in known_files else ""
        sitemap = (root / "sitemap.xml").read_text(encoding="utf-8") if "sitemap.xml" in known_files else ""
        declared_pages = set()
        for item in projected:
            if not isinstance(item, dict):
                continue
            route = item.get("canonicalPath", "")
            output = item.get("outputPath", "")
            declared_pages.add(output)
            if not route.startswith("/articles/") or not output.startswith("articles/"):
                failures.append(f"published article has invalid route or output: {route}")
                continue
            if output not in known_files:
                failures.append(f"published article page missing: {output}")
                continue
            article_html = (root / output).read_text(encoding="utf-8")
            for marker in (
                '<article>',
                f'rel="canonical" href="https://ulfbou.github.io{route}"',
                'application/ld+json',
            ):
                if marker not in article_html:
                    failures.append(f"article page missing invariant: {output}: {marker}")
            if "_framework/blazor" in article_html.lower():
                failures.append(f"article body depends on Blazor startup: {output}")
            if route not in feed:
                failures.append(f"published article absent from feed: {route}")
            if route not in sitemap:
                failures.append(f"published article absent from sitemap: {route}")
        undeclared = sorted(
            path for path in known_files
            if path.startswith("articles/") and path.endswith("/index.html") and path != "articles/index.html" and path not in declared_pages
        )
        failures.extend(f"undeclared article page: {path}" for path in undeclared)
    return {
        "schemaVersion": SCHEMA_VERSION,
        "sourceRevision": normalize_revision(source_revision),
        "artifact": {
            "identityAlgorithm": "sha256(path-utf8-nul-file-sha256-ascii-nul)",
            "sha256": sha256(identity_input),
            "fileCount": len(inventory),
            "files": inventory,
        },
        "currentBaseline": {
            "requiredPaths": required,
            "pages": pages,
            "missingLocalReferences": missing_references,
            "discoveryFiles": {path: path in known_files for path in DISCOVERY_PATHS},
            "effective404": {
                "classification": not_found_behavior,
                "rootIndexSha256": index_hash,
                "notFoundSha256": not_found_hash,
            },
        },
        "privacy": {"violations": privacy},
        "verification": {"passed": not failures, "failures": failures},
    }


def write_atomic(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    handle, temporary = tempfile.mkstemp(prefix=path.name + ".", suffix=".tmp", dir=path.parent)
    try:
        with os.fdopen(handle, "wb") as stream:
            stream.write(data)
            stream.flush()
            os.fsync(stream.fileno())
        os.replace(temporary, path)
    finally:
        if os.path.exists(temporary):
            os.unlink(temporary)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("artifact", type=Path, help="already-produced artifact directory")
    parser.add_argument("--source-revision", help="explicit revision associated by the execution context")
    parser.add_argument("--output", type=Path, required=True, help="JSON report path outside or inside the chosen execution workspace")
    arguments = parser.parse_args(argv)
    try:
        report = inspect(arguments.artifact, arguments.source_revision)
        write_atomic(arguments.output, json.dumps(report, ensure_ascii=False, sort_keys=True, indent=2).encode("utf-8") + b"\n")
        print(f"artifact-sha256: {report['artifact']['sha256']}")
        print(f"files: {report['artifact']['fileCount']}")
        print("PASS" if report["verification"]["passed"] else "FAIL")
        return 0 if report["verification"]["passed"] else 1
    except (BaselineError, OSError, UnicodeError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
