using Chat.API.Hubs;
using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace Chat.Tests.Hubs;

public class ChatHubTests
{
    private readonly Mock<IChatService> _chatService = new();
    private readonly Mock<IUserConnectionTracker> _tracker = new();
    private readonly ChatHub _sut;

    public ChatHubTests()
    {
        _sut = new ChatHub(_chatService.Object, _tracker.Object);
    }

    [Fact]
    public async Task OnConnectedAsync_AddsConnectionAndNotifiesOthers_WhenFirstConnection()
    {
        var caller = new Mock<ISingleClientProxy>();
        var others = new Mock<ISingleClientProxy>();
        var clients = new Mock<IHubCallerClients>();
        clients.Setup(x => x.Caller).Returns(caller.Object);
        clients.Setup(x => x.Others).Returns(others.Object);

        var context = new Mock<HubCallerContext>();
        context.Setup(x => x.ConnectionId).Returns("conn-1");
        context.Setup(x => x.UserIdentifier).Returns("user-1");

        _sut.Context = context.Object;
        _sut.Clients = clients.Object;

        _tracker.Setup(t => t.GetConnections("user-1")).Returns(new List<string> { "conn-1" });
        _tracker.Setup(t => t.AddConnection("user-1", "conn-1"));

        await _sut.OnConnectedAsync();

        _tracker.Verify(t => t.AddConnection("user-1", "conn-1"), Times.Once);
        _tracker.Verify(t => t.GetConnections("user-1"), Times.Once);
        others.Verify(x => x.SendCoreAsync("UserOnline", It.Is<object?[]>(args => args != null && args.Length == 1 && (string)args[0] == "user-1"), default), Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_RemovesConnectionAndNotifiesOthers_WhenUserGoesOffline()
    {
        var others = new Mock<ISingleClientProxy>();
        var clients = new Mock<IHubCallerClients>();
        clients.Setup(x => x.Others).Returns(others.Object);

        var context = new Mock<HubCallerContext>();
        context.Setup(x => x.ConnectionId).Returns("conn-1");
        context.Setup(x => x.UserIdentifier).Returns("user-1");

        _sut.Context = context.Object;
        _sut.Clients = clients.Object;

        _tracker.Setup(t => t.RemoveConnection("user-1", "conn-1"));
        _tracker.Setup(t => t.IsOnline("user-1")).Returns(false);

        await _sut.OnDisconnectedAsync(null);

        _tracker.Verify(t => t.RemoveConnection("user-1", "conn-1"), Times.Once);
        _tracker.Verify(t => t.IsOnline("user-1"), Times.Once);
        others.Verify(x => x.SendCoreAsync("UserOffline", It.Is<object?[]>(args => args != null && args.Length == 1 && (string)args[0] == "user-1"), default), Times.Once);
    }

    [Fact]
    public async Task SendMessage_SendsToReceiverAndCaller()
    {
        var caller = new Mock<ISingleClientProxy>();
        var receiverClient = new Mock<ISingleClientProxy>();
        var clients = new Mock<IHubCallerClients>();
        clients.Setup(x => x.Caller).Returns(caller.Object);
        clients.Setup(x => x.Client("receiver-conn")).Returns(receiverClient.Object);

        var context = new Mock<HubCallerContext>();
        context.Setup(x => x.ConnectionId).Returns("sender-conn");
        context.Setup(x => x.UserIdentifier).Returns("sender");

        _sut.Context = context.Object;
        _sut.Clients = clients.Object;

        var message = new MessageDto(1, "sender", "receiver", "Hello", DateTime.UtcNow, false);
        _chatService.Setup(s => s.SendMessageAsync("sender", "receiver", "Hello")).ReturnsAsync(message);
        _tracker.Setup(t => t.GetConnections("receiver")).Returns(new List<string> { "receiver-conn" });

        await _sut.SendMessage("receiver", "Hello");

        _chatService.Verify(s => s.SendMessageAsync("sender", "receiver", "Hello"), Times.Once);
        receiverClient.Verify(x => x.SendCoreAsync("ReceiveMessage", It.Is<object?[]>(args => args != null && args.Length == 1 && ((MessageDto)args[0]).Content == "Hello"), default), Times.Once);
        caller.Verify(x => x.SendCoreAsync("ReceiveMessage", It.Is<object?[]>(args => args != null && args.Length == 1 && ((MessageDto)args[0]).Content == "Hello"), default), Times.Once);
    }
}
