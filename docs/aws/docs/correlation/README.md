# Correlation Capability Prototype

This directory is the normative entry point for the bounded correlation prototype in `ulfbou.github.io`.

## Status

The specification is ready for prototype development. It is not a production-readiness claim and does not establish successful deployment, provider availability, zero cost, or an SLA.

## Governing proposition

A public GitHub repository governs reviewed policy, terminal decisions, validation, and cleanup evidence. AWS provides least-privilege request execution, minimal active correlation state, external-provider mediation, and revision-bound cleanup. The browser remains credential-free, provider output remains non-authoritative, and the static portfolio remains useful when every dynamic capability is unavailable.

## Normative ownership

- [Product and scope](product-and-scope.md) owns purpose, admitted capability, and non-goals.
- [Governance](governance.md) owns authorities, branches, identities, and mutation rights.
- [Architecture](architecture.md) owns components, state flow, AWS resources, persistence, and IAM boundaries.
- [Provenance and evidence](provenance-and-evidence.md) owns identity domains, canonicalization, traces, and evidence.
- [Security and threat model](security-and-threat-model.md) owns classification, threats, and security requirements.
- [Development specification](development-specification.md) owns source boundaries, domain implementation, tests, and validation.
- [DevOps specification](devops-specification.md) owns workflow, deployment, permission, and handoff contracts.
- [Operations and recovery](operations-and-recovery.md) owns limits, alarms, kill switches, incidents, and recovery.
- [Prototype definition](prototype-definition.md) owns scenarios and bounded behavior.
- [Acceptance and release](acceptance-and-release.md) owns readiness, gates, release evidence, and allowed claims.
- [Decision records](decisions/README.md) own architectural choices and consequences.

Schemas own record shape. Registries own admitted values. Documents own meaning and policy. A repeated statement is informative unless its owning authority says otherwise.

## Requirement families

`COR-PROD`, `COR-GOV`, `COR-ARCH`, `COR-PROV`, `COR-SEC`, `COR-DEV`, `COR-DEVOPS`, `COR-OPS`, `COR-PROT`, and `COR-REL` are stable requirement namespaces.
