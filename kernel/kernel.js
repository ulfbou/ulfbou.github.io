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
