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
