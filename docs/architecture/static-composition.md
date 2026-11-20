# Static composition architecture

## Purpose and authority

Static composition is the accepted architecture for assembling independently produced public areas into one deployable static site artifact. This document governs architecture, output boundaries, and technical responsibilities. It does not govern landing-page, portfolio, article, or learning meaning.

Semantic authority remains with `docs/platform.md`, the applicable area-specific product document, and approved area source. `docs/publication.md` governs approval and release. `docs/contracts/routes-and-output-ownership.md` governs canonical routes and emitted-path ownership.

## Architectural principles

- Semantic authority remains with the applicable platform or area source.
- Producers generate bounded, route-neutral publication units.
- A platform-owned publication plan maps admitted units to concrete output paths.
- Composition starts from an empty destination.
- Every emitted path has exactly one authorized owner.
- Composition produces one complete static site artifact.
- Required validation completes before deployment.
- Deployment publishes the validated artifact without changing its meaning or contents.
- Infrastructure must not create, strengthen, weaken, summarize, or approve public meaning.

A technical component may perform several roles, but role responsibilities and authorities must remain distinguishable.

## Publication units

A publication unit is bounded static output from one authorized producer. A unit must:

- contain every file required for its admitted scope;
- remain independent of its final route, which is not established merely by internal directory layout;
- claim no output outside its admitted allocation;
- expose enough identity, inventory, provenance, and validation information for composition;
- exclude private, candidate, diagnostic, temporary, and undeclared content;
- be reproducible under its governing production contract;
- preserve approved meaning and required accessibility behavior.

A produced unit is not proof of publication approval. Units may use different frameworks, languages, build systems, metadata models, and repository layouts when their outputs satisfy the shared contracts.

## Publication plan

The publication plan is the platform-owned allocation between admitted units and concrete artifact-relative output paths. It must identify, directly or through traceable governed records:

- unit identity and producer identity;
- the admitted source artifact;
- destination path or path prefix;
- permission for protected paths;
- compatibility and redirect obligations;
- required area and platform validation;
- deterministic ordering where ordering affects retained output;
- artifact identity or digest where available.

Undeclared units and duplicate ownership must be rejected. The plan grants output ownership, not semantic authority or release authorization. This contract does not require a particular manifest filename or serialization format.

## Clean composition

Composition must:

1. create a new destination or prove that the destination is empty;
2. admit only units authorized by the publication plan;
3. normalize and validate every destination path;
4. reject absolute, drive-qualified, malformed, and traversing paths;
5. reject ownership collisions after normalization;
6. reject protected-path claims without explicit permission;
7. materialize each admitted unit completely;
8. ensure no stale file from an earlier composition survives;
9. produce a complete deterministic inventory of emitted files;
10. run required area-specific and platform-wide validation;
11. assign a stable digest or equivalent identity to the final artifact;
12. stop before deployment when any mandatory condition is unknown or fails.

Incremental overlay onto an unknown previous output directory is not an accepted composition model. Failure must not preserve partial output as a release candidate.

## Emitted-path ownership

Ownership is enforced at the normalized emitted-path level.

- Source-folder ownership does not establish a public route.
- Producer identity and framework output conventions do not override the publication plan.
- Two units must not emit the same file.
- Overlapping prefixes require explicit rules proving that all resulting file ownership is non-conflicting.
- A file path cannot also be required as a directory by another unit.
- Shared generated assets require an explicit owner.
- Root and platform-wide files require explicit protection.

A producer may create route-neutral files internally, but the composer must place them only at paths allocated by the plan and must not rewrite their public meaning.

## Protected output

Protected output includes any path whose meaning or behavior affects more than one area. The protected set may include:

- `/index.html` and `/404.html`;
- platform navigation and shared identity assets;
- canonical metadata;
- sitemap, feed, and robots output;
- shared manifests and shared assets;
- redirect artifacts;
- other platform-owned root files.

This list describes protected classes and does not claim that every output exists. An area producer may emit a protected path only when the publication plan explicitly grants authority for that exact path or bounded class.

## Determinism and artifact identity

Equivalent admitted units, publication-plan inputs, and composition configuration must produce semantically equivalent output. Retained inventories and manifests must use stable ordering. Machine-specific paths, ambient state, unstated network results, timestamps, locale-sensitive ordering, and randomness are excluded unless a governing contract explicitly admits and records them.

The final artifact must receive a stable digest or equivalent immutable identity. The release record must trace that artifact to admitted units and source revisions. Deployment must verify that the selected artifact is the artifact that passed validation.

Semantic equivalence is always required. Byte identity is required when the applicable producer, inventory, packaging, or artifact contract requires byte reproducibility.

## Responsibilities

- **Semantic authorities** approve public meaning, qualifications, limitations, and corrections.
- **Producers** generate complete bounded static units from approved source.
- **The publication plan** allocates concrete emitted paths and protected permissions.
- **The composer** assembles admitted units without changing meaning.
- **Validators** check producer-specific and platform-wide contracts, ownership, privacy, accessibility, integrity, and compatibility.
- **The deployer** publishes the already validated artifact without substituting or modifying output.
- **Ulf** authorizes the exact release.

Combining technical roles must not combine their authorities. Build success, validation success, or possession of a path does not authorize publication.

## Failure and safe-stop behavior

Composition must fail when:

- a required unit is missing, unidentified, undeclared, or not traceable;
- ownership is absent, duplicated, overlapping without proof, or ambiguous;
- a path is absolute, drive-qualified, malformed, unsafe, or escapes the destination;
- a protected path is claimed without permission;
- the destination is not clean;
- stale, unexpected, partial, or undeclared output exists;
- mandatory validation is unknown or failing;
- private, candidate, draft, diagnostic, or temporary material is detected;
- the composed output differs from the artifact selected for deployment;
- infrastructure cannot preserve authorized meaning.

Failure must not silently deploy stale output, partial output, placeholders, or a previous artifact. Diagnostics should identify the failed contract and affected path or unit without exposing private content.

## Current, transitional, and target behavior

### Verified current behavior

The repository contains one .NET 10 Blazor WebAssembly site project with the professional landing page at `/` and the complete portfolio at `/work/`. Root and work have distinct host documents, metadata, descriptions, and canonical URLs. The workflow is configured to publish one `wwwroot` tree, replace the published `404.html` with the root host, create `.nojekyll`, inspect the produced tree, upload it as one Pages artifact, and deploy it.

The publication-baseline verifier computes a deterministic inventory and artifact SHA-256; requires `index.html`, `work/index.html`, `aws/index.html`, `data/generated/portfolio.json`, and `404.html`; inspects HTML metadata and local references; applies bounded privacy-path rules; and classifies effective `404.html` behavior. This does not prove a clean multi-unit composer, a machine-enforced publication plan, generalized emitted-path ownership, or identity preservation after upload.

### Verified transitional behavior

The root/work allocation is implemented. A static `/aws/` index links to `/aws/ai/`, but the supplied repository does not contain `src/Ulfbou.Site/wwwroot/aws/ai/index.html`. Documentation for `/articles/` exists, but no article output is established.

### Accepted target behavior

Independent area producers create bounded route-neutral units. A platform-owned plan assigns their output to `/`, `/work/`, `/articles/`, and `/aws/ai/`. Clean composition creates one validated static artifact with deterministic inventory, explicit protected-path ownership, traceable identity, and safe-stop behavior. This target is accepted architecture, not a claim of current implementation.

## Non-goals

Static composition is not:

- a semantic authority or content-management system;
- a requirement that all areas use one framework;
- permission for one producer to write the entire site;
- permission to infer route ownership from source layout;
- publication authorization;
- a mechanism for promoting private or candidate content;
- evidence that the target composer, plan, manifest, validator, or immutable-artifact pipeline already exists.
