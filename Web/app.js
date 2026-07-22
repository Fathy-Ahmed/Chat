import { appConfig } from "./js/config/app-config.js";
import { parseJwt } from "./js/core/utils.js";
import { createApiClient } from "./js/services/api-client.js";
import { createAuthService } from "./js/services/auth-service.js";
import { createChatService } from "./js/services/chat-service.js";
import { createSignalRService } from "./js/services/signalr-service.js";
import { createStorageService } from "./js/services/storage-service.js";
import { createAppView } from "./js/ui/app-view.js";
import { getElements } from "./js/ui/elements.js";

const elements = getElements(document);
const storage = createStorageService(localStorage, appConfig.authStorageKey);
const state = {
  auth: storage.getAuth(),
  currentUser: null,
  users: [],
  selectedUser: null,
};

const apiClient = createApiClient({
  baseUrl: appConfig.apiBaseUrl,
  getAccessToken: () => state.auth?.accessToken,
});
const authService = createAuthService(apiClient);
const signalRService = createSignalRService({
  hubUrl: apiClient.resolveUrl(appConfig.hubPath),
  getAccessToken: () => state.auth?.accessToken,
});
const chatService = createChatService(apiClient, signalRService);
const view = createAppView(elements);

function getCurrentUserFromAuth(auth) {
  if (!auth?.accessToken) return null;

  const payload = parseJwt(auth.accessToken);
  return {
    userId:
      payload[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ] || payload.sub,
    userName:
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
      auth.userName,
  };
}

async function loadUsers() {
  if (!state.auth) return;

  state.users = await chatService.loadUsers();
  view.renderUsers(state.users, state.selectedUser?.id, selectUser);
  view.setUsersUpdated();

  if (state.selectedUser) {
    const selectedUser = state.users.find((user) => user.id === state.selectedUser.id);
    if (selectedUser) {
      state.selectedUser = selectedUser;
      view.setSelection(selectedUser);
    }
  }
}

async function loadHistory() {
  if (!state.selectedUser) return;

  const messages = await chatService.loadHistory(state.selectedUser.id);
  view.renderMessages(
    messages,
    state.currentUser?.userId,
    state.selectedUser.userName,
  );
  elements.chatSubtitle.textContent = `Loaded ${messages.length} messages.`;
}

async function selectUser(userId) {
  const user = state.users.find((item) => item.id === userId);
  if (!user) return;

  state.selectedUser = user;
  view.setSelection(user);
  view.setLoggedIn(Boolean(state.auth), true);
  await loadHistory();
  elements.messageInput.focus();
}

async function connect() {
  signalRService.on("reconnecting", () => view.setStatus("Reconnecting...", "brand"));
  signalRService.on("reconnected", async () => {
    view.setStatus("Connected", "success");
    await loadUsers();
    await loadHistory();
  });
  signalRService.on("closed", () => view.setStatus("Disconnected", "danger"));
  signalRService.on("messageReceived", async (message) => {
    const selectedUserId = state.selectedUser?.id;
    const isCurrentConversation =
      selectedUserId &&
      (message.senderId === selectedUserId || message.receiverId === selectedUserId);

    if (isCurrentConversation) {
      await loadHistory();
    } else {
      await loadUsers();
    }
  });
  signalRService.on("userOnline", loadUsers);
  signalRService.on("userOffline", loadUsers);

  await signalRService.connect();
  view.setStatus("Connected", "success");
}

async function signIn(auth) {
  state.auth = auth;
  state.currentUser = getCurrentUserFromAuth(auth);
  storage.saveAuth(auth);
  view.setSessionUser(auth);
  view.setLoggedIn(true, Boolean(state.selectedUser));
  view.setStatus("Connecting...", "brand");
  await connect();
  await loadUsers();
}

async function login() {
  const userName = document.getElementById("loginUserName").value.trim();
  const password = document.getElementById("loginPassword").value;
  await signIn(await authService.login(userName, password));
}

async function register() {
  const userName = document.getElementById("registerUserName").value.trim();
  const password = document.getElementById("registerPassword").value;
  const email = document.getElementById("registerEmail").value.trim();
  await signIn(await authService.register(userName, password, email || null));
}

async function sendMessage() {
  if (!state.selectedUser) return;

  const content = elements.messageInput.value.trim();
  if (!content) return;

  await chatService.sendMessage(state.selectedUser.id, content);
  elements.messageInput.value = "";
}

async function runAction(action, errorMessage) {
  try {
    await action();
  } catch (error) {
    alert(error.message || errorMessage);
  }
}

async function logout() {
  await signalRService.disconnect();
  storage.clearAuth();
  state.auth = null;
  state.currentUser = null;
  state.users = [];
  state.selectedUser = null;
  view.reset();
  view.setSessionUser(null);
  view.setLoggedIn(false, false);
  view.setStatus("Disconnected", "danger");
}

function bindEvents() {
  elements.loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.loginButton.disabled = true;
    await runAction(login, "Login failed.");
    elements.loginButton.disabled = Boolean(state.auth);
  });

  elements.registerForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    elements.registerButton.disabled = true;
    await runAction(register, "Registration failed.");
    elements.registerButton.disabled = Boolean(state.auth);
  });

  elements.sendButton.addEventListener("click", () =>
    runAction(sendMessage, "Unable to send message."),
  );

  elements.messageInput.addEventListener("keydown", (event) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      runAction(sendMessage, "Unable to send message.");
    }
  });

  elements.refreshUsersButton.addEventListener("click", () =>
    runAction(async () => {
      await loadUsers();
      await loadHistory();
    }, "Unable to refresh users."),
  );

  elements.logoutButton.addEventListener("click", logout);
}

async function bootstrap() {
  bindEvents();
  view.setSessionUser(state.auth);

  if (!state.auth) {
    view.setLoggedIn(false, false);
    view.setStatus("Disconnected", "danger");
    return;
  }

  try {
    state.currentUser = getCurrentUserFromAuth(state.auth);
    view.setLoggedIn(true, false);
    view.setStatus("Connecting...", "brand");
    await connect();
    await loadUsers();
  } catch (error) {
    console.error(error);
    await logout();
  }
}

bootstrap();
