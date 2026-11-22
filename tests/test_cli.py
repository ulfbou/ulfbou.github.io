"""Focused tests for the Stage 1 command-line boundary."""

from __future__ import annotations

import subprocess
import sys
from collections.abc import Sequence

import pytest

from ulfbou_site import __version__
from ulfbou_site.cli import main


def _run_cli(arguments: Sequence[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        [sys.executable, "-m", "ulfbou_site", *arguments],
        check=False,
        capture_output=True,
        text=True,
    )


def test_main_without_command_prints_help(capsys: pytest.CaptureFixture[str]) -> None:
    assert main([]) == 0

    captured = capsys.readouterr()
    assert "usage: ulfbou-site" in captured.out
    assert "validate" in captured.out
    assert "build" in captured.out
    assert "check-artifact" in captured.out
    assert captured.err == ""


def test_version_is_available() -> None:
    result = _run_cli(["--version"])

    assert result.returncode == 0
    assert result.stdout == f"ulfbou-site {__version__}\n"
    assert result.stderr == ""


@pytest.mark.parametrize(
    ("command", "capability"),
    [
        ("validate", "source validation"),
        ("build", "static publication"),
        ("check-artifact", "artifact verification"),
    ],
)
def test_later_stage_commands_fail_explicitly(
    command: str,
    capability: str,
) -> None:
    result = _run_cli([command])

    assert result.returncode == 3
    assert result.stdout == ""
    assert result.stderr == (
        f"ulfbou-site: {command}: unavailable in Stage 1; "
        f"{capability} belongs to a later roadmap stage.\n"
    )


def test_unknown_command_is_rejected() -> None:
    result = _run_cli(["unknown"])

    assert result.returncode == 2
    assert result.stdout == ""
    assert "invalid choice: 'unknown'" in result.stderr
