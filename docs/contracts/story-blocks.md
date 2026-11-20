# Portfolio story-block contract

## Purpose and scope

Reusable story blocks extend portfolio project stories and portfolio content. They preserve approved portfolio meaning while allowing richer ordered presentation without project-specific rendering branches.

Story-block generation produces part of the bounded portfolio publication input. It does not allocate the final public route or grant ownership of an emitted output path. Final route and output-path allocation are governed by `docs/architecture/static-composition.md` and `docs/contracts/routes-and-output-ownership.md`.

The accepted portfolio route is `/work/`, and the supplied repository implements it through `Pages/Work.razor` and `wwwroot/work/index.html`. Story blocks remain portfolio-owned content and do not allocate routes beyond that boundary.

Approved portfolio source remains the semantic authority for story-block meaning. Evidence cannot independently create or approve portfolio claims, and generated portfolio data does not become semantic authority merely because it is validated, retained, rendered, or consumed at runtime.

The current portfolio runtime authority is `src/Ulfbou.Site/wwwroot/data/generated/portfolio.json`. Repository generation, runtime loading, validation, tests, and deployment-shape checks support that retained generated projection and reject the superseded `data/portfolio.json` deployment path.

## Common contract

Every block has a stable authored ID, unique non-negative order, explicit admitted type, accessible label, reading depth, provenance, publication metadata, and validated object payload.

Only published public blocks enter the generated projection. Candidate and private blocks remain non-public.

Published portfolio story blocks must preserve approved wording, values, qualifications, limitations, attribution, provenance, reading depth, visibility, and publication state. Generated blocks and runtime projections are replaceable outputs and cannot independently revise or approve their source meaning.

Block IDs must be unique within the project. Block orders must be unique and non-negative within the project. Generated portfolio output preserves admitted blocks in deterministic ascending order.

A published block payload must be an object and must satisfy the payload contract for its admitted type. Missing, empty, malformed, unknown, or unsupported required values fail validation rather than being inferred or replaced.

## Types

The admitted types are prose, quote, reflection, code, terminal, image, links, evidence, callout, relationship, replay, and diagnostic comparison.

The current field and payload expectations are:

- **Prose:** requires a non-empty `text` string.
- **Quote:** requires non-empty `text` and `attribution` strings.
- **Reflection:** requires a non-empty `text` string and remains distinct from evidence.
- **Code:** requires a non-empty `caption` string and a non-empty `lines` string array.
- **Terminal:** requires a non-empty `caption` string and a non-empty `lines` string array.
- **Image:** requires non-empty `source` and `alternative` strings.
- **Links:** requires a non-empty `items` array whose objects contain non-empty `label` and absolute HTTP or HTTPS `url` strings.
- **Evidence:** requires a non-empty `items` array whose objects contain non-empty `label` and `value` strings.
- **Callout:** requires a non-empty `text` string.
- **Relationship:** requires non-empty `subject`, `relationship`, and `target` strings.
- **Replay:** requires a non-empty `steps` string array.
- **Diagnostic comparison:** requires non-empty `before` and `after` strings.

Images require meaningful alternative text. Quotes require attribution. Reflection and evidence remain distinct contracts. Code and terminal payloads remain selectable text.

Evidence blocks present approved evidence values but cannot create, strengthen, approve, or publish portfolio claims. Diagnostic, replay, relationship, and callout blocks must remain bounded by approved portfolio source and must not infer missing meaning.

## Renderer boundary

Every admitted public type requires reusable renderer coverage. Publication validation fails if renderer coverage is absent. The renderer switches only on block type and contains no project-specific branches.

The development fallback is presentation-only and cannot permit publication. Unknown or unsupported block types must not be silently treated as a known type, omitted from public output, or published through fallback presentation.

Renderers must preserve the block’s accessible label, approved payload meaning, order, reading depth, and provenance relationship. Rendering must not invent text, attribution, evidence, links, alternatives, steps, relationships, or diagnostic values.

Code and terminal renderers preserve selectable text. Image renderers expose the approved alternative. Quote renderers expose attribution. Evidence renderers preserve labels and values. Expanded and interactive presentation must remain keyboard-operable, visibly focused, and understandable without color or motion alone.

## Compatibility

Existing approved project fields remain authoritative during this additive transition. Blocks that present established information use the exact approved existing wording or evidence values. The retained generated runtime path remains authoritative for current portfolio runtime consumption.

Existing project fields, story, contribution, reflection, proof, artifact, status, technologies, and limitations remain valid compatibility content. Introducing blocks must not remove, weaken, rename, reinterpret, or silently replace those fields unless a separately approved contract change preserves their complete operational meaning.

Projects without story blocks continue to render through the established project-detail presentation. Projects with blocks retain applicable compatibility presentation, including approved contribution and other established information not fully represented by blocks.

Forward compatibility requires unknown future types to fail governed validation and publication until their enum value, payload contract, renderer, styling, tests, accessibility behavior, and documentation are admitted together. Backward compatibility requires existing approved block types, field names, enum values, payload meaning, ordering, defaults, and validation behavior to remain stable unless an explicit reviewed contract change replaces them.

The runtime data path does not allocate the portfolio’s final public route. Generated runtime data remains a projection from approved portfolio source, not semantic authority.

## Validation and failure behavior

Portfolio story-block validation must establish:

- stable, non-empty block IDs;
- unique IDs and unique non-negative order within a project;
- admitted enum values;
- non-empty accessible labels;
- defined reading depth;
- complete provenance;
- valid publication metadata;
- public visibility for published blocks;
- object payloads with the required type-specific fields;
- renderer coverage for every published type;
- meaningful image alternatives;
- quote attribution;
- valid absolute HTTP or HTTPS link URLs;
- deterministic ordering;
- separation between reflection and evidence;
- preservation of approved wording and evidence values;
- exclusion of candidate and private blocks;
- compatibility with existing approved project fields;
- keyboard, responsive, zoom, reduced-motion, selectable-artifact, and print behavior required by the portfolio experience.

Validation fails for an unknown type, undefined enum value, duplicate identity, duplicate or negative order, missing provenance, invalid publication metadata, malformed payload, missing renderer, absent image alternative, absent quote attribution, invalid link URL, or any inability to preserve approved public meaning.

Publication must remain blocked when renderer coverage or another mandatory story-block result is unknown or failing. Development fallback cannot convert validation failure into publishable output. Failure must not silently omit the block, invent a payload, expose private or candidate material, substitute stale output, or publish a partial story as complete.

## Runtime consumption

Approved portfolio source is validated and compiled into `src/Ulfbou.Site/wwwroot/data/generated/portfolio.json`. The current portfolio service consumes that projection through the application-relative request `data/generated/portfolio.json`.

Only published public blocks are included in the generated projection. Their order, identity, type, accessible label, reading depth, provenance, and payload are preserved for runtime rendering.

Runtime consumption does not authorize changes to portfolio meaning, does not turn generated data into semantic authority, and does not allocate a final public route. The portfolio producer supplies one bounded input to the wider platform; platform composition and emitted-path ownership remain separate concerns.

## Extension rule

Adding a type requires one atomic change to its enum value, payload validation, reusable rendering, styling, tests, and this contract. Adding an ordinary project requires content rather than a project-specific component.

An extension must preserve source authority, public/private filtering, deterministic generation, renderer coverage, accessibility, compatibility, evidence boundaries, and failure behavior. A new type is not admitted merely because a renderer can display it or generated data contains its name.
