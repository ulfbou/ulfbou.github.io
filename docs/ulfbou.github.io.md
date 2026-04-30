%%DX v1.3 author=tool
%%FILE path="ulfbou.github.io/.github/workflows/link-app.yml" readonly="true"
    # .github/workflows/link-app.yml
    #
    # SAOS Kernel — App Registration
    #
    # Triggered exclusively by repository_dispatch from a SAOS app repo.
    # Validates the incoming app-info.json, upserts the app into kernel/apps.json,
    # and commits the result. The subsequent push triggers update-hub.yml.
    #
    # Prerequisites (set once per org):
    #   - The kernel repo must have "Allow GitHub Actions to create and approve PRs"
    #     disabled. Direct commits to main are made by the github-actions bot.
    #   - App repos must store a Fine-Grained PAT (scope: "Actions: write" on this
    #     repo) as the org secret SAOS_KERNEL_TOKEN.
    #
    # Invariants:
    #   - Only payloads conforming to SAOS IPC v1 are accepted.
    #   - A failed validation leaves kernel/apps.json unmodified.
    #   - Concurrency is serialised per app id to prevent registry corruption.
    
    name: Link App
    
    on:
      repository_dispatch:
        types: [saos-app-register]
    
    concurrency:
      # Serialise registrations for the same app id; cancel none (queue them).
      group: link-app-${{ github.event.client_payload.id }}
      cancel-in-progress: false
    
    # Default: minimal read-only permissions for the validate job (workflow_call).
    # The link job overrides this with contents: write.
    permissions:
      contents: read
    
    jobs:
      validate:
        name: Validate manifest
        uses: ./.github/workflows/validate-manifest.yml
        with:
          app_info_json: ${{ toJSON(github.event.client_payload) }}
    
      link:
        name: Update registry
        needs: validate
        if: needs.validate.outputs.valid == 'true'
        runs-on: ubuntu-latest
        permissions:
          contents: write
    
        steps:
          - name: Checkout kernel repo
            uses: actions/checkout@v4
            with:
              # Use a PAT so the resulting push triggers update-hub.yml.
              # GITHUB_TOKEN pushes are intentionally excluded from triggering
              # further workflows; a PAT or App token is required here.
              token: ${{ secrets.SAOS_KERNEL_TOKEN }}
    
          - name: Write app-info.json to workspace
            run: |
              node -e "
                const fs = require('fs');
                fs.writeFileSync('/tmp/app-info.json', process.env.APP_INFO_JSON, 'utf8');
              "
            env:
              APP_INFO_JSON: ${{ toJSON(github.event.client_payload) }}
    
          - name: Run update-registry (validate + link)
            run: node infra/update-registry.mjs /tmp/app-info.json
    
          - name: Commit registry change
            run: |
              APP_ID="${{ github.event.client_payload.id }}"
              APP_VER="${{ github.event.client_payload.version }}"
    
              git config user.name  "github-actions[bot]"
              git config user.email "github-actions[bot]@users.noreply.github.com"
    
              git add kernel/apps.json
    
              # Commit only when there is an actual diff (idempotency guard)
              if git diff --staged --quiet; then
                echo "No change to kernel/apps.json — registry already up-to-date."
              else
                git commit -m "chore(registry): register ${APP_ID}@${APP_VER}"
                git push
              fi
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/.github/workflows/update-hub.yml" readonly="true"
    # .github/workflows/update-hub.yml
    #
    # SAOS Kernel — Hub Deployment
    #
    # Deploys the kernel runtime and app registry to GitHub Pages whenever
    # kernel/ files change on the main branch (typically after link-app.yml
    # commits an updated apps.json).
    #
    # Invariants:
    #   - Only the files under kernel/ and the hub shell (index.html) are served.
    #   - No app code is bundled or mutated by this workflow.
    #   - Deployment is deterministic: same source tree → same pages output.
    #   - GitHub Pages environment URL is recorded as a job output.
    
    name: Update Hub
    
    on:
      push:
        branches:
          - main
        paths:
          - "kernel/**"
          - "index.html"
    
      # Allow manual re-deployment without a code change (e.g. after Pages config reset)
      workflow_dispatch:
    
    # Only one deployment runs at a time; queue subsequent ones.
    concurrency:
      group: pages-deploy
      cancel-in-progress: false
    
    permissions:
      contents: read
      pages: write
      id-token: write
    
    jobs:
      deploy:
        name: Deploy hub to GitHub Pages
        runs-on: ubuntu-latest
    
        environment:
          name: github-pages
          url: ${{ steps.deploy.outputs.page_url }}
    
        steps:
          - name: Checkout kernel repo
            uses: actions/checkout@v4
    
          - name: Assemble hub artifact
            # Collect only the files the hub needs at runtime.
            # sdk/ and docs/ are excluded from the served site intentionally.
            run: |
              mkdir -p _hub
              cp index.html         _hub/index.html
              cp kernel/kernel.js   _hub/kernel.js
              cp kernel/apps.json   _hub/apps.json
    
          - name: Configure GitHub Pages
            uses: actions/configure-pages@v5
    
          - name: Upload hub artifact
            uses: actions/upload-pages-artifact@v3
            with:
              path: _hub
    
          - name: Deploy to GitHub Pages
            id: deploy
            uses: actions/deploy-pages@v4
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/.github/workflows/validate-manifest.yml" readonly="true"
    # .github/workflows/validate-manifest.yml
    #
    # SAOS Kernel — Manifest Validation
    #
    # Validates that an app-info.json payload conforms to SAOS IPC v1 invariants.
    # Can be triggered manually (workflow_dispatch) for offline checks, or called
    # internally by link-app.yml via workflow_call.
    #
    # This workflow NEVER modifies any file in the repository.
    
    name: Validate App Manifest
    
    on:
      workflow_dispatch:
        inputs:
          app_info_json:
            description: "Raw app-info.json content (single-line JSON)"
            required: true
            type: string
    
      workflow_call:
        inputs:
          app_info_json:
            description: "Raw app-info.json content (single-line JSON)"
            required: true
            type: string
        outputs:
          valid:
            description: "'true' when the manifest is valid"
            value: ${{ jobs.validate.outputs.valid }}
    
    jobs:
      validate:
        name: Validate manifest
        runs-on: ubuntu-latest
        permissions:
          contents: read
    
        outputs:
          valid: ${{ steps.check.outputs.valid }}
    
        steps:
          - name: Checkout kernel repo
            uses: actions/checkout@v4
    
          - name: Write manifest to workspace
            # Use a heredoc-safe approach: write via node to avoid shell quoting issues
            run: |
              node -e "
                const fs = require('fs');
                fs.writeFileSync('/tmp/app-info.json', process.env.APP_INFO_JSON, 'utf8');
              "
            env:
              APP_INFO_JSON: ${{ inputs.app_info_json }}
    
          - name: Run validation
            id: check
            run: |
              if node infra/validate-manifest.mjs /tmp/app-info.json; then
                echo "valid=true" >> "$GITHUB_OUTPUT"
              else
                echo "valid=false" >> "$GITHUB_OUTPUT"
                exit 1
              fi
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/.gitignore" readonly="true"
    # DocFX output directory
    _site/
    
    # Backup files
    *~
    
    # VS Code settings
    .vscode/
    
    .history/
    *.code-workspace
    *.suo
    *.user
    *.userosscache
    *.sln.docstates
    *.cache
    .env
    .env.local
    .env.*.local
    .vs/
    
    bin/
    obj/
    
    .dx/*
    .dx
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/Directory.Build.props" readonly="true"
    <Project>
      <PropertyGroup>
        <!-- Disable code analyzers during CI/build to avoid CA* failures blocking generator runs -->
        <RunAnalyzersDuringBuild>false</RunAnalyzersDuringBuild>
        <RunAnalyzersDuringLiveAnalysis>false</RunAnalyzersDuringLiveAnalysis>
        <!-- Do not treat warnings as errors during builds in CI context -->
        <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
      </PropertyGroup>
    </Project>
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docfx.json" readonly="true"
    {
      "metadata": [
        {
          "src": [
            {
              "files": ["**/*.md"],
                        "exclude": ["obj/**", "_site/**"],
                        "src": "docs"
            }
          ],
          "dest": "obj/api"
        }
      ],
      "build": {
        "content": [
                {
                    "files": ["**/*.md"],
                    "src": "docs"
                }
        ],
        "resource": [
                {
                    "files": ["**/*.{png,jpg,jpeg,gif,svg}", "**/*.json"],
                    "src": "assets"
                }
            ],
            "overwrite": [
                {
                    "files": ["**/*.overwrite.md"]
                }
        ],
        "template": [ "default", "themes/custom" ],
        "globalMetadata": {
          "_appTitle": "Knowledge Hub",
                "_enableSearch": true,
                "_appFooter": "© 2025 Ulf Bourelius. All rights reserved.",
                "_enableNewTab": true
        },
            "template": ["default", "themes/custom"],
            "dest": "_site/docs",
            "xref": [],
            "sitemap": {
                "baseUrl": "https://ulfbou.github.io/docs/",
                "priority": 0.5,
                "changefreq": "weekly"
            }
      }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/.net/zentient-framework-introduction.md" readonly="true"
    ---
    title: "Introducing Zentient Framework"
    slug: "zentient-framework-introduction"
    date: "2025-09-04"
    authors:
      - name: "Ulf Bourelius"
    tags:
      - .NET
      - frameworks
      - architecture
      - zentient
    category: .net
    summary: "An introduction to the Zentient Framework, a modular, extensible .NET architecture."
    linkedinTeaser: "Discover the Zentient Framework — a modular, extensible .NET architecture designed for scalability and maintainability. Explore the mono-repo at the link."
    socialImage: "/assets/social/zentient-framework-preview.png"
    canonicalUrl: "https://ulfbou.github.io/docs/.net/zentient-framework-introduction/"
    ---
    
    ## Introduction to Zentient Framework
    
    **Zentient Framework** is a modern, modular foundation for .NET applications — designed to help you build scalable, maintainable, and testable systems with ease.
    
    - **Modular by design**: Core functionality is separated into individual components within the mono-repo, enabling you to adopt only the modules you need.
    - **Extensible architecture**: Clean abstractions and layered design promote flexibility and future growth.
    - **Unified tooling & samples**: The mono-repo includes configs, tools, sample projects, and validation endpoints to get you started quickly.
    
    Explore the full source: [Zentient Framework on GitHub](https://github.com/ulfbou/ZentientFramework)
    
    ### Why you'll love Zentient Framework
    
    - **Consistency across projects**: Reuse architecture patterns across services or products.
    - **Adequate observability**: Built-in observability modules help you instrument and monitor applications effortlessly.
    - **Enterprise-ready**: Clean abstractions and test support make it ideal for professional-grade solutions.
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/about/toc.yml" readonly="true"
    
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/concepts/philosophy.md" readonly="true"
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
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/concepts/toc.yml" readonly="true"
    - name: Philosophy
      href: philosophy.md
    - name: Architecture
      href: architecture.md
    - name: Patterns
      items:
        - name: Modularity
          href: patterns/modularity.md
        - name: Observability
          href: patterns/observability.md
        - name: Error Handling
          href: patterns/error-handling.md
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/guides/setup.md" readonly="true"
    ---
    title: "Setup — Local Development"
    slug: "setup-local"
    date: "2025-09-04"
    authors:
      - name: "Ulf Bourelius"
    category: guides
    tags:
      - setup
      - dev
    summary: "How to get the Zentient mono-repo running locally and build the docs with CLI safeguards."
    ---
    
    # Local setup
    
    Follow these two quick steps to get started.
    
    ## Install prerequisites
    
    - .NET SDK 7+  
    - Node 18+ (optional for tooling)  
    - `docfx` CLI installed globally or available in CI
    
    ## Build commands (CLI-first)
    
    ```bash
    # generate metadata (validate front matter)
    docfx metadata
    
    # build with strictness: treat warnings as errors
    docfx build --warningsAsErrors
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/guides/toc.yml" readonly="true"
    - name: Setup
      href: setup.md
    - name: Quick Start
      href: quick-start.md
    - name: Workflows
      items:
        - name: CI/CD
          href: workflows/ci-cd.md
        - name: Local Development
          href: workflows/local-development.md
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/reference/toc.yml" readonly="true"
    
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/docs/toc.yml" readonly="true"
    - name: Concepts
      href: concepts/toc.yml
    - name: Guides
      href: guides/toc.yml
    - name: Reference
      href: reference/toc.yml
    - name: About
      href: about/toc.yml
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/global.json" readonly="true"
    {
      "sdk": {
        "version": "10.0.203"
      }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/index.html" readonly="true"
    <!doctype html>
    <html lang="en">
    <head>
      <meta charset="utf-8">
      <title>ulfbou.github.io — SAOS Hub</title>
      <meta name="viewport" content="width=device-width,initial-scale=1">
      <style>
        body{font-family:system-ui,Segoe UI,Roboto,sans-serif;margin:2rem;max-width:720px}
        header{display:flex;justify-content:space-between;align-items:baseline}
        #meta{color:#666;font-size:.9rem}
        li{margin:.6rem 0}
        a{ text-decoration:none }
        a:hover{ text-decoration:underline }
      </style>
    </head>
    <body>
      <header>
        <h1>SAOS Hub</h1>
        <div id="meta">loading…</div>
      </header>
      <ul id="apps"></ul>
      <script>
        fetch('./apps.json',{cache:'no-store'})
         .then(r=>r.json())
         .then(d=>{
            document.getElementById('meta').textContent =
              `schema ${d.schemaVersion} • updated ${new Date(d.updatedAt).toLocaleString()}`;
            const ul=document.getElementById('apps');
            if(!d.apps||!d.apps.length){ul.innerHTML='<li>No apps registered yet</li>';return}
            d.apps.forEach(a=>{
              const li=document.createElement('li');
              li.innerHTML=`<a href="${a.entry}">${a.title||a.id}</a> <small>${a.id}@${a.version}</small>`;
              ul.appendChild(li);
            });
          })
         .catch(()=>{document.getElementById('meta').textContent='apps.json not found'});
      </script>
    </body>
    </html>
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/infra/linker.mjs" readonly="true"
    #!/usr/bin/env node
    /**
     * infra/linker.mjs
     *
     * Upserts an app entry from app-info.json into kernel/apps.json.
     * Idempotent: re-running with identical input produces identical output.
     * Apps are sorted alphabetically by id for deterministic diffs.
     *
     * Usage:
     *   node infra/linker.mjs <path-to-app-info.json> [path-to-apps.json]
     *
     * Defaults:
     *   apps.json path → kernel/apps.json (relative to cwd)
     *
     * Exit 0 = success
     * Exit 1 = failure (error written to stderr)
     */
    
    import { readFileSync, writeFileSync } from "node:fs";
    import { resolve } from "node:path";
    
    const [, , appInfoPath, appsJsonPath = "kernel/apps.json"] = process.argv;
    
    if (!appInfoPath) {
      console.error("Usage: linker.mjs <path-to-app-info.json> [path-to-apps.json]");
      process.exit(1);
    }
    
    // ── Read inputs ───────────────────────────────────────────────────────────────
    
    let appInfo;
    try {
      appInfo = JSON.parse(readFileSync(resolve(appInfoPath), "utf8"));
    } catch (err) {
      console.error(`FAIL  Cannot read app-info.json: ${err.message}`);
      process.exit(1);
    }
    
    let registry;
    try {
      registry = JSON.parse(readFileSync(resolve(appsJsonPath), "utf8"));
    } catch (err) {
      console.error(`FAIL  Cannot read ${appsJsonPath}: ${err.message}`);
      process.exit(1);
    }
    
    // ── Normalise registry ────────────────────────────────────────────────────────
    
    if (!Array.isArray(registry.apps)) {
      registry.apps = [];
    }
    registry.schemaVersion = "1.0";
    registry.updatedAt = new Date().toISOString();
    
    // ── Build canonical record ────────────────────────────────────────────────────
    
    const record = {
      id: appInfo.id,
      title: appInfo.title,
      entry: appInfo.entry,
      capabilities: appInfo.capabilities,
      version: appInfo.version,
    };
    
    // ── Upsert ────────────────────────────────────────────────────────────────────
    
    const existingIdx = registry.apps.findIndex((a) => a.id === appInfo.id);
    if (existingIdx >= 0) {
      registry.apps[existingIdx] = record;
    } else {
      registry.apps.push(record);
    }
    
    // ── Sort by id for deterministic diffs ───────────────────────────────────────
    
    registry.apps.sort((a, b) => a.id.localeCompare(b.id));
    
    // ── Write back ────────────────────────────────────────────────────────────────
    
    try {
      writeFileSync(resolve(appsJsonPath), JSON.stringify(registry, null, 2) + "\n", "utf8");
    } catch (err) {
      console.error(`FAIL  Cannot write ${appsJsonPath}: ${err.message}`);
      process.exit(1);
    }
    
    console.log(`OK    ${appInfo.id}@${appInfo.version} linked into ${appsJsonPath}`);
    process.exit(0);
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/infra/update-registry.mjs" readonly="true"
    import fs from 'node:fs';
    import path from 'node:path';
    
    const registryPath = path.join('kernel', 'apps.json');
    const appInfoPath = process.argv[2];
    
    if (!appInfoPath) {
      console.error('Usage: node update-registry.mjs <app-info.json>');
      process.exit(1);
    }
    
    const appInfo = JSON.parse(fs.readFileSync(appInfoPath, 'utf8'));
    
    let registry;
    try {
      registry = JSON.parse(fs.readFileSync(registryPath, 'utf8'));
    } catch {
      registry = { apps: [] };
    }
    
    // --- Issue #2 requirements ---
    if (!registry.schemaVersion) registry.schemaVersion = '1.0';
    registry.schemaVersion = '1.0';
    registry.updatedAt = new Date().toISOString();
    
    if (!Array.isArray(registry.apps)) registry.apps = [];
    
    // upsert app
    const idx = registry.apps.findIndex(a => a.id === appInfo.id);
    if (idx >= 0) {
      registry.apps[idx] = {...registry.apps[idx],...appInfo };
    } else {
      registry.apps.push(appInfo);
    }
    
    fs.writeFileSync(registryPath, JSON.stringify(registry, null, 2) + '\n');
    console.log(`Registry updated: ${appInfo.id}@${appInfo.version}`);
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/infra/validate-manifest.mjs" readonly="true"
    #!/usr/bin/env node
    /**
     * infra/validate-manifest.mjs
     *
     * Validates an app-info.json against SAOS IPC v1 constraints.
     *
     * Usage:
     *   node infra/validate-manifest.mjs <path-to-app-info.json>
     *
     * Exit 0 = valid
     * Exit 1 = invalid (errors written to stderr)
     */
    
    import { readFileSync } from "node:fs";
    import { resolve } from "node:path";
    
    const SAOS_IPC_VERSION = "1.0";
    const VALID_CAPABILITIES = new Set(["user", "admin", "readonly"]);
    const APP_ID_RE = /^[a-z0-9][a-z0-9-]*$/;
    const ENTRY_RE = /^\/[a-z0-9][a-z0-9-]*\/$/;
    
    const [, , inputPath] = process.argv;
    if (!inputPath) {
      console.error("Usage: validate-manifest.mjs <path-to-app-info.json>");
      process.exit(1);
    }
    
    // ── Parse ────────────────────────────────────────────────────────────────────
    
    let manifest;
    try {
      const raw = readFileSync(resolve(inputPath), "utf8");
      manifest = JSON.parse(raw);
    } catch (err) {
      console.error(`FAIL  Cannot parse ${inputPath}: ${err.message}`);
      process.exit(1);
    }
    
    if (typeof manifest !== "object" || manifest === null || Array.isArray(manifest)) {
      console.error("FAIL  Manifest must be a JSON object.");
      process.exit(1);
    }
    
    // ── Validate fields ──────────────────────────────────────────────────────────
    
    const errors = [];
    
    // id ─ lowercase alphanumeric with hyphens, no leading hyphen
    if (typeof manifest.id !== "string" || !APP_ID_RE.test(manifest.id)) {
      errors.push(
        `id must match /^[a-z0-9][a-z0-9-]*$/ (received: ${JSON.stringify(manifest.id)})`
      );
    }
    
    // title ─ non-empty string
    if (typeof manifest.title !== "string" || manifest.title.trim() === "") {
      errors.push("title must be a non-empty string");
    }
    
    // entry ─ absolute path, exactly /<id>/
    if (typeof manifest.entry !== "string" || !ENTRY_RE.test(manifest.entry)) {
      errors.push(
        `entry must be an absolute path like /my-app/ (received: ${JSON.stringify(manifest.entry)})`
      );
    } else if (typeof manifest.id === "string" && APP_ID_RE.test(manifest.id)) {
      const expected = `/${manifest.id}/`;
      if (manifest.entry !== expected) {
        errors.push(
          `entry must exactly equal /${manifest.id}/ (received: ${JSON.stringify(manifest.entry)})`
        );
      }
    }
    
    // capabilities ─ non-empty array of known strings
    if (!Array.isArray(manifest.capabilities) || manifest.capabilities.length === 0) {
      errors.push("capabilities must be a non-empty array");
    } else {
      for (const cap of manifest.capabilities) {
        if (typeof cap !== "string" || !VALID_CAPABILITIES.has(cap)) {
          errors.push(
            `unknown capability: ${JSON.stringify(cap)}  (valid: ${[...VALID_CAPABILITIES].join(", ")})`
          );
        }
      }
    }
    
    // version ─ non-empty string
    if (typeof manifest.version !== "string" || manifest.version.trim() === "") {
      errors.push("version must be a non-empty string");
    }
    
    // saos ─ when present must be "1.0"; absence defaults to "1.0" (forward-compat)
    if (manifest.saos !== undefined && manifest.saos !== SAOS_IPC_VERSION) {
      errors.push(
        `saos must be "${SAOS_IPC_VERSION}" (received: ${JSON.stringify(manifest.saos)})`
      );
    }
    
    // ── Report ───────────────────────────────────────────────────────────────────
    
    if (errors.length > 0) {
      console.error("FAIL  Manifest validation failed:");
      for (const msg of errors) {
        console.error(`      - ${msg}`);
      }
      process.exit(1);
    }
    
    console.log(`OK    ${manifest.id}@${manifest.version} is a valid SAOS IPC v1 manifest`);
    process.exit(0);
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/kernel/apps.json" readonly="true"
    {
      "schemaVersion": "1.0",
      "updatedAt": "2026-04-28T11:12:50.326Z",
      "apps": [
        {
          "capabilities": [
            "user"
          ],
          "description": "Local-first portfolio migrated from legacy static generator",
          "entry": "/portfolio/",
          "id": "portfolio",
          "title": "Ulf Bourelius Portfolio",
          "version": "2026.04.27"
        }
      ]
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/kernel/kernel.js" readonly="true"
    /**
     * SAOS Kernel — Hub Runtime
     *
     * Responsibilities (SAOS IPC v1 §3–§8):
     * - Load apps.json
     * - Initialise shared memory
     * - Emit saos:kernel:ready
     * - Route navigation (saos:nav:navigate)
     * - Handle logout (saos:user:logout)
     * - Handle app announcements (saos:app:announce)
     * - Handle capability requests (saos:capability:request)
     * - Never crash due to a malformed app message (§10)
     */
    
    "use strict";
    
    const SAOS_VERSION = "1.0";
    
    // ---------------------------------------------------------------------------
    // Shared memory — kernel-owned (§7)
    // ---------------------------------------------------------------------------
    
    const Memory = {
      write(key, value) {
        localStorage.setItem(`saos:${key}`, JSON.stringify(value));
      },
      read(key) {
        const raw = localStorage.getItem(`saos:${key}`);
        return raw ? JSON.parse(raw) : null;
      },
      remove(key) {
        localStorage.removeItem(`saos:${key}`);
      },
      writeSession(key, value) {
        sessionStorage.setItem(`saos:${key}`, JSON.stringify(value));
      },
      readSession(key) {
        const raw = sessionStorage.getItem(`saos:${key}`);
        return raw ? JSON.parse(raw) : null;
      },
      clearAll() {
        const keysToRemove = [];
        for (let i = 0; i < localStorage.length; i++) {
          const k = localStorage.key(i);
          if (k && k.startsWith("saos:")) keysToRemove.push(k);
        }
        for (const k of keysToRemove) localStorage.removeItem(k);
    
        const sessionKeysToRemove = [];
        for (let i = 0; i < sessionStorage.length; i++) {
          const k = sessionStorage.key(i);
          if (k && k.startsWith("saos:")) sessionKeysToRemove.push(k);
        }
        for (const k of sessionKeysToRemove) sessionStorage.removeItem(k);
      },
    };
    
    // ---------------------------------------------------------------------------
    // Process registry (§8)
    // ---------------------------------------------------------------------------
    
    const ProcessRegistry = {
      /** @type {Map<string, object>} */
      _procs: new Map(),
    
      register(sourceId, payload) {
        this._procs.set(sourceId, payload);
      },
    
      getAll() {
        return Array.from(this._procs.values());
      },
    };
    
    // ---------------------------------------------------------------------------
    // Event helpers
    // ---------------------------------------------------------------------------
    
    /**
     * Validate a SAOS IPC v1 envelope (§4).
     * Returns `false` for unknown versions or malformed payloads (§10).
     *
     * @param {unknown} detail
     * @returns {boolean}
     */
    function isValidEnvelope(detail) {
      return (
        typeof detail === "object" &&
        detail !== null &&
        detail.version === SAOS_VERSION &&
        typeof detail.payload !== "undefined"
      );
    }
    
    /**
     * Dispatch a SAOS IPC v1-conformant CustomEvent from the kernel.
     *
     * @param {string} action   Full event name suffix after `saos:`, e.g. `"kernel:ready"`.
     * @param {object} payload  Event-specific payload.
     */
    function kernelEmit(action, payload) {
      window.dispatchEvent(
        new CustomEvent(`saos:${action}`, {
          detail: {
            version: SAOS_VERSION,
            payload,
          },
        })
      );
    }
    
    // ---------------------------------------------------------------------------
    // Event handlers
    // ---------------------------------------------------------------------------
    
    /**
     * saos:nav:navigate (§6.1)
     *
     * Validates the destination path and updates the browser history.
     * Apps MUST NOT navigate cross-app via location.href.
     */
    function handleNavigate(e) {
      if (!isValidEnvelope(e.detail)) return;
    
      const { to, mode } = e.detail.payload;
      if (typeof to !== "string" || !to) return;
    
      const validMode = mode === "replace" ? "replace" : "push";
      if (validMode === "replace") {
        history.replaceState(null, "", to);
      } else {
        history.pushState(null, "", to);
      }
    
      // Notify shell components that navigation occurred
      window.dispatchEvent(new PopStateEvent("popstate", { state: null }));
    }
    
    /**
     * saos:user:logout (§6.2)
     *
     * Clears shared memory and redirects to the root.
     */
    function handleLogout(e) {
      if (!isValidEnvelope(e.detail)) return;
      Memory.clearAll();
      window.location.href = "/";
    }
    
    /**
     * saos:app:announce (§8.1)
     *
     * Registers the app in the process registry.
     * The kernel MAY show/hide/decorate navigation based on this.
     */
    function handleAppAnnounce(e) {
      if (!isValidEnvelope(e.detail)) return;
    
      const source = e.detail.source;
      const payload = e.detail.payload;
    
      if (typeof source !== "string" || !source) return;
    
      ProcessRegistry.register(source, {
        source,
        ...payload,
        registeredAt: new Date().toISOString(),
      });
    }
    
    /**
     * saos:capability:request (§6.3)
     *
     * Advisory only — governs UI behaviour, not security.
     * Backend APIs remain authoritative.
     */
    function handleCapabilityRequest(e) {
      if (!isValidEnvelope(e.detail)) return;
      // The kernel acknowledges capability requests but takes no privileged action.
      // Capability-aware UI components should listen to saos:kernel:ready instead.
    }
    
    // ---------------------------------------------------------------------------
    // Bootstrap
    // ---------------------------------------------------------------------------
    
    /**
     * Load the app registry from apps.json and initialise the kernel.
     *
     * @param {string} [appsJsonUrl="./apps.json"]
     */
    async function boot(appsJsonUrl = "./apps.json") {
      // 1. Load apps.json
      let apps = [];
      let capabilities = [];
      try {
        const resp = await fetch(appsJsonUrl);
        if (resp.ok) {
          const config = await resp.json();
          apps = Array.isArray(config.apps) ? config.apps.map((a) => a.id ?? a) : [];
          capabilities = Array.isArray(config.capabilities) ? config.capabilities : [];
        }
      } catch {
        // Non-fatal — kernel continues without app list
      }
    
      // 2. Initialise shared memory
      const theme = Memory.read("theme") ?? "dark";
      Memory.write("theme", theme);
    
      // 3. Attach IPC event listeners
      window.addEventListener("saos:nav:navigate", handleNavigate);
      window.addEventListener("saos:user:logout", handleLogout);
      window.addEventListener("saos:app:announce", handleAppAnnounce);
      window.addEventListener("saos:capability:request", handleCapabilityRequest);
    
      // 4. Emit saos:kernel:ready (§5.1)
      kernelEmit("kernel:ready", {
        theme,
        capabilities,
        apps,
      });
    }
    
    // Expose minimal public API for testing and shell integration
    window.SaosKernel = {
      boot,
      emit: kernelEmit,
      memory: Memory,
      registry: ProcessRegistry,
    };
    
    // Auto-boot when the DOM is ready
    if (document.readyState === "loading") {
      document.addEventListener("DOMContentLoaded", () => boot());
    } else {
      boot();
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/LICENSE.txt" readonly="true"
    MIT License
    
    Copyright (c) [year] [fullname]
    
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:
    
    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.
    
    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/README-SAOS.md" readonly="true"
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
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/README.md" readonly="true"
    # Homepage
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/SaosHub.sln" readonly="true"
    
    Microsoft Visual Studio Solution File, Format Version 12.00
    # Visual Studio Version 18
    VisualStudioVersion = 18.6.11716.218
    MinimumVisualStudioVersion = 10.0.40219.1
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Docs", "Docs", "{02EA681E-C7D8-13C7-8484-4AC65E1B71E8}"
    	ProjectSection(SolutionItems) = preProject
    		docs\toc.yml = docs\toc.yml
    	EndProjectSection
    EndProject
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "about", "about", "{D44C57A9-B739-4F72-9C0A-D3691B97F35F}"
    	ProjectSection(SolutionItems) = preProject
    		docs\about\toc.yml = docs\about\toc.yml
    	EndProjectSection
    EndProject
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "concepts", "concepts", "{A1D5A423-A071-4D8B-985E-DD2C9A6A6DC2}"
    	ProjectSection(SolutionItems) = preProject
    		docs\concepts\philosophy.md = docs\concepts\philosophy.md
    		docs\concepts\toc.yml = docs\concepts\toc.yml
    		docs\guides\setup.md = docs\guides\setup.md
    		docs\guides\toc.yml = docs\guides\toc.yml
    		docs\reference\toc.yml = docs\reference\toc.yml
    	EndProjectSection
    EndProject
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{827E0CD3-B72D-47B6-A68D-7590B98EB39B}"
    EndProject
    Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SaosHub.Blazor", "src\SaosHub.Blazor\SaosHub.Blazor.csproj", "{060CB5CF-CD94-E6DD-4159-6466C23364CD}"
    EndProject
    Global
    	GlobalSection(SolutionConfigurationPlatforms) = preSolution
    		Debug|Any CPU = Debug|Any CPU
    		Debug|x64 = Debug|x64
    		Debug|x86 = Debug|x86
    		Release|Any CPU = Release|Any CPU
    		Release|x64 = Release|x64
    		Release|x86 = Release|x86
    	EndGlobalSection
    	GlobalSection(ProjectConfigurationPlatforms) = postSolution
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|Any CPU.Build.0 = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|x64.ActiveCfg = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|x64.Build.0 = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|x86.ActiveCfg = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|x86.Build.0 = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|Any CPU.ActiveCfg = Release|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|Any CPU.Build.0 = Release|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|x64.ActiveCfg = Release|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|x64.Build.0 = Release|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|x86.ActiveCfg = Release|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|x86.Build.0 = Release|Any CPU
    	EndGlobalSection
    	GlobalSection(SolutionProperties) = preSolution
    		HideSolutionNode = FALSE
    	EndGlobalSection
    	GlobalSection(NestedProjects) = preSolution
    		{D44C57A9-B739-4F72-9C0A-D3691B97F35F} = {02EA681E-C7D8-13C7-8484-4AC65E1B71E8}
    		{A1D5A423-A071-4D8B-985E-DD2C9A6A6DC2} = {02EA681E-C7D8-13C7-8484-4AC65E1B71E8}
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD} = {827E0CD3-B72D-47B6-A68D-7590B98EB39B}
    	EndGlobalSection
    	GlobalSection(ExtensibilityGlobals) = postSolution
    		SolutionGuid = {F0F628E6-391A-491F-91E3-5F2F0EECF2AF}
    	EndGlobalSection
    EndGlobal
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/SaosHub.sln~" readonly="true"
    
    Microsoft Visual Studio Solution File, Format Version 12.00
    # Visual Studio Version 18
    VisualStudioVersion = 18.6.11716.218
    MinimumVisualStudioVersion = 10.0.40219.1
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "Docs", "Docs", "{02EA681E-C7D8-13C7-8484-4AC65E1B71E8}"
    	ProjectSection(SolutionItems) = preProject
    		docs\toc.yml = docs\toc.yml
    	EndProjectSection
    EndProject
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "about", "about", "{D44C57A9-B739-4F72-9C0A-D3691B97F35F}"
    	ProjectSection(SolutionItems) = preProject
    		docs\about\toc.yml = docs\about\toc.yml
    	EndProjectSection
    EndProject
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "concepts", "concepts", "{A1D5A423-A071-4D8B-985E-DD2C9A6A6DC2}"
    	ProjectSection(SolutionItems) = preProject
    		docs\concepts\philosophy.md = docs\concepts\philosophy.md
    	EndProjectSection
    EndProject
    Project("{2150E333-8FDC-42A3-9474-1A3956D46DE8}") = "src", "src", "{827E0CD3-B72D-47B6-A68D-7590B98EB39B}"
    EndProject
    Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "SaosHub.Blazor", "src\SaosHub.Blazor\SaosHub.Blazor.csproj", "{060CB5CF-CD94-E6DD-4159-6466C23364CD}"
    EndProject
    Global
    	GlobalSection(SolutionConfigurationPlatforms) = preSolution
    		Debug|Any CPU = Debug|Any CPU
    		Release|Any CPU = Release|Any CPU
    	EndGlobalSection
    	GlobalSection(ProjectConfigurationPlatforms) = postSolution
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Debug|Any CPU.Build.0 = Debug|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|Any CPU.ActiveCfg = Release|Any CPU
    		{060CB5CF-CD94-E6DD-4159-6466C23364CD}.Release|Any CPU.Build.0 = Release|Any CPU
    	EndGlobalSection
    	GlobalSection(SolutionProperties) = preSolution
    		HideSolutionNode = FALSE
    	EndGlobalSection
    	GlobalSection(NestedProjects) = preSolution
    		{D44C57A9-B739-4F72-9C0A-D3691B97F35F} = {02EA681E-C7D8-13C7-8484-4AC65E1B71E8}
    		{A1D5A423-A071-4D8B-985E-DD2C9A6A6DC2} = {02EA681E-C7D8-13C7-8484-4AC65E1B71E8}
    	EndGlobalSection
    	GlobalSection(ExtensibilityGlobals) = postSolution
    		SolutionGuid = {F0F628E6-391A-491F-91E3-5F2F0EECF2AF}
    	EndGlobalSection
    EndGlobal
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/schema/app-info.schema.json" readonly="true"
    {
      "$schema": "https://json-schema.org/draft-07/schema",
      "$id": "https://raw.githubusercontent.com/ulfbou/SAOS/main/schema/app-info.schema.json",
      "title": "SAOS App Manifest",
      "description": "app-info.json — identity and routing declaration for a SAOS userland application. Validated by the kernel before any registry update.",
      "type": "object",
      "required": ["saos", "id", "title", "entry", "capabilities", "version"],
      "additionalProperties": false,
      "properties": {
        "$schema": {
          "type": "string",
          "description": "Optional JSON Schema reference."
        },
        "saos": {
          "type": "string",
          "const": "1.0",
          "description": "SAOS IPC version. Must be '1.0' for all v1-compliant apps."
        },
        "id": {
          "type": "string",
          "pattern": "^[a-z0-9][a-z0-9-]*$",
          "description": "Canonical app identifier. Must exactly match the GitHub repository name."
        },
        "title": {
          "type": "string",
          "minLength": 1,
          "description": "Human-readable app name shown in hub navigation."
        },
        "entry": {
          "type": "string",
          "pattern": "^\\/[a-z0-9][a-z0-9-]*\\/$",
          "description": "Absolute URL path where the app is served. Must equal /<id>/."
        },
        "capabilities": {
          "type": "array",
          "minItems": 1,
          "uniqueItems": true,
          "items": {
            "type": "string",
            "enum": ["user", "admin", "readonly"]
          },
          "description": "Required capability gates. Advisory — governs UI behaviour only."
        },
        "version": {
          "type": "string",
          "minLength": 1,
          "description": "App version string. Stamped automatically by deploy.yml from the git tag or short SHA."
        }
      }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/dotnet/Saos.Interop/EventArgs.cs" readonly="true"
    namespace Saos.Interop;
    
    /// <summary>
    /// Event args carrying the new theme value, emitted with
    /// <c>saos:kernel:theme-changed</c> (SAOS IPC v1 §5.2).
    /// </summary>
    public sealed class ThemeChangedEventArgs : EventArgs
    {
        /// <summary>The new theme identifier, e.g. <c>"dark"</c> or <c>"light"</c>.</summary>
        public string Theme { get; }
    
        public ThemeChangedEventArgs(string theme)
        {
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
        }
    }
    
    /// <summary>
    /// Event args carrying the <c>saos:kernel:ready</c> payload (§5.1).
    /// </summary>
    public sealed class KernelReadyEventArgs : EventArgs
    {
        public string Theme { get; }
        public IReadOnlyList<string> Capabilities { get; }
        public IReadOnlyList<string> Apps { get; }
    
        public KernelReadyEventArgs(string theme, IReadOnlyList<string> capabilities, IReadOnlyList<string> apps)
        {
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
            Capabilities = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
            Apps = apps ?? throw new ArgumentNullException(nameof(apps));
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/dotnet/Saos.Interop/ISaosKernel.cs" readonly="true"
    namespace Saos.Interop;
    
    /// <summary>
    /// SAOS IPC v1 Blazor WASM SDK interface — <c>ISaosKernel</c>.
    ///
    /// This is the normative surface exposed to Blazor WASM userland applications.
    /// Implementations MUST use the official JavaScript interop bridge
    /// (<c>wwwroot/saosInterop.js</c>) to guarantee ABI conformance.
    ///
    /// See SAOS IPC v1 §9.2.
    /// </summary>
    public interface ISaosKernel
    {
        // -------------------------------------------------------------------------
        // Kernel → App events (§5)
        // -------------------------------------------------------------------------
    
        /// <summary>
        /// Raised once when the kernel has finished bootstrapping
        /// (<c>saos:kernel:ready</c>, §5.1).
        ///
        /// Apps SHOULD NOT emit privileged intents before this event fires.
        /// </summary>
        event EventHandler<KernelReadyEventArgs> KernelReady;
    
        /// <summary>
        /// Raised whenever the active theme changes
        /// (<c>saos:kernel:theme-changed</c>, §5.2).
        /// </summary>
        event EventHandler<ThemeChangedEventArgs> ThemeChanged;
    
        // -------------------------------------------------------------------------
        // App → Kernel syscalls (§6)
        // -------------------------------------------------------------------------
    
        /// <summary>
        /// Request cross-app navigation (<c>saos:nav:navigate</c>, §6.1).
        ///
        /// Apps MUST use this method instead of manipulating <c>location.href</c>.
        /// </summary>
        /// <param name="path">Destination path, e.g. <c>/tools/</c>.</param>
        /// <param name="mode"><c>"push"</c> (default) or <c>"replace"</c>.</param>
        ValueTask NavigateAsync(string path, string mode = "push");
    
        /// <summary>
        /// Announce this application to the kernel (<c>saos:app:announce</c>, §8.1).
        ///
        /// Should be called once <see cref="KernelReady"/> has fired.
        /// </summary>
        ValueTask AnnounceAsync(
            string title,
            string entry,
            string[] capabilities,
            string version);
    
        /// <summary>
        /// Request a user logout (<c>saos:user:logout</c>, §6.2).
        /// </summary>
        ValueTask LogoutAsync();
    
        /// <summary>
        /// Advisory capability request (<c>saos:capability:request</c>, §6.3).
        /// </summary>
        ValueTask RequestCapabilityAsync(string capability);
    
        // -------------------------------------------------------------------------
        // Shared memory — read-only access (§7)
        // -------------------------------------------------------------------------
    
        /// <summary>
        /// Read a kernel-owned value from <c>localStorage</c> (§7.1).
        ///
        /// The key is automatically prefixed with <c>saos:</c>.
        /// Apps MUST NOT write to these keys directly.
        /// </summary>
        ValueTask<TValue?> ReadLocalMemoryAsync<TValue>(string key);
    
        /// <summary>
        /// Read a kernel-owned value from <c>sessionStorage</c> (§7.1).
        ///
        /// The key is automatically prefixed with <c>saos:</c>.
        /// Apps MUST NOT write to these keys directly.
        /// </summary>
        ValueTask<TValue?> ReadSessionMemoryAsync<TValue>(string key);
    
        // -------------------------------------------------------------------------
        // Lifecycle
        // -------------------------------------------------------------------------
    
        /// <summary>
        /// Initialise the interop bridge and subscribe to kernel events.
        /// Call this once from the root component's <c>OnAfterRenderAsync</c>
        /// on the first render.
        /// </summary>
        ValueTask InitializeAsync();
    
        /// <summary>
        /// Unsubscribe from all kernel events and release JS references.
        /// </summary>
        ValueTask DisposeAsync();
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/dotnet/Saos.Interop/Saos.Interop.csproj" readonly="true"
    <Project Sdk="Microsoft.NET.Sdk.Razor">
    
      <PropertyGroup>
        <TargetFramework>net8.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
        <RootNamespace>Saos.Interop</RootNamespace>
        <AssemblyName>Saos.Interop</AssemblyName>
        <Description>SAOS IPC v1 Blazor WASM SDK — the standard library (libc) for SAOS Blazor userland applications.</Description>
        <Version>1.0.0</Version>
        <Authors>SAOS Platform Team</Authors>
        <PackageLicenseExpression>MIT</PackageLicenseExpression>
        <StaticWebAssetBasePath>/</StaticWebAssetBasePath>
      </PropertyGroup>
    
      <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="8.0.0" />
      </ItemGroup>
    
    </Project>
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/dotnet/Saos.Interop/SaosKernel.cs" readonly="true"
    using Microsoft.JSInterop;
    using System.Text.Json;
    
    namespace Saos.Interop;
    
    /// <summary>
    /// SAOS IPC v1 Blazor WASM SDK — <c>SaosKernel</c> implementation.
    ///
    /// Wraps <c>saos-core</c> via the JavaScript interop bridge
    /// (<c>wwwroot/saosInterop.js</c>) to provide an idiomatic C# surface to
    /// Blazor WASM userland applications.
    ///
    /// Register via DI using <see cref="ServiceCollectionExtensions.AddSaosKernel"/>.
    /// See SAOS IPC v1 §9.2.
    /// </summary>
    public sealed class SaosKernel : ISaosKernel, IAsyncDisposable
    {
        private readonly IJSRuntime _js;
        private readonly string _sourceId;
        private DotNetObjectReference<SaosKernel>? _selfRef;
        private bool _initialized;
    
        /// <inheritdoc/>
        public event EventHandler<KernelReadyEventArgs>? KernelReady;
    
        /// <inheritdoc/>
        public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
    
        public SaosKernel(IJSRuntime js, string sourceId)
        {
            _js = js ?? throw new ArgumentNullException(nameof(js));
            _sourceId = !string.IsNullOrWhiteSpace(sourceId)
                ? sourceId
                : throw new ArgumentException("sourceId must be non-empty.", nameof(sourceId));
        }
    
        // -------------------------------------------------------------------------
        // Lifecycle
        // -------------------------------------------------------------------------
    
        /// <inheritdoc/>
        public async ValueTask InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;
    
            _selfRef = DotNetObjectReference.Create(this);
    
            // Register .NET callbacks so the JS bridge can invoke them when
            // kernel events are dispatched on window.
            await _js.InvokeVoidAsync("saosInterop.initialize", _selfRef);
        }
    
        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (_initialized)
            {
                await _js.InvokeVoidAsync("saosInterop.dispose");
            }
            _selfRef?.Dispose();
        }
    
        // -------------------------------------------------------------------------
        // App → Kernel syscalls (§6)
        // -------------------------------------------------------------------------
    
        /// <inheritdoc/>
        public ValueTask NavigateAsync(string path, string mode = "push")
            => _js.InvokeVoidAsync(
                "saosInterop.emit",
                _sourceId,
                "nav:navigate",
                new { to = path, mode });
    
        /// <inheritdoc/>
        public ValueTask AnnounceAsync(
            string title,
            string entry,
            string[] capabilities,
            string version)
            => _js.InvokeVoidAsync(
                "saosInterop.emit",
                _sourceId,
                "app:announce",
                new { title, entry, capabilities, version });
    
        /// <inheritdoc/>
        public ValueTask LogoutAsync()
            => _js.InvokeVoidAsync(
                "saosInterop.emit",
                _sourceId,
                "user:logout",
                new { });
    
        /// <inheritdoc/>
        public ValueTask RequestCapabilityAsync(string capability)
            => _js.InvokeVoidAsync(
                "saosInterop.emit",
                _sourceId,
                "capability:request",
                new { capability });
    
        // -------------------------------------------------------------------------
        // Shared memory — read-only (§7)
        // -------------------------------------------------------------------------
    
        /// <inheritdoc/>
        public async ValueTask<TValue?> ReadLocalMemoryAsync<TValue>(string key)
        {
            var raw = await _js.InvokeAsync<string?>("saosInterop.readLocalMemory", key);
            return Deserialize<TValue>(raw);
        }
    
        /// <inheritdoc/>
        public async ValueTask<TValue?> ReadSessionMemoryAsync<TValue>(string key)
        {
            var raw = await _js.InvokeAsync<string?>("saosInterop.readSessionMemory", key);
            return Deserialize<TValue>(raw);
        }
    
        // -------------------------------------------------------------------------
        // Callbacks invoked from JavaScript (§5)
        // -------------------------------------------------------------------------
    
        /// <summary>
        /// Invoked by the JS bridge when <c>saos:kernel:ready</c> fires (§5.1).
        /// </summary>
        [JSInvokable]
        public void OnKernelReady(string theme, string[] capabilities, string[] apps)
        {
            KernelReady?.Invoke(this, new KernelReadyEventArgs(theme, capabilities, apps));
        }
    
        /// <summary>
        /// Invoked by the JS bridge when <c>saos:kernel:theme-changed</c> fires (§5.2).
        /// </summary>
        [JSInvokable]
        public void OnThemeChanged(string theme)
        {
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(theme));
        }
    
        // -------------------------------------------------------------------------
        // Internal helpers
        // -------------------------------------------------------------------------
    
        private static TValue? Deserialize<TValue>(string? raw)
        {
            if (raw is null) return default;
            try
            {
                return JsonSerializer.Deserialize<TValue>(raw);
            }
            catch (JsonException)
            {
                return default;
            }
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/dotnet/Saos.Interop/ServiceCollectionExtensions.cs" readonly="true"
    using Microsoft.Extensions.DependencyInjection;
    
    namespace Saos.Interop;
    
    /// <summary>
    /// DI registration helpers for the SAOS Interop SDK.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="ISaosKernel"/> as a scoped service using the
        /// supplied <paramref name="sourceId"/> (canonical app id / repository name).
        ///
        /// Usage in <c>Program.cs</c>:
        /// <code>
        /// builder.Services.AddSaosKernel("my-app");
        /// </code>
        /// </summary>
        public static IServiceCollection AddSaosKernel(
            this IServiceCollection services,
            string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("sourceId must be non-empty.", nameof(sourceId));
    
            services.AddScoped<ISaosKernel>(sp =>
            {
                var js = sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
                return new SaosKernel(js, sourceId);
            });
    
            return services;
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/dotnet/Saos.Interop/wwwroot/saosInterop.js" readonly="true"
    /**
     * SAOS IPC v1 — JavaScript Interop Bridge for Blazor WASM
     *
     * This script MUST be loaded before the Blazor runtime initialises.
     * Add it to your app's index.html:
     *
     *   <script src="_content/Saos.Interop/saosInterop.js"></script>
     *
     * Blazor developers MUST NOT dispatch CustomEvents directly;
     * always use the ISaosKernel C# interface instead.
     *
     * See SAOS IPC v1 §9.2.
     */
    
    window.saosInterop = (() => {
      "use strict";
    
      /** @type {{ invokeMethodAsync: (method: string, ...args: unknown[]) => Promise<unknown> } | null} */
      let _dotNetRef = null;
    
      /** @type {Array<() => void>} */
      const _disposers = [];
    
      /**
       * Build and dispatch a SAOS IPC v1-conformant CustomEvent.
       *
       * @param {string} source   Canonical app id (§4).
       * @param {string} action   Category:action without the saos: prefix (§3.2).
       * @param {object} payload  Event-specific payload (§4).
       */
      function emit(source, action, payload) {
        window.dispatchEvent(
          new CustomEvent(`saos:${action}`, {
            detail: {
              version: "1.0",
              source,
              payload,
            },
          })
        );
      }
    
      /**
       * Validate a SAOS IPC v1 event envelope (§4).
       * Unknown versions MUST be ignored (§4 ABI Rules).
       *
       * @param {unknown} detail
       * @returns {boolean}
       */
      function isValidEnvelope(detail) {
        return (
          typeof detail === "object" &&
          detail !== null &&
          detail.version === "1.0" &&
          typeof detail.payload !== "undefined"
        );
      }
    
      /**
       * Attach window-level listeners for kernel events and wire them to the
       * .NET object reference so C# event handlers are invoked.
       *
       * @param {{ invokeMethodAsync: (method: string, ...args: unknown[]) => Promise<unknown> }} dotNetRef
       */
      function initialize(dotNetRef) {
        _dotNetRef = dotNetRef;
    
        function onKernelReady(e) {
          if (!isValidEnvelope(e.detail)) return;
          const p = e.detail.payload;
          _dotNetRef.invokeMethodAsync(
            "OnKernelReady",
            p.theme ?? "",
            p.capabilities ?? [],
            p.apps ?? []
          );
        }
    
        function onThemeChanged(e) {
          if (!isValidEnvelope(e.detail)) return;
          const p = e.detail.payload;
          _dotNetRef.invokeMethodAsync("OnThemeChanged", p.theme ?? "");
        }
    
        window.addEventListener("saos:kernel:ready", onKernelReady);
        window.addEventListener("saos:kernel:theme-changed", onThemeChanged);
    
        _disposers.push(
          () => window.removeEventListener("saos:kernel:ready", onKernelReady),
          () => window.removeEventListener("saos:kernel:theme-changed", onThemeChanged)
        );
      }
    
      /** Remove all listeners and release the .NET reference. */
      function dispose() {
        for (const d of _disposers) d();
        _disposers.length = 0;
        _dotNetRef = null;
      }
    
      /**
       * Read-only access to a kernel-owned localStorage key (§7).
       * Returns the raw JSON string so C# can deserialize it.
       *
       * @param {string} key  Key suffix (saos: prefix is added automatically).
       * @returns {string | null}
       */
      function readLocalMemory(key) {
        return localStorage.getItem(`saos:${key}`);
      }
    
      /**
       * Read-only access to a kernel-owned sessionStorage key (§7).
       *
       * @param {string} key  Key suffix (saos: prefix is added automatically).
       * @returns {string | null}
       */
      function readSessionMemory(key) {
        return sessionStorage.getItem(`saos:${key}`);
      }
    
      return { emit, initialize, dispose, readLocalMemory, readSessionMemory };
    })();
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/js/saos-core/__tests__/saos-core.test.ts" readonly="true"
    /**
     * saos-core — Unit tests
     *
     * Tests validate the ABI rules defined in SAOS IPC v1 §4, §5, §6, §7, §8.
     */
    
    import { SaosProcess, SaosEvent } from "../src/saos-core";
    
    // jsdom provides window/localStorage/sessionStorage via jest testEnvironment: "jsdom"
    
    describe("SaosProcess constructor", () => {
      it("throws when sourceId is empty", () => {
        expect(() => new SaosProcess("")).toThrow();
      });
    
      it("throws when sourceId is whitespace", () => {
        expect(() => new SaosProcess("   ")).toThrow();
      });
    
      it("creates an instance with a valid sourceId", () => {
        expect(new SaosProcess("my-app")).toBeInstanceOf(SaosProcess);
      });
    });
    
    describe("SaosProcess.emit (§6)", () => {
      let proc: SaosProcess;
    
      beforeEach(() => {
        proc = new SaosProcess("test-app");
      });
    
      afterEach(() => {
        proc.dispose();
      });
    
      it("dispatches a CustomEvent prefixed with saos:", (done) => {
        window.addEventListener("saos:nav:navigate", (e) => {
          expect(e).toBeInstanceOf(CustomEvent);
          done();
        }, { once: true });
    
        proc.emit("nav:navigate", { to: "/tools/", mode: "push" });
      });
    
      it("envelope version is always '1.0'", (done) => {
        window.addEventListener("saos:nav:navigate", (e) => {
          const detail = (e as CustomEvent<SaosEvent>).detail;
          expect(detail.version).toBe("1.0");
          done();
        }, { once: true });
    
        proc.emit("nav:navigate", { to: "/tools/", mode: "push" });
      });
    
      it("envelope source matches sourceId", (done) => {
        window.addEventListener("saos:app:announce", (e) => {
          const detail = (e as CustomEvent<SaosEvent>).detail;
          expect(detail.source).toBe("test-app");
          done();
        }, { once: true });
    
        proc.emit("app:announce", { title: "Test", entry: "/test/", capabilities: [], version: "1.0.0" });
      });
    
      it("includes optional capability and correlationId", (done) => {
        window.addEventListener("saos:capability:request", (e) => {
          const detail = (e as CustomEvent<SaosEvent>).detail;
          expect(detail.capability).toBe("admin");
          expect(detail.correlationId).toBe("trace-123");
          done();
        }, { once: true });
    
        proc.emit("capability:request", { capability: "admin" }, {
          capability: "admin",
          correlationId: "trace-123",
        });
      });
    });
    
    describe("SaosProcess convenience methods (§6)", () => {
      let proc: SaosProcess;
    
      beforeEach(() => {
        proc = new SaosProcess("test-app");
      });
    
      afterEach(() => {
        proc.dispose();
      });
    
      it("navigate() emits saos:nav:navigate with correct payload", (done) => {
        window.addEventListener("saos:nav:navigate", (e) => {
          const detail = (e as CustomEvent<SaosEvent<{ to: string; mode: string }>>).detail;
          expect(detail.payload.to).toBe("/dashboard/");
          expect(detail.payload.mode).toBe("push");
          done();
        }, { once: true });
    
        proc.navigate("/dashboard/");
      });
    
      it("navigate() accepts 'replace' mode", (done) => {
        window.addEventListener("saos:nav:navigate", (e) => {
          const detail = (e as CustomEvent<SaosEvent<{ to: string; mode: string }>>).detail;
          expect(detail.payload.mode).toBe("replace");
          done();
        }, { once: true });
    
        proc.navigate("/tools/", "replace");
      });
    
      it("logout() emits saos:user:logout", (done) => {
        window.addEventListener("saos:user:logout", () => done(), { once: true });
        proc.logout();
      });
    
      it("requestCapability() emits saos:capability:request", (done) => {
        window.addEventListener("saos:capability:request", (e) => {
          const detail = (e as CustomEvent<SaosEvent<{ capability: string }>>).detail;
          expect(detail.payload.capability).toBe("admin");
          done();
        }, { once: true });
    
        proc.requestCapability("admin");
      });
    
      it("announce() emits saos:app:announce", (done) => {
        window.addEventListener("saos:app:announce", (e) => {
          const detail = (e as CustomEvent<SaosEvent<{ title: string }>>).detail;
          expect(detail.payload.title).toBe("My App");
          done();
        }, { once: true });
    
        proc.announce({ title: "My App", entry: "/my-app/", capabilities: ["user"], version: "1.0.0" });
      });
    });
    
    describe("SaosProcess.on (§5)", () => {
      let proc: SaosProcess;
    
      beforeEach(() => {
        proc = new SaosProcess("test-app");
      });
    
      afterEach(() => {
        proc.dispose();
      });
    
      it("receives saos:kernel:ready envelope", (done) => {
        proc.on<{ theme: string; capabilities: string[]; apps: string[] }>("kernel:ready", (e) => {
          expect(e.version).toBe("1.0");
          expect(e.payload.theme).toBe("dark");
          done();
        });
    
        window.dispatchEvent(new CustomEvent("saos:kernel:ready", {
          detail: {
            version: "1.0",
            payload: { theme: "dark", capabilities: ["user"], apps: ["dashboard"] },
          },
        }));
      });
    
      it("ignores events with wrong version (§4 — unknown versions MUST be ignored)", () => {
        const handler = jest.fn();
        proc.on("kernel:ready", handler);
    
        window.dispatchEvent(new CustomEvent("saos:kernel:ready", {
          detail: { version: "2.0", payload: {} },
        }));
    
        expect(handler).not.toHaveBeenCalled();
      });
    
      it("ignores malformed envelopes (§10)", () => {
        const handler = jest.fn();
        proc.on("kernel:ready", handler);
    
        window.dispatchEvent(new CustomEvent("saos:kernel:ready", {
          detail: "this is not an object",
        }));
    
        expect(handler).not.toHaveBeenCalled();
      });
    
      it("disposer returned by on() removes the listener", () => {
        const handler = jest.fn();
        const dispose = proc.on("kernel:theme-changed", handler);
    
        dispose();
    
        window.dispatchEvent(new CustomEvent("saos:kernel:theme-changed", {
          detail: { version: "1.0", payload: { theme: "light" } },
        }));
    
        expect(handler).not.toHaveBeenCalled();
      });
    });
    
    describe("SaosProcess.once", () => {
      let proc: SaosProcess;
    
      beforeEach(() => {
        proc = new SaosProcess("test-app");
      });
    
      afterEach(() => {
        proc.dispose();
      });
    
      it("fires only once", () => {
        const handler = jest.fn();
        proc.once("kernel:ready", handler);
    
        const detail = { version: "1.0", payload: { theme: "dark", capabilities: [], apps: [] } };
    
        window.dispatchEvent(new CustomEvent("saos:kernel:ready", { detail }));
        window.dispatchEvent(new CustomEvent("saos:kernel:ready", { detail }));
    
        expect(handler).toHaveBeenCalledTimes(1);
      });
    });
    
    describe("SaosProcess shared memory — read-only (§7)", () => {
      let proc: SaosProcess;
    
      beforeEach(() => {
        proc = new SaosProcess("test-app");
        localStorage.clear();
        sessionStorage.clear();
      });
    
      afterEach(() => {
        proc.dispose();
      });
    
      it("readLocalMemory returns null for absent keys", () => {
        expect(proc.readLocalMemory("user")).toBeNull();
      });
    
      it("readLocalMemory parses JSON written with the saos: prefix", () => {
        localStorage.setItem("saos:user", JSON.stringify({ name: "Alice" }));
        expect(proc.readLocalMemory<{ name: string }>("user")).toEqual({ name: "Alice" });
      });
    
      it("readLocalMemory returns null for malformed JSON", () => {
        localStorage.setItem("saos:user", "not-json");
        expect(proc.readLocalMemory("user")).toBeNull();
      });
    
      it("readSessionMemory returns null for absent keys", () => {
        expect(proc.readSessionMemory("session")).toBeNull();
      });
    
      it("readSessionMemory parses JSON written with the saos: prefix", () => {
        sessionStorage.setItem("saos:session", JSON.stringify({ token: "xyz" }));
        expect(proc.readSessionMemory<{ token: string }>("session")).toEqual({ token: "xyz" });
      });
    });
    
    describe("SaosProcess.dispose", () => {
      it("removes all listeners after dispose()", () => {
        const proc = new SaosProcess("test-app");
        const handler = jest.fn();
        proc.on("kernel:ready", handler);
    
        proc.dispose();
    
        window.dispatchEvent(new CustomEvent("saos:kernel:ready", {
          detail: { version: "1.0", payload: { theme: "dark", capabilities: [], apps: [] } },
        }));
    
        expect(handler).not.toHaveBeenCalled();
      });
    });
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/js/saos-core/package.json" readonly="true"
    {
      "name": "saos-core",
      "version": "1.0.0",
      "description": "SAOS IPC v1 JavaScript/TypeScript SDK — the standard library (libc) for SAOS userland applications",
      "main": "dist/saos-core.js",
      "module": "dist/saos-core.esm.js",
      "types": "dist/saos-core.d.ts",
      "files": [
        "dist"
      ],
      "scripts": {
        "build": "tsc",
        "build:watch": "tsc --watch",
        "test": "node --experimental-vm-modules node_modules/.bin/jest"
      },
      "keywords": [
        "saos",
        "ipc",
        "sdk",
        "blazor",
        "wasm"
      ],
      "license": "MIT",
      "devDependencies": {
        "@types/jest": "^29.5.12",
        "jest": "^29.7.0",
        "jest-environment-jsdom": "^30.3.0",
        "ts-jest": "^29.1.4",
        "typescript": "^5.4.5"
      },
      "jest": {
        "preset": "ts-jest",
        "testEnvironment": "jsdom",
        "testMatch": [
          "**/__tests__/**/*.test.ts"
        ]
      }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/js/saos-core/src/index.ts" readonly="true"
    /**
     * saos-core — SAOS IPC v1 JavaScript/TypeScript SDK
     *
     * Public surface — re-export everything from saos-core.ts so consumers can
     * import directly from `"saos-core"`.
     */
    export {
      SaosProcess,
    } from "./saos-core";
    
    export type {
      SaosEvent,
      KernelReadyPayload,
      ThemeChangedPayload,
      NavigatePayload,
      CapabilityRequestPayload,
      AppAnnouncePayload,
    } from "./saos-core";
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/js/saos-core/src/saos-core.ts" readonly="true"
    /**
     * SAOS IPC v1 — JavaScript/TypeScript SDK (saos-core)
     *
     * This is the normative reference SDK adapter for SAOS IPC v1.
     * All compliant SAOS userland applications MUST use this SDK (or a
     * byte-for-byte compatible reimplementation) to guarantee ABI correctness
     * and shared-memory discipline.
     *
     * @see docs/ipc-v1.md
     * @version 1.0.0
     */
    
    // ---------------------------------------------------------------------------
    // ABI — Event Envelope (§4)
    // ---------------------------------------------------------------------------
    
    /**
     * The frozen SAOS IPC v1 envelope that wraps every custom event's `detail`.
     *
     * Rules (§4):
     * - `version` MUST be "1.0".
     * - Unknown versions MUST be ignored by the kernel.
     * - Malformed envelopes MUST NOT cause kernel failure.
     */
    export interface SaosEvent<T = unknown> {
      readonly version: "1.0";
      /** Canonical app id (repository name). */
      readonly source: string;
      /** Requested capability — advisory only. */
      readonly capability?: string;
      /** Optional tracing correlation token. */
      readonly correlationId?: string;
      readonly payload: T;
    }
    
    // ---------------------------------------------------------------------------
    // Payload schemas (§5, §6, §8)
    // ---------------------------------------------------------------------------
    
    /** `saos:kernel:ready` payload (§5.1) */
    export interface KernelReadyPayload {
      readonly theme: string;
      readonly capabilities: readonly string[];
      readonly apps: readonly string[];
    }
    
    /** `saos:kernel:theme-changed` payload (§5.2) */
    export interface ThemeChangedPayload {
      readonly theme: string;
    }
    
    /** `saos:nav:navigate` payload (§6.1) */
    export interface NavigatePayload {
      readonly to: string;
      readonly mode: "push" | "replace";
    }
    
    /** `saos:capability:request` payload (§6.3) */
    export interface CapabilityRequestPayload {
      readonly capability: string;
    }
    
    /** `saos:app:announce` payload (§8.1) */
    export interface AppAnnouncePayload {
      readonly title: string;
      readonly entry: string;
      readonly capabilities: readonly string[];
      readonly version: string;
    }
    
    // ---------------------------------------------------------------------------
    // SaosProcess — primary SDK class (§9.1)
    // ---------------------------------------------------------------------------
    
    /**
     * `SaosProcess` is the SDK handle for a SAOS userland application.
     *
     * Responsibilities:
     * - Emits properly-enveloped IPC events via `window.dispatchEvent`.
     * - Provides read-only access to kernel shared memory.
     * - Prevents accidental writes to `localStorage.saos:*` or
     *   `sessionStorage.saos:*` keys (§7.2).
     *
     * Usage:
     * ```ts
     * const proc = new SaosProcess("my-app");
     *
     * proc.on("kernel:ready", (e) => {
     *   console.log("theme:", e.payload.theme);
     *   proc.announce({ title: "My App", entry: "/my-app/", capabilities: ["user"], version: "1.0.0" });
     * });
     * ```
     */
    export class SaosProcess {
      private readonly _sourceId: string;
      private readonly _listeners: Map<string, EventListenerOrEventListenerObject[]>;
    
      constructor(sourceId: string) {
        if (!sourceId || sourceId.trim() === "") {
          throw new Error("[saos-core] sourceId must be a non-empty string.");
        }
        this._sourceId = sourceId;
        this._listeners = new Map();
      }
    
      // -------------------------------------------------------------------------
      // Emit — App → Kernel syscalls (§6)
      // -------------------------------------------------------------------------
    
      /**
       * Dispatches a SAOS IPC event on `window`.
       *
       * The `action` is prefixed with `saos:` automatically so callers provide
       * only the `<category>:<action>` portion (e.g. `"nav:navigate"`).
       *
       * @param action   Category and action, e.g. `"nav:navigate"`.
       * @param payload  The event-specific payload.
       * @param options  Optional capability and correlationId metadata.
       */
      emit<T>(
        action: string,
        payload: T,
        options?: { capability?: string; correlationId?: string }
      ): void {
        const envelope: SaosEvent<T> = {
          version: "1.0",
          source: this._sourceId,
          capability: options?.capability,
          correlationId: options?.correlationId,
          payload,
        };
    
        window.dispatchEvent(
          new CustomEvent(`saos:${action}`, { detail: envelope })
        );
      }
    
      // -------------------------------------------------------------------------
      // Convenience syscalls
      // -------------------------------------------------------------------------
    
      /**
       * Request cross-app navigation (§6.1).
       * Apps MUST use this method instead of `location.href`.
       */
      navigate(to: string, mode: "push" | "replace" = "push"): void {
        this.emit<NavigatePayload>("nav:navigate", { to, mode });
      }
    
      /**
       * Request a user logout (§6.2).
       */
      logout(): void {
        this.emit("user:logout", {});
      }
    
      /**
       * Request a capability (advisory, §6.3).
       */
      requestCapability(capability: string): void {
        this.emit<CapabilityRequestPayload>(
          "capability:request",
          { capability },
          { capability }
        );
      }
    
      /**
       * Announce this app to the kernel (§8.1).
       * Should be called once `saos:kernel:ready` is received.
       */
      announce(payload: AppAnnouncePayload): void {
        this.emit<AppAnnouncePayload>("app:announce", payload);
      }
    
      // -------------------------------------------------------------------------
      // Listen — Kernel → App events (§5)
      // -------------------------------------------------------------------------
    
      /**
       * Register a listener for a kernel-emitted SAOS event.
       *
       * The `action` is prefixed with `saos:` automatically.
       * The listener receives the typed `SaosEvent<T>` envelope.
       *
       * @param action   E.g. `"kernel:ready"` or `"kernel:theme-changed"`.
       * @param listener Callback that receives the parsed envelope.
       * @returns        A disposer function; call it to remove the listener.
       */
      on<T>(
        action: string,
        listener: (event: SaosEvent<T>) => void
      ): () => void {
        const eventName = `saos:${action}`;
        const handler = (e: Event) => {
          const ce = e as CustomEvent<SaosEvent<T>>;
          if (!SaosProcess._isValidEnvelope(ce.detail)) {
            return;
          }
          listener(ce.detail);
        };
    
        window.addEventListener(eventName, handler);
    
        if (!this._listeners.has(eventName)) {
          this._listeners.set(eventName, []);
        }
        this._listeners.get(eventName)!.push(handler);
    
        return () => {
          window.removeEventListener(eventName, handler);
          const list = this._listeners.get(eventName);
          if (list) {
            const idx = list.indexOf(handler);
            if (idx !== -1) list.splice(idx, 1);
          }
        };
      }
    
      /**
       * Register a one-time listener that removes itself after the first call.
       */
      once<T>(
        action: string,
        listener: (event: SaosEvent<T>) => void
      ): () => void {
        let dispose: (() => void) | undefined;
        dispose = this.on<T>(action, (event) => {
          dispose?.();
          listener(event);
        });
        return dispose;
      }
    
      // -------------------------------------------------------------------------
      // Shared memory — read-only access (§7)
      // -------------------------------------------------------------------------
    
      /**
       * Read a value from kernel-managed `localStorage`.
       *
       * Apps MUST NOT write to `localStorage.saos:*` keys directly (§7.2).
       * This method provides safe read-only access.
       *
       * @param key  The key suffix (without the `saos:` prefix).
       * @returns    Parsed value or `null` if absent.
       */
      readLocalMemory<T = unknown>(key: string): T | null {
        const raw = localStorage.getItem(`saos:${key}`);
        if (raw === null) return null;
        try {
          return JSON.parse(raw) as T;
        } catch {
          return null;
        }
      }
    
      /**
       * Read a value from kernel-managed `sessionStorage`.
       *
       * @param key  The key suffix (without the `saos:` prefix).
       * @returns    Parsed value or `null` if absent.
       */
      readSessionMemory<T = unknown>(key: string): T | null {
        const raw = sessionStorage.getItem(`saos:${key}`);
        if (raw === null) return null;
        try {
          return JSON.parse(raw) as T;
        } catch {
          return null;
        }
      }
    
      // -------------------------------------------------------------------------
      // Lifecycle
      // -------------------------------------------------------------------------
    
      /**
       * Remove all event listeners registered via `on()` or `once()`.
       * Call this when the application is unmounting/disposing.
       */
      dispose(): void {
        for (const [eventName, handlers] of this._listeners) {
          for (const handler of handlers) {
            window.removeEventListener(eventName, handler);
          }
        }
        this._listeners.clear();
      }
    
      // -------------------------------------------------------------------------
      // Internal helpers
      // -------------------------------------------------------------------------
    
      private static _isValidEnvelope(detail: unknown): detail is SaosEvent {
        if (typeof detail !== "object" || detail === null) return false;
        const d = detail as Record<string, unknown>;
        return d["version"] === "1.0" && typeof d["payload"] !== "undefined";
      }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/sdk/js/saos-core/tsconfig.json" readonly="true"
    {
      "compilerOptions": {
        "target": "ES2020",
        "module": "ES2020",
        "moduleResolution": "node",
        "declaration": true,
        "declarationMap": true,
        "sourceMap": true,
        "strict": true,
        "esModuleInterop": true,
        "outDir": "./dist",
        "rootDir": "./src",
        "lib": ["ES2020", "DOM"]
      },
      "include": ["src/**/*.ts"],
      "exclude": ["node_modules", "dist", "__tests__"]
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/_Imports.razor" readonly="true"
    @using System.Net.Http
    @using System.Net.Http.Json
    @using Microsoft.AspNetCore.Components.Forms
    @using Microsoft.AspNetCore.Components.Routing
    @using Microsoft.AspNetCore.Components.Web
    @using Microsoft.AspNetCore.Components.Web.Virtualization
    @using Microsoft.JSInterop
    @using SaosHub.Blazor
    @using SaosHub.Blazor.Shared
    @using SaosHub.Blazor.Components
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/App.razor" readonly="true"
    @using SaosHub.Blazor.Components.Layout
    <Router AppAssembly="@typeof(App).Assembly">
        <Found Context="routeData">
            <RouteView RouteData="@routeData" DefaultLayout="@typeof(SaosLayout)" />
        </Found>
        <NotFound>
            <LayoutView Layout="@typeof(SaosLayout)">
                <p>404 — kernel path not found</p>
            </LayoutView>
        </NotFound>
    </Router>
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/Layout/SaosLayout.razor" readonly="true"
    @inherits LayoutComponentBase
    
    @if (!_bootDone)
    {
        <SaosBoot OnFinished="HandleBootFinished" />
    }
    
    <div id="os" class="@(_bootDone? "visible" : "")">
        <SaosTitleBar />
        <div class="shell">
            <SaosSidebar ActiveView="@_currentView" OnNavigate="Navigate" />
            <main class="main">
                <CascadingValue Value="_currentView">
                    @Body
                </CascadingValue>
            </main>
        </div>
    </div>
    
    @code {
        private bool _bootDone;
        private string _currentView = "overview";
    
        private void HandleBootFinished() => _bootDone = true;
        private void Navigate(string id) => _currentView = id;
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosBoot.razor" readonly="true"
    <div id="boot" class="@(_hidden? "hidden" : "")">
      <div class="boot-logo">@Logo</div>
      <div class="boot-sub">@Subtitle</div>
      <div class="boot-bar-wrap"><div class="boot-bar" style="width:@_progress%"></div></div>
      <div class="boot-msg">@_message</div>
    </div>
    
    @code {
        [Parameter] public string Logo { get; set; } = "SAOS";
        [Parameter] public string Subtitle { get; set; } = "STATIC APPLICATION OPERATING SYSTEM";
        [Parameter] public EventCallback OnFinished { get; set; }
    
        private int _progress;
        private string _message = "initializing kernel...";
        private bool _hidden;
        private readonly string[] _msgs = new[] {
            "initializing kernel...",
            "loading IPC bus...",
            "mounting /apps...",
            "checking registry...",
            "handshake complete"
        };
    
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            var rnd = new Random();
            for (int i = 0; i <= 100; i += rnd.Next(4,9))
            {
                _progress = Math.Min(i, 100);
                _message = _msgs[Math.Min(_progress / 20, _msgs.Length - 1)];
                StateHasChanged();
                await Task.Delay(35);
            }
            await Task.Delay(250);
            _hidden = true;
            StateHasChanged();
            await OnFinished.InvokeAsync();
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosBoot.razor.css" readonly="true"
    .boot-logo { font-family: var(--font-display); font-size: 64px; font-weight: 800; letter-spacing: -2px; color: var(--green); text-shadow: 0 0 40px var(--green), 0 0 80px rgba(63,185,80,0.3); animation: pulse-glow 2s ease-in-out infinite; }
    .boot-sub { font-size: 11px; letter-spacing: 4px; color: var(--text-muted); text-transform: uppercase; margin-top: 8px; margin-bottom: 40px; }
    .boot-bar-wrap { width: 280px; height: 2px; background: var(--border); border-radius: 2px; overflow: hidden; }
    .boot-bar { height: 100%; background: var(--green); box-shadow: 0 0 10px var(--green); transition: width 0.05s linear; }
    .boot-msg { margin-top: 16px; font-size: 11px; color: var(--text-dim); height: 16px; letter-spacing: 0.5px; }
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosCard.razor" readonly="true"
    <div class="card @Variant">
      <div class="card-header">
        <span class="card-icon">@Icon</span>
        <h3>@Title</h3>
      </div>
      <div class="card-body">
        @ChildContent
      </div>
    </div>
    
    @code {
        [Parameter] public string Title { get; set; } = "";
        [Parameter] public string Icon { get; set; } = "◉";
        [Parameter] public string Variant { get; set; } = "";
        [Parameter] public RenderFragment? ChildContent { get; set; }
    }
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosCodeBlock.razor" readonly="true"
    @inject IJSRuntime JS
    <div class="code-block">
      <div class="code-header">
        <span>@Lang</span>
        <button class="copy-btn" @onclick="Copy">@(_copied? "copied!" : "copy")</button>
      </div>
      <pre><code>@Code</code></pre>
    </div>
    
    @code {
        [Parameter] public string Lang { get; set; } = "";
        [Parameter] public string Code { get; set; } = "";
        private bool _copied;
    
        private async Task Copy()
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", Code);
            _copied = true;
            StateHasChanged();
            await Task.Delay(1500);
            _copied = false;
            StateHasChanged();
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosDiagPanel.razor" readonly="true"
    @implements IDisposable
    <div class="diag-panel">
      <div class="diag-header">kernel://events</div>
      <div class="diag-body">
        @foreach(var e in _events){
          <div class="diag-event">
            <span class="diag-time">@e.Time</span>
            <span class="diag-type @e.Cls">@e.Type</span>
            <span class="diag-msg">@e.Msg</span>
          </div>
        }
      </div>
    </div>
    @code {
      record Ev(string Time,string Type,string Cls,string Msg);
      List<Ev> _events=new();
      Timer? _t;
      string[][] _pool = new[]{ new[]{"reg","reg","MyLab v1.3.0 registered"}, new[]{"navigate","nav","→ /mylab/components"}, new[]{"ready","rdy","kernel handshake OK"}, new[]{"error","err","app-info.json not found"}};
      protected override void OnInitialized(){ for(int i=0;i<5;i++)Add(); _t=new(_=>{Add();InvokeAsync(StateHasChanged);},null,3200,3200);}
      void Add(){ var r=new Random(); var x=_pool[r.Next(_pool.Length)]; _events.Insert(0,new(DateTime.Now.ToString("HH:mm:ss"),x[0],x[1],x[2])); if(_events.Count>6)_events.RemoveAt(6);}
      public void Dispose()=>_t?.Dispose();
    }
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosProgressBar.razor" readonly="true"
    <div class="progress-item">
      <div class="progress-label"><span>@Label</span><span>@Value%</span></div>
      <div class="progress-bar"><div class="progress-fill" data-width="@Value" style="width:@Value%; background: var(--@Color)"></div></div>
    </div>
    
    @code {
        [Parameter] public string Label { get; set; } = "";
        [Parameter] public int Value { get; set; }
        [Parameter] public string Color { get; set; } = "green";
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosSidebar.razor" readonly="true"
    <aside class="sidebar">
      @foreach (var sec in _sections)
      {
        <div class="sidebar-section">
          <div class="sidebar-label">@sec.Label</div>
          @foreach (var item in sec.Items)
          {
            <div class="sidebar-item @(item.Id == ActiveView? "active" : "")" @onclick="() => OnNavigate.InvokeAsync(item.Id)">
              <span>@item.Icon</span><span>@item.Name</span>
            </div>
          }
        </div>
      }
    </aside>
    
    @code {
        [Parameter] public string ActiveView { get; set; } = "overview";
        [Parameter] public EventCallback<string> OnNavigate { get; set; }
    
        private record Item(string Id, string Name, string Icon);
        private record Section(string Label, Item[] Items);
        private readonly Section[] _sections = new[]
        {
            new Section("System", new[]{ new Item("overview","Overview","◉"), new Item("architecture","Architecture","◎"), new Item("phases","Phases","◍") }),
            new Section("Repository", new[]{ new Item("repo","Structure","▦"), new Item("template","Template","✦") }),
            new Section("Diagnostics", new[]{ new Item("diag","Live Feed","⎔") })
        };
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosSidebar.razor.cs" readonly="true"
    using Microsoft.AspNetCore.Components;
    
    namespace SaosRoadmap.Components;
    
    public partial class SaosSidebar
    {
        [Parameter] public EventCallback<string> OnNavigate { get; set; }
        private string _active = "overview";
    
        private record NavItem(string Key, string Label, string Icon);
        private NavItem[] SystemItems = new[] {
            new NavItem("overview","Overview","◉"),
            new NavItem("architecture","Architecture","◎"),
            new NavItem("phases","Phases","◍"),
        };
        private NavItem[] RepoItems = new[] {
            new NavItem("repo","Structure","▦"),
            new NavItem("template","Template","✦"),
            new NavItem("diag","Live Feed","⎔"),
        };
    
        private async Task Activate(string key)
        {
            _active = key;
            await OnNavigate.InvokeAsync(key);
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosTitleBar.razor" readonly="true"
    @implements IDisposable
    <div class="titlebar">
      <div class="titlebar-dots">
        <div class="dot dot-red"></div>
        <div class="dot dot-yellow"></div>
        <div class="dot dot-green"></div>
      </div>
      <div class="titlebar-center">
        <span class="titlebar-logo">SAOS</span>
        <span class="titlebar-slash">/</span>
        <span class="titlebar-path">roadmap.sh</span>
      </div>
      <div class="titlebar-right">
        <span class="status-pill pill-green">LIVE</span>
        <span class="status-pill pill-blue">v0.1</span>
        <span class="clock">@_time</span>
      </div>
    </div>
    
    @code {
        private string _time = DateTime.Now.ToString("HH:mm:ss");
        private System.Threading.Timer? _timer;
    
        protected override void OnInitialized()
        {
            _timer = new System.Threading.Timer(_ => {
                _time = DateTime.Now.ToString("HH:mm:ss");
                InvokeAsync(StateHasChanged);
            }, null, 0, 1000);
        }
    
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/SaosTreeView.razor" readonly="true"
    @inject IJSRuntime JS
    <div class="tree" id="@TreeId"></div>
    
    @code {
        [Parameter] public string TreeId { get; set; } = "repoTree";
    
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // trees are rendered by saosInterop.navigate when section becomes active
                await Task.Delay(1);
            }
        }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Components/UI/SaosTreeView.razor.bak" readonly="true"
    
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Pages/Index.razor" readonly="true"
    @page "/"
    
    <section id="overview" class="section @(CurrentView=="overview"?"active":"")">
      <div class="section-header">
        <h1>Static Application Operating System</h1>
        <p class="subtitle">A kernel for Blazor WASM micro-frontends on GitHub Pages</p>
      </div>
      <div class="grid-3">
        <SaosCard Title="Zero Config Apps" Icon="✦" Variant="green">
          <p>Apps self-register via <code>saos.app.register</code>. No central routing table.</p>
        </SaosCard>
        <SaosCard Title="Kernel IPC" Icon="⎔" Variant="blue">
          <p>Event bus in TypeScript. Apps publish/subscribe without knowing each other.</p>
        </SaosCard>
        <SaosCard Title="GitOps Deploy" Icon="⬢" Variant="purple">
          <p>Each app repo dispatches to the kernel repo. Kernel rebuilds <code>apps.json</code>.</p>
        </SaosCard>
      </div>
    </section>
    
    <section id="architecture" class="section @(CurrentView=="architecture"?"active":"")">
      <div class="section-header"><h2>Architecture</h2></div>
      <SaosCodeBlock Lang="typescript" Code="@_kernelCode" />
    </section>
    
    <section id="phases" class="section @(CurrentView=="phases"?"active":"")">
      <div class="section-header"><h2>Implementation Phases</h2></div>
      <div class="phase-list">
        <SaosProgressBar Label="Phase 1 - Kernel Shell" Value="100" Color="green" />
        <SaosProgressBar Label="Phase 2 - App Registry" Value="75" Color="blue" />
        <SaosProgressBar Label="Phase 3 - Interop NuGet" Value="40" Color="orange" />
        <SaosProgressBar Label="Phase 4 - Automation" Value="20" Color="purple" />
      </div>
    </section>
    
    <section id="repo" class="section @(CurrentView=="repo"?"active":"")">
      <div class="section-header"><h2>Repository Structure</h2></div>
      <div class="tree" id="repoTree"></div>
    </section>
    
    <section id="template" class="section @(CurrentView=="template"?"active":"")">
      <div class="section-header"><h2>dotnet new Template</h2></div>
      <div class="tree" id="templateTree"></div>
      <SaosCodeBlock Lang="bash" Code="dotnet new install Saos.Templates
    dotnet new saos-blazor -n MyLab" />
    </section>
    
    <section id="diag" class="section @(CurrentView=="diag"?"active":"")">
      <div class="section-header"><h2>Live Diagnostics</h2></div>
      <SaosDiagPanel />
    </section>
    
    @code {
        [CascadingParameter] public string CurrentView { get; set; } = "overview";
    
        private string _kernelCode = @"// kernel.ts
    window.saos = {
      register: (m) => bus.emit('saos.app.register', m),
      navigate: (p) => bus.emit('saos.navigate', p)
    };";
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Program.cs" readonly="true"
    using Microsoft.AspNetCore.Components.Web;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<SaosHub.Blazor.App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    await builder.Build().RunAsync();
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Properties/launchSettings.json" readonly="true"
    {
      "profiles": {
        "SaosHub.Blazor": {
          "commandName": "Project",
          "launchBrowser": true,
          "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
          },
          "applicationUrl": "https://localhost:64805;http://localhost:64806"
        }
      }
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/SaosHub.Blazor.csproj" readonly="true"
    <Project Sdk="Microsoft.NET.Sdk.BlazorWebAssembly">
      <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <Nullable>enable</Nullable>
        <ImplicitUsings>enable</ImplicitUsings>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="Microsoft.AspNetCore.Components.WebAssembly" Version="10.0.0" />
      </ItemGroup>
    </Project>
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Shared/MainLayout.razor" readonly="true"
    @inherits LayoutComponentBase
    
    @if (!_bootDone)
    {
        <SaosBoot OnFinished="HandleBootFinished" />
    }
    
    <div id="os" class="@(_bootDone? "visible" : "")">
        <SaosTitleBar />
        <div class="shell">
            <SaosSidebar ActiveView="@_currentView" OnNavigate="Navigate" />
            <main class="main">
                <CascadingValue Value="_currentView">
                    @Body
                </CascadingValue>
            </main>
        </div>
    </div>
    
    @code {
        private bool _bootDone;
        private string _currentView = "overview";
    
        private void HandleBootFinished() => _bootDone = true;
        private void Navigate(string id) => _currentView = id;
    }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/Views/BlazorInfra.razor" readonly="true"
    
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/wwwroot/css/app.css" readonly="true"
    
      :root {
        --bg: #080c10;
        --surface: #0d1117;
        --surface2: #161b22;
        --surface3: #1c2333;
        --border: #21262d;
        --border-bright: #30363d;
        --text: #e6edf3;
        --text-muted: #7d8590;
        --text-dim: #484f58;
        --green: #3fb950;
        --green-glow: rgba(63,185,80,0.15);
        --blue: #58a6ff;
        --blue-glow: rgba(88,166,255,0.12);
        --orange: #e3b341;
        --orange-glow: rgba(227,179,65,0.12);
        --purple: #bc8cff;
        --purple-glow: rgba(188,140,255,0.12);
        --red: #f85149;
        --accent: #3fb950;
        --font-mono: 'JetBrains Mono', monospace;
        --font-display: 'Syne', sans-serif;
      }
    
      *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
    
      body {
        background: var(--bg);
        color: var(--text);
        font-family: var(--font-mono);
        font-size: 13px;
        line-height: 1.6;
        min-height: 100vh;
        overflow-x: hidden;
      }
    
      /* Scanline overlay */
      body::before {
        content: '';
        position: fixed;
        inset: 0;
        background: repeating-linear-gradient(
          0deg,
          transparent,
          transparent 2px,
          rgba(0,0,0,0.03) 2px,
          rgba(0,0,0,0.03) 4px
        );
        pointer-events: none;
        z-index: 9999;
      }
    
      /* ── BOOT SCREEN ── */
      #boot {
        position: fixed;
        inset: 0;
        background: var(--bg);
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        z-index: 1000;
        transition: opacity 0.6s ease, transform 0.6s ease;
      }
      #boot.hidden { opacity: 0; pointer-events: none; transform: scale(1.02); }
    
      .boot-logo {
        font-family: var(--font-display);
        font-size: 64px;
        font-weight: 800;
        letter-spacing: -2px;
        color: var(--green);
        text-shadow: 0 0 40px var(--green), 0 0 80px rgba(63,185,80,0.3);
        animation: pulse-glow 2s ease-in-out infinite;
      }
      .boot-sub {
        font-size: 11px;
        letter-spacing: 4px;
        color: var(--text-muted);
        text-transform: uppercase;
        margin-top: 8px;
        margin-bottom: 40px;
      }
      .boot-bar-wrap {
        width: 280px;
        height: 2px;
        background: var(--border);
        border-radius: 2px;
        overflow: hidden;
      }
      .boot-bar {
        height: 100%;
        background: var(--green);
        width: 0%;
        box-shadow: 0 0 10px var(--green);
        transition: width 0.05s linear;
      }
      .boot-msg {
        margin-top: 16px;
        font-size: 11px;
        color: var(--text-dim);
        height: 16px;
        letter-spacing: 0.5px;
      }
    
      @keyframes pulse-glow {
        0%,100% { text-shadow: 0 0 30px var(--green), 0 0 60px rgba(63,185,80,0.2); }
        50% { text-shadow: 0 0 50px var(--green), 0 0 100px rgba(63,185,80,0.4), 0 0 140px rgba(63,185,80,0.15); }
      }
    
      /* ── MAIN OS SHELL ── */
      #os { display: flex; flex-direction: column; min-height: 100vh; opacity: 0; transition: opacity 0.5s ease; }
      #os.visible { opacity: 1; }
    
      /* ── TITLEBAR ── */
      .titlebar {
        height: 40px;
        background: var(--surface);
        border-bottom: 1px solid var(--border);
        display: flex;
        align-items: center;
        padding: 0 16px;
        gap: 16px;
        position: sticky;
        top: 0;
        z-index: 100;
        backdrop-filter: blur(20px);
      }
      .titlebar-dots { display: flex; gap: 6px; }
      .dot {
        width: 12px; height: 12px; border-radius: 50%;
        cursor: pointer; transition: filter 0.2s;
      }
      .dot:hover { filter: brightness(1.3); }
      .dot-red { background: #ff5f57; }
      .dot-yellow { background: #febc2e; }
      .dot-green { background: #28c840; }
    
      .titlebar-center {
        flex: 1; display: flex; align-items: center; justify-content: center;
        gap: 8px;
      }
      .titlebar-logo {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 14px;
        color: var(--green);
        letter-spacing: 1px;
      }
      .titlebar-slash { color: var(--text-dim); }
      .titlebar-path { color: var(--text-muted); font-size: 12px; }
    
      .titlebar-right {
        display: flex; gap: 12px; align-items: center;
      }
      .status-pill {
        padding: 2px 10px;
        border-radius: 20px;
        font-size: 10px;
        letter-spacing: 1px;
        text-transform: uppercase;
        font-weight: 500;
      }
      .pill-green { background: var(--green-glow); color: var(--green); border: 1px solid rgba(63,185,80,0.3); }
      .pill-blue { background: var(--blue-glow); color: var(--blue); border: 1px solid rgba(88,166,255,0.3); }
      .clock { color: var(--text-muted); font-size: 11px; }
    
      /* ── LAYOUT ── */
      .shell { display: flex; flex: 1; }
    
      /* ── SIDEBAR ── */
      .sidebar {
        width: 220px;
        background: var(--surface);
        border-right: 1px solid var(--border);
        padding: 16px 0;
        flex-shrink: 0;
        position: sticky;
        top: 40px;
        height: calc(100vh - 40px);
        overflow-y: auto;
        display: flex;
        flex-direction: column;
      }
      .sidebar::-webkit-scrollbar { width: 4px; }
      .sidebar::-webkit-scrollbar-thumb { background: var(--border-bright); border-radius: 2px; }
    
      .sidebar-section { padding: 0 8px; margin-bottom: 8px; }
      .sidebar-label {
        font-size: 9px;
        letter-spacing: 2px;
        text-transform: uppercase;
        color: var(--text-dim);
        padding: 4px 8px 8px;
        font-weight: 500;
      }
      .sidebar-item {
        display: flex; align-items: center; gap: 10px;
        padding: 7px 10px;
        border-radius: 6px;
        cursor: pointer;
        transition: background 0.15s, color 0.15s;
        color: var(--text-muted);
        font-size: 12px;
        border: 1px solid transparent;
      }
      .sidebar-item:hover { background: var(--surface2); color: var(--text); }
      .sidebar-item.active {
        background: var(--green-glow);
        color: var(--green);
        border-color: rgba(63,185,80,0.2);
      }
      .sidebar-icon { font-size: 14px; width: 18px; text-align: center; }
      .sidebar-badge {
        margin-left: auto;
        background: var(--surface3);
        color: var(--text-dim);
        font-size: 9px;
        padding: 1px 6px;
        border-radius: 10px;
      }
      .sidebar-badge.active-badge { background: rgba(63,185,80,0.2); color: var(--green); }
    
      .sidebar-bottom {
        margin-top: auto;
        padding: 8px;
        border-top: 1px solid var(--border);
        font-size: 10px;
        color: var(--text-dim);
        text-align: center;
        line-height: 1.8;
      }
      .sidebar-bottom a { color: var(--blue); text-decoration: none; }
    
      /* ── MAIN CONTENT ── */
      .main { flex: 1; padding: 32px; overflow-y: auto; max-width: 1100px; }
      .view { display: none; }
      .view.active { display: block; animation: fadeIn 0.25s ease; }
    
      @keyframes fadeIn { from { opacity: 0; transform: translateY(6px); } to { opacity: 1; transform: translateY(0); } }
    
      /* ── SECTION HEADERS ── */
      .view-header { margin-bottom: 32px; }
      .view-title {
        font-family: var(--font-display);
        font-size: 28px;
        font-weight: 800;
        color: var(--text);
        letter-spacing: -0.5px;
        margin-bottom: 6px;
      }
      .view-title span { color: var(--green); }
      .view-subtitle { color: var(--text-muted); font-size: 12px; line-height: 1.7; max-width: 600px; }
    
      /* ── CARDS ── */
      .card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 20px;
        margin-bottom: 16px;
        transition: border-color 0.2s, box-shadow 0.2s;
      }
      .card:hover { border-color: var(--border-bright); box-shadow: 0 4px 24px rgba(0,0,0,0.3); }
      .card-title {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 15px;
        color: var(--text);
        margin-bottom: 8px;
        display: flex;
        align-items: center;
        gap: 10px;
      }
      .card-body { color: var(--text-muted); font-size: 12px; line-height: 1.8; }
    
      /* ── PHASE CARDS ── */
      .phase-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 24px; }
      @media (max-width: 700px) { .phase-grid { grid-template-columns: 1fr; } }
    
      .phase-card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 20px;
        cursor: pointer;
        transition: all 0.2s;
        position: relative;
        overflow: hidden;
      }
      .phase-card::before {
        content: '';
        position: absolute;
        top: 0; left: 0; right: 0;
        height: 2px;
      }
      .phase-card.p0::before { background: var(--green); }
      .phase-card.p1::before { background: var(--blue); }
      .phase-card.p2::before { background: var(--purple); }
      .phase-card.p3::before { background: var(--orange); }
      .phase-card:hover { border-color: var(--border-bright); transform: translateY(-2px); box-shadow: 0 8px 32px rgba(0,0,0,0.4); }
    
      .phase-num {
        font-size: 10px;
        letter-spacing: 2px;
        text-transform: uppercase;
        margin-bottom: 8px;
        font-weight: 500;
      }
      .p0 .phase-num { color: var(--green); }
      .p1 .phase-num { color: var(--blue); }
      .p2 .phase-num { color: var(--purple); }
      .p3 .phase-num { color: var(--orange); }
    
      .phase-name {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 16px;
        color: var(--text);
        margin-bottom: 6px;
      }
      .phase-timeline { font-size: 11px; color: var(--text-dim); margin-bottom: 12px; }
      .phase-items { list-style: none; }
      .phase-items li {
        font-size: 11px;
        color: var(--text-muted);
        padding: 3px 0;
        display: flex;
        align-items: flex-start;
        gap: 8px;
      }
      .phase-items li::before { content: '›'; color: var(--text-dim); flex-shrink: 0; margin-top: 1px; }
    
      .phase-status {
        display: inline-flex;
        align-items: center;
        gap: 5px;
        font-size: 9px;
        letter-spacing: 1px;
        text-transform: uppercase;
        padding: 3px 8px;
        border-radius: 20px;
        margin-bottom: 12px;
      }
      .status-active { background: var(--green-glow); color: var(--green); border: 1px solid rgba(63,185,80,0.25); }
      .status-planned { background: var(--blue-glow); color: var(--blue); border: 1px solid rgba(88,166,255,0.25); }
      .status-future { background: var(--purple-glow); color: var(--purple); border: 1px solid rgba(188,140,255,0.25); }
      .status-polish { background: var(--orange-glow); color: var(--orange); border: 1px solid rgba(227,179,65,0.25); }
      .status-dot { width: 5px; height: 5px; border-radius: 50%; background: currentColor; }
      .blink { animation: blink 1.2s step-end infinite; }
      @keyframes blink { 0%,100% { opacity: 1; } 50% { opacity: 0; } }
    
      /* ── TREE VIEW ── */
      .tree { font-size: 12px; color: var(--text-muted); line-height: 2; }
      .tree .branch {
        display: flex; align-items: flex-start; gap: 0;
        cursor: pointer;
        padding: 2px 8px;
        border-radius: 4px;
        transition: background 0.15s;
      }
      .tree .branch:hover { background: var(--surface2); color: var(--text); }
      .tree-connector { color: var(--text-dim); white-space: pre; font-family: var(--font-mono); }
      .tree-icon { margin-right: 6px; font-size: 13px; }
      .tree-name { flex: 1; }
      .tree-comment { color: var(--text-dim); font-size: 11px; }
      .tree-name.dir { color: var(--blue); font-weight: 500; }
      .tree-name.file { color: var(--text-muted); }
      .tree-name.special { color: var(--green); }
      .tree-name.config { color: var(--orange); }
      .tree-children { padding-left: 0; }
      .tree-item { transition: all 0.2s; }
      .tree-item.collapsed .tree-children { display: none; }
    
      /* ── CODE BLOCKS ── */
      .code-block {
        background: var(--surface2);
        border: 1px solid var(--border);
        border-radius: 8px;
        overflow: hidden;
        margin: 16px 0;
      }
      .code-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 10px 16px;
        border-bottom: 1px solid var(--border);
        background: var(--surface3);
      }
      .code-lang {
        font-size: 10px;
        letter-spacing: 1.5px;
        text-transform: uppercase;
        color: var(--text-dim);
        font-weight: 500;
      }
      .code-copy {
        font-size: 10px;
        color: var(--text-dim);
        cursor: pointer;
        padding: 2px 8px;
        border-radius: 4px;
        border: 1px solid var(--border);
        background: transparent;
        color: var(--text-muted);
        transition: all 0.2s;
        font-family: var(--font-mono);
      }
      .code-copy:hover { background: var(--surface); color: var(--text); border-color: var(--border-bright); }
      pre {
        padding: 16px;
        overflow-x: auto;
        font-size: 12px;
        line-height: 1.7;
        color: var(--text);
      }
      pre::-webkit-scrollbar { height: 4px; }
      pre::-webkit-scrollbar-thumb { background: var(--border-bright); border-radius: 2px; }
    
      .kw { color: #ff7b72; }
      .str { color: #a5d6ff; }
      .cm { color: var(--text-dim); font-style: italic; }
      .at { color: var(--orange); }
      .ty { color: #ffa657; }
      .fn { color: #d2a8ff; }
      .val { color: var(--green); }
      .tag { color: #7ee787; }
      .attr { color: #79c0ff; }
    
      /* ── COMPARISON ── */
      .compare-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin: 20px 0; }
      @media (max-width: 600px) { .compare-grid { grid-template-columns: 1fr; } }
    
      .compare-col {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        overflow: hidden;
      }
      .compare-header {
        padding: 12px 16px;
        font-size: 11px;
        font-weight: 600;
        letter-spacing: 1px;
        text-transform: uppercase;
        border-bottom: 1px solid var(--border);
      }
      .compare-bad .compare-header { background: rgba(248,81,73,0.08); color: var(--red); border-bottom-color: rgba(248,81,73,0.2); }
      .compare-good .compare-header { background: var(--green-glow); color: var(--green); border-bottom-color: rgba(63,185,80,0.2); }
    
      .compare-items { padding: 12px 16px; }
      .compare-item {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        padding: 5px 0;
        font-size: 11px;
        color: var(--text-muted);
        border-bottom: 1px solid var(--border);
      }
      .compare-item:last-child { border-bottom: none; }
      .ci-icon { flex-shrink: 0; margin-top: 1px; }
    
      /* ── FEATURE CARDS ── */
      .features-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 16px; margin: 20px 0; }
      .feature-card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 20px;
        transition: all 0.2s;
        cursor: pointer;
      }
      .feature-card:hover { border-color: var(--border-bright); transform: translateY(-2px); box-shadow: 0 6px 24px rgba(0,0,0,0.3); }
      .feature-icon {
        font-size: 24px;
        margin-bottom: 12px;
        display: block;
      }
      .feature-title {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 14px;
        color: var(--text);
        margin-bottom: 8px;
      }
      .feature-desc { font-size: 11px; color: var(--text-muted); line-height: 1.7; }
      .feature-tag {
        display: inline-block;
        margin-top: 10px;
        font-size: 9px;
        letter-spacing: 1px;
        text-transform: uppercase;
        padding: 2px 8px;
        border-radius: 4px;
        background: var(--surface2);
        color: var(--text-dim);
        border: 1px solid var(--border);
      }
    
      /* ── TERMINAL ── */
      .terminal {
        background: #0d1117;
        border: 1px solid var(--border);
        border-radius: 10px;
        overflow: hidden;
        margin: 20px 0;
      }
      .terminal-header {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 14px;
        background: var(--surface2);
        border-bottom: 1px solid var(--border);
      }
      .t-dot { width: 10px; height: 10px; border-radius: 50%; }
      .t-r { background: #ff5f57; }
      .t-y { background: #febc2e; }
      .t-g { background: #28c840; }
      .terminal-title { margin-left: auto; font-size: 10px; color: var(--text-dim); margin-right: auto; }
      .terminal-body { padding: 16px 20px; font-size: 12px; line-height: 2; }
      .t-prompt { color: var(--green); }
      .t-cmd { color: var(--text); }
      .t-out { color: var(--text-muted); }
      .t-success { color: var(--green); }
      .t-info { color: var(--blue); }
      .t-warn { color: var(--orange); }
      .t-cursor {
        display: inline-block;
        width: 8px; height: 14px;
        background: var(--green);
        vertical-align: middle;
        animation: blink 1s step-end infinite;
        margin-left: 2px;
      }
    
      /* ── DIAGNOSTICS PANEL ── */
      .diagnostics {
        background: rgba(0,0,0,0.6);
        border: 1px solid rgba(63,185,80,0.3);
        border-radius: 8px;
        padding: 12px 16px;
        font-size: 11px;
        backdrop-filter: blur(8px);
        margin: 20px 0;
      }
      .diag-header {
        color: var(--green);
        font-size: 9px;
        letter-spacing: 2px;
        text-transform: uppercase;
        margin-bottom: 10px;
        display: flex;
        align-items: center;
        gap: 8px;
      }
      .diag-header::after { content: ''; flex: 1; height: 1px; background: rgba(63,185,80,0.2); }
      .diag-event {
        display: flex;
        gap: 12px;
        padding: 4px 0;
        border-bottom: 1px solid rgba(255,255,255,0.04);
        align-items: center;
      }
      .diag-event:last-child { border-bottom: none; }
      .diag-time { color: var(--text-dim); width: 70px; flex-shrink: 0; }
      .diag-type {
        width: 80px;
        flex-shrink: 0;
        font-weight: 500;
      }
      .diag-type.nav { color: var(--blue); }
      .diag-type.reg { color: var(--green); }
      .diag-type.rdy { color: var(--purple); }
      .diag-type.err { color: var(--red); }
      .diag-msg { color: var(--text-muted); flex: 1; }
    
      /* ── ARCHITECTURE VIZ ── */
      .arch-diagram {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 24px;
        margin: 20px 0;
        position: relative;
      }
      .arch-row { display: flex; gap: 12px; margin-bottom: 12px; align-items: stretch; }
      .arch-box {
        flex: 1;
        background: var(--surface2);
        border: 1px solid var(--border);
        border-radius: 8px;
        padding: 14px 16px;
        text-align: center;
        transition: all 0.2s;
        cursor: pointer;
        position: relative;
      }
      .arch-box:hover { border-color: var(--border-bright); box-shadow: 0 4px 20px rgba(0,0,0,0.3); }
      .arch-box-icon { font-size: 20px; margin-bottom: 6px; display: block; }
      .arch-box-title { font-family: var(--font-display); font-weight: 700; font-size: 12px; color: var(--text); }
      .arch-box-sub { font-size: 10px; color: var(--text-dim); margin-top: 4px; }
      .arch-box.highlight-green { border-color: rgba(63,185,80,0.4); background: var(--green-glow); }
      .arch-box.highlight-blue { border-color: rgba(88,166,255,0.4); background: var(--blue-glow); }
      .arch-box.highlight-purple { border-color: rgba(188,140,255,0.4); background: var(--purple-glow); }
      .arch-connector { display: flex; justify-content: center; color: var(--text-dim); margin: -4px 0; font-size: 16px; }
      .arch-label {
        font-size: 9px;
        letter-spacing: 2px;
        text-transform: uppercase;
        color: var(--text-dim);
        margin-bottom: 8px;
        text-align: center;
      }
    
      /* ── PROGRESS BARS ── */
      .progress-item { margin-bottom: 16px; }
      .progress-label {
        display: flex;
        justify-content: space-between;
        margin-bottom: 6px;
        font-size: 12px;
        color: var(--text-muted);
      }
      .progress-pct { color: var(--text-dim); font-size: 11px; }
      .progress-bar { height: 4px; background: var(--surface2); border-radius: 4px; overflow: hidden; }
      .progress-fill {
        height: 100%;
        border-radius: 4px;
        background: var(--green);
        box-shadow: 0 0 8px var(--green);
        transition: width 1s cubic-bezier(0.4, 0, 0.2, 1);
        width: 0;
      }
      .progress-fill.blue { background: var(--blue); box-shadow: 0 0 8px var(--blue); }
      .progress-fill.purple { background: var(--purple); box-shadow: 0 0 8px var(--purple); }
      .progress-fill.orange { background: var(--orange); box-shadow: 0 0 8px var(--orange); }
    
      /* ── STATUSBAR ── */
      .statusbar {
        height: 24px;
        background: var(--green);
        display: flex;
        align-items: center;
        padding: 0 16px;
        gap: 20px;
        font-size: 11px;
        color: rgba(0,0,0,0.8);
        font-weight: 500;
        position: sticky;
        bottom: 0;
      }
      .statusbar-item { display: flex; align-items: center; gap: 5px; }
      .statusbar-sep { color: rgba(0,0,0,0.3); }
    
      /* ── DIVIDERS ── */
      .divider {
        height: 1px;
        background: var(--border);
        margin: 24px 0;
      }
      .section-label {
        font-size: 10px;
        letter-spacing: 2px;
        text-transform: uppercase;
        color: var(--text-dim);
        margin-bottom: 16px;
        display: flex;
        align-items: center;
        gap: 12px;
      }
      .section-label::after { content: ''; flex: 1; height: 1px; background: var(--border); }
    
      /* ── TOOLTIPS ── */
      [data-tip] { position: relative; }
      [data-tip]:hover::after {
        content: attr(data-tip);
        position: absolute;
        bottom: calc(100% + 8px);
        left: 50%;
        transform: translateX(-50%);
        background: var(--surface3);
        border: 1px solid var(--border-bright);
        color: var(--text);
        font-size: 10px;
        padding: 4px 8px;
        border-radius: 4px;
        white-space: nowrap;
        z-index: 200;
        pointer-events: none;
      }
    
      /* ── HIGHLIGHT BOX ── */
      .highlight-box {
        background: var(--green-glow);
        border: 1px solid rgba(63,185,80,0.25);
        border-radius: 8px;
        padding: 14px 18px;
        font-size: 12px;
        color: var(--text);
        line-height: 1.7;
        margin: 16px 0;
      }
      .highlight-box strong { color: var(--green); }
    
      .info-box {
        background: var(--blue-glow);
        border: 1px solid rgba(88,166,255,0.25);
        border-radius: 8px;
        padding: 14px 18px;
        font-size: 12px;
        color: var(--text-muted);
        line-height: 1.7;
        margin: 16px 0;
      }
      .info-box strong { color: var(--blue); }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/wwwroot/css/saos.css" readonly="true"
    
      :root {
        --bg: #080c10;
        --surface: #0d1117;
        --surface2: #161b22;
        --surface3: #1c2333;
        --border: #21262d;
        --border-bright: #30363d;
        --text: #e6edf3;
        --text-muted: #7d8590;
        --text-dim: #484f58;
        --green: #3fb950;
        --green-glow: rgba(63,185,80,0.15);
        --blue: #58a6ff;
        --blue-glow: rgba(88,166,255,0.12);
        --orange: #e3b341;
        --orange-glow: rgba(227,179,65,0.12);
        --purple: #bc8cff;
        --purple-glow: rgba(188,140,255,0.12);
        --red: #f85149;
        --accent: #3fb950;
        --font-mono: 'JetBrains Mono', monospace;
        --font-display: 'Syne', sans-serif;
      }
    
      *, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }
    
      body {
        background: var(--bg);
        color: var(--text);
        font-family: var(--font-mono);
        font-size: 13px;
        line-height: 1.6;
        min-height: 100vh;
        overflow-x: hidden;
      }
    
      /* Scanline overlay */
      body::before {
        content: '';
        position: fixed;
        inset: 0;
        background: repeating-linear-gradient(
          0deg,
          transparent,
          transparent 2px,
          rgba(0,0,0,0.03) 2px,
          rgba(0,0,0,0.03) 4px
        );
        pointer-events: none;
        z-index: 9999;
      }
    
      /* ── BOOT SCREEN ── */
      #boot {
        position: fixed;
        inset: 0;
        background: var(--bg);
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        z-index: 1000;
        transition: opacity 0.6s ease, transform 0.6s ease;
      }
      #boot.hidden { opacity: 0; pointer-events: none; transform: scale(1.02); }
    
      .boot-logo {
        font-family: var(--font-display);
        font-size: 64px;
        font-weight: 800;
        letter-spacing: -2px;
        color: var(--green);
        text-shadow: 0 0 40px var(--green), 0 0 80px rgba(63,185,80,0.3);
        animation: pulse-glow 2s ease-in-out infinite;
      }
      .boot-sub {
        font-size: 11px;
        letter-spacing: 4px;
        color: var(--text-muted);
        text-transform: uppercase;
        margin-top: 8px;
        margin-bottom: 40px;
      }
      .boot-bar-wrap {
        width: 280px;
        height: 2px;
        background: var(--border);
        border-radius: 2px;
        overflow: hidden;
      }
      .boot-bar {
        height: 100%;
        background: var(--green);
        width: 0%;
        box-shadow: 0 0 10px var(--green);
        transition: width 0.05s linear;
      }
      .boot-msg {
        margin-top: 16px;
        font-size: 11px;
        color: var(--text-dim);
        height: 16px;
        letter-spacing: 0.5px;
      }
    
      @keyframes pulse-glow {
        0%,100% { text-shadow: 0 0 30px var(--green), 0 0 60px rgba(63,185,80,0.2); }
        50% { text-shadow: 0 0 50px var(--green), 0 0 100px rgba(63,185,80,0.4), 0 0 140px rgba(63,185,80,0.15); }
      }
    
      /* ── MAIN OS SHELL ── */
      #os { display: flex; flex-direction: column; min-height: 100vh; opacity: 0; transition: opacity 0.5s ease; }
      #os.visible { opacity: 1; }
    
      /* ── TITLEBAR ── */
      .titlebar {
        height: 40px;
        background: var(--surface);
        border-bottom: 1px solid var(--border);
        display: flex;
        align-items: center;
        padding: 0 16px;
        gap: 16px;
        position: sticky;
        top: 0;
        z-index: 100;
        backdrop-filter: blur(20px);
      }
      .titlebar-dots { display: flex; gap: 6px; }
      .dot {
        width: 12px; height: 12px; border-radius: 50%;
        cursor: pointer; transition: filter 0.2s;
      }
      .dot:hover { filter: brightness(1.3); }
      .dot-red { background: #ff5f57; }
      .dot-yellow { background: #febc2e; }
      .dot-green { background: #28c840; }
    
      .titlebar-center {
        flex: 1; display: flex; align-items: center; justify-content: center;
        gap: 8px;
      }
      .titlebar-logo {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 14px;
        color: var(--green);
        letter-spacing: 1px;
      }
      .titlebar-slash { color: var(--text-dim); }
      .titlebar-path { color: var(--text-muted); font-size: 12px; }
    
      .titlebar-right {
        display: flex; gap: 12px; align-items: center;
      }
      .status-pill {
        padding: 2px 10px;
        border-radius: 20px;
        font-size: 10px;
        letter-spacing: 1px;
        text-transform: uppercase;
        font-weight: 500;
      }
      .pill-green { background: var(--green-glow); color: var(--green); border: 1px solid rgba(63,185,80,0.3); }
      .pill-blue { background: var(--blue-glow); color: var(--blue); border: 1px solid rgba(88,166,255,0.3); }
      .clock { color: var(--text-muted); font-size: 11px; }
    
      /* ── LAYOUT ── */
      .shell { display: flex; flex: 1; }
    
      /* ── SIDEBAR ── */
      .sidebar {
        width: 220px;
        background: var(--surface);
        border-right: 1px solid var(--border);
        padding: 16px 0;
        flex-shrink: 0;
        position: sticky;
        top: 40px;
        height: calc(100vh - 40px);
        overflow-y: auto;
        display: flex;
        flex-direction: column;
      }
      .sidebar::-webkit-scrollbar { width: 4px; }
      .sidebar::-webkit-scrollbar-thumb { background: var(--border-bright); border-radius: 2px; }
    
      .sidebar-section { padding: 0 8px; margin-bottom: 8px; }
      .sidebar-label {
        font-size: 9px;
        letter-spacing: 2px;
        text-transform: uppercase;
        color: var(--text-dim);
        padding: 4px 8px 8px;
        font-weight: 500;
      }
      .sidebar-item {
        display: flex; align-items: center; gap: 10px;
        padding: 7px 10px;
        border-radius: 6px;
        cursor: pointer;
        transition: background 0.15s, color 0.15s;
        color: var(--text-muted);
        font-size: 12px;
        border: 1px solid transparent;
      }
      .sidebar-item:hover { background: var(--surface2); color: var(--text); }
      .sidebar-item.active {
        background: var(--green-glow);
        color: var(--green);
        border-color: rgba(63,185,80,0.2);
      }
      .sidebar-icon { font-size: 14px; width: 18px; text-align: center; }
      .sidebar-badge {
        margin-left: auto;
        background: var(--surface3);
        color: var(--text-dim);
        font-size: 9px;
        padding: 1px 6px;
        border-radius: 10px;
      }
      .sidebar-badge.active-badge { background: rgba(63,185,80,0.2); color: var(--green); }
    
      .sidebar-bottom {
        margin-top: auto;
        padding: 8px;
        border-top: 1px solid var(--border);
        font-size: 10px;
        color: var(--text-dim);
        text-align: center;
        line-height: 1.8;
      }
      .sidebar-bottom a { color: var(--blue); text-decoration: none; }
    
      /* ── MAIN CONTENT ── */
      .main { flex: 1; padding: 32px; overflow-y: auto; max-width: 1100px; }
      .view { display: none; }
      .view.active { display: block; animation: fadeIn 0.25s ease; }
    
      @keyframes fadeIn { from { opacity: 0; transform: translateY(6px); } to { opacity: 1; transform: translateY(0); } }
    
      /* ── SECTION HEADERS ── */
      .view-header { margin-bottom: 32px; }
      .view-title {
        font-family: var(--font-display);
        font-size: 28px;
        font-weight: 800;
        color: var(--text);
        letter-spacing: -0.5px;
        margin-bottom: 6px;
      }
      .view-title span { color: var(--green); }
      .view-subtitle { color: var(--text-muted); font-size: 12px; line-height: 1.7; max-width: 600px; }
    
      /* ── CARDS ── */
      .card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 20px;
        margin-bottom: 16px;
        transition: border-color 0.2s, box-shadow 0.2s;
      }
      .card:hover { border-color: var(--border-bright); box-shadow: 0 4px 24px rgba(0,0,0,0.3); }
      .card-title {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 15px;
        color: var(--text);
        margin-bottom: 8px;
        display: flex;
        align-items: center;
        gap: 10px;
      }
      .card-body { color: var(--text-muted); font-size: 12px; line-height: 1.8; }
    
      /* ── PHASE CARDS ── */
      .phase-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 24px; }
      @media (max-width: 700px) { .phase-grid { grid-template-columns: 1fr; } }
    
      .phase-card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 20px;
        cursor: pointer;
        transition: all 0.2s;
        position: relative;
        overflow: hidden;
      }
      .phase-card::before {
        content: '';
        position: absolute;
        top: 0; left: 0; right: 0;
        height: 2px;
      }
      .phase-card.p0::before { background: var(--green); }
      .phase-card.p1::before { background: var(--blue); }
      .phase-card.p2::before { background: var(--purple); }
      .phase-card.p3::before { background: var(--orange); }
      .phase-card:hover { border-color: var(--border-bright); transform: translateY(-2px); box-shadow: 0 8px 32px rgba(0,0,0,0.4); }
    
      .phase-num {
        font-size: 10px;
        letter-spacing: 2px;
        text-transform: uppercase;
        margin-bottom: 8px;
        font-weight: 500;
      }
      .p0 .phase-num { color: var(--green); }
      .p1 .phase-num { color: var(--blue); }
      .p2 .phase-num { color: var(--purple); }
      .p3 .phase-num { color: var(--orange); }
    
      .phase-name {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 16px;
        color: var(--text);
        margin-bottom: 6px;
      }
      .phase-timeline { font-size: 11px; color: var(--text-dim); margin-bottom: 12px; }
      .phase-items { list-style: none; }
      .phase-items li {
        font-size: 11px;
        color: var(--text-muted);
        padding: 3px 0;
        display: flex;
        align-items: flex-start;
        gap: 8px;
      }
      .phase-items li::before { content: '›'; color: var(--text-dim); flex-shrink: 0; margin-top: 1px; }
    
      .phase-status {
        display: inline-flex;
        align-items: center;
        gap: 5px;
        font-size: 9px;
        letter-spacing: 1px;
        text-transform: uppercase;
        padding: 3px 8px;
        border-radius: 20px;
        margin-bottom: 12px;
      }
      .status-active { background: var(--green-glow); color: var(--green); border: 1px solid rgba(63,185,80,0.25); }
      .status-planned { background: var(--blue-glow); color: var(--blue); border: 1px solid rgba(88,166,255,0.25); }
      .status-future { background: var(--purple-glow); color: var(--purple); border: 1px solid rgba(188,140,255,0.25); }
      .status-polish { background: var(--orange-glow); color: var(--orange); border: 1px solid rgba(227,179,65,0.25); }
      .status-dot { width: 5px; height: 5px; border-radius: 50%; background: currentColor; }
      .blink { animation: blink 1.2s step-end infinite; }
      @keyframes blink { 0%,100% { opacity: 1; } 50% { opacity: 0; } }
    
      /* ── TREE VIEW ── */
      .tree { font-size: 12px; color: var(--text-muted); line-height: 2; }
      .tree .branch {
        display: flex; align-items: flex-start; gap: 0;
        cursor: pointer;
        padding: 2px 8px;
        border-radius: 4px;
        transition: background 0.15s;
      }
      .tree .branch:hover { background: var(--surface2); color: var(--text); }
      .tree-connector { color: var(--text-dim); white-space: pre; font-family: var(--font-mono); }
      .tree-icon { margin-right: 6px; font-size: 13px; }
      .tree-name { flex: 1; }
      .tree-comment { color: var(--text-dim); font-size: 11px; }
      .tree-name.dir { color: var(--blue); font-weight: 500; }
      .tree-name.file { color: var(--text-muted); }
      .tree-name.special { color: var(--green); }
      .tree-name.config { color: var(--orange); }
      .tree-children { padding-left: 0; }
      .tree-item { transition: all 0.2s; }
      .tree-item.collapsed .tree-children { display: none; }
    
      /* ── CODE BLOCKS ── */
      .code-block {
        background: var(--surface2);
        border: 1px solid var(--border);
        border-radius: 8px;
        overflow: hidden;
        margin: 16px 0;
      }
      .code-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 10px 16px;
        border-bottom: 1px solid var(--border);
        background: var(--surface3);
      }
      .code-lang {
        font-size: 10px;
        letter-spacing: 1.5px;
        text-transform: uppercase;
        color: var(--text-dim);
        font-weight: 500;
      }
      .code-copy {
        font-size: 10px;
        color: var(--text-dim);
        cursor: pointer;
        padding: 2px 8px;
        border-radius: 4px;
        border: 1px solid var(--border);
        background: transparent;
        color: var(--text-muted);
        transition: all 0.2s;
        font-family: var(--font-mono);
      }
      .code-copy:hover { background: var(--surface); color: var(--text); border-color: var(--border-bright); }
      pre {
        padding: 16px;
        overflow-x: auto;
        font-size: 12px;
        line-height: 1.7;
        color: var(--text);
      }
      pre::-webkit-scrollbar { height: 4px; }
      pre::-webkit-scrollbar-thumb { background: var(--border-bright); border-radius: 2px; }
    
      .kw { color: #ff7b72; }
      .str { color: #a5d6ff; }
      .cm { color: var(--text-dim); font-style: italic; }
      .at { color: var(--orange); }
      .ty { color: #ffa657; }
      .fn { color: #d2a8ff; }
      .val { color: var(--green); }
      .tag { color: #7ee787; }
      .attr { color: #79c0ff; }
    
      /* ── COMPARISON ── */
      .compare-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin: 20px 0; }
      @media (max-width: 600px) { .compare-grid { grid-template-columns: 1fr; } }
    
      .compare-col {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        overflow: hidden;
      }
      .compare-header {
        padding: 12px 16px;
        font-size: 11px;
        font-weight: 600;
        letter-spacing: 1px;
        text-transform: uppercase;
        border-bottom: 1px solid var(--border);
      }
      .compare-bad .compare-header { background: rgba(248,81,73,0.08); color: var(--red); border-bottom-color: rgba(248,81,73,0.2); }
      .compare-good .compare-header { background: var(--green-glow); color: var(--green); border-bottom-color: rgba(63,185,80,0.2); }
    
      .compare-items { padding: 12px 16px; }
      .compare-item {
        display: flex;
        align-items: flex-start;
        gap: 8px;
        padding: 5px 0;
        font-size: 11px;
        color: var(--text-muted);
        border-bottom: 1px solid var(--border);
      }
      .compare-item:last-child { border-bottom: none; }
      .ci-icon { flex-shrink: 0; margin-top: 1px; }
    
      /* ── FEATURE CARDS ── */
      .features-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 16px; margin: 20px 0; }
      .feature-card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 20px;
        transition: all 0.2s;
        cursor: pointer;
      }
      .feature-card:hover { border-color: var(--border-bright); transform: translateY(-2px); box-shadow: 0 6px 24px rgba(0,0,0,0.3); }
      .feature-icon {
        font-size: 24px;
        margin-bottom: 12px;
        display: block;
      }
      .feature-title {
        font-family: var(--font-display);
        font-weight: 700;
        font-size: 14px;
        color: var(--text);
        margin-bottom: 8px;
      }
      .feature-desc { font-size: 11px; color: var(--text-muted); line-height: 1.7; }
      .feature-tag {
        display: inline-block;
        margin-top: 10px;
        font-size: 9px;
        letter-spacing: 1px;
        text-transform: uppercase;
        padding: 2px 8px;
        border-radius: 4px;
        background: var(--surface2);
        color: var(--text-dim);
        border: 1px solid var(--border);
      }
    
      /* ── TERMINAL ── */
      .terminal {
        background: #0d1117;
        border: 1px solid var(--border);
        border-radius: 10px;
        overflow: hidden;
        margin: 20px 0;
      }
      .terminal-header {
        display: flex;
        align-items: center;
        gap: 8px;
        padding: 10px 14px;
        background: var(--surface2);
        border-bottom: 1px solid var(--border);
      }
      .t-dot { width: 10px; height: 10px; border-radius: 50%; }
      .t-r { background: #ff5f57; }
      .t-y { background: #febc2e; }
      .t-g { background: #28c840; }
      .terminal-title { margin-left: auto; font-size: 10px; color: var(--text-dim); margin-right: auto; }
      .terminal-body { padding: 16px 20px; font-size: 12px; line-height: 2; }
      .t-prompt { color: var(--green); }
      .t-cmd { color: var(--text); }
      .t-out { color: var(--text-muted); }
      .t-success { color: var(--green); }
      .t-info { color: var(--blue); }
      .t-warn { color: var(--orange); }
      .t-cursor {
        display: inline-block;
        width: 8px; height: 14px;
        background: var(--green);
        vertical-align: middle;
        animation: blink 1s step-end infinite;
        margin-left: 2px;
      }
    
      /* ── DIAGNOSTICS PANEL ── */
      .diagnostics {
        background: rgba(0,0,0,0.6);
        border: 1px solid rgba(63,185,80,0.3);
        border-radius: 8px;
        padding: 12px 16px;
        font-size: 11px;
        backdrop-filter: blur(8px);
        margin: 20px 0;
      }
      .diag-header {
        color: var(--green);
        font-size: 9px;
        letter-spacing: 2px;
        text-transform: uppercase;
        margin-bottom: 10px;
        display: flex;
        align-items: center;
        gap: 8px;
      }
      .diag-header::after { content: ''; flex: 1; height: 1px; background: rgba(63,185,80,0.2); }
      .diag-event {
        display: flex;
        gap: 12px;
        padding: 4px 0;
        border-bottom: 1px solid rgba(255,255,255,0.04);
        align-items: center;
      }
      .diag-event:last-child { border-bottom: none; }
      .diag-time { color: var(--text-dim); width: 70px; flex-shrink: 0; }
      .diag-type {
        width: 80px;
        flex-shrink: 0;
        font-weight: 500;
      }
      .diag-type.nav { color: var(--blue); }
      .diag-type.reg { color: var(--green); }
      .diag-type.rdy { color: var(--purple); }
      .diag-type.err { color: var(--red); }
      .diag-msg { color: var(--text-muted); flex: 1; }
    
      /* ── ARCHITECTURE VIZ ── */
      .arch-diagram {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: 10px;
        padding: 24px;
        margin: 20px 0;
        position: relative;
      }
      .arch-row { display: flex; gap: 12px; margin-bottom: 12px; align-items: stretch; }
      .arch-box {
        flex: 1;
        background: var(--surface2);
        border: 1px solid var(--border);
        border-radius: 8px;
        padding: 14px 16px;
        text-align: center;
        transition: all 0.2s;
        cursor: pointer;
        position: relative;
      }
      .arch-box:hover { border-color: var(--border-bright); box-shadow: 0 4px 20px rgba(0,0,0,0.3); }
      .arch-box-icon { font-size: 20px; margin-bottom: 6px; display: block; }
      .arch-box-title { font-family: var(--font-display); font-weight: 700; font-size: 12px; color: var(--text); }
      .arch-box-sub { font-size: 10px; color: var(--text-dim); margin-top: 4px; }
      .arch-box.highlight-green { border-color: rgba(63,185,80,0.4); background: var(--green-glow); }
      .arch-box.highlight-blue { border-color: rgba(88,166,255,0.4); background: var(--blue-glow); }
      .arch-box.highlight-purple { border-color: rgba(188,140,255,0.4); background: var(--purple-glow); }
      .arch-connector { display: flex; justify-content: center; color: var(--text-dim); margin: -4px 0; font-size: 16px; }
      .arch-label {
        font-size: 9px;
        letter-spacing: 2px;
        text-transform: uppercase;
        color: var(--text-dim);
        margin-bottom: 8px;
        text-align: center;
      }
    
      /* ── PROGRESS BARS ── */
      .progress-item { margin-bottom: 16px; }
      .progress-label {
        display: flex;
        justify-content: space-between;
        margin-bottom: 6px;
        font-size: 12px;
        color: var(--text-muted);
      }
      .progress-pct { color: var(--text-dim); font-size: 11px; }
      .progress-bar { height: 4px; background: var(--surface2); border-radius: 4px; overflow: hidden; }
      .progress-fill {
        height: 100%;
        border-radius: 4px;
        background: var(--green);
        box-shadow: 0 0 8px var(--green);
        transition: width 1s cubic-bezier(0.4, 0, 0.2, 1);
        width: 0;
      }
      .progress-fill.blue { background: var(--blue); box-shadow: 0 0 8px var(--blue); }
      .progress-fill.purple { background: var(--purple); box-shadow: 0 0 8px var(--purple); }
      .progress-fill.orange { background: var(--orange); box-shadow: 0 0 8px var(--orange); }
    
      /* ── STATUSBAR ── */
      .statusbar {
        height: 24px;
        background: var(--green);
        display: flex;
        align-items: center;
        padding: 0 16px;
        gap: 20px;
        font-size: 11px;
        color: rgba(0,0,0,0.8);
        font-weight: 500;
        position: sticky;
        bottom: 0;
      }
      .statusbar-item { display: flex; align-items: center; gap: 5px; }
      .statusbar-sep { color: rgba(0,0,0,0.3); }
    
      /* ── DIVIDERS ── */
      .divider {
        height: 1px;
        background: var(--border);
        margin: 24px 0;
      }
      .section-label {
        font-size: 10px;
        letter-spacing: 2px;
        text-transform: uppercase;
        color: var(--text-dim);
        margin-bottom: 16px;
        display: flex;
        align-items: center;
        gap: 12px;
      }
      .section-label::after { content: ''; flex: 1; height: 1px; background: var(--border); }
    
      /* ── TOOLTIPS ── */
      [data-tip] { position: relative; }
      [data-tip]:hover::after {
        content: attr(data-tip);
        position: absolute;
        bottom: calc(100% + 8px);
        left: 50%;
        transform: translateX(-50%);
        background: var(--surface3);
        border: 1px solid var(--border-bright);
        color: var(--text);
        font-size: 10px;
        padding: 4px 8px;
        border-radius: 4px;
        white-space: nowrap;
        z-index: 200;
        pointer-events: none;
      }
    
      /* ── HIGHLIGHT BOX ── */
      .highlight-box {
        background: var(--green-glow);
        border: 1px solid rgba(63,185,80,0.25);
        border-radius: 8px;
        padding: 14px 18px;
        font-size: 12px;
        color: var(--text);
        line-height: 1.7;
        margin: 16px 0;
      }
      .highlight-box strong { color: var(--green); }
    
      .info-box {
        background: var(--blue-glow);
        border: 1px solid rgba(88,166,255,0.25);
        border-radius: 8px;
        padding: 14px 18px;
        font-size: 12px;
        color: var(--text-muted);
        line-height: 1.7;
        margin: 16px 0;
      }
      .info-box strong { color: var(--blue); }
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/wwwroot/favicon.ico" readonly="true"
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/wwwroot/index.html" readonly="true"
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="utf-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>SAOS — Static Application Operating System</title>
      <base href="/" />
      <link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@300;400;500;700&family=Syne:wght@400;600;700;800&display=swap" rel="stylesheet">
      <link href="css/app.css" rel="stylesheet" />
      <link href="SaosHub.Blazor.styles.css" rel="stylesheet" />
    </head>
    <body>
      <div id="app">Loading kernel...</div>
      <script src="js/saos.js"></script>
      <script src="_framework/blazor.webassembly.js"></script>
    </body>
    </html>
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/wwwroot/js/saos-interop.js" readonly="true"
    
    // ── BOOT SEQUENCE ──
    const bootMessages = [
      'Initializing kernel...',
      'Loading IPC bus...',
      'Reading apps.json registry...',
      'Mounting automation plane...',
      'Verifying kernel invariants...',
      'Blazor WASM runtime ready.',
      'Starting SAOS shell...'
    ];
    
    let bootProgress = 0;
    let msgIdx = 0;
    const bar = document.getElementById('bootBar');
    const msg = document.getElementById('bootMsg');
    
    function bootStep() {
      if (bootProgress >= 100) {
        setTimeout(launchOS, 300);
        return;
      }
      const step = Math.min(Math.random() * 18 + 4, 100 - bootProgress);
      bootProgress += step;
      bar.style.width = Math.min(bootProgress, 100) + '%';
      if (msgIdx < bootMessages.length && bootProgress > (msgIdx + 1) * (100 / bootMessages.length)) {
        msg.textContent = bootMessages[msgIdx++];
      }
      setTimeout(bootStep, 80 + Math.random() * 120);
    }
    bootStep();
    
    function launchOS() {
      document.getElementById('boot').classList.add('hidden');
      const os = document.getElementById('os');
      os.classList.add('visible');
      setTimeout(() => {
        animateProgressBars();
        startDiagnostics();
      }, 400);
    }
    
    // ── CLOCK ──
    function updateClock() {
      const now = new Date();
      document.getElementById('clock').textContent =
        now.toLocaleTimeString('en-US', { hour12: false });
    }
    setInterval(updateClock, 1000);
    updateClock();
    
    // ── VIEW SWITCHING ──
    function switchView(id, el) {
      document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
      document.querySelectorAll('.sidebar-item').forEach(s => s.classList.remove('active'));
    
      const view = document.getElementById('view-' + id);
      if (view) view.classList.add('active');
    
      const sidebarEl = el || document.querySelector('[data-view=' + id + ']');
      if (sidebarEl) sidebarEl.classList.add('active');
    
      document.getElementById('titlePath').textContent = id;
      document.getElementById('sbView').textContent = id;
    
      // Animate progress bars if switching to progress view
      if (id === 'progress') setTimeout(animateProgressBars, 100);
    }
    
    // ── REPO TREE ──
    const treeData = [
      { name: 'ulfbou.github.io/', cls: 'dir', icon: '📁', children: [
        { name: 'kernel/', cls: 'dir', icon: '📁', comment: '← never edited by hand', children: [
          { name: 'src/', cls: 'dir', icon: '📁', children: [
            { name: 'ipc/', cls: 'dir', icon: '📁', children: [
              { name: 'bus.ts', cls: 'file', icon: '📄', comment: 'IPC event bus' },
              { name: 'types.ts', cls: 'config', icon: '📄', comment: 'SaosEvent, AppManifest' },
            ]},
            { name: 'router/', cls: 'dir', icon: '📁', children: [
              { name: 'kernelRouter.ts', cls: 'file', icon: '📄' },
            ]},
            { name: 'shell/', cls: 'dir', icon: '📁', children: [
              { name: 'index.html', cls: 'special', icon: '🌐', comment: 'OS entry point' },
              { name: 'kernel.ts', cls: 'file', icon: '📄' },
            ]},
          ]},
          { name: 'apps.json', cls: 'special', icon: '📋', comment: '← auto-generated only' },
        ]},
        { name: 'automation/', cls: 'dir', icon: '📁', comment: 'Automation Plane', children: [
          { name: '.github/workflows/', cls: 'dir', icon: '📁', children: [
            { name: 'on-app-register.yml', cls: 'config', icon: '⚙️', comment: 'saos.app.register handler' },
            { name: 'deploy-kernel.yml', cls: 'config', icon: '⚙️' },
          ]},
          { name: 'scripts/', cls: 'dir', icon: '📁', children: [
            { name: 'update-registry.ts', cls: 'file', icon: '📄', comment: 'writes apps.json' },
          ]},
        ]},
        { name: 'packages/', cls: 'dir', icon: '📁', comment: 'NuGet / npm', children: [
          { name: 'Saos.Interop/', cls: 'dir', icon: '📁', comment: 'NuGet → all Blazor apps', children: [
            { name: 'SaosKernelExtensions.cs', cls: 'file', icon: '📄', comment: 'AddSaosKernel()' },
            { name: 'SaosRouterAdapter.cs', cls: 'file', icon: '📄' },
            { name: 'SaosDiagnostics.razor', cls: 'file', icon: '📄' },
            { name: 'ISaosContext.cs', cls: 'special', icon: '📄' },
          ]},
          { name: 'Saos.Templates/', cls: 'dir', icon: '📁', comment: 'dotnet new saos-blazor', children: [
            { name: 'template.json', cls: 'config', icon: '⚙️' },
            { name: 'MyApp.csproj', cls: 'config', icon: '⚙️', comment: 'MSBuild target embedded' },
            { name: 'Program.cs', cls: 'file', icon: '📄' },
          ]},
        ]},
      ]}
    ];
    
    function renderTree(nodes, prefix = '', isLast = true) {
      let html = '';
      nodes.forEach((node, i) => {
        const last = i === nodes.length - 1;
        const connector = last ? '└── ' : '├── ';
        const childPrefix = prefix + (last ? '    ' : '│   ');
        const hasChildren = node.children && node.children.length > 0;
        html += `<div class="tree-item ${hasChildren ? '' : ''}" id="tnode-${Math.random().toString(36).slice(2)}">`;
        html += `<div class="branch" onclick="toggleNode(this)">`;
        html += `<span class="tree-connector">${prefix}${connector}</span>`;
        html += `<span class="tree-icon">${node.icon}</span>`;
        html += `<span class="tree-name ${node.cls}">${node.name}</span>`;
        if (node.comment) html += `<span class="tree-comment">  ${node.comment}</span>`;
        html += `</div>`;
        if (hasChildren) {
          html += `<div class="tree-children">${renderTree(node.children, childPrefix, last)}</div>`;
        }
        html += `</div>`;
      });
      return html;
    }
    
    function toggleNode(el) {
      const item = el.parentElement;
      item.classList.toggle('collapsed');
    }
    
    document.getElementById('repoTree').innerHTML = renderTree(treeData);
    
    // Template tree
    const templateData = [
      { name: 'saos-blazor/', cls: 'dir', icon: '📁', children: [
        { name: '.template.config/', cls: 'dir', icon: '📁', children: [
          { name: 'template.json', cls: 'config', icon: '⚙️', comment: 'dotnet template metadata' },
        ]},
        { name: 'MyApp.csproj', cls: 'config', icon: '⚙️', comment: 'MSBuild target + Saos.Interop ref' },
        { name: 'Program.cs', cls: 'special', icon: '📄', comment: 'AddSaosKernel pre-wired' },
        { name: 'App.razor', cls: 'file', icon: '📄' },
        { name: 'Pages/', cls: 'dir', icon: '📁', children: [
          { name: 'Index.razor', cls: 'file', icon: '📄', comment: 'OnKernelReady sample' },
        ]},
        { name: 'wwwroot/', cls: 'dir', icon: '📁', children: [
          { name: 'index.html', cls: 'file', icon: '🌐' },
          { name: 'icon.png', cls: 'file', icon: '🖼️' },
        ]},
      ]}
    ];
    document.getElementById('templateTree').innerHTML = renderTree(templateData);
    
    // ── PROGRESS BARS ──
    function animateProgressBars() {
      document.querySelectorAll('.progress-fill').forEach(el => {
        const w = el.getAttribute('data-width') || '0';
        el.style.width = w + '%';
      });
    }
    
    // ── COPY CODE ──
    function copyCode(btn) {
      const pre = btn.closest('.code-block').querySelector('pre');
      const text = pre.innerText;
      navigator.clipboard.writeText(text).then(() => {
        btn.textContent = 'copied!';
        setTimeout(() => btn.textContent = 'copy', 1500);
      });
    }
    
    // ── DIAGNOSTICS SIMULATION ──
    const diagTemplates = [
      { type: 'reg', cls: 'reg', msgs: ['MyLab v1.3.0 registered', 'Portfolio v2.1.0 registered', 'Sandbox v0.4.2 registered'] },
      { type: 'navigate', cls: 'nav', msgs: ['→ /mylab/components', '→ /portfolio/projects', '→ /', '→ /sandbox/test'] },
      { type: 'ready', cls: 'rdy', msgs: ['kernel handshake OK', 'IPC bus connected', 'SaosContext injected'] },
      { type: 'error', cls: 'err', msgs: ['app-info.json not found', 'IPC timeout 5000ms'] },
    ];
    
    let diagEvents = [];
    
    function addDiagEvent() {
      const tpl = diagTemplates[Math.floor(Math.random() * (Math.random() > 0.15 ? 3 : 4))];
      const msg = tpl.msgs[Math.floor(Math.random() * tpl.msgs.length)];
      const now = new Date();
      const time = now.toLocaleTimeString('en-US', { hour12: false, hour: '2-digit', minute: '2-digit', second: '2-digit' });
      diagEvents.unshift({ type: tpl.type, cls: tpl.cls, msg, time });
      if (diagEvents.length > 5) diagEvents.pop();
      renderDiag();
    }
    
    function renderDiag() {
      const container = document.getElementById('diagEvents');
      if (!container) return;
      container.innerHTML = diagEvents.map(e => `
        <div class="diag-event">
          <span class="diag-time">${e.time}</span>
          <span class="diag-type ${e.cls}">${e.type}</span>
          <span class="diag-msg">${e.msg}</span>
        </div>
      `).join('');
    }
    
    function startDiagnostics() {
      // Seed with some events
      for (let i = 0; i < 5; i++) {
        const tpl = diagTemplates[i % 3];
        const msg = tpl.msgs[i % tpl.msgs.length];
        const ago = new Date(Date.now() - (5 - i) * 8000);
        const time = ago.toLocaleTimeString('en-US', { hour12: false, hour: '2-digit', minute: '2-digit', second: '2-digit' });
        diagEvents.push({ type: tpl.type, cls: tpl.cls, msg, time });
      }
      renderDiag();
      setInterval(addDiagEvent, 3200);
    }
    
    
    // Blazor interop for boot sequence
    window.saosBoot = {
      start: function(dotnetRef) {
        let p = 0;
        const msgs = ["Initializing kernel...","Loading apps.json...","Connecting IPC bus...","Mounting shell...","Ready"];
        const interval = setInterval(() => {
          p += 20;
          const msg = msgs[Math.min(Math.floor(p/20), msgs.length-1)];
          dotnetRef.invokeMethodAsync('UpdateBoot', p, msg);
          const bar = document.getElementById('bootBar');
          if(bar) bar.style.width = p + '%';
          const msgEl = document.getElementById('bootMsg');
          if(msgEl) msgEl.textContent = msg;
          if (p >= 100) {
            clearInterval(interval);
            setTimeout(() => dotnetRef.invokeMethodAsync('BootComplete'), 300);
          }
        }, 180);
      }
    };
    
    // View switching (will be overridden by Blazor)
    window.saosNavigate = function(viewId) {
      document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
      document.querySelectorAll('.sidebar-item').forEach(i => i.classList.remove('active'));
      const view = document.getElementById('view-' + viewId);
      if(view) view.classList.add('active');
      const item = document.querySelector(`[data-view="${viewId}"]`);
      if(item) item.classList.add('active');
    };
    
%%ENDBLOCK

%%FILE path="ulfbou.github.io/src/SaosHub.Blazor/wwwroot/js/saos.js" readonly="true"
    // SAOS – Blazor interop only (original boot code removed)
    window.saosInterop = {
      navigate: function(viewId) {
        document.querySelectorAll('.section').forEach(v => v.classList.remove('active'));
        document.querySelectorAll('.sidebar-item').forEach(i => i.classList.remove('active'));
        const view = document.getElementById(viewId);
        if (view) view.classList.add('active');
        const item = document.querySelector(`[data-target="${viewId}"]`);
        if (item) item.classList.add('active');
        if (viewId === 'phases') {
          document.querySelectorAll('.progress-fill').forEach(el => {
            const w = el.getAttribute('data-width') || '0';
            el.style.width = w + '%';
          });
        }
        // render trees on first view
        if (viewId === 'repo' &&!document.getElementById('repoTree').innerHTML) {
          document.getElementById('repoTree').innerHTML = renderTree(treeData);
        }
        if (viewId === 'template' &&!document.getElementById('templateTree').innerHTML) {
          document.getElementById('templateTree').innerHTML = renderTree(templateData);
        }
      }
    };
    
    // --- Tree data from your original roadmap ---
    const treeData = [{"name":"kernel/","cls":"dir","icon":"📁","children":[{"name":"kernel.js","cls":"file","icon":"📄"},{"name":"apps.json","cls":"special","icon":"📋"}]}];
    const templateData = [{"name":"saos-blazor/","cls":"dir","icon":"📁","children":[{"name":"Program.cs","cls":"special","icon":"📄"}]}];
    
    function renderTree(nodes, prefix='') {
      let html='';
      nodes.forEach((node,i)=>{
        const last=i===nodes.length-1;
        const connector=last?'└── ':'├── ';
        const childPrefix=prefix+(last?' ':'│ ');
        html+=`<div class="tree-item"><div class="branch" onclick="this.parentElement.classList.toggle('collapsed')">`;
        html+=`<span class="tree-connector">${prefix}${connector}</span>`;
        html+=`<span class="tree-icon">${node.icon}</span>`;
        html+=`<span class="tree-name ${node.cls}">${node.name}</span>`;
        if(node.comment) html+=`<span class="tree-comment"> ${node.comment}</span>`;
        html+=`</div>`;
        if(node.children) html+=`<div class="tree-children">${renderTree(node.children,childPrefix)}</div>`;
        html+=`</div>`;
      });
      return html;
    }
    
%%ENDBLOCK

%%END
