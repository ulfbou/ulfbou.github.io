# Portfolio data prototype

This dependency-free prototype proves governance before automation. Its fixture is not approved portfolio content and is not deployed.

```bash
ROOT=tools/portfolio-data/fixtures/valid/one-approved-claim
python tools/portfolio-data/cli.py validate \
  --governance "$ROOT/governance.json" \
  --approved "$ROOT/approved.json" \
  --evidence "$ROOT/evidence.json"
python tools/portfolio-data/cli.py compile \
  --governance "$ROOT/governance.json" \
  --approved "$ROOT/approved.json" \
  --evidence "$ROOT/evidence.json" \
  --output "$ROOT/evidence-index.json"
python tools/portfolio-data/cli.py verify \
  --governance "$ROOT/governance.json" \
  --approved "$ROOT/approved.json" \
  --evidence "$ROOT/evidence.json" \
  --projection "$ROOT/evidence-index.json"
python tools/portfolio-data/tests.py
```

The fixture revision and observation digest are synthetic contract data. A real evidence package remains blocked until an exact source revision and an approved Approved-source project claim exist.
