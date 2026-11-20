#!/usr/bin/env python3
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).parent
SPEC = importlib.util.spec_from_file_location("publication_baseline", ROOT / "cli.py")
CLI = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(CLI)


class BaselineTests(unittest.TestCase):
    def artifact(self, root: Path, *, missing_asset: bool = False, private_file: bool = False) -> Path:
        artifact = root / "artifact"
        (artifact / "aws").mkdir(parents=True)
        (artifact / "work").mkdir(parents=True)
        (artifact / "data/generated").mkdir(parents=True)
        (artifact / "css/aws").mkdir(parents=True)
        index = '<!doctype html><html lang="en"><head><title>Root</title><meta name="description" content="Root description"><link rel="canonical" href="https://ulfbou.github.io/"><link rel="stylesheet" href="css/site.css"></head><body></body></html>\n'
        (artifact / "index.html").write_text(index, encoding="utf-8")
        (artifact / "404.html").write_text(index, encoding="utf-8")
        (artifact / "aws/index.html").write_text(
            '<!doctype html><html lang="sv"><head><title>AWS</title><link rel="stylesheet" href="/css/aws/base.css"></head><body><a href="/aws/ai/">AI</a></body></html>\n',
            encoding="utf-8",
        )
        (artifact / "work/index.html").write_text(
            '<!doctype html><html lang="en"><head><title>Work</title><meta name="description" content="Complete portfolio"><link rel="canonical" href="https://ulfbou.github.io/work/"><link rel="stylesheet" href="/css/site.css"></head><body></body></html>\n',
            encoding="utf-8",
        )
        (artifact / "data/generated/portfolio.json").write_text('{}\n', encoding="utf-8")
        (artifact / "css/aws/base.css").write_text("body{}\n", encoding="utf-8")
        if not missing_asset:
            (artifact / "css/site.css").write_text("body{}\n", encoding="utf-8")
            (artifact / "aws/ai").mkdir(parents=True)
            (artifact / "aws/ai/index.html").write_text('<html lang="en"><title>AI</title></html>\n', encoding="utf-8")
        if private_file:
            (artifact / "handoff.txt").write_text("private\n", encoding="utf-8")
        return artifact

    def test_report_is_deterministic_and_identifies_effective_404(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            first = CLI.inspect(artifact, "ABCDEF1")
            second = CLI.inspect(artifact, "abcdef1")
            self.assertEqual(CLI.canonical_json(first), CLI.canonical_json(second))
            self.assertTrue(first["verification"]["passed"])
            self.assertEqual("abcdef1", first["sourceRevision"])
            self.assertEqual("byte-identical-to-root-index", first["currentBaseline"]["effective404"]["classification"])
            self.assertEqual("https://ulfbou.github.io/", first["currentBaseline"]["pages"][0]["canonical"])

    def test_file_change_changes_artifact_identity(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            before = CLI.inspect(artifact)["artifact"]["sha256"]
            (artifact / "css/site.css").write_text("body{color:black}\n", encoding="utf-8")
            after = CLI.inspect(artifact)["artifact"]["sha256"]
            self.assertNotEqual(before, after)

    def test_missing_local_reference_fails(self):
        with tempfile.TemporaryDirectory() as temporary:
            report = CLI.inspect(self.artifact(Path(temporary), missing_asset=True))
            self.assertFalse(report["verification"]["passed"])
            self.assertTrue(any("missing or unsafe local asset reference" in item for item in report["verification"]["failures"]))

    def test_prohibited_collaboration_file_fails(self):
        with tempfile.TemporaryDirectory() as temporary:
            report = CLI.inspect(self.artifact(Path(temporary), private_file=True))
            self.assertFalse(report["verification"]["passed"])
            self.assertEqual("collaboration material", report["privacy"]["violations"][0]["reason"])

    def test_framework_diagnostics_names_are_not_privacy_violations(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            framework = artifact / "_framework"
            framework.mkdir(exist_ok=True)
            for suffix in ("", ".br", ".gz"):
                (framework / f"System.Diagnostics.DiagnosticSource.test.wasm{suffix}").write_bytes(b"framework")

            report = CLI.inspect(artifact)

            violation_paths = {item["path"] for item in report["privacy"]["violations"]}
            self.assertFalse(
                violation_paths
                & {
                    f"_framework/System.Diagnostics.DiagnosticSource.test.wasm{suffix}"
                    for suffix in ("", ".br", ".gz")
                }
            )
            self.assertTrue(report["verification"]["passed"])

    def test_explicit_diagnostic_artifact_remains_prohibited(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            diagnostic = artifact / "collaboration" / "diagnostics" / "verifier-output.txt"
            diagnostic.parent.mkdir(parents=True)
            diagnostic.write_text("private diagnostic output\n", encoding="utf-8")

            report = CLI.inspect(artifact)

            self.assertFalse(report["verification"]["passed"])
            self.assertIn(
                {"path": "collaboration/diagnostics/verifier-output.txt", "reason": "diagnostic material"},
                report["privacy"]["violations"],
            )

    def test_missing_target_route_is_observed_not_required(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            (artifact / "aws/ai/index.html").unlink()
            report = CLI.inspect(artifact)
            self.assertTrue(report["verification"]["passed"], "a missing linked target route is reported but is not an Increment 2 requirement")
            self.assertTrue(report["currentBaseline"]["requiredPaths"]["work/index.html"])
            self.assertNotIn("articles/index.html", report["currentBaseline"]["requiredPaths"])
            self.assertNotIn("aws/ai/index.html", report["currentBaseline"]["requiredPaths"])

    def test_work_route_is_required_and_has_distinct_metadata(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            report = CLI.inspect(artifact)

            self.assertTrue(report["currentBaseline"]["requiredPaths"]["work/index.html"])
            pages = {page["path"]: page for page in report["currentBaseline"]["pages"]}
            self.assertEqual("Root", pages["index.html"]["title"])
            self.assertEqual("Work", pages["work/index.html"]["title"])
            self.assertNotEqual(pages["index.html"]["canonical"], pages["work/index.html"]["canonical"])

    def test_missing_work_route_fails(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            (artifact / "work/index.html").unlink()
            report = CLI.inspect(artifact)

            self.assertFalse(report["verification"]["passed"])
            self.assertIn("missing required current path: work/index.html", report["verification"]["failures"])

    def test_invalid_revision_is_rejected(self):
        with tempfile.TemporaryDirectory() as temporary:
            artifact = self.artifact(Path(temporary))
            with self.assertRaises(CLI.BaselineError):
                CLI.inspect(artifact, "not-a-revision")


if __name__ == "__main__":
    unittest.main()
