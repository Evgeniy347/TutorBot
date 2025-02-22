using Microsoft.EntityFrameworkCore;

namespace TutorBot.Core
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<MessageHistory> MessageHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=TutorBotDb;Username=postgres;Password=your_password");
        }
    }
}
