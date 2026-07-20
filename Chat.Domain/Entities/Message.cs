namespace Chat.Domain.Entities;

public class Message
{
    public int Id { get; set; }
    public required string SenderId { get; set; }
    public required string ReceiverId { get; set; }
    public required string Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }

    public ApplicationUser? Sender { get; set; }
    public ApplicationUser? Receiver { get; set; }
}
