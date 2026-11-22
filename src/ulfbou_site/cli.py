"""Command-line boundary for the ulfbou.github.io publication platform."""

from __future__ import annotations

import argparse
from collections.abc import Sequence

from ulfbou_site import __version__

_NOT_IMPLEMENTED_EXIT = 3
_LATER_STAGE = {
    "validate": "source validation",
    "build": "static publication",
    "check-artifact": "artifact verification",
}


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="ulfbou-site",
        description="Static publication platform for ulfbou.github.io.",
    )
    parser.add_argument(
        "--version",
        action="version",
        version=f"%(prog)s {__version__}",
    )

    commands = parser.add_subparsers(dest="command", metavar="COMMAND")
    for command, capability in _LATER_STAGE.items():
        commands.add_parser(
            command,
            help=f"{capability.capitalize()} boundary; implemented in a later roadmap stage.",
        )

    return parser


def main(argv: Sequence[str] | None = None) -> int:
    """Run the command-line interface and return a process status."""

    parser = _build_parser()
    arguments = parser.parse_args(argv)

    if arguments.command is None:
        parser.print_help()
        return 0

    capability = _LATER_STAGE[arguments.command]
    parser.exit(
        _NOT_IMPLEMENTED_EXIT,
        (
            f"ulfbou-site: {arguments.command}: unavailable in Stage 1; "
            f"{capability} belongs to a later roadmap stage.\n"
        ),
    )


if __name__ == "__main__":
    raise SystemExit(main())
