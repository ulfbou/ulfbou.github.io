#!/usr/bin/env python3
"""Validate and preview an external article publication package."""

from __future__ import annotations

import argparse
import hashlib
import json
import shutil
import subprocess
import sys
import tempfile
import zipfile
from pathlib import Path, PurePosixPath
from typing import Any

SUPPORTED_VERSION = "1.0"
ARTICLE_PRODUCT = "articles"
HASH_CHUNK_SIZE = 1024 * 1024

MANIFEST_FILE = "manifest.json"
ARTICLES_FILE = "articles.json"
SERIES_FILE = "series.json"
PROJECTION_FILE = "data/generated/articles.json"

REQUIRED_PACKAGE_FILES = {
    MANIFEST_FILE,
    ARTICLES_FILE,
    SERIES_FILE,
    PROJECTION_FILE,
}

REQUIRED_ARTICLE_FIELDS = {
    "id",
    "slug",
    "status",
    "title",
    "deck",
    "description",
    "author",
    "language",
    "publishedDate",
    "modifiedDate",
    "readingMinutes",
    "bodyPath",
    "seriesId",
    "seriesPart",
    "socialImage",
    "socialImageAlt",
    "canonicalPath",
    "editorialApproval",
}


class PackageError(ValueError):
    """Raised when a publication package violates the consumer contract."""


def load_json(path: Path) -> dict[str, Any]:
    """Load a JSON object and convert parsing or file errors to PackageError."""
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as exc:
        raise PackageError(f"{path}: invalid JSON: {exc}") from exc

    return value


def safe_relative(value: str) -> Path:
    """Convert a safe POSIX package path to a local relative Path."""
    path = PurePosixPath(value)
    if not value or path.is_absolute() or ".." in path.parts or "." in path.parts:
        raise PackageError(f"unsafe package path: {value!r}")

    return Path(*path.parts)


def sha256(path: Path) -> str:
    """Return the lowercase SHA-256 digest for a file."""
    digest = hashlib.sha256()
    with path.open("rb") as file:
        for chunk in iter(lambda: file.read(HASH_CHUNK_SIZE), b""):
            digest.update(chunk)

    return digest.hexdigest()


def _validate_contract(document: dict[str, Any], document_name: str) -> None:
    if document.get("schemaVersion") != SUPPORTED_VERSION:
        raise PackageError(f"unsupported {document_name} contract")


def _validate_manifest(package: Path) -> set[str]:
    manifest = load_json(package / MANIFEST_FILE)
    if (
        manifest.get("schemaVersion") != SUPPORTED_VERSION
        or manifest.get("product") != ARTICLE_PRODUCT
    ):
        raise PackageError("unsupported manifest contract")

    entries = manifest.get("files")
    if not isinstance(entries, list):
        raise PackageError("manifest files must be an array")

    declared_paths: set[str] = set()
    required_entry_fields = {"path", "bytes", "sha256"}

    for entry in entries:
        if not isinstance(entry, dict) or set(entry) != required_entry_fields:
            raise PackageError("invalid manifest entry")

        relative_path = safe_relative(entry["path"])
        manifest_path = relative_path.as_posix()
        if manifest_path in declared_paths:
            raise PackageError(f"duplicate manifest path: {manifest_path}")

        declared_paths.add(manifest_path)
        file_path = package / relative_path

        if not file_path.is_file():
            raise PackageError(f"missing declared file: {manifest_path}")
        if file_path.stat().st_size != entry["bytes"]:
            raise PackageError(f"byte count mismatch: {manifest_path}")
        if sha256(file_path) != entry["sha256"]:
            raise PackageError(f"SHA-256 mismatch: {manifest_path}")

    actual_paths = {
        path.relative_to(package).as_posix()
        for path in package.rglob("*")
        if path.is_file() and path.name != MANIFEST_FILE
    }
    if actual_paths != declared_paths:
        undeclared = sorted(actual_paths - declared_paths)
        missing = sorted(declared_paths - actual_paths)
        raise PackageError(
            f"inventory mismatch: undeclared={undeclared} missing={missing}"
        )

    package_inventory = declared_paths | {MANIFEST_FILE}
    if not REQUIRED_PACKAGE_FILES <= package_inventory:
        raise PackageError("required package files are absent")

    return declared_paths


def _validate_article_records(
    articles: dict[str, Any],
    series: dict[str, Any],
    projection: dict[str, Any],
    declared_paths: set[str],
) -> tuple[list[dict[str, Any]], set[str]]:
    records = articles.get("articles")
    projected_records = projection.get("articles")
    series_records = series.get("series")

    if (
        not isinstance(records, list)
        or not isinstance(projected_records, list)
        or not isinstance(series_records, list)
    ):
        raise PackageError("invalid article or series arrays")

    series_ids = {
        record.get("id")
        for record in series_records
        if isinstance(record, dict)
    }
    slugs: set[str] = set()

    for article in records:
        if not isinstance(article, dict) or not REQUIRED_ARTICLE_FIELDS <= set(article):
            raise PackageError("article lacks required canonical fields")

        approval = article["editorialApproval"]
        if article["status"] != "published" or approval.get("approved") is not True:
            raise PackageError(
                f"consumer package contains ineligible article: {article.get('id')}"
            )

        slug = article["slug"]
        if slug in slugs or article["seriesId"] not in series_ids:
            raise PackageError("duplicate slug or unknown series")
        slugs.add(slug)

        body_path = safe_relative(article["bodyPath"]).as_posix()
        image_path = safe_relative(article["socialImage"].lstrip("/")).as_posix()
        if body_path not in declared_paths or image_path not in declared_paths:
            raise PackageError(f"article asset is not manifest-backed: {slug}")

    projected_slugs = {article.get("slug") for article in projected_records}
    if projected_slugs != slugs:
        raise PackageError("projection and canonical article sets differ")

    return records, slugs


def verify(package: Path) -> dict[str, Any]:
    """Verify package integrity, compatibility, and publication eligibility."""
    declared_paths = _validate_manifest(package)

    articles = load_json(package / ARTICLES_FILE)
    series = load_json(package / SERIES_FILE)
    projection = load_json(package / PROJECTION_FILE)

    for document_name, document in (
        ("articles", articles),
        ("series", series),
        ("projection", projection),
    ):
        _validate_contract(document, document_name)

    records, slugs = _validate_article_records(
        articles,
        series,
        projection,
        declared_paths,
    )

    return {
        "schemaVersion": SUPPORTED_VERSION,
        "articles": len(records),
        "slugs": sorted(slugs),
        "files": len(declared_paths),
    }


def prepare(package: Path, source: Path, site: Path) -> dict[str, Any]:
    """Verify and stage package inputs for the existing website renderer."""
    verification = verify(package)
    source.mkdir(parents=True, exist_ok=True)
    site.mkdir(parents=True, exist_ok=True)

    shutil.copy2(package / ARTICLES_FILE, source / ARTICLES_FILE)
    shutil.copy2(package / SERIES_FILE, source / SERIES_FILE)
    shutil.copytree(package / "bodies", source / "bodies", dirs_exist_ok=True)
    shutil.copytree(package / "assets", site / "assets", dirs_exist_ok=True)

    return verification


def unpack_zip(archive: Path, target: Path) -> None:
    """Validate archive member paths before extracting the package."""
    with zipfile.ZipFile(archive) as package_archive:
        for member in package_archive.infolist():
            member_path = member.filename.rstrip("/")
            if member_path:
                safe_relative(member_path)

        package_archive.extractall(target)


def parse_arguments() -> argparse.Namespace:
    parser = argparse.ArgumentParser()
    parser.add_argument("package")
    parser.add_argument("--repo", default=".")
    parser.add_argument("--preview-root")
    parser.add_argument("--verify-only", action="store_true")
    return parser.parse_args()


def _resolve_package(supplied_package: Path, workspace: Path) -> Path:
    if supplied_package.suffix.lower() != ".zip":
        return supplied_package

    package = workspace / "package"
    package.mkdir()
    unpack_zip(supplied_package, package)
    return package


def _create_preview(
    repository: Path,
    package: Path,
    workspace: Path,
    preview: Path,
) -> dict[str, Any]:
    if preview.exists():
        shutil.rmtree(preview)

    shutil.copytree(repository / "src/Ulfbou.Site/wwwroot", preview)

    source = workspace / "source"
    verification = prepare(package, source, preview)
    article_project = repository / "src/Ulfbou.Articles/Ulfbou.Articles.csproj"

    subprocess.run(
        [
            "dotnet",
            "run",
            "--project",
            str(article_project),
            "--",
            str(source),
            str(preview / "articles"),
            str(preview),
        ],
        check=True,
        cwd=repository,
    )

    report = {
        **verification,
        "previewRoot": str(preview),
        "articleIndex": str(preview / "articles/index.html"),
    }
    report_path = preview.parent / "preview-report.json"
    report_path.write_text(
        json.dumps(report, indent=2) + "\n",
        encoding="utf-8",
    )

    return report


def main() -> int:
    arguments = parse_arguments()
    repository = Path(arguments.repo).resolve()
    supplied_package = Path(arguments.package).resolve()

    with tempfile.TemporaryDirectory(prefix="article-package-") as temporary_directory:
        workspace = Path(temporary_directory)
        package = _resolve_package(supplied_package, workspace)
        verification = verify(package)

        if arguments.verify_only:
            print(json.dumps(verification, indent=2))
            return 0

        preview = (
            Path(arguments.preview_root).resolve()
            if arguments.preview_root
            else repository / "artifacts/article-package-preview/site"
        )
        report = _create_preview(repository, package, workspace, preview)
        print(json.dumps(report, indent=2))

    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (PackageError, subprocess.CalledProcessError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(2)
