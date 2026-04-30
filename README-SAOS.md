# SAOS — Static Application Operating System

SAOS is a platform for hosting isolated single-page applications (SPAs) as
first-class "processes" within a shared browser shell, coordinated by a
lightweight kernel via a well-defined IPC protocol.

**This repository is the kernel repo.** It owns the hub runtime, the app
registry, and the Automation Plane (GitHub Actions workflows) that keep both
up-to-date without human intervention.

---

## What this repo is

The SAOS kernel — the single authoritative hub for all registered apps.

It provides:

- **`kernel/kernel.js`** — the hub runtime (router, process registry, IPC dispatcher)
- **`kernel/apps.json`** — the live app registry, updated automatically by CI
- **`infra/`** — idempotent Node.js scripts used by CI to validate and register apps
- **`.github/workflows/`** — the Automation Plane (treat these as kernel syscalls)
- **`sdk/`** — libc adapters for JavaScript/TypeScript and Blazor WASM

---

## What this repo is not

- It is **not** an app repo. No userland code belongs here.
- It is **not** edited by hand. `kernel/apps.json` is maintained exclusively by `link-app.yml`.
- It is **not** a monorepo for apps. Each app lives in its own repository.

---

## Architecture

```
┌─────────────────────────────────────────────┐
│                  Browser Tab                │
│  ┌──────────────────────────────────────┐   │
│  │         SAOS Kernel (Hub)            │   │
│  │  kernel.js  +  apps.json            │   │
│  └──────────┬────────────────┬──────────┘   │
│             │  CustomEvent   │              │
│    ┌────────┴────────┐  ┌────┴────────┐     │
│    │  App: dashboard │  │  App: tools │     │
│    │  (/dashboard/)  │  │  (/tools/)  │     │
│    └─────────────────┘  └─────────────┘     │
└─────────────────────────────────────────────┘
```

All communication uses **SAOS IPC v1** — a `CustomEvent`-based protocol with a
frozen ABI. See [`docs/ipc-v1.md`](docs/ipc-v1.md) for the normative specification.

---

## Invariant guarantees

| Guarantee | Mechanism |
|-----------|-----------|
| Hub never edited by hand | `kernel/apps.json` is only written by `link-app.yml` |
| Rejected invalid apps | `validate-manifest.yml` enforces IPC v1 schema before any write |
| Deterministic registry | `infra/linker.mjs` sorts apps alphabetically; identical input → identical output |
| Idempotent registration | Re-registering the same app at the same version is a no-op |
| Upgrade safety | A failed validation leaves `apps.json` unchanged |
| Zero manual deploys | `update-hub.yml` deploys to GitHub Pages on every qualifying push |

---

## Repository layout

```
SAOS/
├── index.html                         # Hub shell (loaded by GitHub Pages)
├── kernel/
│   ├── kernel.js                      # Hub runtime — router, registry, IPC dispatcher
│   └── apps.json                      # Live app registry (CI-managed)
├── infra/
│   ├── validate-manifest.mjs          # Validates app-info.json against IPC v1
│   ├── linker.mjs                     # Upserts app entry into apps.json
│   └── update-registry.mjs           # Orchestrates validate → link
├── .github/workflows/
│   ├── validate-manifest.yml          # Standalone manifest validation
│   ├── link-app.yml                   # repository_dispatch handler (app registration)
│   └── update-hub.yml                 # Deploys hub to GitHub Pages
├── sdk/
│   ├── js/saos-core/                  # JS/TS libc (SaosProcess, SaosEvent)
│   └── dotnet/Saos.Interop/           # Blazor WASM libc
├── saos-app-template/                 # Reference template for new app repos
│   ├── app-info.json
│   ├── .github/workflows/deploy.yml
│   └── README.md
└── docs/
    └── ipc-v1.md                      # Normative IPC v1 specification
```

---

## Automation Plane (GitHub Actions)

### `validate-manifest.yml`

Validates an `app-info.json` payload. Can be triggered manually (`workflow_dispatch`)
or called internally by `link-app.yml` (`workflow_call`).

**Inputs:** `app_info_json` — raw JSON string of the manifest.  
**Outputs:** `valid` — `"true"` | `"false"`.

### `link-app.yml`

Triggered by a `repository_dispatch` event of type `saos-app-register` from an
app repo. Validates the manifest, upserts the app into `kernel/apps.json`, and
commits the result. The resulting push triggers `update-hub.yml`.

**Requires:** org secret `SAOS_KERNEL_TOKEN` stored in the app repos (Fine-Grained PAT
with **Actions: write** on this repo).

### `update-hub.yml`

Deploys `index.html`, `kernel/kernel.js`, and `kernel/apps.json` to GitHub Pages
whenever those files change on `main`. Can also be triggered manually.

**Requires:** GitHub Pages enabled (Source: GitHub Actions) on this repo.

---

## How installation works (for platform operators)

### 1 — Enable GitHub Pages

In this repo's settings → Pages → Source: **GitHub Actions**.

### 2 — Create the org secret and variable

| Name | Type | Value |
|------|------|-------|
| `SAOS_KERNEL_TOKEN` | Org secret | Fine-Grained PAT with **Actions: write** on this repo |
| `SAOS_KERNEL_REPO` | Org variable | `<org>/SAOS` (the full repo slug of this repo) |

### 3 — Add an app

Fork [`saos-app-template`](saos-app-template/README.md), rename the repo to the
app's id, update `app-info.json`, and push. The rest is automated.

---

## IPC v1 event reference

| Event | Direction | Purpose |
|-------|-----------|---------|
| `saos:kernel:ready` | Kernel → App | Kernel bootstrap complete |
| `saos:kernel:theme-changed` | Kernel → App | Theme update |
| `saos:nav:navigate` | App → Kernel | Cross-app navigation |
| `saos:user:logout` | App → Kernel | Logout request |
| `saos:capability:request` | App → Kernel | Advisory capability request |
| `saos:app:announce` | App → Kernel | App self-registration |

All events use the envelope:

```json
{ "version": "1.0", "source": "<app-id>", "payload": { ... } }
```

See [`docs/ipc-v1.md`](docs/ipc-v1.md) for the complete normative specification.

---

## SDK quick start

### JavaScript / TypeScript

```bash
cd sdk/js/saos-core
npm install
npm test
npm run build
```

```ts
import { SaosProcess } from "saos-core";

const proc = new SaosProcess("my-app");

proc.on("kernel:ready", (e) => {
  proc.announce({
    title: "My App",
    entry: "/my-app/",
    capabilities: ["user"],
    version: "2026.04.20",
  });
});

proc.navigate("/tools/");
```

### Blazor WASM

1. Reference `sdk/dotnet/Saos.Interop/Saos.Interop.csproj`.
2. Add the JS bridge to `index.html`:
   ```html
   <script src="_content/Saos.Interop/saosInterop.js"></script>
   ```
3. Register in `Program.cs`:
   ```csharp
   builder.Services.AddSaosKernel("my-blazor-app");
   ```
4. Inject and use `ISaosKernel` in components.

---

## License

MIT
