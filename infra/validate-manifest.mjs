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
