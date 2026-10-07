# Accountable portfolio architecture

## North star

The repository is an accountable, evidence-backed static portfolio. Approved source remains the sole authority for public meaning. Evidence can support an approved claim but cannot create, strengthen, approve, or publish one. Generated projections are replaceable and reproducible. The public product remains useful without collection, scheduled work, an operational data branch, or inference.

## Planes

```text
source repositories -> observations -> operational data plane
                                      -> human review
protected default branch -> approved source -> deterministic compiler -> static site
```

The future `portfolio-data` orphan branch is an operational data plane for evidence, candidates, reviews, runs, and retained projections. It is introduced only when repeated collection would create material default-branch churn.

## Automation tiers

- Tier 0 is deterministic publication work triggered by approved-source changes.
- Tier 1 is bounded deterministic maintenance. Failure can make evidence stale but cannot invalidate an already published site.
- Tier 2 is optional probabilistic assistance. It is disabled in the prototype, candidate-only when admitted, never scheduled, and never part of publication.

## Prototype boundary

The first prototype is fixture-backed because Phase 2 does not yet contain approved project records. It proves authority separation, two-layer visibility, content-addressed package identity, deterministic projection, stable diagnostics, and machine-readable AI optionality. It does not alter the site, deployment, approved source, or Phase 2 acceptance state.
