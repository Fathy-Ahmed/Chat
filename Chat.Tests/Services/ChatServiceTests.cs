using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Application.Services;
using Chat.Domain.Entities;
using Moq;

namespace Chat.Tests.Services;

public class ChatServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IUserConnectionTracker> _userConnectionTracker = new();
    private readonly ChatService _sut;

    public ChatServiceTests()
    {
        _sut = new ChatService(_messageRepository.Object, _userRepository.Object, _userConnectionTracker.Object);
    }

    [Fact]
    public async Task SendMessageAsync_WhenContentIsEmpty_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.SendMessageAsync("sender", "receiver", "   "));

        Assert.Equal("Message content cannot be empty.", exception.Message);
    }

    [Fact]
    public async Task SendMessageAsync_WhenValidContent_ReturnsMappedMessageDto()
    {
        var savedMessage = new Message
        {
            Id = 7,
            SenderId = "sender",
            ReceiverId = "receiver",
            Content = "Hello",
            SentAt = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc),
            IsRead = false
        };

        _messageRepository
            .Setup(r => r.AddAsync(It.IsAny<Message>()))
            .ReturnsAsync(savedMessage);

        var result = await _sut.SendMessageAsync("sender", "receiver", "Hello");

        Assert.NotNull(result);
        Assert.Equal(savedMessage.Id, result.Id);
        Assert.Equal(savedMessage.SenderId, result.SenderId);
        Assert.Equal(savedMessage.ReceiverId, result.ReceiverId);
        Assert.Equal(savedMessage.Content, result.Content);
        Assert.Equal(savedMessage.SentAt, result.SentAt);
        Assert.Equal(savedMessage.IsRead, result.IsRead);

        _messageRepository.Verify(r => r.AddAsync(It.Is<Message>(m =>
            m.SenderId == "sender" &&
            m.ReceiverId == "receiver" &&
            m.Content == "Hello")), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsMappedMessageDtos()
    {
        var messages = new List<Message>
        {
            new()
            {
                Id = 1,
                SenderId = "user1",
                ReceiverId = "user2",
                Content = "Hi",
                SentAt = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc),
                IsRead = true
            }
        };

        _messageRepository
            .Setup(r => r.GetConversationAsync("user1", "user2", 0, 50))
            .ReturnsAsync(messages);

        var result = await _sut.GetHistoryAsync("user1", "user2");

        Assert.Single(result);
        Assert.IsType<MessageDto>(result[0]);
        Assert.Equal("Hi", result[0].Content);
        Assert.True(result[0].IsRead);
    }

    [Fact]
    public async Task GetAvailableUsersAsync_ReturnsUsersWithOnlineStatus()
    {
        var users = new List<ApplicationUser>
        {
            new() { Id = "u1", UserName = "Alice", LastSeen = new DateTime(2026, 7, 21, 9, 0, 0, DateTimeKind.Utc) },
            new() { Id = "u2", UserName = "Bob", LastSeen = null }
        };

        _userRepository
            .Setup(r => r.GetAvailableUsersAsync("current"))
            .ReturnsAsync(users);

        _userConnectionTracker
            .Setup(t => t.GetOnlineUserIds())
            .Returns(new List<string> { "u1" });

        var result = await _sut.GetAvailableUsersAsync("current");

        Assert.Equal(2, result.Count);
        Assert.True(result[0].IsOnline);
        Assert.False(result[1].IsOnline);
        Assert.Equal("Alice", result[0].UserName);
        Assert.Equal("Bob", result[1].UserName);
    }
}
