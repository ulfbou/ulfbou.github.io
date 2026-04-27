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
