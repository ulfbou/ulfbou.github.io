# Validation and evidence

Canonical baseline:

```bash
dotnet restore Ulfbou.Portfolio.slnx
dotnet build Ulfbou.Portfolio.slnx --no-restore -c Release
dotnet test Ulfbou.Portfolio.slnx --no-build -c Release
```

Phase 1 automated evidence covers restore/build/tests, JSON deserialization, required fields, unique IDs, known facets, `/`, loading/failure/empty states, filtering, expansion, deployment artifact, `.nojekyll`, and prohibited placeholders/private markers.

Manual evidence covers opening comprehension, distinct propositions, contribution/reflection separation, maturity qualification, keyboard/focus, landmarks/headings/names/states, narrow/medium/wide layouts, 200%/400% zoom, reduced motion, selectable artifacts, print, deployed root load, and content approval.

Phase 2 adds schema/semantic/reference/publication failures, candidate/private exclusion, fixtures, equivalent generation, and traceability. Later phases add block, package, collector, and curation gates defined by the roadmap.

Every criterion records stable name, command/review, result, evidence location, revision, reviewer where manual, and limitations. Unknown mandatory results fail. Semantic comparison, not byte identity, decides regression.

## Fixture-backed evidence prototype
Run `python tools/portfolio-data/tests.py`. The prototype must reject admitted inference adapters, unresolved references, non-public references, non-public observations, symbolic revisions, path traversal, duplicate identities, stale package identities, and non-reproducible projections.

The fixture is not approved portfolio content. The prototype adds no Python or NuGet dependency, GitHub Action, repository secret, deployment input, or runtime dependency, and does not start Phase 2.
