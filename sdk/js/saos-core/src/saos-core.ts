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
