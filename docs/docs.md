%%DX v1.3 author=tool
%%FILE path="docs/dev_guide.md" readonly="true"
    # Getting Started with SAOS Application Development
    
    **Audience:** Application Developers
    **Applies To:** SAOS Application Runtime v1
    **Version:** 1.0
    
    ---
    
    # 1. Introduction
    
    SAOS applications are intentionally isolated executable units.
    
    They are not plugins.
    
    They are not embedded components inside a shared runtime.
    
    They are independent applications operating under strict platform contracts.
    
    This guide explains:
    
    * how SAOS applications are structured
    * how applications declare capabilities
    * how lifecycle coordination works
    * how applications communicate with the kernel
    * how to build applications that remain operationally independent
    
    ---
    
    # 2. Core Philosophy
    
    Before writing code, understand the architectural model.
    
    SAOS applications are designed around three principles:
    
    * isolation
    * explicit contracts
    * local ownership
    
    Applications are not expected to share runtime state.
    
    Applications communicate through protocols.
    
    Applications own their own behavior boundaries.
    
    ---
    
    # 3. Local-First Ownership
    
    ## 3.1 Definition
    
    SAOS applications are expected to own their own state locally whenever possible.
    
    This is called:
    
    > Local-First Ownership
    
    ---
    
    ## 3.2 Why This Matters
    
    Traditional frontend ecosystems centralize state into global stores.
    
    Over time this creates:
    
    * hidden coupling
    * synchronization complexity
    * coordination bottlenecks
    * backend dependency escalation
    
    SAOS rejects centralized frontend ownership.
    
    Each application should own:
    
    * its projections
    * its reducers
    * its event history
    * its local behavior
    
    ---
    
    ## 3.3 Recommended Ownership Model
    
    Applications should prefer:
    
    * local event stores
    * local projections
    * local reducers
    * protocol communication
    
    Applications should avoid:
    
    * global mutable stores
    * cross-app state mutation
    * implicit shared caches
    
    ---
    
    # 4. Application Structure
    
    Minimal application structure:
    
    ```txt id="1uc6p0"
    inventory.app/
    ├── app-info.json
    ├── src/
    ├── dist/
    └── package.json
    ```
    
    ---
    
    # 5. app-info.json
    
    ## 5.1 Purpose
    
    Every application must provide:
    
    ```txt id="2jlwmn"
    app-info.json
    ```
    
    This manifest defines the executable contract between the application and the platform.
    
    Applications without a valid manifest cannot be linked into SAOS.
    
    ---
    
    # 6. Required Manifest Fields
    
    ## Example
    
    ```json id="q5djgh"
    {
      "id": "inventory.app",
      "name": "Inventory Application",
      "version": "1.2.0",
    
      "entry": {
        "browser": "./dist/index.js"
      },
    
      "capabilities": [
        "ipc.send",
        "ipc.request",
        "shell.navigate"
      ]
    }
    ```
    
    ---
    
    # 7. Manifest Specification
    
    ## 7.1 id
    
    Globally unique application identifier.
    
    Example:
    
    ```json id="bwwrpd"
    "id": "inventory.app"
    ```
    
    Requirements:
    
    * must be stable
    * must be globally unique
    * must not change between versions
    
    ---
    
    ## 7.2 version
    
    Application semantic version.
    
    Example:
    
    ```json id="7khf3v"
    "version": "1.2.0"
    ```
    
    Applications must version all releases.
    
    Immutable deployments require immutable versions.
    
    ---
    
    ## 7.3 entry
    
    Defines executable entry points.
    
    Example:
    
    ```json id="6r2ksx"
    "entry": {
      "browser": "./dist/index.js"
    }
    ```
    
    Future runtimes may support:
    
    * browser
    * native
    * worker
    * server-side shells
    
    ---
    
    ## 7.4 capabilities
    
    Explicit capability declarations.
    
    Example:
    
    ```json id="jwl1q4"
    "capabilities": [
      "ipc.send",
      "ipc.request"
    ]
    ```
    
    Capabilities are mandatory declarations.
    
    Undeclared behavior is forbidden.
    
    ---
    
    # 8. Capability Philosophy
    
    Capabilities define what an application is allowed to do.
    
    Applications receive only explicitly granted powers.
    
    This minimizes:
    
    * accidental privilege escalation
    * runtime abuse
    * hidden dependencies
    
    ---
    
    # 9. Example Capabilities
    
    | Capability     | Description                    |
    | -------------- | ------------------------------ |
    | ipc.send       | Send fire-and-forget messages  |
    | ipc.request    | Perform request-response calls |
    | shell.navigate | Request navigation changes     |
    | mount.attach   | Request mount operations       |
    
    Capabilities may be denied by platform policy.
    
    ---
    
    # 10. Application Lifecycle
    
    Applications operate through explicit lifecycle coordination.
    
    Applications must not assume permanent execution.
    
    ---
    
    # 11. Lifecycle Hooks
    
    ## 11.1 boot
    
    Called when application initializes.
    
    Example:
    
    ```ts id="d7dkt7"
    export async function boot(context) {
      await initializeStore()
    }
    ```
    
    Purpose:
    
    * initialize local state
    * prepare projections
    * attach event listeners
    
    ---
    
    ## 11.2 mount
    
    Called when application becomes visible or active.
    
    Example:
    
    ```ts id="0mp5um"
    export async function mount(context) {
      renderApp(context.element)
    }
    ```
    
    Purpose:
    
    * attach UI
    * activate interaction
    * bind visual resources
    
    ---
    
    ## 11.3 unmount
    
    Called before detachment.
    
    Example:
    
    ```ts id="a7c2qq"
    export async function unmount() {
      disposeResources()
    }
    ```
    
    Purpose:
    
    * cleanup subscriptions
    * release resources
    * stop observers
    
    ---
    
    ## 11.4 shutdown
    
    Called before termination.
    
    Example:
    
    ```ts id="lmz2ux"
    export async function shutdown() {
      await flushPendingEvents()
    }
    ```
    
    Purpose:
    
    * persist local state
    * finalize cleanup
    * release handles
    
    ---
    
    # 12. Lifecycle Guarantees
    
    Applications must assume:
    
    * lifecycle interruption may occur anytime
    * mounts are temporary
    * termination may be forced
    
    Applications must therefore remain resilient.
    
    ---
    
    # 13. Saos.Interop SDK
    
    ## 13.1 Purpose
    
    Applications communicate with the platform exclusively through:
    
    ```txt id="j4lm4w"
    Saos.Interop
    ```
    
    This SDK provides safe kernel interaction primitives.
    
    Applications do not communicate directly with other applications.
    
    All communication passes through the kernel IPC Bus.
    
    ---
    
    # 14. Why Direct App Communication Is Forbidden
    
    Direct runtime coupling creates:
    
    * hidden dependencies
    * runtime instability
    * security violations
    * topology leakage
    
    SAOS preserves isolation through mediated communication.
    
    ---
    
    # 15. Installing the SDK
    
    Example:
    
    ```bash id="f4r1y4"
    npm install @saos/interop
    ```
    
    ---
    
    # 16. Basic SDK Usage
    
    ## Initialize Kernel Connection
    
    ```ts id="yvx83g"
    import { connect } from "@saos/interop"
    
    const kernel = await connect()
    ```
    
    ---
    
    # 17. Sending Messages
    
    ## Fire-and-forget
    
    ```ts id="pjlwm0"
    await kernel.send("inventory.app", {
      type: "inventory.refresh-requested"
    })
    ```
    
    ---
    
    # 18. Request/Response Communication
    
    ```ts id="qq52lw"
    const result = await kernel.request(
      "pricing.app",
      {
        type: "pricing.resolve",
        payload: {
          sku: "A-100"
        }
      }
    )
    ```
    
    ---
    
    # 19. Navigation Requests
    
    ```ts id="3kz7yo"
    await kernel.navigate("/products/42")
    ```
    
    Applications request navigation.
    
    They do not mutate global routing directly.
    
    ---
    
    # 20. Mount Operations
    
    ```ts id="x0hfxd"
    await kernel.mount(
      "chat.app",
      "sidebar"
    )
    ```
    
    Mount authorization depends on platform policy.
    
    ---
    
    # 21. Isolation Guarantees
    
    Applications cannot:
    
    * access another app's memory
    * mutate another app's state
    * bypass IPC routing
    * inspect private runtime internals
    
    This restriction is intentional.
    
    Isolation preserves system survivability.
    
    ---
    
    # 22. Recommended Architecture
    
    Applications should prefer:
    
    * pure reducers
    * local projections
    * explicit protocols
    * append-only events
    
    Applications should avoid:
    
    * global mutable state
    * shared runtime caches
    * direct runtime references
    
    ---
    
    # 23. Failure Philosophy
    
    Applications must fail independently.
    
    A crashing application should not destabilize:
    
    * the kernel
    * the shell
    * other applications
    
    Failure containment is part of the platform contract.
    
    ---
    
    # 24. Testing Recommendations
    
    Applications should test:
    
    * reducer determinism
    * protocol compatibility
    * replay correctness
    * lifecycle resilience
    
    ---
    
    # 25. Anti-Patterns
    
    The following patterns are strongly discouraged:
    
    * global singleton state
    * implicit cross-app contracts
    * runtime monkey patching
    * hidden capability assumptions
    * direct DOM ownership outside mounts
    
    ---
    
    # 26. Final Principle
    
    A SAOS application is an isolated executable citizen.
    
    It owns its own behavior.
    
    It communicates through contracts.
    
    It survives independently.
    
    The platform provides boundaries.
    
    Applications provide discipline within those boundaries.
    
%%ENDBLOCK

%%FILE path="docs/implementation_automation_plane.md" readonly="true"
    # IMPLEMENTATION_AUTOMATION_PLANE.md
    
    # SAOS Automation Plane Implementation Guide
    
    **Status:** Draft v1
    **Applies To:** SAOS Deployment Infrastructure and Runtime Linking
    **Version:** 1.0
    
    ---
    
    # 1. Purpose
    
    This document defines the SAOS Automation Plane.
    
    The Automation Plane is responsible for transforming independently built applications into a coherent executable ecosystem without introducing runtime coupling.
    
    Its responsibilities include:
    
    * application registration
    * deployment coordination
    * runtime linking
    * version activation
    * rollback safety
    * ABI preservation
    * atomic publication
    
    The Automation Plane exists to ensure that independently evolving applications can participate in a stable platform without creating operational entropy.
    
    ---
    
    # 2. Architectural Principle
    
    SAOS applications are isolated artifacts.
    
    They are not dynamically discovered through runtime mutation.
    
    They are linked intentionally.
    
    This distinction is critical.
    
    Traditional plugin ecosystems allow runtime drift.
    
    SAOS uses deterministic linking.
    
    ---
    
    # 3. The Linker Pattern
    
    ## 3.1 Overview
    
    The SAOS platform uses a linker model rather than a dynamic discovery model.
    
    Applications do not self-register at runtime.
    
    Instead, applications are linked into the ecosystem through a deterministic manifest.
    
    This manifest is:
    
    ```txt id="9s8n0j"
    apps.json
    ```
    
    ---
    
    ## 3.2 apps.json as Global Process Table
    
    `apps.json` functions as the platform-wide Global Process Table.
    
    It defines:
    
    * available applications
    * active versions
    * mount targets
    * protocol contracts
    * capability grants
    * routing ownership
    * deployment locations
    
    The runtime treats this manifest as authoritative.
    
    ---
    
    ## 3.3 Example Structure
    
    ```json id="9t0lru"
    {
      "apps": [
        {
          "id": "inventory.app",
          "version": "4.2.1",
          "entry": "/apps/inventory.app/4.2.1/index.js",
          "capabilities": [
            "ipc.send",
            "ipc.request"
          ]
        },
        {
          "id": "shell.app",
          "version": "2.0.0",
          "entry": "/apps/shell.app/2.0.0/index.js"
        }
      ]
    }
    ```
    
    ---
    
    # 4. Why Static Linking Matters
    
    Runtime discovery creates instability.
    
    Applications become able to:
    
    * mutate runtime topology
    * introduce hidden dependencies
    * create implicit contracts
    * destabilize startup order
    
    SAOS rejects runtime topology mutation.
    
    The system topology must be inspectable before execution begins.
    
    ---
    
    # 5. Automation Plane Responsibilities
    
    The Automation Plane is responsible for:
    
    * artifact publication
    * manifest generation
    * atomic activation
    * version registration
    * rollback coordination
    * dependency validation
    * ABI verification
    
    ---
    
    # 6. Cross-Repository Dispatch
    
    ## 6.1 Problem
    
    Applications exist in independent repositories.
    
    Each repository may:
    
    * evolve independently
    * deploy independently
    * version independently
    
    However, the platform manifest must remain coherent.
    
    ---
    
    ## 6.2 Solution
    
    SAOS uses Cross-Repository Dispatch.
    
    Application builds trigger controlled updates into the Kernel Hub.
    
    ---
    
    # 7. Kernel Hub
    
    ## 7.1 Definition
    
    The Kernel Hub is the authoritative runtime publication repository.
    
    It contains:
    
    * deployable artifacts
    * version manifests
    * linkage metadata
    * runtime activation manifests
    
    The Kernel Hub does not build applications.
    
    It links them.
    
    ---
    
    # 8. Cross-Repository Dispatch Flow
    
    ## Step 1 — App Repository Build
    
    Application repository produces immutable artifact.
    
    Example:
    
    ```txt id="u5r3cc"
    inventory.app@4.2.1
    ```
    
    ---
    
    ## Step 2 — Artifact Publication
    
    Build artifact uploads to artifact storage.
    
    Example:
    
    ```txt id="b9i1gv"
    /artifacts/inventory.app/4.2.1/
    ```
    
    ---
    
    ## Step 3 — Dispatch Trigger
    
    Repository emits deployment dispatch event.
    
    Example:
    
    ```json id="1o7g4s"
    {
      "appId": "inventory.app",
      "version": "4.2.1",
      "artifact": "/artifacts/inventory.app/4.2.1/"
    }
    ```
    
    ---
    
    ## Step 4 — Kernel Hub Receives Dispatch
    
    Kernel Hub validates:
    
    * artifact integrity
    * manifest correctness
    * ABI compatibility
    * capability declarations
    * protocol contracts
    
    ---
    
    ## Step 5 — Atomic Manifest Mutation
    
    Kernel Hub updates `apps.json`.
    
    This update must be atomic.
    
    Partial topology activation is forbidden.
    
    ---
    
    ## Step 6 — Activation
    
    Runtime loads updated topology.
    
    Applications become available immediately.
    
    ---
    
    # 9. Atomic Activation Requirement
    
    Manifest updates must be atomic.
    
    The runtime must never observe:
    
    * partially written manifests
    * incomplete topology updates
    * inconsistent activation state
    
    Failure must preserve previous known-good topology.
    
    ---
    
    # 10. Side-by-Side Deployment Strategy
    
    ## 10.1 Principle
    
    SAOS never deploys by overwriting active versions.
    
    Instead, all versions coexist side-by-side.
    
    ---
    
    ## 10.2 Versioned Paths
    
    Artifacts deploy into immutable versioned directories.
    
    Example:
    
    ```txt id="9grg2w"
    /apps/inventory.app/4.1.0/
    /apps/inventory.app/4.2.0/
    /apps/inventory.app/4.2.1/
    ```
    
    Existing versions remain untouched.
    
    ---
    
    # 11. Why This Matters
    
    Overwriting artifacts creates catastrophic risk:
    
    * broken rollbacks
    * inconsistent cache states
    * impossible forensic analysis
    * ABI instability
    
    Side-by-side deployment eliminates these problems.
    
    ---
    
    # 12. Rollback Strategy
    
    Rollback becomes a manifest switch.
    
    Example:
    
    ```json id="ejrtul"
    {
      "id": "inventory.app",
      "version": "4.1.0"
    }
    ```
    
    No rebuild required.
    
    No redeploy required.
    
    No artifact restoration required.
    
    Rollback becomes instantaneous.
    
    ---
    
    # 13. ABI Stability
    
    ## 13.1 Definition
    
    ABI stability means previously linked applications continue functioning even as newer versions appear.
    
    ---
    
    ## 13.2 How SAOS Preserves ABI Stability
    
    Since versions remain immutable:
    
    * old integrations remain executable
    * protocol expectations remain preserved
    * rollback targets remain intact
    
    No application is forced onto newer runtime assumptions.
    
    ---
    
    # 14. Immutable Artifact Principle
    
    Published artifacts must never mutate.
    
    If code changes:
    
    * a new version must be produced
    
    Mutation-in-place is forbidden.
    
    ---
    
    # 15. Garbage Collection Strategy
    
    Artifacts are not deleted automatically.
    
    Deletion risks:
    
    * rollback destruction
    * replay inconsistency
    * forensic loss
    * dependency breakage
    
    Retention policy must be explicit and controlled.
    
    ---
    
    # 16. Runtime Linking Model
    
    At startup:
    
    1. kernel loads `apps.json`
    2. runtime topology is constructed
    3. capabilities are assigned
    4. mount graph is resolved
    5. applications activate
    
    No runtime discovery occurs outside manifest definition.
    
    ---
    
    # 17. Deployment Safety Guarantees
    
    The Automation Plane must guarantee:
    
    * atomic activation
    * immutable artifacts
    * deterministic topology
    * rollback safety
    * manifest integrity
    * reproducible deployments
    
    ---
    
    # 18. Operational Advantages
    
    This architecture eliminates major operational failure modes:
    
    | Traditional Failure     | SAOS Resolution     |
    | ----------------------- | ------------------- |
    | Runtime drift           | Static linking      |
    | Broken rollback         | Versioned artifacts |
    | Dependency mutation     | Immutable releases  |
    | Deployment corruption   | Atomic activation   |
    | Runtime discovery chaos | Manifest topology   |
    
    ---
    
    # 19. Anti-Patterns
    
    The following patterns are forbidden:
    
    * mutable deployments
    * in-place artifact replacement
    * runtime self-registration
    * dynamic topology mutation
    * shared deployment directories
    * automatic destructive cleanup
    
    ---
    
    # 20. Final Principle
    
    The Automation Plane exists to preserve operational determinism.
    
    Applications evolve independently.
    
    The platform remains coherent.
    
    Linking is intentional.
    
    Deployment is immutable.
    
    Rollback is instantaneous.
    
    History remains executable.
    
    Nothing is overwritten.
    
    Nothing is forgotten.
    
%%ENDBLOCK

%%FILE path="docs/philosophy.md" readonly="true"
    # PHILOSOPHY.md
    
    ## SAOS Manifesto
    
    ### The Principle of Irreversible Simplification
    
    Software systems do not collapse because they are too small.
    They collapse because they are allowed to grow without consequence.
    
    Every abstraction added for convenience becomes a permanent maintenance burden.
    Every extension point becomes a future compatibility problem.
    Every shared runtime becomes a negotiation between conflicting assumptions.
    Every “temporary” integration becomes infrastructure that must survive forever.
    
    Most platforms fail slowly.
    
    They decay through accumulation.
    
    SAOS exists to stop that process permanently.
    
    ---
    
    # The Core Premise
    
    SAOS — Static Application Operating System — is built on a single uncompromising belief:
    
    > Complexity must be structurally impossible, not culturally discouraged.
    
    Traditional architectures rely on discipline.
    
    SAOS relies on boundaries.
    
    The platform is intentionally small, static, hostile to expansion, and resistant to mutation.
    Not because flexibility is undesirable — but because unrestricted flexibility inevitably destroys systems over time.
    
    SAOS chooses irreversible simplification.
    
    Once complexity is removed from the kernel, it cannot silently reappear later.
    
    ---
    
    # The Kernel Must Remain Small Forever
    
    The kernel is not a framework.
    
    It is not a convenience layer.
    
    It is not a developer experience product.
    
    The kernel is law.
    
    Its responsibility is singular:
    
    * define contracts
    * enforce isolation
    * guarantee determinism
    * coordinate execution
    * refuse ambiguity
    
    Nothing else belongs there.
    
    Every feature added to the kernel increases:
    
    * upgrade risk
    * coupling pressure
    * compatibility obligations
    * emergent behavior
    * operational surface area
    
    Most platforms expand until they become impossible to reason about.
    
    SAOS does the opposite.
    
    The kernel must become smaller over time, not larger.
    
    If something can exist outside the kernel, it must.
    
    If something requires negotiation between applications, it does not belong in the kernel.
    
    If something introduces hidden state, runtime mutation, or shared ownership, it is rejected.
    
    ---
    
    # Kernel as Law
    
    In SAOS, the boundary between platform and application is absolute.
    
    The kernel defines law.
    
    Applications either comply with the law or they do not run.
    
    There is no privileged escape hatch.
    
    No hidden APIs.
    
    No shared mutable runtime.
    
    No implicit contracts.
    
    No dependency leakage.
    
    Applications are external actors operating under strict jurisdiction.
    
    This is intentional hostility.
    
    Not toward developers — but toward entropy.
    
    A platform that permits unrestricted integration eventually loses the ability to govern itself.
    
    SAOS preserves sovereignty through isolation.
    
    ---
    
    # Hostile Isolation
    
    Applications are not trusted.
    
    This is a foundational principle.
    
    Traditional frontend ecosystems encourage deep integration:
    
    * shared dependency graphs
    * shared runtime state
    * cross-application imports
    * implicit contracts
    * runtime composition
    * global event systems
    
    These patterns create invisible coupling.
    
    Invisible coupling creates cascading failure.
    
    Cascading failure creates operational decay.
    
    SAOS rejects this entirely.
    
    Applications communicate only through explicit contracts.
    
    Contracts are static.
    
    Boundaries are enforced.
    
    Violations are impossible to hide.
    
    An application must be replaceable without destabilizing the system around it.
    
    If replacing an application risks platform integrity, the architecture has already failed.
    
    ---
    
    # Why We Reject Developer Convenience
    
    Developer convenience is often deferred complexity.
    
    A shortcut today becomes infrastructure tomorrow.
    
    SAOS intentionally rejects many forms of convenience because convenience frequently bypasses architectural accountability.
    
    Examples include:
    
    * implicit shared state
    * runtime dependency injection across boundaries
    * dynamic plugin mutation
    * uncontrolled extension systems
    * global registries
    * undocumented integration paths
    
    These patterns feel productive early.
    
    They become catastrophic later.
    
    SAOS optimizes for long-term survivability over short-term ergonomics.
    
    The platform should remain understandable after ten years, not merely productive after ten days.
    
    ---
    
    # Against Operational Decay
    
    Most systems become operationally heavier over time.
    
    More services.
    
    More orchestration.
    
    More synchronization.
    
    More backend coordination.
    
    More deployment dependencies.
    
    More runtime negotiation.
    
    This is operational decay.
    
    Micro-frontends often accelerate this problem instead of solving it.
    
    They fragment deployment while retaining shared runtime assumptions.
    
    They distribute complexity rather than eliminating it.
    
    The result is usually:
    
    * duplicated infrastructure
    * runtime version conflicts
    * coordination overhead
    * hidden coupling
    * backend dependency explosion
    * cross-team synchronization costs
    
    Micro-frontends promise independence.
    
    In practice, they frequently create distributed monoliths.
    
    SAOS takes a fundamentally different approach.
    
    Applications are isolated by default.
    
    Contracts are static and enforceable.
    
    The kernel remains invariant.
    
    No application can expand platform responsibility through integration pressure.
    
    This prevents architectural drift before it begins.
    
    ---
    
    # The Elimination of Backend Creep
    
    Traditional platforms gradually centralize behavior into backend systems.
    
    Why?
    
    Because frontend boundaries are weak.
    
    As coordination becomes difficult, logic migrates toward centralized services.
    
    Over time:
    
    * orchestration accumulates
    * state aggregation grows
    * backend gateways expand
    * platform teams become bottlenecks
    
    This is backend creep.
    
    SAOS prevents this structurally.
    
    Applications own their own responsibility domains.
    
    The kernel coordinates but does not absorb business logic.
    
    No central orchestration layer is allowed to become an implicit brain for the ecosystem.
    
    The result is a system that resists gravitational collapse into a distributed monolith.
    
    ---
    
    # Static Systems Age Better
    
    Dynamic systems optimize for adaptation.
    
    Static systems optimize for permanence.
    
    SAOS chooses permanence.
    
    A static contract is auditable.
    
    A static boundary is enforceable.
    
    A static platform is understandable.
    
    Systems that cannot surprise operators are systems that survive.
    
    This is not about nostalgia for rigidity.
    
    It is about survivability at scale.
    
    ---
    
    # Final Principle
    
    SAOS is not designed to maximize expansion.
    
    It is designed to preserve integrity.
    
    Every decision is filtered through a single question:
    
    > Does this reduce irreversible complexity — or introduce it?
    
    If complexity can return later through convenience, abstraction leakage, or integration pressure, then simplification was never real.
    
    True simplification is irreversible.
    
    That is the philosophy of SAOS.
    
    
%%ENDBLOCK

%%FILE path="docs/spec_compaction.md" readonly="true"
    # SAOS Event Compaction Specification
    
    **Status:** Draft v1
    **Applies To:** SAOS Event Persistence Infrastructure
    **Version:** 1.0
    
    ---
    
    # 1. Purpose
    
    This document defines the SAOS Event Compaction system.
    
    Compaction is the second half of Long-Term Persistence.
    
    The first half is:
    
    * immutable history
    * replay integrity
    * schema evolution through upcasting
    
    The second half is survival at scale.
    
    Without compaction, immutable logs inevitably become operationally hostile.
    
    Replay time grows without bound.
    
    Storage pressure accumulates indefinitely.
    
    Startup reconstruction degrades over time.
    
    Eventually the system collapses under the weight of its own memory.
    
    This violates the principle of:
    
    > Irreversible Simplification
    
    A system that requires infinite replay growth is not simplified.
    
    It is merely delayed complexity.
    
    Compaction exists to preserve deterministic replay while preventing operational decay.
    
    ---
    
    # 2. Architectural Principle
    
    History remains immutable.
    
    Replay remains deterministic.
    
    Infrastructure absorbs scale pressure.
    
    The domain remains pure.
    
    ---
    
    # 3. The Problem of Infinite Replay
    
    ## 3.1 Immutable Growth
    
    Event logs are append-only.
    
    Over time this creates unbounded accumulation.
    
    Example:
    
    ```txt id="y4zj1w"
    event_1
    event_2
    event_3
    ...
    event_4,000,000
    ```
    
    Full replay eventually becomes operationally expensive.
    
    ---
    
    ## 3.2 Operational Decay
    
    Without compaction:
    
    * startup cost increases indefinitely
    * replay latency increases indefinitely
    * memory pressure accumulates
    * battery consumption increases
    * mobile execution becomes unstable
    
    Operational complexity returns through scale accumulation.
    
    ---
    
    # 4. Kernel as Law
    
    Applications are not trusted to manage historical integrity.
    
    History management is a platform concern.
    
    The kernel enforces compaction as infrastructure governance.
    
    This prevents:
    
    * inconsistent replay behavior
    * application-specific history corruption
    * destructive optimization shortcuts
    * replay fragmentation
    
    The kernel preserves replay order.
    
    Applications consume replay.
    
    Applications do not govern it.
    
    ---
    
    # 5. Compaction Overview
    
    Compaction reduces replay cost by introducing deterministic replay checkpoints.
    
    These checkpoints are called:
    
    > Summary Events
    
    A Summary Event becomes a new replay base.
    
    Replay no longer begins from origin.
    
    Replay begins from the latest valid Summary Event.
    
    ---
    
    # 6. Summary Event Pattern
    
    ## 6.1 Definition
    
    A Summary Event is a deterministic replay checkpoint produced by replaying events through Pure Reducers.
    
    The Summary Event represents:
    
    * the accumulated state
    * at a precise historical boundary
    * generated deterministically
    
    ---
    
    ## 6.2 Critical Rule
    
    Summary Events are derived artifacts.
    
    They are not independent truth.
    
    Truth still originates from immutable events.
    
    ---
    
    # 7. Compaction Flow
    
    ---
    
    ## Step 1 — Event Window Selection
    
    A bounded event range is selected.
    
    Example:
    
    ```txt id="4x0mnv"
    events 1 -> 10,000
    ```
    
    ---
    
    ## Step 2 — Upcasting
    
    All events must first be upcast to latest schema representation.
    
    Compaction before upcasting is forbidden.
    
    ---
    
    ## Step 3 — Deterministic Replay
    
    Events replay through Pure Reducers.
    
    Example:
    
    ```txt id="7n5mjl"
    (state, event) -> newState
    ```
    
    ---
    
    ## Step 4 — Summary State Production
    
    Replay produces canonical projected state.
    
    ---
    
    ## Step 5 — Summary Event Emission
    
    A deterministic Summary Event is persisted.
    
    ---
    
    # 8. Compaction Triggers
    
    Compaction occurs only through deterministic triggers.
    
    ---
    
    ## 8.1 Count-Based Trigger
    
    Compaction occurs after a fixed event threshold.
    
    Example:
    
    ```txt id="0z2t5v"
    every 10,000 events
    ```
    
    ---
    
    ## 8.2 Time-Based Trigger
    
    Compaction occurs after deterministic temporal windows.
    
    Example:
    
    ```txt id="uqyk97"
    every 7 days
    ```
    
    ---
    
    ## 8.3 Size-Based Trigger
    
    Compaction occurs after replay size exceeds threshold.
    
    Example:
    
    ```txt id="scybpa"
    50MB replay window
    ```
    
    ---
    
    # 9. Deterministic Trigger Requirement
    
    Triggers must behave identically across devices.
    
    Non-deterministic compaction schedules create replay divergence risks.
    
    Forbidden examples:
    
    * battery-dependent triggers
    * CPU-dependent triggers
    * device-speed heuristics
    
    Compaction boundaries must remain reproducible.
    
    ---
    
    # 10. Summary Event Structure
    
    ## 10.1 Canonical Structure
    
    ```json id="2bjlwm"
    {
      "id": "summary_102",
      "type": "system.summary",
      "sourceVersion": 7,
      "eventCount": 10000,
      "stateHash": "sha256:abc123",
      "prevSummaryId": "summary_101",
      "timestamp": 1760000000,
      "state": {}
    }
    ```
    
    ---
    
    ## 10.2 Field Definitions
    
    | Field         | Description                           |
    | ------------- | ------------------------------------- |
    | id            | Unique summary identifier             |
    | type          | Summary event type                    |
    | sourceVersion | Schema version used during replay     |
    | eventCount    | Number of compacted events            |
    | stateHash     | Deterministic hash of resulting state |
    | prevSummaryId | Previous summary checkpoint           |
    | timestamp     | Creation timestamp                    |
    | state         | Full projected replay state           |
    
    ---
    
    # 11. Why Full State Exists
    
    The Summary Event contains the complete projected state.
    
    This allows replay continuation without reprocessing entire history.
    
    ---
    
    # 12. Replay After Compaction
    
    Replay changes from:
    
    ```txt id="djlwmc"
    origin -> event_1 -> ... -> event_1,000,000
    ```
    
    to:
    
    ```txt id="9bhb1g"
    summary_102 -> remaining_events
    ```
    
    This dramatically reduces replay cost.
    
    ---
    
    # 13. Tiered Replay Integration
    
    ## 13.1 Relationship to Boot Hints
    
    Boot Hints remain non-authoritative.
    
    Summary Events become authoritative replay bases.
    
    ---
    
    ## 13.2 Boot Hint Role
    
    Boot Hints exist for startup responsiveness only.
    
    Possible location:
    
    ```txt id="6hjs7n"
    LocalStorage
    ```
    
    Boot Hints may be discarded anytime.
    
    ---
    
    ## 13.3 Summary Event Role
    
    Summary Events exist inside authoritative persistent storage.
    
    Example:
    
    ```txt id="x6ud6d"
    IndexedDB
    ```
    
    Summary Events participate in replay correctness.
    
    Boot Hints do not.
    
    ---
    
    # 14. Replay Flow with Compaction
    
    ---
    
    ## Phase 1 — Load Boot Hint
    
    Temporary state hydration for fast startup.
    
    State marked provisional.
    
    ---
    
    ## Phase 2 — Load Latest Summary Event
    
    Kernel retrieves latest valid Summary Event.
    
    ---
    
    ## Phase 3 — Replay Remaining Events
    
    Only post-summary events replay.
    
    ---
    
    ## Phase 4 — Validation
    
    Replay result validates against expected state hash.
    
    ---
    
    ## Phase 5 — Finalization
    
    Authoritative state replaces provisional state.
    
    ---
    
    # 15. Determinism Contract
    
    Compaction must preserve exact replay correctness.
    
    ---
    
    ## Formal Rule
    
    ### GIVEN
    
    A sequence of events:
    
    ```txt id="w3gjkp"
    E1 ... EN
    ```
    
    ### WHEN
    
    Compaction produces Summary Event:
    
    ```txt id="2qkxya"
    S
    ```
    
    ### THEN
    
    The resulting replay state after:
    
    ```txt id="t2c3fy"
    S + remaining events
    ```
    
    MUST equal:
    
    ```txt id="5d79kg"
    full replay from origin
    ```
    
    ---
    
    # 16. Deterministic Test Template
    
    ## GIVEN
    
    ```txt id="c8uyfy"
    events:
    - cart.created
    - cart.item-added A
    - cart.item-added B
    ```
    
    ---
    
    ## WHEN
    
    ```txt id="3q5nmq"
    compaction runs after event 3
    ```
    
    ---
    
    ## THEN
    
    ```txt id="t5cqj0"
    state(summary replay) == state(full replay)
    ```
    
    ---
    
    # 17. Example Replay Test
    
    ```ts id="w12vqt"
    describe("compaction determinism", () => {
    
      it("produces identical replay state", () => {
    
        const fullReplay = replay(allEvents)
    
        const summary = compact(allEvents)
    
        const compactReplay = replayFromSummary(
          summary,
          remainingEvents
        )
    
        expect(compactReplay).toEqual(fullReplay)
      })
    })
    ```
    
    ---
    
    # 18. Interaction with Upcasting
    
    ## 18.1 Mandatory Ordering
    
    Compaction must occur only after upcasting.
    
    Required pipeline:
    
    ```txt id="fxtwji"
    raw event
    -> upcast
    -> replay
    -> compaction
    ```
    
    Forbidden pipeline:
    
    ```txt id="1n8r5n"
    raw event
    -> compaction
    -> upcast
    ```
    
    ---
    
    ## 18.2 Why This Matters
    
    Compacting outdated schemas permanently embeds historical incompatibility into replay checkpoints.
    
    This corrupts future replay integrity.
    
    ---
    
    # 19. Failure Modes
    
    ---
    
    ## 19.1 Corrupt Summary Event
    
    If Summary Event corruption detected:
    
    * summary must be rejected
    * replay falls back to earlier summary or origin replay
    
    Silent continuation is forbidden.
    
    ---
    
    ## 19.2 Missing Summary Event
    
    If expected summary missing:
    
    * replay falls back safely
    * telemetry emitted
    
    ---
    
    ## 19.3 Hash Mismatch
    
    If replayed state hash differs from stored hash:
    
    * summary considered invalid
    * replay fallback required
    * corruption event logged
    
    ---
    
    # 20. Explicit Replay Fallback
    
    Failure handling must be explicit.
    
    Fallback hierarchy:
    
    ```txt id="gq5o22"
    latest summary
    -> previous summary
    -> origin replay
    ```
    
    ---
    
    # 21. Forbidden Optimizations
    
    The following are forbidden:
    
    * destructive log truncation
    * mutable history rewriting
    * lossy snapshots
    * partial replay assumptions
    * reducer mutation during compaction
    
    ---
    
    # 22. Compaction Ownership
    
    Compaction belongs to infrastructure.
    
    Not domain logic.
    
    Not applications.
    
    Not reducers.
    
    Reducers remain pure replay functions only.
    
    ---
    
    # 23. Why Destructive Truncation Is Forbidden
    
    Deleting historical events destroys:
    
    * replay auditability
    * forensic reconstruction
    * deterministic verification
    * historical integrity
    
    History must remain reconstructable.
    
    ---
    
    # 24. Why Lossy Snapshots Are Forbidden
    
    Lossy snapshots create hidden replay divergence.
    
    Summary Events must preserve deterministic reconstruction capability.
    
    ---
    
    # 25. Long-Term Persistence Strategy
    
    Long-term persistence requires both:
    
    | Concern          | Solution   |
    | ---------------- | ---------- |
    | schema evolution | upcasting  |
    | replay scale     | compaction |
    
    Neither alone is sufficient.
    
    ---
    
    # 26. Final Principle
    
    Immutable history without scale control becomes operational decay.
    
    Compaction exists to preserve replay integrity without sacrificing determinism.
    
    History remains immutable.
    
    Infrastructure absorbs scale.
    
    The domain remains pure.
    
%%ENDBLOCK

%%FILE path="docs/spec_event_sourcing.md" readonly="true"
    # SPEC_EVENT_SOURCING.md
    
    # SAOS Client-Side Event Sourcing Engine
    
    **Status:** Draft v1
    **Applies To:** SAOS Stateful Runtime Systems
    **Version:** 1.0
    
    ---
    
    # 1. Purpose
    
    This document defines the architecture and operational model of the SAOS Client-Side Event Sourcing Engine.
    
    The purpose of the engine is to eliminate hidden mutable state and replace imperative service mutation with deterministic event-driven reconstruction.
    
    Instead of storing truth in mutable service objects, SAOS stores immutable facts.
    
    State is derived.
    
    Never owned.
    
    ---
    
    # 2. Core Principle
    
    Traditional frontend systems centralize behavior into stateful services.
    
    Example:
    
    ```ts id="k6g5dn"
    cartService.addItem(product)
    ```
    
    This pattern creates hidden mutation.
    
    Mutation creates temporal ambiguity.
    
    Temporal ambiguity destroys determinism.
    
    SAOS replaces this model entirely.
    
    ---
    
    # 3. Event-Sourced State Model
    
    In SAOS:
    
    * events are immutable facts
    * state is derived from events
    * projections accumulate facts
    * policies make decisions
    * reducers transform state deterministically
    
    Truth exists in the event log.
    
    Everything else is reconstruction.
    
    ---
    
    # 4. From Stateful Services to Pure Reducers
    
    ## 4.1 Traditional Stateful Model
    
    Traditional frontend architectures frequently use mutable singleton services.
    
    Example:
    
    ```ts id="ux94xg"
    class CartService {
      private items = []
    
      add(item) {
        this.items.push(item)
      }
    }
    ```
    
    Problems:
    
    * hidden mutation
    * unpredictable ordering
    * difficult replay
    * difficult testing
    * temporal coupling
    * impossible reconstruction
    
    ---
    
    ## 4.2 SAOS Reducer Model
    
    SAOS replaces mutation with deterministic reduction.
    
    Canonical form:
    
    ```ts id="9g3suh"
    (state, event) -> newState
    ```
    
    Example:
    
    ```ts id="73v2oi"
    function cartReducer(state, event) {
      switch (event.type) {
        case "cart.item-added":
          return {
            ...state,
            items: [...state.items, event.payload]
          }
    
        default:
          return state
      }
    }
    ```
    
    Properties:
    
    * deterministic
    * replayable
    * serializable
    * testable
    * side-effect free
    
    ---
    
    # 5. Reducer Requirements
    
    Reducers must be:
    
    * pure
    * deterministic
    * synchronous
    * side-effect free
    
    Reducers must not:
    
    * perform IO
    * mutate external state
    * access timers
    * access random generators
    * dispatch network calls
    
    Given identical input streams, reducers must always produce identical output state.
    
    ---
    
    # 6. Separation of Responsibilities
    
    SAOS strictly separates:
    
    * fact accumulation
    * decision logic
    
    This separation is mandatory.
    
    ---
    
    # 7. Fact Accumulation (Projections)
    
    ## 7.1 Purpose
    
    Projections accumulate facts into queryable state.
    
    A projection answers:
    
    > "What is true right now based on known events?"
    
    ---
    
    ## 7.2 Projection Characteristics
    
    Projections:
    
    * consume events
    * derive state
    * contain no business decisions
    * are replayable
    * are disposable
    
    ---
    
    ## 7.3 Example
    
    Events:
    
    ```json id="2m9d2z"
    [
      { "type": "cart.item-added", "sku": "A" },
      { "type": "cart.item-added", "sku": "B" }
    ]
    ```
    
    Projection result:
    
    ```json id="dr7w0z"
    {
      "items": ["A", "B"]
    }
    ```
    
    ---
    
    # 8. Decision Logic (Policies)
    
    ## 8.1 Purpose
    
    Policies evaluate facts and decide whether actions should occur.
    
    Policies answer:
    
    > "Given the current facts, what should happen next?"
    
    ---
    
    ## 8.2 Policy Characteristics
    
    Policies:
    
    * observe state and events
    * produce commands or reactions
    * contain business rules
    * do not mutate state directly
    
    ---
    
    ## 8.3 Example
    
    ```ts id="j59xmt"
    function lowStockPolicy(state, event) {
      if (
        event.type === "inventory.changed" &&
        event.remaining < 5
      ) {
        return {
          type: "inventory.restock-requested"
        }
      }
    }
    ```
    
    ---
    
    # 9. Why This Separation Matters
    
    Combining projection logic and decision logic creates hidden coupling.
    
    Example anti-pattern:
    
    ```ts id="4o9q8u"
    if (itemCount > 5) {
      state.discount = true
    }
    ```
    
    This mixes:
    
    * factual accumulation
    * business policy
    * state mutation
    
    SAOS forbids this pattern.
    
    Projections accumulate facts.
    
    Policies interpret facts.
    
    ---
    
    # 10. Event Store Architecture
    
    ## 10.1 Ground Truth
    
    The authoritative event log resides in IndexedDB.
    
    IndexedDB is the persistent source of truth.
    
    ---
    
    ## 10.2 Event Properties
    
    Events must be:
    
    * immutable
    * append-only
    * timestamped
    * ordered
    * uniquely identifiable
    
    ---
    
    ## 10.3 Event Envelope
    
    Example:
    
    ```json id="ov7u5j"
    {
      "id": "evt_1021",
      "type": "cart.item-added",
      "timestamp": 1760000000,
      "payload": {
        "sku": "A-100"
      }
    }
    ```
    
    ---
    
    # 11. Tiered Replay Strategy
    
    ## 11.1 Problem
    
    Full replay from persistent storage may become expensive as event volume grows.
    
    However, traditional snapshots introduce dangerous problems:
    
    * snapshot drift
    * invalid state hydration
    * hidden corruption
    * temporal inconsistency
    
    SAOS rejects authoritative snapshots.
    
    ---
    
    # 12. Boot Hints
    
    ## 12.1 Definition
    
    Boot Hints are non-authoritative cached projection states.
    
    They exist only to accelerate startup.
    
    They are disposable.
    
    ---
    
    ## 12.2 Storage Location
    
    Boot Hints may be stored in:
    
    * LocalStorage
    * memory caches
    * transient browser storage
    
    ---
    
    ## 12.3 Critical Rule
    
    Boot Hints are never authoritative.
    
    They are optimization hints only.
    
    Truth always comes from replay.
    
    ---
    
    ## 12.4 Example
    
    ```json id="kq6r8f"
    {
      "projection": "cart",
      "lastEventId": 1050,
      "cachedState": {
        "items": 14
      }
    }
    ```
    
    ---
    
    # 13. Tiered Replay Flow
    
    ## 13.1 Phase 1 — Boot Hint Load
    
    System loads temporary projection hints.
    
    UI becomes immediately responsive.
    
    State is marked provisional.
    
    ---
    
    ## 13.2 Phase 2 — IndexedDB Replay
    
    Authoritative events load from IndexedDB.
    
    Reducers replay events.
    
    Projections reconstruct deterministically.
    
    ---
    
    ## 13.3 Phase 3 — Reconciliation
    
    Boot Hint state is compared against authoritative replay.
    
    If mismatch exists:
    
    * replayed state wins
    * hint is discarded
    * corruption telemetry may be emitted
    
    ---
    
    # 14. Why This Matters
    
    This architecture preserves:
    
    * deterministic truth
    * startup responsiveness
    * replay integrity
    * operational simplicity
    
    without introducing authoritative snapshot corruption.
    
    ---
    
    # 15. Replay Determinism
    
    Deterministic replay is mandatory.
    
    Given identical event streams:
    
    * projections must produce identical state
    * policies must produce identical decisions
    
    across all environments.
    
    ---
    
    # 16. Replay Test Structure
    
    All replay tests must use deterministic GIVEN/WHEN/THEN structure.
    
    ---
    
    ## 16.1 GIVEN
    
    Defines initial state and event stream.
    
    Example:
    
    ```txt id="0syjff"
    GIVEN events:
    - cart.created
    - cart.item-added sku=A
    - cart.item-added sku=B
    ```
    
    ---
    
    ## 16.2 WHEN
    
    Defines replay execution.
    
    Example:
    
    ```txt id="rtr6r2"
    WHEN replay executes through cartProjection
    ```
    
    ---
    
    ## 16.3 THEN
    
    Defines expected deterministic outcome.
    
    Example:
    
    ```txt id="gms9bw"
    THEN resulting state equals:
    {
      items: ["A", "B"]
    }
    ```
    
    ---
    
    # 17. Deterministic Replay Example
    
    ```ts id="9aq1yy"
    describe("cart projection replay", () => {
      it("reconstructs cart deterministically", () => {
    
        const events = [
          { type: "cart.item-added", payload: { sku: "A" }},
          { type: "cart.item-added", payload: { sku: "B" }}
        ]
    
        const state = replay(events, cartReducer)
    
        expect(state.items).toEqual(["A", "B"])
      })
    })
    ```
    
    ---
    
    # 18. Failure Recovery
    
    If projection corruption occurs:
    
    * projections may be discarded
    * replay may restart from event origin
    * Boot Hints may be invalidated
    
    Recovery must never require mutable state repair.
    
    Truth already exists in the log.
    
    ---
    
    # 19. Anti-Patterns
    
    The following patterns are forbidden:
    
    * mutable singleton stores
    * hidden in-memory authority
    * reducer side effects
    * authoritative snapshots
    * direct projection mutation
    * event rewriting
    
    ---
    
    # 20. Final Principle
    
    The SAOS Event Engine treats state as a consequence.
    
    Not as ownership.
    
    Events are truth.
    
    Reducers are mathematics.
    
    Projections are disposable.
    
    Policies are interpreters.
    
    The system survives because it can always reconstruct itself from immutable facts.
    
%%ENDBLOCK

%%FILE path="docs/spec_kernel_ipc.md" readonly="true"
    # SAOS Kernel and IPC Bus Specification v1
    
    **Status:** Draft v1
    **Applies To:** SAOS Runtime Implementations
    **Version:** 1.0
    **Classification:** Core Platform Specification
    
    ---
    
    # 1. Purpose
    
    This document defines the SAOS Kernel and Inter-Process Communication (IPC) Bus.
    
    The SAOS kernel is intentionally minimal.
    
    It exists only to:
    
    * enforce isolation
    * route messages
    * govern resources
    * maintain deterministic execution boundaries
    * coordinate application lifecycle
    * enforce protocol contracts
    
    The kernel is not an application framework.
    
    The kernel does not provide shared business logic.
    
    The kernel does not provide convenience abstractions beyond what is required for safe orchestration.
    
    ---
    
    # 2. Architectural Principles
    
    ## 2.1 Irreducible Kernel
    
    The kernel must remain permanently small.
    
    Functionality that can exist outside the kernel must not be absorbed into it.
    
    The kernel is a governance layer, not an application runtime ecosystem.
    
    ---
    
    ## 2.2 Protocol-First Architecture
    
    All interaction occurs through explicit protocols.
    
    No application may directly access another application’s memory, runtime, or internal state.
    
    All communication occurs through the IPC Bus.
    
    ---
    
    ## 2.3 Hostile Execution Model
    
    Applications are treated as potentially hostile executables.
    
    Trust is never implicit.
    
    Isolation is mandatory.
    
    Applications receive capabilities explicitly.
    
    Capabilities are revocable.
    
    ---
    
    ## 2.4 Replaceable Shell Principle
    
    The shell is not privileged.
    
    The shell is an application.
    
    Any shell implementation may be replaced without modifying the kernel.
    
    The kernel does not depend on visual rendering assumptions.
    
    ---
    
    # 3. Kernel Responsibilities
    
    The kernel is responsible for:
    
    * application lifecycle management
    * IPC routing
    * capability enforcement
    * sandbox governance
    * quota enforcement
    * mount graph management
    * navigation coordination
    * protocol validation
    * execution supervision
    
    The kernel must not:
    
    * contain business logic
    * contain application-specific orchestration
    * maintain shared mutable state between applications
    * expose hidden integration APIs
    
    ---
    
    # 4. Boundary 2 Isolation Model
    
    ## 4.1 Definition
    
    Boundary 2 is the mandatory isolation boundary between applications and the kernel.
    
    Applications execute as untrusted units.
    
    Applications must never be allowed to:
    
    * directly mutate kernel state
    * inspect other application memory
    * bypass IPC routing
    * allocate unrestricted resources
    * execute privileged operations without explicit capability grants
    
    ---
    
    ## 4.2 Isolation Guarantees
    
    Each application must operate within an isolated execution boundary.
    
    Isolation implementation may vary by runtime platform, including:
    
    * Web Workers
    * iframes
    * isolated JS runtimes
    * WASM sandboxes
    * process isolation
    * containerized runtimes
    
    Implementation details are runtime-specific.
    
    Isolation guarantees are mandatory.
    
    ---
    
    ## 4.3 Capability Model
    
    Applications operate through explicit capabilities.
    
    Capabilities define allowed actions.
    
    Example capabilities:
    
    * ipc.send
    * ipc.request
    * shell.navigate
    * storage.read
    * storage.write
    * mount.attach
    
    Capabilities may be:
    
    * granted
    * denied
    * revoked
    * scoped
    * rate-limited
    
    ---
    
    ## 4.4 Resource Governance
    
    The kernel must enforce resource quotas.
    
    Governed resources include:
    
    * CPU time
    * memory allocation
    * IPC throughput
    * storage usage
    * mount count
    * network access
    * execution duration
    
    Quota violations may trigger:
    
    * throttling
    * suspension
    * termination
    * capability revocation
    
    ---
    
    ## 4.5 Fault Containment
    
    Application failure must remain isolated.
    
    A crashing application must not destabilize:
    
    * the kernel
    * the shell
    * other applications
    * the IPC Bus
    
    ---
    
    # 5. IPC Bus Overview
    
    ## 5.1 Purpose
    
    The IPC Bus is the exclusive communication layer of SAOS.
    
    All cross-boundary interaction must pass through the bus.
    
    No direct application references are permitted.
    
    ---
    
    ## 5.2 Design Constraints
    
    The IPC Bus must be:
    
    * deterministic
    * inspectable
    * protocol-driven
    * transport-agnostic
    * replayable
    * auditable
    
    ---
    
    ## 5.3 Communication Types
    
    The IPC Bus supports:
    
    | Type      | Description                           |
    | --------- | ------------------------------------- |
    | Event     | Fire-and-forget message               |
    | Request   | Request-response interaction          |
    | Signal    | Kernel-level control message          |
    | Lifecycle | Mount/unmount/navigation coordination |
    
    ---
    
    # 6. Annoyingly Primitive API
    
    The SAOS kernel intentionally exposes an extremely small API surface.
    
    This API is intentionally restrictive.
    
    Higher-level abstractions must exist outside the kernel.
    
    ---
    
    ## 6.1 send
    
    Dispatches a one-way message.
    
    ### Signature
    
    ```ts
    send(target, message)
    ```
    
    ### Behavior
    
    * asynchronous
    * no response expected
    * delivery may fail
    * kernel validates permissions
    
    ### Example
    
    ```ts
    send("inventory.app", {
      type: "inventory.updated",
      payload: {
        sku: "A-100"
      }
    })
    ```
    
    ---
    
    ## 6.2 request
    
    Dispatches a request expecting a response.
    
    ### Signature
    
    ```ts
    request(target, message, timeout?)
    ```
    
    ### Behavior
    
    * asynchronous
    * correlation required
    * timeout enforced by kernel
    * response must conform to protocol contract
    
    ### Example
    
    ```ts
    const result = await request("pricing.app", {
      type: "pricing.resolve",
      payload: {
        sku: "A-100"
      }
    })
    ```
    
    ---
    
    ## 6.3 mount
    
    Requests attachment of an application into a mount region.
    
    ### Signature
    
    ```ts
    mount(target, region, options?)
    ```
    
    ### Behavior
    
    * kernel validates mount permissions
    * shell determines rendering implementation
    * mount lifecycle becomes observable
    
    ### Example
    
    ```ts
    mount("chat.app", "sidebar")
    ```
    
    ---
    
    ## 6.4 unmount
    
    Requests detachment of an application.
    
    ### Signature
    
    ```ts
    unmount(target)
    ```
    
    ### Behavior
    
    * application receives lifecycle shutdown signal
    * kernel may force termination
    
    ---
    
    ## 6.5 navigate
    
    Requests navigation state transition.
    
    ### Signature
    
    ```ts
    navigate(route, options?)
    ```
    
    ### Behavior
    
    * navigation ownership belongs to shell
    * kernel coordinates routing event propagation
    * applications cannot directly mutate global navigation state
    
    ### Example
    
    ```ts
    navigate("/products/42")
    ```
    
    ---
    
    # 7. Message Schema
    
    All IPC messages must conform to the canonical envelope format.
    
    ---
    
    ## 7.1 Canonical Envelope
    
    ```json
    {
      "id": "msg_01JXZ...",
      "type": "inventory.updated",
      "source": "inventory.app",
      "target": "analytics.app",
      "timestamp": 1760000000,
      "correlationId": null,
      "payload": {},
      "meta": {}
    }
    ```
    
    ---
    
    ## 7.2 Required Fields
    
    | Field     | Description               |
    | --------- | ------------------------- |
    | id        | Unique message identifier |
    | type      | Protocol message type     |
    | source    | Sender identity           |
    | target    | Receiver identity         |
    | timestamp | UTC epoch timestamp       |
    | payload   | Message payload           |
    
    ---
    
    ## 7.3 Optional Fields
    
    | Field         | Description                  |
    | ------------- | ---------------------------- |
    | correlationId | Request-response correlation |
    | meta          | Transport metadata           |
    | ttl           | Message expiration           |
    | priority      | Delivery priority            |
    
    ---
    
    # 8. Protocol Contracts
    
    ## 8.1 Explicit Contracts
    
    All IPC interactions must be contract-defined.
    
    Ad hoc message structures are forbidden.
    
    ---
    
    ## 8.2 Versioning
    
    Protocols must be versioned explicitly.
    
    Example:
    
    ```txt
    inventory.resolve.v1
    inventory.resolve.v2
    ```
    
    ---
    
    ## 8.3 Compatibility
    
    Backward compatibility is not assumed.
    
    Protocol evolution must be explicit.
    
    ---
    
    # 9. Shell Independence
    
    The shell is replaceable.
    
    The kernel must not assume:
    
    * DOM structure
    * rendering engine
    * visual composition model
    * layout strategy
    
    Shell implementations may include:
    
    * browser shells
    * terminal shells
    * native shells
    * kiosk shells
    * accessibility-first shells
    
    Applications communicate through protocols, not visual hierarchy assumptions.
    
    ---
    
    # 10. Observability
    
    The kernel must expose inspection capabilities for:
    
    * IPC traffic
    * quota violations
    * lifecycle events
    * capability grants
    * mount topology
    * crash events
    
    Observability must not violate isolation guarantees.
    
    ---
    
    # 11. Security Requirements
    
    The kernel must reject:
    
    * unauthorized capability escalation
    * direct runtime access attempts
    * protocol spoofing
    * malformed IPC payloads
    * unauthorized mount requests
    
    All messages must undergo validation before routing.
    
    ---
    
    # 12. Non-Goals
    
    The kernel is explicitly not responsible for:
    
    * state management frameworks
    * UI composition frameworks
    * dependency injection systems
    * shared application stores
    * application business logic
    * cross-application runtime composition
    
    ---
    
    # 13. Final Principle
    
    The SAOS Kernel exists to preserve boundaries.
    
    Not to erase them.
    
    The platform achieves longevity through restriction.
    
    Every additional kernel feature increases systemic risk.
    
    Therefore the kernel must remain:
    
    * small
    * hostile
    * deterministic
    * enforceable
    * replaceable
    * protocol-driven
    
    The simplest kernel that can preserve order is the correct kernel.
    
%%ENDBLOCK

%%FILE path="docs/spec_upcasting.md" readonly="true"
    # SAOS Event Upcasting Pipeline Specification
    
    **Status:** Draft v1
    **Applies To:** SAOS Event Persistence and Replay Infrastructure
    **Version:** 1.0
    
    ---
    
    # 1. Purpose
    
    This document defines the SAOS Event Upcasting Pipeline.
    
    The purpose of the pipeline is to preserve long-term replay integrity in immutable event systems while allowing domain schemas to evolve over time.
    
    The upcasting pipeline exists to solve a fundamental problem:
    
    > immutable events outlive the code that created them
    
    Without controlled evolution, historical events become unreadable.
    
    Unreadable history destroys replay.
    
    Destroyed replay destroys the system.
    
    ---
    
    # 2. The Problem of Schema Drift
    
    ## 2.1 Definition
    
    Schema drift occurs when domain models evolve while historical events remain permanently frozen in older formats.
    
    Example:
    
    ### Historical Event (v1)
    
    ```json id="1b3v4u"
    {
      "type": "user.created",
      "name": "Ada"
    }
    ```
    
    ### Modern Domain Model (v3)
    
    ```ts id="p9jv2d"
    {
      firstName: string
      lastName: string
      displayName: string
    }
    ```
    
    The historical event no longer matches the current model.
    
    ---
    
    ## 2.2 Why This Is Existential
    
    In traditional CRUD systems, old rows may be migrated destructively.
    
    Event-sourced systems cannot do this safely.
    
    The log is immutable.
    
    Historical events are permanent.
    
    If old events become incompatible with modern replay code:
    
    * replay fails
    * projections become unrecoverable
    * deterministic reconstruction collapses
    * long-term persistence becomes impossible
    
    Schema drift is therefore an existential threat to immutable systems.
    
    ---
    
    # 3. Core Principle
    
    Historical truth must remain immutable.
    
    Interpretation may evolve.
    
    SAOS preserves immutable logs by transforming historical representations during replay rather than mutating stored events.
    
    This transformation process is called upcasting.
    
    ---
    
    # 4. Architectural Principle
    
    The domain model must remain clean.
    
    Historical compatibility logic must not pollute domain behavior.
    
    The infrastructure layer absorbs historical complexity.
    
    ---
    
    # 5. Purity of Domain Models
    
    ## 5.1 Forbidden Pattern
    
    Domain models must not contain historical branching logic.
    
    Example anti-pattern:
    
    ```csharp id="uh2hdy"
    public class UserCreated
    {
        public string FirstName { get; set; }
    
        public string? LegacyName { get; set; }
    }
    ```
    
    Or:
    
    ```csharp id="5y9mf9"
    if(event.Version == 1)
    {
        ...
    }
    ```
    
    inside domain behavior.
    
    This contaminates the domain with infrastructure history.
    
    ---
    
    ## 5.2 Required Principle
    
    Domain models represent current truth only.
    
    They must remain:
    
    * clean
    * coherent
    * version-agnostic
    * behavior-focused
    
    Historical adaptation belongs outside the domain.
    
    ---
    
    # 6. Upcasting Pipeline Overview
    
    The SAOS Upcasting Pipeline transforms historical event payloads into modern canonical representations before deserialization occurs.
    
    This is a mandatory infrastructure layer.
    
    ---
    
    # 7. Pipeline Flow
    
    ## Stage 1 — Raw Event Retrieval
    
    Historical event data loads from persistent storage.
    
    Example:
    
    ```json id="zq9qqv"
    {
      "type": "user.created",
      "version": 1,
      "payload": {
        "name": "Ada Lovelace"
      }
    }
    ```
    
    At this stage the payload is treated as raw JSON.
    
    No domain deserialization has occurred.
    
    ---
    
    ## Stage 2 — Upcast Resolution
    
    The system resolves required transformations based on:
    
    * event type
    * schema version
    * target version
    
    ---
    
    ## Stage 3 — Sequential Upcasting
    
    The raw JSON payload is transformed incrementally through version-specific upcasters.
    
    Example:
    
    ```txt id="u3epkm"
    v1 -> v2
    v2 -> v3
    v3 -> v4
    ```
    
    Each transformation performs one explicit evolutionary step.
    
    ---
    
    ## Stage 4 — Canonical JSON Production
    
    The pipeline produces fully modernized JSON.
    
    Example:
    
    ```json id="z7npmd"
    {
      "firstName": "Ada",
      "lastName": "Lovelace",
      "displayName": "Ada Lovelace"
    }
    ```
    
    ---
    
    ## Stage 5 — Domain Deserialization
    
    Only after upcasting completes does deserialization occur.
    
    The domain model receives clean modern data only.
    
    ---
    
    # 8. Why Upcasting Happens Before Deserialization
    
    Deserialization into outdated types creates contamination pressure.
    
    If deserialization occurs too early:
    
    * legacy types proliferate
    * compatibility logic leaks upward
    * domain purity collapses
    
    SAOS instead transforms raw data first.
    
    Then deserializes once.
    
    Into the modern model.
    
    ---
    
    # 9. Upcaster Design
    
    ## 9.1 Interface
    
    Each schema transition is represented by a dedicated upcaster.
    
    Example:
    
    ```csharp id="p8gt74"
    public interface IUpcaster
    {
        string EventType { get; }
    
        int FromVersion { get; }
    
        int ToVersion { get; }
    
        JsonObject Upcast(JsonObject payload);
    }
    ```
    
    ---
    
    ## 9.2 Responsibilities
    
    An upcaster must:
    
    * transform exactly one version step
    * remain deterministic
    * avoid side effects
    * preserve semantic meaning
    
    ---
    
    ## 9.3 Forbidden Responsibilities
    
    Upcasters must not:
    
    * access databases
    * perform network calls
    * invoke domain services
    * mutate persisted history
    
    ---
    
    # 10. Example Upcaster
    
    ## v1 → v2 Transformation
    
    Historical schema:
    
    ```json id="n2m0ae"
    {
      "name": "Ada Lovelace"
    }
    ```
    
    New schema:
    
    ```json id="68fx64"
    {
      "firstName": "Ada",
      "lastName": "Lovelace"
    }
    ```
    
    Implementation:
    
    ```csharp id="mzyqod"
    public sealed class UserCreatedV1ToV2 : IUpcaster
    {
        public string EventType => "user.created";
    
        public int FromVersion => 1;
    
        public int ToVersion => 2;
    
        public JsonObject Upcast(JsonObject payload)
        {
            var parts = payload["name"]
                .GetValue<string>()
                .Split(' ');
    
            return new JsonObject
            {
                ["firstName"] = parts[0],
                ["lastName"] = parts[1]
            };
        }
    }
    ```
    
    ---
    
    # 11. Chained Upcasting
    
    Events may require multiple transformations.
    
    Example:
    
    ```txt id="5uv5f5"
    v1 -> v2 -> v3 -> v4
    ```
    
    The pipeline must execute transformations sequentially.
    
    Skipping intermediate transformations is forbidden unless explicitly guaranteed safe.
    
    ---
    
    # 12. Determinism Requirements
    
    Upcasting must be deterministic.
    
    Given identical historical input:
    
    * identical output must always be produced
    
    regardless of runtime environment.
    
    ---
    
    # 13. Version Metadata
    
    Each persisted event must contain:
    
    | Field     | Purpose              |
    | --------- | -------------------- |
    | type      | event classification |
    | version   | schema version       |
    | timestamp | historical ordering  |
    | payload   | immutable event data |
    
    ---
    
    # 14. Failure Handling
    
    If an event cannot be upcast:
    
    * replay must fail explicitly
    * corruption must be observable
    * silent degradation is forbidden
    
    Invalid history must never be ignored.
    
    ---
    
    # 15. Performance Considerations
    
    Upcasting introduces replay cost.
    
    However:
    
    * correctness is prioritized over speed
    * replay integrity is mandatory
    * deterministic reconstruction is non-negotiable
    
    Optimization may exist around the pipeline.
    
    The pipeline itself must remain authoritative.
    
    ---
    
    # 16. Testing Requirements
    
    Every upcaster must include deterministic transformation tests.
    
    ---
    
    ## Example
    
    ### GIVEN
    
    ```json id="z8u6hc"
    {
      "version": 1,
      "payload": {
        "name": "Ada Lovelace"
      }
    }
    ```
    
    ### WHEN
    
    ```txt id="88qztq"
    UserCreatedV1ToV2 executes
    ```
    
    ### THEN
    
    ```json id="b9d3ho"
    {
      "firstName": "Ada",
      "lastName": "Lovelace"
    }
    ```
    
    ---
    
    # 17. Infrastructure Ownership
    
    Upcasting belongs exclusively to infrastructure.
    
    Not domain logic.
    
    Not application services.
    
    Not projections.
    
    This separation preserves architectural clarity.
    
    ---
    
    # 18. Long-Term Evolution Strategy
    
    SAOS assumes systems may survive for decades.
    
    Therefore:
    
    * schema evolution is inevitable
    * historical compatibility is unavoidable
    * replay integrity must survive indefinitely
    
    The upcasting pipeline exists to make long-term evolution survivable.
    
    ---
    
    # 19. Anti-Patterns
    
    The following patterns are forbidden:
    
    * mutable event rewriting
    * destructive migrations of immutable logs
    * domain-level version branching
    * legacy compatibility inside aggregates
    * dual-version domain models
    * runtime deserialization guessing
    
    ---
    
    # 20. Final Principle
    
    Immutable logs create permanent memory.
    
    Permanent memory creates permanent responsibility.
    
    The SAOS Upcasting Pipeline exists to ensure history remains replayable without corrupting the domain with the burden of the past.
    
    History remains immutable.
    
    Infrastructure absorbs evolution.
    
    The domain remains pure.
    
%%ENDBLOCK

%%END
