# SAOS Provisional Decisions (v0.1)

This document formalizes the architectural decisions captured in `context/SAOS_CONTEXT.md`. These decisions are binding for Phase 0 and Phase 1 of the SAOS prototype.

| # | Decision | Specification |
|---|---|---|
| 1 | **Hash Alg** | SHA-256 hex lowercase |
| 2 | **Capability** | Opaque string, exact-match semantics |
| 3 | **DomainEvent** | `{id:uuidv4, ts:ISO8601, type:string, payload:object}` |
| 4 | **Manifest** | Closed schema (no extensions) with `prevHash` for lineage |
| 5 | **Timeout** | 5000ms for manifest validation and inter-zone coordination |
| 6 | **UUID** | v4 (standard) generated at build-time where applicable |
| 7 | **IndexedDB** | Primary storage for 'events' (raw) and 'summaries' (compacted) |
| 8 | **Compaction** | Triggered every 1000 events to produce summary state |
| 9 | **Navigation** | All routes MUST start with `/app/{id}/` prefix |
| 10 | **Error** | On hash mismatch, kernel must replay from the previous known good state |

## Kernel Law
- **Kernel Size**: Must remain < 800 LOC.
- **Responsibilities**: mount, mediate IPC, enforce navigation.
- **Isolation**: No shared state, no runtime discovery.
