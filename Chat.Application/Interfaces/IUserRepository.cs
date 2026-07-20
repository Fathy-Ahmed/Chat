using Chat.Domain.Entities;

namespace Chat.Application.Interfaces;

public interface IUserRepository
{
    Task<List<ApplicationUser>> GetAvailableUsersAsync(string currentUserId);
    Task<ApplicationUser?> FindByNameAsync(string userName);
}