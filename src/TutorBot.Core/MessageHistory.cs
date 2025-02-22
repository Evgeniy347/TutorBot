using System.ComponentModel.DataAnnotations;

namespace TutorBot.Core
{
    internal class MessageHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public long UserId { get; set; }

        [Required]
        public string? MessageText { get; set; } 

        [Required]
        public DateTime Timestamp { get; set; }
    }
}
