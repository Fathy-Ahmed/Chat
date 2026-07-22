import { escapeHtml, formatTime } from "../core/utils.js";

export function createAppView(elements) {
  function setStatus(text, tone = "") {
    elements.connectionState.textContent = text;
    elements.socketStatus.textContent = text;
    elements.connectionDot.className = `dot ${tone}`.trim();
    elements.socketDot.className = `dot ${tone}`.trim();
  }

  function setLoggedIn(isLoggedIn, hasSelection) {
    elements.loginButton.disabled = isLoggedIn;
    elements.refreshUsersButton.disabled = !isLoggedIn;
    elements.logoutButton.disabled = !isLoggedIn;
    elements.messageInput.disabled = !isLoggedIn || !hasSelection;
    elements.sendButton.disabled = !isLoggedIn || !hasSelection;
    elements.authState.textContent = isLoggedIn ? "Signed in" : "Not signed in";
  }

  function setSessionUser(auth) {
    elements.sessionUser.textContent = auth
      ? `${auth.userName} (${auth.userId})`
      : "No active user";
  }

  function renderUsers(users, selectedUserId, onSelect) {
    elements.presenceCount.textContent = `${users.length} user${users.length === 1 ? "" : "s"} visible`;

    if (!users.length) {
      elements.usersList.innerHTML = '<div class="small">No other users are available.</div>';
      return;
    }

    elements.usersList.innerHTML = users
      .map((user) => {
        const activeClass = user.id === selectedUserId ? "active" : "";
        const onlineClass = user.isOnline ? "success" : "";
        const lastSeen = user.lastSeen ? ` · Last seen ${formatTime(user.lastSeen)}` : "";
        return `
          <button type="button" class="user-card ${activeClass}" data-user-id="${escapeHtml(user.id)}">
            <div class="user-name"><span class="dot ${onlineClass}"></span><span>${escapeHtml(user.userName)}</span></div>
            <div class="small">${user.isOnline ? "Online" : "Offline"}${lastSeen}</div>
          </button>`;
      })
      .join("");

    elements.usersList.querySelectorAll("[data-user-id]").forEach((button) => {
      button.addEventListener("click", () => onSelect(button.dataset.userId));
    });
  }

  function renderMessages(messages, currentUserId, selectedUserName) {
    if (!messages.length) {
      elements.emptyState.hidden = false;
      elements.messageStack.hidden = true;
      elements.messageStack.innerHTML = "";
      return;
    }

    elements.emptyState.hidden = true;
    elements.messageStack.hidden = false;
    elements.messageStack.innerHTML = messages
      .map((message) => {
        const mine = message.senderId === currentUserId;
        const senderLabel = mine ? "You" : selectedUserName || message.senderId;
        return `
          <article class="message ${mine ? "me" : ""}">
            <div>${escapeHtml(message.content)}</div>
            <div class="meta">
              <span>${escapeHtml(senderLabel)}</span>
              <span>${formatTime(message.sentAt)}</span>
            </div>
          </article>`;
      })
      .join("");

    elements.messageStack.scrollTop = elements.messageStack.scrollHeight;
  }

  function setSelection(user) {
    elements.selectedUserLabel.textContent = user
      ? `Chatting with ${user.userName}`
      : "Select a user to start chatting";
    elements.chatTitle.textContent = user ? user.userName : "Conversation";
    elements.chatSubtitle.textContent = user
      ? user.isOnline
        ? "User is online right now."
        : "User is currently offline."
      : "Pick a user from the list to load history.";
  }

  function setUsersUpdated() {
    elements.usersUpdated.textContent = `Updated ${new Date().toLocaleTimeString()}`;
  }

  function reset() {
    elements.usersUpdated.textContent = "Waiting for login";
    elements.usersList.innerHTML = '<div class="small">Sign in to load the user list.</div>';
    setSelection(null);
    renderMessages([], null, null);
  }

  return {
    setStatus,
    setLoggedIn,
    setSessionUser,
    renderUsers,
    renderMessages,
    setSelection,
    setUsersUpdated,
    reset,
  };
}
