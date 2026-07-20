namespace Chat.API.Contracts.Auth;

public sealed record LoginResponseDto(string AccessToken, string UserId, string UserName, string? Email);