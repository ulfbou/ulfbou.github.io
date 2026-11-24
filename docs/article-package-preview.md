# External article package preview

`tools/article-package/consumer.py` is the website-owned boundary for a `ulfbou/content` article publication package. It validates contract version `1.0`, the exact manifest inventory, byte counts, SHA-256 values, publication eligibility, article/series relationships, and manifest-backed bodies and social images before invoking the existing website-owned renderer.

The preview is isolated under `artifacts/article-package-preview/`; production `src/Ulfbou.Site/wwwroot/articles/`, the deployment workflow, and canonical content are not replaced.

```bash
python tools/article-package/consumer.py /path/to/article-package.zip --repo .
python -m unittest discover -s tools/article-package/tests -v
```

After parity review, a later increment may wire a pinned package identity into GitHub Actions. This preview increment deliberately does not choose or activate cross-repository credentials, trigger deployment, remove `content/articles/`, or alter production routes.
