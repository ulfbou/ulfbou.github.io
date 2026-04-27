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
