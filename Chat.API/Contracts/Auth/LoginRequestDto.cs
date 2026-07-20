namespace Chat.API.Contracts.Auth;

public sealed record LoginRequestDto(string UserName, string Password);