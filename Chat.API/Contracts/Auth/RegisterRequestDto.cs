namespace Chat.API.Contracts.Auth;

public sealed record RegisterRequestDto(string UserName, string Password, string? Email);
