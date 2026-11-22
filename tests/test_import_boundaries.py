"""Static enforcement of the initial Python package dependency boundary."""

from __future__ import annotations

import ast
from pathlib import Path

PACKAGE_ROOT = Path("src/ulfbou_site")
STABLE_LAYER_NAMES = {"domain", "application"}


def _imported_modules(path: Path) -> set[str]:
    tree = ast.parse(path.read_text(encoding="utf-8"), filename=str(path))
    modules: set[str] = set()

    for node in ast.walk(tree):
        if isinstance(node, ast.Import):
            modules.update(alias.name for alias in node.names)
        elif isinstance(node, ast.ImportFrom) and node.module is not None:
            modules.add(node.module)

    return modules


def test_stable_layers_do_not_import_cli_delivery_code() -> None:
    violations: list[str] = []

    for layer_name in sorted(STABLE_LAYER_NAMES):
        layer = PACKAGE_ROOT / layer_name
        if not layer.exists():
            continue

        for path in sorted(layer.rglob("*.py")):
            imports = _imported_modules(path)
            if any(
                module == "ulfbou_site.cli" or module.startswith("ulfbou_site.cli.")
                for module in imports
            ):
                violations.append(str(path))

    assert violations == []
