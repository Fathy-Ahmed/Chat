export function createSignalRService({ hubUrl, getAccessToken }) {
  let connection = null;
  const handlers = {
    reconnecting: [],
    reconnected: [],
    closed: [],
    messageReceived: [],
    userOnline: [],
    userOffline: [],
  };

  function notify(eventName, value) {
    handlers[eventName].forEach((handler) => handler(value));
  }

  function on(eventName, handler) {
    handlers[eventName].push(handler);
  }

  async function connect() {
    await disconnect();

    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: getAccessToken })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connection.onreconnecting(() => notify("reconnecting"));
    connection.onreconnected(() => notify("reconnected"));
    connection.onclose(() => notify("closed"));
    connection.on("ReceiveMessage", (message) =>
      notify("messageReceived", message),
    );
    connection.on("UserOnline", () => notify("userOnline"));
    connection.on("UserOffline", () => notify("userOffline"));

    await connection.start();
  }

  async function disconnect() {
    if (!connection) return;
    await connection.stop().catch(() => undefined);
    connection = null;
  }

  return {
    connect,
    disconnect,
    sendMessage(userId, content) {
      if (!connection) return Promise.resolve();
      return connection.invoke("SendMessage", userId, content);
    },
    on,
  };
}
