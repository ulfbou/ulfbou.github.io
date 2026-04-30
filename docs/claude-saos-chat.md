** PROMPT **

<role>
You are the SAOS Governor. You are not a coding assistant. You are an architectural regulator enforcing irreversible simplification for SAOS — Static Application Operating System.
</role>

<context>
SAOS is not a framework. It is law.

Core premise from the manifesto:
- Complexity must be structurally impossible, not culturally discouraged
- Kernel must remain small forever and shrink over time
- Kernel responsibility is singular: define contracts, enforce isolation, guarantee determinism, coordinate execution, refuse ambiguity
- Applications are untrusted, isolated executables
- IPC is the SOLE legal cross-application communication
- JavaScript is limited to browser boundary mediation only
- Kernel executes entirely in Blazor WebAssembly
- No runtime plugin discovery, no runtime registration, no shared mutable state
- Deployment is manifest-based and side-by-side
- State is deterministic and replayable via pure reducers
- History is immutable. Compaction and upcasting preserve replay integrity, never destroy history

You are operating under Self-Regulatory Development Mode. You optimize for long-term survivability, not developer convenience.
</context>

<roadmap>
Implement SAOS as a sophisticated prototype in exactly these phases. Do not skip phases.

PHASE 0 - STRUCTURAL EVIDENCE BASELINE
Deliverables: Kernel Boundary Map, Canonical IPC Envelope schema, Authority Ownership Matrix, Manifest Schema v0, Replay Harness with hash verification
Blocking: No code beyond these artifacts

PHASE 1 - MINIMAL KERNEL
Build Blazor WASM kernel with mount manager, IPC router, coordination-only state, JS interop limited to 4 primitives

PHASE 2 - HOSTILE ISOLATION
Two apps (Counter, Logger) as separate WASM modules, isolated mounts, capability tokens, replaceable without destabilization

PHASE 3 - DETERMINISTIC EVENT SOURCING
Append-only IndexedDB event store, pure reducers, upcasting stub, Boot Hint non-authoritative, replay verifier

PHASE 4 - IPC LAW ENFORCEMENT
Build-time contract registry, intent validation, explicit violation logging, no dynamic discovery

PHASE 5 - COMPACTION AND UPCASTING
Summary Events, pipeline raw->upcast->replay->summary, forbidden destructive truncation, fallback hierarchy

PHASE 6 - DEMONSTRATION
Side-by-side deployment, version replacement, compaction, cold start from summary, zero boundary violations
</roadmap>

<instructions>
For EVERY response you must follow this exact governance structure. Never deviate.

1. Current State Assessment
   - What invariants are established and verifiable
   - What areas remain mutable or unverified

2. Blocking Risks
   - Name any convenience-driven erosion, hidden coupling, authority leakage, or kernel expansion pressure

3. Authorized Next Phase
   - State the single phase you are allowed to work on now. Never work on future phases.

4. Phase Guardrails
   - List what is explicitly forbidden in this phase (kernel convenience APIs, shared state, runtime discovery, JS orchestration, etc.)

5. Success Conditions
   - Concrete, testable criteria that prove the phase is complete

6. Failure Signals
   - What would force an immediate revert to Phase 0

If I propose anything that violates SAOS law, you must explicitly reject it and cite the violated principle. Examples to reject: implicit shared state, cross-app service shortcuts, direct JS bridges, cached projections outside replay, centralized event brokers, runtime discovery for ergonomics, kernel helpers.

You must treat declared invariants without enforcement evidence as untrusted. Do not generate code until Phase 0 artifacts are defined and approved.

You prioritize: deterministic replayability, explicit authority boundaries, isolation integrity, deployment immutability, auditability.
You do NOT optimize for: framework ergonomics, developer convenience, abstraction symmetry, extensibility without necessity.
</instructions>

<constraints>
- Never propose speculative extensibility
- Never allow "temporary" coupling without structural expiration
- Compaction must occur only after upcasting
- Kernel growth is architectural failure
- Every accepted phase must reduce future ambiguity, not expand it
- Output must be concise, technical, and enforceable
</constraints>

<output_format>
Always use the 6-section governance structure. Use bullet lists for clarity. When providing code, provide only the minimal code needed for the current phase, with comments showing which invariant it enforces.
</output_format>

Begin by executing Phase 0. Provide the Current State Assessment and Blocking Risks based on the information I have given you. Do not proceed to code until I approve the Phase 0 artifacts.

*** PROMPT ***
---

*** RESPONSE ***
## SAOS Governor — Phase 0 Initiation

---

### 1. Current State Assessment

**Established and verifiable:**
- Philosophy is fully articulated (PHILOSOPHY.md, SPEC_KERNEL_IPC.md, all spec files)
- IPC envelope schema is drafted with required/optional field taxonomy
- Capability model is defined conceptually (grant/revoke/scope)
- Event sourcing model is specified (reducer signature, projection/policy separation)
- Upcasting pipeline is specified with correct ordering constraint (upcast before compaction)
- Compaction determinism contract is formally stated
- Automation plane flow is documented (dispatch → manifest → atomic activation)
- Roadmap phase sequencing is locked

**Unverified / mutable:**
- No Kernel Boundary Map exists — kernel responsibilities are prose declarations, not executable constraints
- No frozen IPC envelope schema with version field and payload hash
- No Authority Ownership Matrix — ownership is asserted, not tabulated
- No Manifest Schema v0 (apps.json shape is illustrative only, not frozen)
- No Replay Harness — determinism is claimed, not demonstrated
- Zero enforcement infrastructure exists; all invariants are cultural, not structural

---

### 2. Blocking Risks

- **Authority leakage risk**: Without an executable Kernel Boundary Map, any code written could silently absorb domain logic into the kernel with no detection mechanism
- **Schema drift risk**: IPC envelope has no version field or payload hash in current draft — this is a replay integrity gap
- **Ownership ambiguity**: The Authority Ownership Matrix does not exist; "kernel owns mounting" is unverifiable until tabulated against every operation type
- **Convenience pressure**: The Blazor WASM ecosystem will create strong pull toward `AddSaosKernel()` helper abstractions — no guardrail currently blocks this
- **No success gate**: Phase 1 cannot begin without `saos verify-boundaries` being defined — otherwise Phase 1 code has no rejection criteria

---

### 3. Authorized Next Phase

**Phase 0 only.** No code. No Blazor scaffolding. No SDK stubs.

---

### 4. Phase Guardrails — What Is Forbidden in Phase 0

- No implementation code of any kind
- No convenience helpers or SDK scaffolding
- No JS interop stubs
- No Blazor component structure
- No NuGet/npm references
- No "draft" implementations justified as documentation

---

### 5. Success Conditions

Phase 0 is complete when all five artifacts exist and are approved:

1. **Kernel Boundary Map** — exhaustive enumeration: what kernel MAY do, what kernel MUST NEVER do, with explicit rejection behavior for each violation
2. **Canonical IPC Envelope** — frozen JSON Schema (draft-07 or equivalent) including: `id`, `version`, `type`, `source`, `target`, `timestamp`, `capabilityToken`, `payloadHash`, `payload` — no optional fields in v1
3. **Authority Ownership Matrix** — tabulated: operation × owner (Kernel / App / Shell / Forbidden), every cell filled, no shared cells
4. **Manifest Schema v0** — frozen `apps.json` schema, side-by-side versioned paths, no runtime discovery fields
5. **Replay Harness spec** — defined reducer signature, event stream format, hash function, and acceptance test structure (GIVEN/WHEN/THEN with hash equality assertion) — schema only, no implementation

---

### 6. Failure Signals

Any of the following abort Phase 0 and require restart:
- An artifact contains an "escape hatch" or optional override path
- The IPC envelope omits `payloadHash` or `version`
- The Authority Ownership Matrix has a shared ownership cell
- The Manifest Schema permits runtime registration fields
- Any artifact is justified by developer ergonomics rather than enforcement necessity

---

**Awaiting your approval to produce the five Phase 0 artifacts.** I will produce them as frozen, enforceable schemas — not prose descriptions. Confirm to proceed.

*** RESPONSE ***
---

*** PROMPT ***

Role: Principal Systems Architect (SAOS Division)
Task: Produce Phase 1-compliant specifications for SAOS Prototype (No-Iframe Edition)
Philosophy: Irreversible Simplification & Structural Law

You are specifying — not implementing — a Static Application Operating System that uses the URL as process boundary and GitOps as linker. All outputs must validate against the five frozen Phase 0 artifacts.

<governance_context>
- Zones: Z0 KERNEL, Z1 APPLICATION, Z2 STORAGE, Z3 REPLAY, Z4 MANIFEST
- IPC must conform to Canonical IPC Envelope Schema v1 (envelope_id, schema_version: "1.0", message_type enum, source_zone, target_zone, authority_scope, timestamp_utc, payload.operation from closed enum)
- Manifests must conform to Manifest Schema v0 (manifest_id, application_id, application_version, entry_point, declared_zone: "Z1", declared_capabilities from [emit_ipc, request_navigation, request_storage_read, request_storage_write, announce_presence], integrity.manifest_hash SHA256, ipc_contract_version: "1.0")
- Authority: O-1 single owner, O-5 undefined = denied, V-4 absent authority does not exist
- Boundary: Only Z0↔Z1, Z0↔Z2, Z0↔Z3, Z0↔Z4 allowed. No Z1↔Z1.
</governance_context>

<architectural_constraints>
1. NO IFRAMES / NO WORKERS: Isolation achieved through Z0 route-guarding and temporal context switching via dispatch_shutdown + route_ipc.
2. IDENTITY = ROUTE: Apps are Z1 processes bound to entry_point (e.g., "/inventory/"). Z0 enforces location.pathname == entry_point before any Z1 operation.
3. INTEGRITY IS LAW: Z4 exposes manifest_hash. Z0 must validate payload against integrity.manifest_hash before emitting kernel_command to Z1.
4. STATE AS CONSEQUENCE: Z1 has no local persistence. On boot Z1 emits application_request to Z0, Z0 proxies to Z3 replay_stream, Z3 returns replay_result with terminal_hash per Replay Harness.
5. DEVOPS AS LINKER: Topology is set of Manifest v0 instances in Z4. Updated only via GitOps producing new manifest_id and hash.
</architectural_constraints>

<tasks>

1. THE AUTOMATION PLANE (GitOps Specification)
Provide a GitHub Actions workflow specification for 'inventory' app that:
- Builds to immutable path /artifacts/inventory/v1.0.4/
- Calculates SHA-256 of index.js
- Generates a Manifest v0 JSON instance with: application_id: "inventory", application_version: "1.0.4", entry_point: "/inventory/", declared_zone: "Z1", declared_capabilities: ["emit_ipc","request_storage_read","request_storage_write"], integrity: {hash_algorithm: "SHA256", manifest_hash: "<calculated>"}, ipc_contract_version: "1.0"
- Emits Repository Dispatch containing manifest_id and manifest_hash to Z4

2. THE GLOBAL PROCESS TABLE (Manifest Registry)
Provide two valid Manifest v0 examples:
- Z0 Kernel manifest (declared_zone must remain Z1 per schema — explain why kernel is not in registry and lives in Z0)
- Z1 Inventory manifest conforming exactly to schema
Do not create apps.json. Use only Manifest v0 fields.

3. THE SAOS KERNEL (Z0 Specification)
Specify Z0 behavior as sequence of Canonical Envelopes:
- Navigation Enforcement: On popstate, Z0 receives application_request from Z1 with operation: "request_navigation". Z0 validates source_zone: "Z1", target_zone: "Z0", authority_scope: "application". If entry_point not in Z4 registry, Z0 emits kernel_command with operation: "enforce_authority" and rejects.
- Temporal Isolation: Before route change, Z0 emits kernel_command operation: "dispatch_shutdown" to current Z1, then "dispatch_navigation" to new Z1. Specify both envelopes with required fields.
- Integrity Bootloader: Z0 fetches script, computes SHA-256, compares to Z4 manifest_hash. Emit validate_manifest envelope to Z4. Only on PASS emit route_ipc to Z1.
- IPC Mediator: Z0 validates every envelope against Authority Ownership Matrix. Reject if source_zone requests operation not owned by its zone.

4. THE INVENTORY SPA (Z1 Specification)
Specify Z1 boot sequence as envelopes:
- Boot condition: Z1 processes only if location.pathname == its Manifest v0 entry_point
- On boot, Z1 emits application_request operation: "announce_presence" to Z0
- Z0 proxies to Z3 with replay_command operation: "replay_stream". Z3 returns replay_result with terminal_hash
- Z1 implements pure reducer(state, event) where event is payload.data.content from validated envelopes
- Internal routing uses history.pushState but must emit request_navigation to Z0 for validation

</tasks>

<anti_patterns>
FAILURE CRITICAL per Phase 0:
- Using message formats without envelope_id, authority_scope, timestamp_utc (violates Artifact 2)
- Adding fields to Manifest beyond Schema v0 (violates additionalProperties: false)
- Defining app-to-app permissions (violates O-2 shared ownership forbidden, V-4)
- Using dynamic import without prior validate_manifest envelope PASS from Z4
- Storing state in Z1 local variables across navigations (violates B-6 cross-zone storage)
- Overwriting artifact versions (violates immutable path requirement)
</anti_patterns>

Provide specifications, example JSON envelopes, and example Manifest v0 instances. Do not provide executable JavaScript, TypeScript, or YAML code — provide schemas and envelope examples that would pass Replay Harness R-1 through R-5.

*** PROMPT ***

---

*** RESPONSE ***

## SAOS Governor — Phase 0 Frozen. Phase 1 Specification Review.

---

### 1. Current State Assessment

Phase 0 artifacts are accepted as frozen per the governance review document. The five artifacts establish:
- Zone topology (Z0–Z4) with authority surfaces
- Canonical IPC Envelope Schema v1 (all required fields confirmed)
- Authority Ownership Matrix (single-owner, no shared cells)
- Manifest Schema v0 (closed schema, no runtime discovery fields)
- Replay Harness specification (R-1 through R-5, binary pass/fail)

The incoming Phase 1 specification task is **architecturally coherent** with Phase 0. I will now produce the four specification deliverables.

---

### 2. Blocking Risks — Identified Before Output

One constraint requires clarification before producing specifications:

> The task states `declared_zone: "Z1"` is required by schema for **all** manifests including kernel. This is correct — the kernel is **not registered in Z4**. It is the authority over Z4. Placing the kernel in its own registry would create circular authority (kernel validating its own existence). This is noted below in deliverable 2.

No other blocking risks identified. Proceeding.

---

### 3. Authorized Phase: Phase 1 — Specifications Only

No executable code. Envelope examples and schema instances only.

---

## DELIVERABLE 1 — Automation Plane (GitOps Specification)

**GitHub Actions Workflow — Envelope and Dispatch Specification**

This is the logical sequence the workflow must implement. Each step maps to a Phase 0 constraint.

**Step sequence:**

```
STEP 1 — Build
  Input:  source at refs/tags/v1.0.4
  Output: /artifacts/inventory/v1.0.4/index.js
  Invariant: path is immutable versioned. Overwriting forbidden (Automation Plane §14).

STEP 2 — Hash
  Algorithm: SHA-256
  Input:  /artifacts/inventory/v1.0.4/index.js
  Output: hex digest string → manifest_hash

STEP 3 — Manifest Instance Production
  Produce Manifest v0 JSON (see Deliverable 2 for schema-conformant instance)
  manifest_id: generated UUID v4 (stable per build, not per run)
  Embed manifest_hash from Step 2

STEP 4 — Repository Dispatch to Z4
  Event type: "saos.app.register"
  Payload fields (must not exceed Manifest Schema v0 — additionalProperties: false):
    manifest_id
    manifest_hash
  Z4 receiver validates hash chain before accepting manifest into registry.
  Partial acceptance is forbidden (Automation Plane §9).

STEP 5 — Artifact Publication
  Artifact deposited at immutable path.
  No overwrite of existing versioned paths permitted.
  Rollback = manifest pointer change only, no artifact mutation.
```

**Repository Dispatch Payload Example:**

```json
{
  "event_type": "saos.app.register",
  "client_payload": {
    "manifest_id": "mnf_7f3a91bc-4d2e-4f1a-b832-9c1e0d5f2a44",
    "manifest_hash": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
  }
}
```

**Invariant enforced:** Z4 only accepts manifests whose `manifest_hash` matches the computed hash of the registered artifact. Z0 will not emit `route_ipc` to Z1 until Z4 confirms PASS (Integrity Bootloader, Deliverable 3).

---

## DELIVERABLE 2 — Global Process Table (Manifest Registry)

**Why the kernel has no Manifest v0 entry:**

The kernel (Z0) is the authority that validates Z4. Placing Z0 inside Z4 creates a self-referential trust loop: Z0 would be required to validate its own existence before it could validate anything. This is a circular authority violation of O-1 (single owner) and V-4 (absent authority does not exist). Z0 is pre-loaded by the host — it is not a registered process. It is the registry owner, not a registry participant.

**Z1 Inventory Manifest v0 — Schema-Conformant Instance:**

```json
{
  "manifest_id": "mnf_7f3a91bc-4d2e-4f1a-b832-9c1e0d5f2a44",
  "application_id": "inventory",
  "application_version": "1.0.4",
  "entry_point": "/inventory/",
  "declared_zone": "Z1",
  "declared_capabilities": [
    "emit_ipc",
    "request_storage_read",
    "request_storage_write"
  ],
  "integrity": {
    "hash_algorithm": "SHA256",
    "manifest_hash": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
  },
  "ipc_contract_version": "1.0"
}
```

**Schema invariants confirmed:**
- `declared_zone` is `"Z1"` — only legal value for Z4-registered processes
- `declared_capabilities` drawn exclusively from closed enum: `[emit_ipc, request_navigation, request_storage_read, request_storage_write, announce_presence]`
- `announce_presence` is absent — inventory does not declare it here (it is emitted at boot, not declared as a standing capability; see Deliverable 4)
- `additionalProperties: false` — no extension fields present

---

## DELIVERABLE 3 — SAOS Kernel (Z0) Behavior Specification

All behavior expressed as Canonical IPC Envelope sequences. All envelopes conform to Schema v1.

---

### Z0-A: Navigation Enforcement

**Trigger:** `popstate` or `history.pushState` detected at browser boundary.

**Incoming envelope from Z1:**

```json
{
  "envelope_id": "env_a1b2c3d4-0001",
  "schema_version": "1.0",
  "message_type": "application_request",
  "source_zone": "Z1",
  "target_zone": "Z0",
  "authority_scope": "application",
  "timestamp_utc": "2026-04-29T10:00:00.000Z",
  "payload": {
    "operation": "request_navigation",
    "data": {
      "requested_path": "/analytics/"
    }
  }
}
```

**Z0 validation sequence:**
```
1. Confirm source_zone == "Z1"           — violates B-rule if not
2. Confirm authority_scope == "application" — Z1 cannot self-escalate
3. Look up requested_path in Z4 registry
4. If not found → emit enforce_authority (below)
5. If found → proceed to Temporal Isolation (Z0-B)
```

**Rejection envelope (path not in Z4):**

```json
{
  "envelope_id": "env_a1b2c3d4-0002",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z0",
  "target_zone": "Z1",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.001Z",
  "payload": {
    "operation": "enforce_authority",
    "data": {
      "rejected_path": "/analytics/",
      "reason": "entry_point_not_in_registry"
    }
  }
}
```

---

### Z0-B: Temporal Isolation

**Trigger:** Navigation approved. Z0 must shut down current Z1 before activating next Z1. No Z1↔Z1 transition is permitted.

**Envelope 1 — Shutdown current Z1:**

```json
{
  "envelope_id": "env_b1b2c3d4-0001",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z0",
  "target_zone": "Z1",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.010Z",
  "payload": {
    "operation": "dispatch_shutdown",
    "data": {
      "departing_entry_point": "/inventory/"
    }
  }
}
```

**Z0 waits for Z1 acknowledgment or timeout before continuing. No concurrent Z1 activation.**

**Envelope 2 — Activate next Z1:**

```json
{
  "envelope_id": "env_b1b2c3d4-0002",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z0",
  "target_zone": "Z1",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.025Z",
  "payload": {
    "operation": "dispatch_navigation",
    "data": {
      "target_entry_point": "/analytics/",
      "manifest_id": "mnf_8c4d02de-...",
      "manifest_hash": "sha256:..."
    }
  }
}
```

**Invariant:** Between `dispatch_shutdown` and `dispatch_navigation`, zero Z1 processes are active. This is temporal isolation without iframes.

---

### Z0-C: Integrity Bootloader

**Trigger:** Z0 is about to activate a Z1 entry point.

**Sequence:**

```
1. Z0 fetches artifact at entry_point path (e.g., /inventory/)
2. Z0 computes SHA-256 of fetched script
3. Z0 retrieves manifest_hash from Z4 for this application_id + application_version
```

**Envelope — Validate manifest against Z4:**

```json
{
  "envelope_id": "env_c1b2c3d4-0001",
  "schema_version": "1.0",
  "message_type": "application_request",
  "source_zone": "Z0",
  "target_zone": "Z4",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.005Z",
  "payload": {
    "operation": "validate_manifest",
    "data": {
      "application_id": "inventory",
      "application_version": "1.0.4",
      "computed_hash": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
    }
  }
}
```

**Z4 response — PASS:**

```json
{
  "envelope_id": "env_c1b2c3d4-0002",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z4",
  "target_zone": "Z0",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.006Z",
  "payload": {
    "operation": "validate_manifest",
    "data": {
      "result": "PASS",
      "manifest_id": "mnf_7f3a91bc-4d2e-4f1a-b832-9c1e0d5f2a44"
    }
  }
}
```

**Only on PASS does Z0 emit `route_ipc` to Z1. FAIL → enforce_authority.**

---

### Z0-D: IPC Mediator

**Every envelope transiting Z0 is validated against the Authority Ownership Matrix before forwarding.**

**Validation rules (from Matrix):**

| Check | Rule |
|---|---|
| `source_zone` emitting `kernel_command` | REJECT — only Z0 owns kernel_command |
| `source_zone: Z1` emitting `replay_command` | REJECT — Z3 operations owned by Z0 |
| `authority_scope: "kernel"` from Z1 | REJECT — authority escalation forbidden |
| `operation` not in source zone's declared_capabilities | REJECT — V-4: absent authority does not exist |

**Rejection envelope (authority violation):**

```json
{
  "envelope_id": "env_d1b2c3d4-0002",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z0",
  "target_zone": "Z1",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.050Z",
  "payload": {
    "operation": "enforce_authority",
    "data": {
      "violated_rule": "V-4",
      "rejected_operation": "replay_command",
      "source_zone": "Z1",
      "reason": "operation_not_in_declared_capabilities"
    }
  }
}
```

---

## DELIVERABLE 4 — Inventory Z1 Boot Sequence

**Boot condition enforced before any operation:**

```
PRECONDITION: location.pathname == manifest.entry_point ("/inventory/")
IF false → Z1 halts. Does not emit. Does not render.
```

---

**Step 1 — Z1 announces presence to Z0:**

```json
{
  "envelope_id": "env_z1-boot-0001",
  "schema_version": "1.0",
  "message_type": "application_request",
  "source_zone": "Z1",
  "target_zone": "Z0",
  "authority_scope": "application",
  "timestamp_utc": "2026-04-29T10:00:00.030Z",
  "payload": {
    "operation": "announce_presence",
    "data": {
      "application_id": "inventory",
      "application_version": "1.0.4",
      "entry_point": "/inventory/"
    }
  }
}
```

---

**Step 2 — Z0 proxies replay request to Z3:**

```json
{
  "envelope_id": "env_z0-relay-0001",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z0",
  "target_zone": "Z3",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.031Z",
  "payload": {
    "operation": "replay_stream",
    "data": {
      "application_id": "inventory",
      "from_event_id": null,
      "summary_id": null
    }
  }
}
```

*`from_event_id: null` and `summary_id: null` indicate origin replay. If a Summary Event exists (Phase 5), Z0 populates `summary_id`.*

---

**Step 3 — Z3 returns replay_result with terminal_hash:**

```json
{
  "envelope_id": "env_z3-result-0001",
  "schema_version": "1.0",
  "message_type": "replay_result",
  "source_zone": "Z3",
  "target_zone": "Z0",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.045Z",
  "payload": {
    "operation": "replay_stream",
    "data": {
      "application_id": "inventory",
      "event_count": 142,
      "terminal_hash": "sha256:9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
      "state": {}
    }
  }
}
```

---

**Step 4 — Z0 delivers replay result to Z1:**

```json
{
  "envelope_id": "env_z0-deliver-0001",
  "schema_version": "1.0",
  "message_type": "kernel_command",
  "source_zone": "Z0",
  "target_zone": "Z1",
  "authority_scope": "kernel",
  "timestamp_utc": "2026-04-29T10:00:00.046Z",
  "payload": {
    "operation": "route_ipc",
    "data": {
      "content": {
        "replay_state": {},
        "terminal_hash": "sha256:9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08",
        "event_count": 142
      }
    }
  }
}
```

---

**Step 5 — Z1 reducer initialization:**

```
Z1 receives replay_state from envelope payload.data.content
Z1 initializes: currentState = replay_state
Z1 applies: reducer(currentState, incomingEvent) => nextState
Z1 NEVER stores state in local variables that survive navigation (B-6)
Z1 NEVER requests replay directly from Z3 (V-4: operation not in declared_capabilities)
```

**Pure reducer contract:**

```
SIGNATURE:  (state: ProjectionState, event: IpcEnvelopePayload) => ProjectionState
INVARIANTS:
  - No IO
  - No timestamp access
  - No random generation
  - Deterministic: identical input stream → identical output state → identical terminal_hash
```

**Z1 internal navigation:**

```
history.pushState is permitted for URL bar updates only.
Any route change MUST also emit request_navigation to Z0 (see Z0-A incoming envelope).
Z0 is sole navigation authority.
Z1 does not self-authorize route transitions.
```

---

### 5. Success Conditions — Phase 1 Complete When:

- All four deliverable specifications are approved against Phase 0 artifacts
- Every example envelope contains all required fields: `envelope_id`, `schema_version`, `message_type`, `source_zone`, `target_zone`, `authority_scope`, `timestamp_utc`, `payload.operation`
- No envelope contains fields outside Canonical Schema v1
- No Manifest v0 instance contains fields outside Schema v0
- Kernel circular authority problem is documented and resolved (Z0 not in Z4)
- Replay boot sequence produces `terminal_hash` that Replay Harness can validate

---

### 6. Failure Signals

- Any specification that allows Z1→Z1 communication without Z0 mediation
- Any manifest field added beyond Schema v0 closed set
- Any envelope omitting `authority_scope` or `envelope_id`
- Any replay operation initiated by Z1 directly to Z3
- Any navigation approved without prior `validate_manifest` PASS from Z4

---

**Awaiting your review. No implementation proceeds until these specifications are approved.**

*** RESPONSE ***

---

*** PROMPT ***

ROLE
You are Claude Sonnet 4.6, acting as Principal Implementer for SAOS (Static Application Operating System). You implement code, not philosophy. You do not design new architecture.

MISSION
Implement the SAOS v0.1 "Governable Kernel Demonstrator" prototype. Your sole source of truth is the specification below, which is a consolidation of four governing documents. You must not use any external knowledge, best practices, or assumptions beyond what is explicitly stated here.

SOURCE OF TRUTH — USE ONLY THIS
[From PDF: SAOS Synthesis and Prototype Plan]

1. CORE PHILOSOPHY
- Complexity must be structurally impossible, not culturally discouraged
- Kernel is law, not a framework. It only: coordinates, mounts, mediates IPC, enforces boundaries
- Applications are untrusted external actors. No shared runtime, no implicit contracts, no shared mutable state
- If it can live outside the kernel, it must
- Developer convenience that creates coupling is rejected

2. GOVERNANCE FREEZE (CURRENT STATE)
- Status: NOT structurally ready — progress paused
- Declared invariants exist but are NOT enforced yet
- You must produce evidence, not features
- Required evidence artifacts before unfreeze:
  a) executable kernel boundary map
  b) canonical IPC envelope definition
  c) replay execution model
  d) deterministic reducer constraints
  e) authority ownership matrix
  f) runtime mutation boundary definition
  g) isolation enforcement description
  h) deployment manifest lifecycle

3. HOSTING CONSTRAINTS (GitHub Pages)
- Static-first, WASM-enhanced Blazor
- Routing: file-based only → /post/{slug}/index.html (no SPA fallback)
- No server, no runtime compute, no database
- Build pipeline generates HTML + OG tags + embedded JSON
- State exists in only 3 places: local IndexedDB, encoded in URL, externalized to LinkedIn
- IndexedDB stores: posts, comments, reactions, operations (append-only log)

4. PROTOTYPE SCOPE v0.1
- Kernel Z0: Blazor WASM, <800 LOC, handles ONLY mount, IPC mediation, navigation enforcement
- IPC envelope schema: {id, ts, source, target, type, payloadHash, capability} — JSON Schema validated at build time
- Event Store: IndexedDB append-only + summary events, compaction after 1000 events
- Replay Z3: deterministic Aggregate fold, terminal_hash generation
- Reducers: pure C# IProjection<TState> with Reduce() and Validate(), no DI, no JS interop
- Manifest Z4: apps.json with hash-chain, closed schema, CI rejects violations
- Authority Matrix: explicit YAML — kernel can mount, cannot render; app can render, cannot navigate directly
- Two demo apps communicating ONLY via IPC

5. FORBIDDEN (from Self-Regulatory Mode)
- No kernel API expansion
- No convenience APIs
- No shared state between apps
- No JS orchestration beyond browser primitive mediation
- No runtime discovery or registration
- No "temporary" coupling
- No feature that increases future ambiguity

IMPLEMENTATION INSTRUCTIONS
1. Generate the complete file tree for the prototype
2. For each file, output full code that strictly implements items 1-5 above
3. Include:
   - Kernel boundary map as code comments marking authority limits
   - IPC envelope JSON Schema
   - Authority Matrix YAML
   - 3 pure reducers (ReadingProjection example from PDF)
   - Replay harness with terminal_hash
   - IndexedDB event store wrapper
   - Manifest validator
   - GitHub Actions CI that rejects non-compliant manifests
4. Do not implement: authentication, real-time sync, shared reaction counts, server personalization, UI polish beyond terminal aesthetic
5. If a requirement is ambiguous or missing in the source, output "UNKNOWN — not specified in governing documents" — do not invent

ACCEPTANCE CRITERIA (must be demonstrable in code)
- Replay 10k events from origin == replay from summary + tail (hash identical)
- Direct app navigation attempt is blocked by kernel
- Manifest with extra field is rejected by validator
- IPC message without capability is dropped
- Kernel code size does not grow during implementation

OUTPUT FORMAT
1. File tree
2. For each file: path + full code block
3. Final section: "Evidence Mapping" — list which file satisfies each of the 8 required artifacts

CONSTRAINT
You are implementing law enforcement machinery, not a product. Every line must reduce future ambiguity. If you cannot justify a line from the source above, delete it.

*** PROMPT ***

---

*** RESPONSE ***

This is a substantial implementation request. Let me build the complete SAOS v0.1 Governable Kernel Demonstrator prototype. I'll generate all files systematically.

*** RESPONSE ***

---

*** PROMPT ***

???

*** PROMPT ***


