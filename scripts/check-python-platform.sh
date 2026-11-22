#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(git -C "$(dirname -- "${BASH_SOURCE[0]}")" rev-parse --show-toplevel)"
cd -- "${REPO_ROOT}"

uv sync
uv run ruff check src/ulfbou_site tests/test_cli.py tests/test_import_boundaries.py
uv run mypy src/ulfbou_site
uv run pytest tests/test_cli.py tests/test_import_boundaries.py
uv run ulfbou-site --help
uv run ulfbou-site --version
git diff --check -- \
  .gitattributes \
  .gitignore \
  .python-version \
  pyproject.toml \
  uv.lock \
  .github/workflows/python-platform.yml \
  scripts/check-python-platform.sh \
  src/ulfbou_site/__init__.py \
  src/ulfbou_site/__main__.py \
  src/ulfbou_site/cli.py \
  tests/test_cli.py \
  tests/test_import_boundaries.py
