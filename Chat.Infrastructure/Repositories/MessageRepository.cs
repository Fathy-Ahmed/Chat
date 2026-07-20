using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<List<Message>> GetConversationAsync(string userA, string userB, int skip = 0, int take = 50)
    {
        return await _context.Messages
            .Where(m => (m.SenderId == userA && m.ReceiverId == userB) ||
                        (m.SenderId == userB && m.ReceiverId == userA))
            .OrderByDescending(m => m.SentAt)
            .Skip(skip)
            .Take(take)
            .OrderBy(m => m.SentAt) // re-sort ascending after paging for display
            .ToListAsync();
    }
}
