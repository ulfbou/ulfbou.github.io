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
