#!/usr/bin/env node
/**
 * infra/update-registry.mjs
 *
 * Orchestrates validate → link in a single atomic pass.
 * Intended to be called from GitHub Actions; outputs structured log lines.
 *
 * Usage:
 *   node infra/update-registry.mjs <path-to-app-info.json>
 *
 * Exit 0 = registry updated successfully
 * Exit 1 = validation or write failure (error written to stderr)
 */

import { execFileSync } from "node:child_process";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = dirname(fileURLToPath(import.meta.url));

const [, , appInfoPath] = process.argv;
if (!appInfoPath) {
  console.error("Usage: update-registry.mjs <path-to-app-info.json>");
  process.exit(1);
}

const resolvedPath = resolve(appInfoPath);

// ── Step 1: Validate ─────────────────────────────────────────────────────────

console.log("::group::Validate manifest");
try {
  execFileSync(
    process.execPath,
    [resolve(__dirname, "validate-manifest.mjs"), resolvedPath],
    { stdio: "inherit" }
  );
} catch {
  console.error("::endgroup::");
  console.error("::error::Manifest validation failed — registry not updated.");
  process.exit(1);
}
console.log("::endgroup::");

// ── Step 2: Link ─────────────────────────────────────────────────────────────

console.log("::group::Link into registry");
try {
  execFileSync(
    process.execPath,
    [resolve(__dirname, "linker.mjs"), resolvedPath],
    { stdio: "inherit" }
  );
} catch {
  console.error("::endgroup::");
  console.error("::error::Registry link failed.");
  process.exit(1);
}
console.log("::endgroup::");

process.exit(0);
