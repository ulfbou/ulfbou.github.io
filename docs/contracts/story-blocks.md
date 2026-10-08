# Story-block contract

Phase 3 adds reusable project-story blocks without moving runtime authority from `src/Ulfbou.Site/wwwroot/data/portfolio.json`.

## Common contract

Every block has a stable authored ID, unique non-negative order, explicit admitted type, accessible label, reading depth, provenance, publication metadata, and validated object payload.

Only published public blocks enter the generated projection. Candidate and private blocks remain non-public.

## Types

The admitted types are prose, quote, reflection, code, terminal, image, links, evidence, callout, relationship, replay, and diagnostic comparison.

Images require meaningful alternative text. Quotes require attribution. Reflection and evidence remain distinct contracts. Code and terminal payloads remain selectable text.

## Renderer boundary

Every admitted public type requires reusable renderer coverage. Publication validation fails if renderer coverage is absent. The renderer switches only on block type and contains no project-specific branches.

The development fallback is presentation-only and cannot permit publication.

## Compatibility

Existing Phase 2 project fields remain authoritative during this additive transition. Blocks that present established information use the exact approved Phase 2 wording or evidence values. The legacy runtime path remains authoritative.

## Extension rule

Adding a type requires one atomic change to its enum value, payload validation, reusable rendering, styling, tests, and this contract. Adding an ordinary project requires content rather than a project-specific component.
