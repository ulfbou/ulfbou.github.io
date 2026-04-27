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
