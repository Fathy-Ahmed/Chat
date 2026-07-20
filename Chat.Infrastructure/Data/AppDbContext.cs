using Chat.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chat.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Message> Messages => Set<Message>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // required — Identity configures its own tables here

        builder.Entity<Message>(entity =>
        {
            entity.HasIndex(m => new { m.SenderId, m.ReceiverId });
            entity.HasIndex(m => m.SentAt);

            entity.HasOne(m => m.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(m => m.SenderId)
                  .OnDelete(DeleteBehavior.Restrict); // avoid multiple cascade paths

            entity.HasOne(m => m.Receiver)
                  .WithMany(u => u.ReceivedMessages)
                  .HasForeignKey(m => m.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
