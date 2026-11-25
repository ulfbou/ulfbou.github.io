# Production article package consumption

`ulfbou/content` owns canonical article meaning and promotes immutable private GitHub Releases. This repository owns acquisition, validation, rendering and deployment. The fixture under `tools/article-package/parity/` proves compatibility only and is never a production pin.

## Reviewed production pin

`tools/article-package/production-pin.json` is configured only from validated immutable provider-release evidence. A configured review must bind the provider repository, full provider commit, release tag, publication ID, GitHub asset ID, exact asset filename, archive SHA-256, contract version, product and sorted expected slugs. Floating identities are rejected.

The private-repository credential is exposed only as `ARTICLE_PROVIDER_TOKEN` to the acquisition step. It needs read-only access to releases in `ulfbou/content`. Do not expose it to fork pull requests, generated pages, logs or artifacts. Pull requests continue to use the committed parity fixture. Production deployment must acquire the reviewed pin in a trusted branch context.

## Acquisition

```bash
ARTICLE_PROVIDER_TOKEN=... python tools/article-package/acquire.py \
  --pin tools/article-package/production-pin.json \
  --output .article-package/validated
```

The command resolves the exact tag, verifies the release target and asset ID/name, downloads only that asset, verifies the archive SHA-256 before extraction, rejects unsafe, duplicate and symbolic-link members, then invokes the existing consumer validator. It replaces the requested output only after full validation, preserving the previous validated input on failure.

The renderer cutover and deployment-workflow change must be made only after the real promotion evidence has populated and reviewed the production pin. Until that change, `content/articles/` remains the website build input and production cutover is not complete.
