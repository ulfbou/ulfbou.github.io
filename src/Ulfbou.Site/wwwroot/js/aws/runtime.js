/**
 * Shared AWS Runtime Contract v1
 * Provides baseline capabilities and runtime contract versioning for AWS learning SPAs.
 */
(function (global) {
  "use strict";

  const CONTRACT_VERSION = 1;

  const runtime = Object.freeze({
    contractVersion: CONTRACT_VERSION,
    environment: "production",
    createStorageHelper(namespace) {
      const prefix = `${namespace}.`;
      return Object.freeze({
        getItem(key, fallback = null) {
          try {
            const val = localStorage.getItem(prefix + key);
            return val !== null ? JSON.parse(val) : fallback;
          } catch {
            return fallback;
          }
        },
        setItem(key, value) {
          try {
            localStorage.setItem(prefix + key, JSON.stringify(value));
          } catch (err) {
            console.error("Storage write failed:", err);
          }
        },
        removeItem(key) {
          try {
            localStorage.removeItem(prefix + key);
          } catch (err) {
            console.error("Storage remove failed:", err);
          }
        }
      });
    }
  });

  global.UlfbouAws = runtime;
})(typeof globalThis !== "undefined" ? globalThis : window);
