using Chat.Application.Interfaces;
using Chat.Infrastructure.Data;
using Chat.Infrastructure.Repositories;
using Chat.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null)));

        services.AddIdentityCore<Domain.Entities.ApplicationUser>(options =>
        {
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<DbSeeder>();
        services.AddSingleton<IUserConnectionTracker, InMemoryUserConnectionTracker>();

        return services;
    }
}
