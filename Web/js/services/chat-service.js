export function createChatService(apiClient, connection) {
  return {
    loadUsers() {
      return apiClient.request("/api/chat/users");
    },
    loadHistory(userId) {
      return apiClient.request(`/api/chat/history/${encodeURIComponent(userId)}`);
    },
    sendMessage(userId, content) {
      return connection.sendMessage(userId, content);
    },
  };
}
