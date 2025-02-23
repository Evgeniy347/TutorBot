using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.ComponentModel.DataAnnotations;

namespace TutorBot.Core
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        { 
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //dotnet ef migrations add InitialCreate
            //dotnet ef database update
            //optionsBuilder.UseNpgsql();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public DbSet<MessageHistory> MessageHistories { get; set; }

        public DbSet<ServiceStatusHistory> ServiceHistories { get; set; }
    }

    public class ServiceStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public required string Status { get; set; }

        [Required]
        public required string Message { get; set; }
    }

    public class MessageHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public required string MessageText { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
