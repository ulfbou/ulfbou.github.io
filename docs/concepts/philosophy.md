---
title: "Zentient: Philosophy"
slug: "zentient-philosophy"
date: "2025-09-04"
authors:
  - name: "Ulf Bourelius"
category: concepts
tags:
  - design
  - principles
summary: "Core design principles of the Zentient Framework: modularity, clarity, and testability."
---

# Zentient — Philosophy

> Zentient is a design-first framework: favor small, well-tested modules over large, tightly-coupled systems.

:::note
This page is canonical. For implementation notes see the Architecture page.
:::

## Key principles

1. **Modularity** — components are small, composable, and well-documented.  
2. **Observability** — defaults for telemetry so teams have immediate insights.  
3. **Contract-first** — heavy use of interfaces and minimal surface area.

## Patterns: quick comparison

```table
| Pattern | Benefit | When to use |
|---|---:|---|
| Adapter | Encapsulate third-party APIs | Integration layer |
| Mediator | Decouple components | Complex workflows |
````

## Example: When to choose modularization

```alert
type: warning
title: When not to over-modularize
Avoid creating micro-modules for single-use code — prefer cohesive modules that are useful across features.
```

### See also

* [Architecture](./architecture.md)
* [Modularity pattern](./patterns/modularity.md)

