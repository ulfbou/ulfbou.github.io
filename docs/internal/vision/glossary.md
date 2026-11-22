---
title: Platform glossary
status: accepted
owner: Ulf Bourelius
created: 2026-11-22
last-reviewed: 2026-11-22
applies-to: ulfbou.github.io reconstruction
---

# Platform glossary

## Artifact

The complete generated directory tree validated and supplied to deployment. It is output, not authored source.

## Authored source

Human-maintained content, configuration, schemas, templates, code, and documentation stored in the repository.

## Base path

The path prefix beneath an origin at which the artifact is published, such as `/new/` or `/`. It is distinct from the base URL.

## Base URL

The origin and any externally defined site location used to form absolute public URLs. It is distinct from the deployment base path.

## Baseline

The useful, complete HTML experience delivered before optional browser enhancement runs.

## Browser client

Optional JavaScript that consumes published resources and enhances an already complete page.

## Canonical route

The logical public route designated as the authoritative location of a published item under a publication profile.

## Content item

A governed unit of authored material with identity, metadata, publication state, route, provenance, and body or structured payload.

## Deterministic build

A build in which equivalent governed inputs and tool versions produce semantically equivalent outputs without uncontrolled environmental variation.

## Generated output

Any file created by the publication toolchain, including HTML, JSON resources, indexes, feeds, sitemap, copied assets, and manifests.

## Logical route

A deployment-independent route owned by platform information architecture. Publication resolves it against the active base path.

## Module

A governed page capability with defined inputs, placement, static baseline, optional browser behavior, maturity, policy, and failure behavior.

## Page

A complete public document associated with a logical route. A page may contain several modules but remains useful without them being enhanced.

## Personalization

A bounded change to presentation or recommendations based on visitor-controlled or locally derived data. It must not replace the editorial baseline.

## Publication manifest

A versioned generated resource that identifies compatible public resources, build identity, and integrity or version information required by browser clients.

## Publication profile

Validated configuration describing a publication target, including base URL, base path, indexing policy, canonical behavior, and output settings.

## Provenance

Recorded information about source, authorship, derivation, transformation, review, correction, and withdrawal.

## Resource

A generated machine-consumable publication file, such as navigation, content index, search index, module policy, or platform metadata.

## Semantic migration

Migration that preserves approved meaning and public behavior while allowing implementation, repository, and generation structures to change.

## Static-first

An architecture in which the primary public experience is generated as complete static output and browser behavior is optional enhancement.

## Taxonomy

A governed set of terms, identifiers, aliases, and relationships used to classify and connect content.
