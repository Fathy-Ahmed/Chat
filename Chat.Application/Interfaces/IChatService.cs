using Chat.Application.DTOs;

namespace Chat.Application.Interfaces;

public interface IChatService
{
    Task<MessageDto> SendMessageAsync(string senderId, string receiverId, string content);
    Task<List<MessageDto>> GetHistoryAsync(string userId, string otherUserId);
    Task<List<UserDto>> GetAvailableUsersAsync(string currentUserId);
}
