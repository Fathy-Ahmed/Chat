using Microsoft.AspNetCore.Identity;

namespace Chat.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public bool IsOnline { get; set; }
    public DateTime? LastSeen { get; set; }
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
}
