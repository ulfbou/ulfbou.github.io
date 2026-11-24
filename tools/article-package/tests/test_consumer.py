"""Tests for the external article publication package consumer."""

from __future__ import annotations

import hashlib
import json
import sys
import tempfile
import unittest
from pathlib import Path
from typing import Any

sys.path.insert(0, str(Path(__file__).parents[1]))

from consumer import PackageError, verify

SCHEMA_VERSION = "1.0"


class ConsumerTests(unittest.TestCase):
    def write_json(self, path: Path, value: dict[str, Any]) -> None:
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(json.dumps(value) + "\n")

    def create_package(self, root: Path) -> None:
        documents = {
            "articles.json": {
                "schemaVersion": SCHEMA_VERSION,
                "articles": [
                    {
                        "id": "a",
                        "slug": "a",
                        "status": "published",
                        "title": "A",
                        "deck": "D",
                        "description": "S",
                        "author": "U",
                        "language": "en",
                        "publishedDate": "2026-01-01",
                        "modifiedDate": "2026-01-01",
                        "readingMinutes": 1,
                        "bodyPath": "bodies/a.md",
                        "seriesId": "s",
                        "seriesPart": 1,
                        "socialImage": "/assets/social/articles/a.png",
                        "socialImageAlt": "A",
                        "canonicalPath": "/articles/a/",
                        "editorialApproval": {"approved": True},
                    }
                ],
            },
            "series.json": {
                "schemaVersion": SCHEMA_VERSION,
                "series": [
                    {
                        "id": "s",
                        "title": "S",
                        "description": "D",
                        "status": "ongoing",
                    }
                ],
            },
            "data/generated/articles.json": {
                "schemaVersion": SCHEMA_VERSION,
                "articles": [{"slug": "a"}],
            },
        }

        for relative_path, document in documents.items():
            self.write_json(root / relative_path, document)

        body = root / "bodies/a.md"
        body.parent.mkdir(exist_ok=True)
        body.write_text("# A\n")

        image = root / "assets/social/articles/a.png"
        image.parent.mkdir(parents=True)
        image.write_bytes(b"png")

        manifest_entries = []
        package_files = sorted(path for path in root.rglob("*") if path.is_file())
        for package_file in package_files:
            content = package_file.read_bytes()
            manifest_entries.append(
                {
                    "path": package_file.relative_to(root).as_posix(),
                    "bytes": len(content),
                    "sha256": hashlib.sha256(content).hexdigest(),
                }
            )

        self.write_json(
            root / "manifest.json",
            {
                "schemaVersion": SCHEMA_VERSION,
                "product": "articles",
                "files": manifest_entries,
            },
        )

    def test_valid_package(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            package = Path(temporary_directory)
            self.create_package(package)

            result = verify(package)

            self.assertEqual(result["slugs"], ["a"])

    def test_tampering_fails(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            package = Path(temporary_directory)
            self.create_package(package)
            (package / "bodies/a.md").write_text("changed")

            with self.assertRaises(PackageError):
                verify(package)

    def test_unknown_version_fails(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            package = Path(temporary_directory)
            self.create_package(package)

            manifest_path = package / "manifest.json"
            manifest = json.loads(manifest_path.read_text())
            manifest["schemaVersion"] = "2.0"
            manifest_path.write_text(json.dumps(manifest))

            with self.assertRaises(PackageError):
                verify(package)


if __name__ == "__main__":
    unittest.main()
