export function createAuthService(apiClient) {
  return {
    login(userName, password) {
      return apiClient.request("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ userName, password }),
      });
    },
    register(userName, password, email) {
      return apiClient.request("/api/auth/register", {
        method: "POST",
        body: JSON.stringify({ userName, password, email }),
      });
    },
  };
}
