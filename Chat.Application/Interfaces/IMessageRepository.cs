using Chat.Domain.Entities;

namespace Chat.Application.Interfaces;

public interface IMessageRepository
{
    Task<Message> AddAsync(Message message);
    Task<List<Message>> GetConversationAsync(string userA, string userB, int skip = 0, int take = 50);
}
