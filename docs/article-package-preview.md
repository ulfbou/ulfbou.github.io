# External article package preview

`tools/article-package/consumer.py` is the website-owned boundary for a `ulfbou/content` article publication package. It validates contract version `1.0`, the exact manifest inventory, byte counts, SHA-256 values, publication eligibility, article/series relationships, and manifest-backed bodies and social images before invoking the existing website-owned renderer.

The preview is isolated under `artifacts/article-package-preview/`; production `src/Ulfbou.Site/wwwroot/articles/`, the deployment workflow, and canonical content are not replaced.

```bash
python tools/article-package/consumer.py /path/to/article-package.zip --repo .
python -m unittest discover -s tools/article-package/tests -v
```

After parity review, a later increment may wire a pinned package identity into GitHub Actions. This preview increment deliberately does not choose or activate cross-repository credentials, trigger deployment, remove `content/articles/`, or alter production routes.

## Deterministic fixture parity gate

The parity gate proves that the reviewed two-article fixture and the website-local article source produce semantically equivalent website-owned output through the same `src/Ulfbou.Articles` renderer. The fixture pin is a closed contract, identifies provider revision `c59f549ee2ea93ca84dbd8e17c5330f06edad1e2`, and is explicitly marked `fixtureOnly`. It does not represent a GitHub Release.

Run from the repository root:

```bash
python tools/article-package/parity.py \
  --local-source content/articles \
  --package tools/article-package/parity/fixtures/articles-parity.zip \
  --pin tools/article-package/parity/pin.json \
  --output artifacts/article-package-parity
```

Regenerate the reviewed fixture deterministically with:

```bash
python tools/article-package/parity/generate_fixture.py
```

The command verifies the archive digest before extraction, reuses the package consumer, invokes the real .NET renderer twice in isolated roots, compares HTML, JSON, Atom, sitemap, canonical metadata, and social images, and inventories protected production paths before and after execution. It safely replaces its own artifact root and removes temporary package state after success or failure.

Reports are written to:

- `artifacts/article-package-parity/reports/parity-report.json`
- `artifacts/article-package-parity/reports/parity-report.md`

`FIXTURE PARITY PROVEN` means only that the committed deterministic fixture matches the website-local source under this gate. `FIXTURE PARITY NOT PROVEN` means a required stage failed, was skipped, or found a material difference. Neither verdict authorizes production cutover. The gate performs no live release retrieval, credential use, workflow deployment, production-source replacement, or Pages deployment.
