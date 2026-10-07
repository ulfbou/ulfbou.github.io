# Evidence package contract

An evidence package contains bounded observations about one exact repository revision. It grants no publication authority.

`packageId` is `evp_` plus SHA-256 of canonical JSON containing repository identity, full revision, collector contract version, normalized scope includes and excludes, redaction policy, exclusion policy, and ordered observation digests. Observation digests cover each complete canonical observation.

Paths are normalized repository-relative paths. Absolute paths, traversal, empty paths, and symbolic revisions fail validation. Package and observation identities are unique. Public projection requires both the observation and its approved reference to declare `Public` visibility.
