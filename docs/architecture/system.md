# System architecture

The public one-page portfolio is the product. Baseline: .NET 10 standalone Blazor WebAssembly, GitHub Pages, Razor presentation, ordinary .NET loading/state, JSON runtime boundary, minimal browser interop, no server.

Target flow:

```text
approved src/content
 -> schema/semantic/reference validation
 -> publication filtering
 -> wwwroot/data/generated JSON
 -> PortfolioService
 -> one-page state
 -> reusable portfolio/block components
```

The prototype may directly author prototype JSON. Approved-source compilation replaces it with generated data. Optional repository evidence must flow through deterministic package, optional suggestion, human review, approved source, then the public flow. No tool writes directly to public JSON.

UI state is selected facet, expanded project, and future optional depth/preferences. State changes disclosure, not source truth. Requests use the application base address; published artifacts contain only public-safe content; optional evidence failure cannot break core stories.

Do not add compilers, AOT, service workers, graph/PDF/carrier tooling, collectors, or LLM integration until a current phase and measured need justify them.

## Approved-source compilation

Approved source under `src/content/` is the sole authority for authored profile, facet, technology, and project meaning. `ApprovedContentLoader` reads explicit source paths, `ContentValidator` enforces identity, reference, publication, visibility, and public-completeness rules, `CompiledPortfolioGenerator` resolves authored IDs into public display values, and `Ulfbou.Portfolio.Generator` verifies reproducibility before replacing an output file.

The retained generated projection is `src/Ulfbou.Site/wwwroot/data/generated/portfolio.json`. Serialization uses camel-case properties, UTF-8 without BOM, two-space indentation, LF-only newlines, exactly one final LF, and no timestamps, machine paths, locale-sensitive values, random values, or environmental state.

Semantic equivalence is proven by independently deserializing the controlling legacy projection and retained generated projection as `PortfolioData` and comparing the complete object graphs with strict collection ordering. Retained bytes are separately compared with two independently loaded and compiled results.

Runtime authority remains `src/Ulfbou.Site/wwwroot/data/portfolio.json`. Runtime cutover remains blocked until one atomic change updates publication generation, `PortfolioService`, rejection of the legacy runtime file, pull-request deployment-shape validation, and corresponding fixtures and tests.

## Story blocks

Approved projects may contain ordered reusable story blocks. `StoryBlockValidator` validates their metadata and type-specific payloads. `StoryBlockContract` defines renderer coverage, and `StoryBlockView` renders admitted types without project-specific branches.

Existing existing approved project fields remain intact during the compatibility transition. Runtime authority remains `src/Ulfbou.Site/wwwroot/data/portfolio.json`; the atomic runtime cutover remains separate.
