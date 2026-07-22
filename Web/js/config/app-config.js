const configuredApiBaseUrl = window.__CHAT_CONFIG__?.apiBaseUrl;

export const appConfig = {
  apiBaseUrl: (
    configuredApiBaseUrl ||
    (window.location.protocol === "file:"
      ? "http://localhost:5063"
      : window.location.origin)
  ).replace(/\/$/, ""),
  authStorageKey: "chat.demo.auth",
  hubPath: "/chathub",
};
