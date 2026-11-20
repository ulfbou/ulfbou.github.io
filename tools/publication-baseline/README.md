# Current publication baseline verifier

This dependency-free tool inspects an already-produced static publication artifact. It does not build, reconstruct, approve, release, upload, or deploy an artifact, and it does not infer live GitHub Pages behavior.

## Automated inspection

```bash
python tools/publication-baseline/cli.py publish/wwwroot \
  --source-revision "$GITHUB_SHA" \
  --output "$RUNNER_TEMP/publication-baseline.json"
```

`--source-revision` is optional and is recorded only when the execution context explicitly supplies it. The report contains:

- the complete artifact-relative file inventory, byte lengths, and SHA-256 hashes;
- a deterministic artifact SHA-256 derived from ordered paths and file hashes;
- presence of the current root index, generated portfolio data, `/aws/` index, and `404.html`;
- metadata and local references from emitted HTML;
- missing or unsafe local references;
- presence or absence of common platform discovery files;
- whether `404.html` is byte-identical to the root index, an HTML refresh, missing, or otherwise runtime-defined;
- bounded path-based privacy violations;
- an explicit pass/fail result.

The current required paths include `work/index.html` because `/work/` is the implemented complete-portfolio route. `articles/index.html` and `aws/ai/index.html` remain non-required target routes. A missing future target route is baseline evidence, not an Increment 2 implementation failure. A local link from an existing emitted page to a missing resource is reported as a broken current-artifact reference.

The JSON report is execution evidence only when the named artifact was actually produced and inspected. A synthetic test fixture is never evidence about the public artifact. A passing report does not prove semantic approval, accessibility, workflow success, deployment, hosting behavior, or live public behavior.

## Manual observation record

Use the same artifact identity from the automated report for every observation. Record the source revision only when it is explicitly known. Store no personal learner state, private URLs, screenshots containing private data, raw diagnostics, or local filesystem paths in a public result.

For each observation record:

- artifact SHA-256;
- explicit source revision, if known;
- observation date and reviewer;
- environment: browser name and version, operating system, viewport, zoom, JavaScript state, and reduced-motion state;
- route or fallback request observed;
- result: `pass`, `fail`, or `not-determinable`;
- concise evidence and limitation.

Perform these bounded observations:

1. Load `/` directly. Record title, visible initial state, portfolio-data loading outcome, local asset loading, and ordinary navigation available before interaction.
2. Operate every current keyboard-reachable control on `/`. Record tab order, visible focus, facet selection, project disclosure, retry behavior when available, and whether focus becomes lost or obscured.
3. Review `/` at representative narrow, medium, and wide viewports. Use 320 CSS pixels, 768 CSS pixels, and 1280 CSS pixels unless the environment cannot provide them; record any substitution.
4. Review `/` at 200% and 400% browser zoom. Record reflow, horizontal scrolling, clipped content, and availability of controls and qualifications.
5. Enable reduced motion at the operating-system or browser-emulation level, reload `/`, and record whether movement and transitions respect the preference.
6. Disable JavaScript and load `/` directly. Record exactly what orientation, navigation, and approved portfolio meaning remain available. Do not convert absence of behavior into an inferred pass.
7. Load `/aws/` directly. Record title, language, layout, keyboard focus, asset loading, link destinations, and the observed result of following `/aws/ai/` without claiming that target route is required by Increment 2.
8. Request one unknown root path and one unknown path beneath `/aws/`. Record the actual status, final URL, rendered content, address-bar behavior, and whether the result is the root application, an HTML refresh, a host fallback, or a host 404.
9. Compare the observed fallback with the automated report's `effective404` classification. Record any host-dependent difference without changing the artifact report.

A procedure is not a result. Do not update verified-current documentation until the automated report and these observations identify the same artifact and provide the evidence needed for the specific correction.

## Tests

```bash
python tools/publication-baseline/tests.py
```

The tests use synthetic artifacts solely to verify determinism, identity changes, metadata inspection, effective-404 classification, missing-reference failure, privacy failure, target-route non-requirement, and revision validation.
