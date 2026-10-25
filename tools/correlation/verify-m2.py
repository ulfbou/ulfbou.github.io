#!/usr/bin/env python3
from pathlib import Path
import sys
root = Path(__file__).resolve().parents[2]
required = [
    "src/Ulfbou.Correlation.Application/CorrelationApplication.cs",
    "src/Ulfbou.Correlation.Application/CorrelationBrokers.cs",
    "src/Ulfbou.Correlation.Application/Ulfbou.Correlation.Application.csproj",
    "tests/Ulfbou.Correlation.Application.Tests/CorrelationVerticalSliceTests.cs",
    "tests/Ulfbou.Correlation.Application.Tests/Ulfbou.Correlation.Application.Tests.csproj",
]
missing = [path for path in required if not (root / path).is_file()]
if missing:
    print("M2-MISSING: " + ", ".join(missing), file=sys.stderr)
    raise SystemExit(1)
source = "\n".join((root / path).read_text(encoding="utf-8") for path in required if path.endswith(".cs"))
for marker in ["CandidateAnalysis", "CancelAsync", "BROKER-LATE-RESULT", "STORE-CONFLICT", "Authoritative"]:
    if marker not in source:
        print(f"M2-MARKER: {marker}", file=sys.stderr)
        raise SystemExit(1)
for prohibited in ["Amazon.S3", "Amazon.Lambda", "AWSSDK", "HttpClient"]:
    if prohibited in source:
        print(f"M2-LIVE-DEPENDENCY: {prohibited}", file=sys.stderr)
        raise SystemExit(1)
solution = (root / "Ulfbou.Portfolio.slnx").read_text(encoding="utf-8")
for project in ["src/Ulfbou.Correlation.Application/Ulfbou.Correlation.Application.csproj", "tests/Ulfbou.Correlation.Application.Tests/Ulfbou.Correlation.Application.Tests.csproj"]:
    if project not in solution:
        print(f"M2-SOLUTION: {project}", file=sys.stderr)
        raise SystemExit(1)
print("PASS: M2 local vertical slice structure")
