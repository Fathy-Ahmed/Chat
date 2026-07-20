namespace Chat.Application.DTOs;

public record UserDto(string Id, string UserName, bool IsOnline, DateTime? LastSeen);