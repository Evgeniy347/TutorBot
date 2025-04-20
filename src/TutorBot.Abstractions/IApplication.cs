
namespace TutorBot.Abstractions
{
    public interface IApplication
    {
        IHistoryService HistoryService { get; }
        IChatService ChatService { get; }
        IALServiceService ALService { get; }
    }

    public interface IALServiceService
    {
        Task<string> TransferQuestionAL(long chatID, string currentMessage, Guid sessionID);
    }

    public interface IHistoryService
    {
        Task AddHistory(MessageHistory history);
        Task AddStatusService(string status, string? message = null);
        Task<MessageHistory[]> GetMessages(long chatID, int offcet, int count, bool revers);
    }

    public interface IChatService
    {
        Task<ChatEntry?> Find(long chatID);
        Task<ChatEntry> Create(long userID, string firstName, string lastName, string username, long chatID);
        Task<ChatEntry[]> GetChats();
        Task Update(ChatEntry chat);
    }

    public class ChatEntry
    {
        public long ID { get; set; }
        public long ChatID { get; set; }
        public long UserID { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public long CountMessages { get; set; }
        public string GroupNumber { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime TimeCreate { get; set; }
        public DateTime TimeLastUpdate { get; set; }
        public bool IsFirstMessage { get; set; }

        public string? LastActionKey { get; set; }
        public Guid SessionID { get; set; }

        public long NextCount() => CountMessages++;
    }

    public record MessageHistory(long ChatID, DateTime Timestamp, string MessageText, string Type, long OrderID, Guid SessionID, int ID = -1);
}
