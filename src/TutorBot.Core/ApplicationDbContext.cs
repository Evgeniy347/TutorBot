using Microsoft.EntityFrameworkCore;
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
            //optionsBuilder.UseNpgsql("");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public DbSet<DBMessageHistory> MessageHistories { get; set; }

        public DbSet<DBChatEntry> Chats { get; set; }

        public DbSet<DBChatEntryVersion> ChatsVersions { get; set; }

        public DbSet<DBServiceStatusHistory> ServiceHistories { get; set; }
    }

    [Index(nameof(ChatID))]
    public class DBChatEntry
    {
        [Key]
        public long Id { get; set; }
        public long Version { get; set; }
        public long ChatID { get; set; }
        public long UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty; 
        public string GroupNumber { get; set; } = string.Empty;
        [Required]
        public DateTime TimeCreate { get; set; }
        [Required]
        public DateTime TimeModified { get; set; }
        public bool IsFirstMessage { get; set; }
        public string? LastActionKey { get; set; }
        public bool IsAdmin { get; set; }
        public bool EnableAdminError { get; set; }

        public Guid SessionID { get; set; }
    }

    [Index(nameof(ChatID))]
    public class DBChatEntryVersion
    {
        [Key]
        public long UniqueId { get; set; }
        public long Id { get; set; }
        public long Version { get; set; }
        public long ChatID { get; set; }
        public long UserID { get; set; } 
        public string FullName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty; 
        public string GroupNumber { get; set; } = string.Empty;
        [Required]
        public DateTime TimeCreate { get; set; }
        [Required]
        public DateTime TimeModified { get; set; }
        public bool IsFirstMessage { get; set; }
        public string? LastActionKey { get; set; }
        public bool IsAdmin { get; set; }
        public bool EnableAdminError { get; set; }

        public Guid SessionID { get; set; }
    }

    public class DBServiceStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;
    }

    [Index(nameof(ChatID), nameof(SessionID))]
    public class DBMessageHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long ChatID { get; set; }

        [Required]
        public long UserID { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public string MessageText { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = string.Empty;

        public Guid SessionID { get; set; }
    }
}
