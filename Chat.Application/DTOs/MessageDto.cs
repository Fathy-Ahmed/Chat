namespace Chat.Application.DTOs;

public record MessageDto(int Id, string SenderId, string ReceiverId, string Content, DateTime SentAt, bool IsRead);
