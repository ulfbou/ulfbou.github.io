# SAOS Phase 0 Certification

This document maps the structural evidence baseline (Phase 0) to the physical files in this repository. Completion of this checklist is required to authorize Phase 1.

| Artifact | Specification | Current Evidence (File Path) | Status |
|---|---|---|---|
| **1. Kernel Boundary Map** | Definition of what kernel MAY and MUST NEVER do | `src/Z0/Kernel.cs` (Comments) | ✅ |
| **2. Canonical IPC Envelope** | Frozen JSON Schema with required headers and payload hash | `schemas/ipc-envelope.schema.json` | ✅ |
| **3. Authority Ownership Matrix** | Operation × Owner (Kernel/App/Shell/Forbidden) table | `governance/authority-matrix.yaml` | ✅ |
| **4. Manifest Schema v0** | Frozen `apps.json` schema for registry | `schemas/apps-manifest.schema.json` | ✅ |
| **5. Replay Harness Spec** | Reducer signature, event stream format, hash function | `src/Core/IProjection.cs` & `src/Z3/ReplayEngine.cs` | ✅ |
| **6. Provisional Decisions** | Enforced architectural constants | `governance/provisional-decisions.md` | ✅ |
| **7. Automation Plane** | GitOps-based linking and validation workflow | `.github/workflows/validate-manifest.yml` | ✅ |
| **8. Registry Integrity** | Proof of manifest validation and hash verification | `src/Z4/ManifestValidator.cs` | ✅ |

## Governance Approval
Certification marks the completion of Phase 0. No application code or runtime SDK may be implemented until this baseline is verified against the hash of these artifacts.
