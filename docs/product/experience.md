# Portfolio experience under `/work/`

## Purpose and authority

This document governs the portfolio experience accepted at `/work/`. The portfolio is one public area of the wider `ulfbou.github.io` platform and remains governed by portfolio-specific semantic authority.

The professional landing page at `/` provides concise orientation and launches visitors toward the platform’s public areas. It may present featured or selected portfolio material as a summary and entry point, but that material is not the complete portfolio. Portfolio exploration, project discovery, evidence, maturity, limitations, and detailed project stories belong to `/work/`.

The portfolio experience may link to `/` for professional orientation, `/articles/` for relevant technical writing, and `/aws/ai/` for relevant AWS Cloud and AI learning experiences. Cross-area links must identify the destination and its value. Navigation to another area does not grant the portfolio authority over landing-page, article, or learning meaning.

The accepted portfolio allocation is `/work/`, and the supplied repository implements it through `Pages/Work.razor` and `wwwroot/work/index.html`. The professional landing page is implemented separately at `/`.

## Portfolio exploration

Visitors first encounter the portfolio’s concise point of view, then choose a facet or browse all work, scan each project’s proposition, status, and technology, and expand a project without losing page context. Filtering changes which projects are emphasized or shown, while project ordering and selection remain governed by approved portfolio content.

Expanded detail shows story, contribution, reflection, artifact, evidence, limitations, project status, and applicable story blocks. Progressive disclosure keeps the overview concise while allowing visitors to inspect deeper material in place.

Story blocks may present approved prose, quotations, reflections, code, terminal material, images, links, evidence, callouts, relationships, replays, and diagnostic comparisons. They must preserve their approved order, reading depth, provenance, accessible labels, and type-specific meaning.

Evidence must remain subordinate to approved portfolio claims. Evidence presentation must not overwhelm the project story, create claims, imply unsupported maturity, or obscure qualifications and limitations. Limitations and project status must remain visible at the depth where they qualify the related meaning.

External links must have a clear purpose and must not imply that an external destination has approved a portfolio claim. A project’s evidence pointers, source links, tests, documentation, or related articles must remain distinguishable from the approved claim that they support or contextualize.

## Interaction and state

Selection changes emphasis, never truth. Filtering, sorting where admitted, expansion, collapse, and progressive disclosure must not alter approved portfolio meaning or create a different factual version of a project.

Essential meaning cannot depend on interaction. Controls must be native, keyboard-usable, visibly focused, and expose their state. Expanded and selected state must be communicated programmatically and visually without relying on color, hover, or motion alone.

Filtering must preserve an understandable empty state when no project matches. Changing a facet may close an expanded project, but it must not silently change project content. In-place expansion must preserve page context and predictable reading and tab order.

Motion may clarify selection or expansion but must remain restrained and optional. Reduced-motion preferences must be respected. Interaction feedback must explain the resulting state without disruptive layout movement.

## Direct loading and progressive enhancement

The portfolio must support direct loading at its allocated route and preserve enough context for visitors arriving without first using `/`. Direct loading must identify the portfolio area and provide understandable routes to the professional landing page and other principal areas.

Essential portfolio meaning and navigation must not depend on JavaScript-only discovery. When JavaScript is unavailable or an interaction cannot run, the experience should provide useful portfolio orientation, ordinary navigation, and access to essential approved meaning through the applicable static-publication design.

Loading, unavailable, empty, and failure states must be understandable. A portfolio-data failure must not invent content, expose diagnostics or private information, or present stale or partial material as current approved meaning. Retry behavior may be offered when supported.

## Responsive and accessible behavior

The portfolio must remain usable across narrow, medium, and wide layouts, touch input, text enlargement, browser zoom, and reflow. Essential facts, controls, evidence qualifications, project status, and limitations must not disappear at narrow widths.

Keyboard operation must cover facet selection, project expansion, links, story-block controls, and retry behavior where present. Focus must remain visible and predictable. Disclosure controls must communicate expanded state and their controlled content.

Code and terminal material must remain readable and selectable. Meaningful images require suitable alternatives. Quotations require attribution. Evidence and reflection must remain distinguishable. Contrast must remain sufficient across normal, hover, focus, selected, expanded, error, and disabled states.

Print behavior must preserve all portfolio projects and their details, including content hidden only for interactive filtering or collapsed presentation. Printed output must retain evidence, limitations, project status, readable artifacts, and essential qualifications without requiring interaction.

## Content and claim boundaries

The portfolio is curated rather than exhaustive. Counts of repositories, lines, files, packages, workflows, evidence paths, or other quantities are never quality scores or credibility scores.

Tone is precise, reflective, technically specific, transparent about maturity, and free from unsupported promotion. Project discovery and presentation must not invent claims, suppress limitations, or imply production readiness, adoption, impact, ownership, or completeness beyond approved source.

Generated projections, runtime data, repository observations, evidence, and presentation state are not semantic authorities. They may render or support approved portfolio meaning but cannot independently create, strengthen, approve, or publish it.

## Current and target behavior

### Verified current behavior

The supplied repository implements the portfolio at `/work/` as part of a .NET 10 Blazor WebAssembly application. It supports facet filtering, project discovery, in-place expansion, progressive disclosure, project status, technologies, story blocks, contribution, reflection, evidence, limitations, loading, empty and failure states, native controls, focus styles, reduced-motion styles, responsive layouts, and print expansion of filtered or collapsed detail.

The implementation loads `data/generated/portfolio.json`. `wwwroot/work/index.html` provides direct-entry metadata, root-relative framework and stylesheet references, and no-script orientation. Repository files establish the route and configured presentation, but not successful build, browser, accessibility, responsive, print, hosted, or live behavior. No-JavaScript access to complete interactive portfolio detail is not implemented; the host explicitly says JavaScript is required for filtering and disclosure.

### Continuing target behavior

`/work/` remains the complete portfolio allocation. `/` is the professional landing page. `/articles/` and `/aws/ai/` remain separate areas and are not established as complete current outputs. Further work must preserve approved meaning, evidence boundaries, limitations, accessibility, direct loading, and compatibility obligations.
