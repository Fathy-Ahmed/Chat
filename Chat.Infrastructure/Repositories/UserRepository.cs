using Chat.Application.Interfaces;
using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicationUser>> GetAvailableUsersAsync(string currentUserId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(user => user.Id != currentUserId)
            .OrderBy(user => user.UserName)
            .ToListAsync();
    }

    public Task<ApplicationUser?> FindByNameAsync(string userName)
    {
        return _context.Users.FirstOrDefaultAsync(user => user.UserName == userName);
    }
}