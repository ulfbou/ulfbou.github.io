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

Phase 1 may directly author prototype JSON. Phase 2 replaces it with generated data. Optional repository evidence must flow through deterministic package, optional suggestion, human review, approved source, then the public flow. No tool writes directly to public JSON.

UI state is selected facet, expanded project, and future optional depth/preferences. State changes disclosure, not source truth. Requests use the application base address; published artifacts contain only public-safe content; optional evidence failure cannot break core stories.

Do not add compilers, AOT, service workers, graph/PDF/carrier tooling, collectors, or LLM integration until a current phase and measured need justify them.
