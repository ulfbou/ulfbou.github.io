#!/usr/bin/env python3
from pathlib import Path
import re, sys
root = Path(__file__).resolve().parents[2]
required = [
    root / "src/Ulfbou.Correlation.Core/CorrelationDomain.cs",
    root / "src/Ulfbou.Correlation.Core/Ulfbou.Correlation.Core.csproj",
    root / "tests/Ulfbou.Correlation.Core.Tests/CorrelationDomainTests.cs",
    root / "tests/Ulfbou.Correlation.Core.Tests/Ulfbou.Correlation.Core.Tests.csproj",
]
missing = [str(path.relative_to(root)) for path in required if not path.is_file()]
if missing:
    print("M1-MISSING: " + ", ".join(missing), file=sys.stderr)
    raise SystemExit(1)
source = required[0].read_text(encoding="utf-8")
for marker in ["CANON-DUPLICATE", "STATE-TERMINAL", "DeleteConditionally", "active/v1/"]:
    if marker not in source:
        print(f"M1-MARKER: {marker}", file=sys.stderr)
        raise SystemExit(1)
solution = (root / "Ulfbou.Portfolio.slnx").read_text(encoding="utf-8")
for project in ["src/Ulfbou.Correlation.Core/Ulfbou.Correlation.Core.csproj", "tests/Ulfbou.Correlation.Core.Tests/Ulfbou.Correlation.Core.Tests.csproj"]:
    if project not in solution:
        print(f"M1-SOLUTION: {project}", file=sys.stderr)
        raise SystemExit(1)
print("PASS: M1 source and solution structure")
