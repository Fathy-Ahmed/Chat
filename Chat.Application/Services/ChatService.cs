using Chat.Application.DTOs;
using Chat.Application.Interfaces;
using Chat.Domain.Entities;

namespace Chat.Application.Services;

public class ChatService : IChatService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserConnectionTracker _userConnectionTracker;

    public ChatService(
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IUserConnectionTracker userConnectionTracker)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _userConnectionTracker = userConnectionTracker;
    }


    public async Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.");

        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        var saved = await _messageRepository.AddAsync(message);
        return new MessageDto(saved.Id, saved.SenderId, saved.ReceiverId, saved.Content, saved.SentAt, saved.IsRead);
    }

    public async Task<List<MessageDto>> GetHistoryAsync(string userId, string otherUserId)
    {
        var messages = await _messageRepository.GetConversationAsync(userId, otherUserId);

        return messages
            .Select(m => new MessageDto(m.Id, m.SenderId, m.ReceiverId, m.Content, m.SentAt, m.IsRead))
            .ToList();
    }

    public async Task<List<UserDto>> GetAvailableUsersAsync(string currentUserId)
    {
        var users = await _userRepository.GetAvailableUsersAsync(currentUserId);
        var onlineUserIds = _userConnectionTracker.GetOnlineUserIds().ToHashSet();

        return users
            .Select(user => new UserDto(user.Id, user.UserName ?? string.Empty, onlineUserIds.Contains(user.Id), user.LastSeen))
            .ToList();
    }

}
