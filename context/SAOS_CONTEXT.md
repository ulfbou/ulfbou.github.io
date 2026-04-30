# SAOS v0.1 Context

## Provisional Decisions (Option C)
1. Hash: SHA-256 hex lowercase
2. Capability: opaque string, exact-match
3. DomainEvent: {id:uuidv4, ts:ISO8601, type:string, payload:object}
4. Manifest: closed schema with prevHash
5. Timeout: 5000ms
6. UUID: v4 build-time
7. IndexedDB: stores 'events' and 'summaries'
8. Compaction: @1000 events
9. Navigation: route must start with /app/{id}/
10. Error: on hash mismatch, replay from prev

## Kernel Law
- Kernel <800 LOC
- ONLY: mount, mediate IPC, enforce navigation
- No shared state, no runtime discovery
