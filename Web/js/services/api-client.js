export function createApiClient({ baseUrl, getAccessToken }) {
  function resolveUrl(path) {
    if (/^https?:\/\//i.test(path)) return path;
    return `${baseUrl}${path}`;
  }

  async function request(path, options = {}) {
    const headers = new Headers(options.headers || {});
    const accessToken = getAccessToken();

    if (accessToken) {
      headers.set("Authorization", `Bearer ${accessToken}`);
    }

    if (options.body && !headers.has("Content-Type")) {
      headers.set("Content-Type", "application/json");
    }

    const response = await fetch(resolveUrl(path), {
      ...options,
      headers,
    });

    if (!response.ok) {
      const message = await response.text();
      throw new Error(message || `Request failed with status ${response.status}`);
    }

    if (response.status === 204) return null;
    return response.json();
  }

  return { request, resolveUrl };
}
