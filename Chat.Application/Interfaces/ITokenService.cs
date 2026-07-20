using Chat.Domain.Entities;

namespace Chat.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}