using Chat.Domain.Entities;
using Chat.Infrastructure.Data;
using Chat.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Services;

public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DbSeeder(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        var firstUser = await EnsureUserAsync(DemoSeedData.FirstUserName, DemoSeedData.FirstUserEmail, DemoSeedData.FirstUserPassword);
        var secondUser = await EnsureUserAsync(DemoSeedData.SecondUserName, DemoSeedData.SecondUserEmail, DemoSeedData.SecondUserPassword);

        await EnsureMessageAsync(firstUser.Id, secondUser.Id, DemoSeedData.FirstMessage, true, DateTime.UtcNow.AddMinutes(-10));
        await EnsureMessageAsync(secondUser.Id, firstUser.Id, DemoSeedData.SecondMessage, false, DateTime.UtcNow.AddMinutes(-5));

        await _context.SaveChangesAsync();
    }

    private async Task<ApplicationUser> EnsureUserAsync(string userName, string email, string password)
    {
        var existingUser = await _userManager.FindByNameAsync(userName);
        if (existingUser != null)
            return existingUser;

        var user = new ApplicationUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true,
            IsOnline = false,
            LastSeen = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Failed to seed {userName}: {string.Join(", ", result.Errors.Select(error => error.Description))}");

        return user;
    }

    private async Task EnsureMessageAsync(string senderId, string receiverId, string content, bool isRead, DateTime sentAt)
    {
        var exists = await _context.Messages.AnyAsync(message =>
            message.SenderId == senderId &&
            message.ReceiverId == receiverId &&
            message.Content == content);

        if (exists)
            return;

        _context.Messages.Add(new Message
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Content = content,
            SentAt = sentAt,
            IsRead = isRead
        });
    }
}