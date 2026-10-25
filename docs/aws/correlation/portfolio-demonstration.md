# Portfolio demonstration

## Purpose

The static demonstration at `/correlation/` explains the bounded correlation architecture without requiring AWS, the external provider, runtime-branch access, credentials, or live operational evidence.

## Publication boundary

The reviewed scenario and replay are static fixtures. The replay is explicitly synthetic and public-transformed. It is not presented as a live execution record. Provider responses are rendered only when they satisfy the stable normalized contract and identify themselves as non-authoritative candidate analysis.

## Dynamic configuration

`correlation/demo/runtime-config.json` is intentionally absent from reviewed source. Deployment may publish it from `runtime-config.example.json` only after the sandbox endpoint and reviewed scenario digest have passed acceptance. The browser admits only HTTPS API Gateway origins in `eu-north-1` and the `/production` stage.

Removing that optional file, disabling API Gateway, setting Lambda reserved concurrency to zero, exhausting provider quota, or losing provider availability leaves all reviewed explanatory content and fixtures usable.

## Accepted public claim

> The demonstration presents a design for reviewed GitHub governance, bounded AWS execution, non-authoritative provider mediation, terminal tombstones, and revision-bound eventual cleanup. Dynamic availability, production scale, zero cost, provider quality, and exactly-once processing are not claimed.
