using Chat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Chat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IUserConnectionTracker _tracker;

    public ChatHub(IChatService chatService, IUserConnectionTracker tracker)
    {
        _chatService = chatService;
        _tracker = tracker;
    }


    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;
        _tracker.AddConnection(userId, Context.ConnectionId);

        // only announce if this is their first tab/connection
        if (_tracker.GetConnections(userId).Count == 1)
            await Clients.Others.SendAsync("UserOnline", userId);

        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier!;
        _tracker.RemoveConnection(userId, Context.ConnectionId);

        if (!_tracker.IsOnline(userId))
            await Clients.Others.SendAsync("UserOffline", userId);

        await base.OnDisconnectedAsync(exception);
    }


    public async Task SendMessage(string receiverId, string content)
    {
        var senderId = Context.UserIdentifier!;
        var message = await _chatService.SendMessageAsync(senderId, receiverId, content);

        // push to every open tab/device the receiver has
        var receiverConnections = _tracker.GetConnections(receiverId);
        foreach (var connId in receiverConnections)
            await Clients.Client(connId).SendAsync("ReceiveMessage", message);

        // echo back to sender (so their other tabs update too)
        await Clients.Caller.SendAsync("ReceiveMessage", message);
    }

}
