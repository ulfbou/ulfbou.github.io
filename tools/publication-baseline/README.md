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

1. Load `/` directly and record landing-page title, initial state, assets, Home/Work navigation, GitHub link, focus, responsive behavior, zoom, reduced motion, and no-JavaScript orientation.
2. Load `/work/` directly and record portfolio title, generated-data loading, framework and stylesheet loading, navigation, facet controls, project disclosure, retry behavior, focus, responsive behavior, zoom, reduced motion, print behavior, and the no-script orientation. Do not infer access to complete portfolio detail without JavaScript.
3. Load `/aws/` directly and record title, language, layout, focus, assets, links, and the observed result of `/aws/ai/` without treating that target as a required current path.
4. Request one unknown root path and one unknown `/aws/` path. Record status, final URL, content, address-bar behavior, and fallback classification.
5. Compare observed fallback behavior with the automated report's `effective404` classification without changing the artifact report.

A procedure is not a result. Do not update verified-current documentation until the automated report and these observations identify the same artifact and provide the evidence needed for the specific correction.

## Tests

```bash
python tools/publication-baseline/tests.py
```

The tests use synthetic artifacts solely to verify determinism, identity changes, metadata inspection, effective-404 classification, missing-reference failure, privacy failure, target-route non-requirement, and revision validation.
