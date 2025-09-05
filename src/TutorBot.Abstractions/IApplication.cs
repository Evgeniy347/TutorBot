
namespace TutorBot.Abstractions
{
    public interface IApplication
    {
        IHistoryService HistoryService { get; }
        IChatService ChatService { get; }
        IALServiceService ALService { get; }

        Task EnsureCreated();
    }

    public interface IALServiceService
    {
        public bool Enable { get; }
        Task<string> TransferQuestionAL(long chatID, string currentMessage, Guid sessionID);
        Task<string> AskAssistant(long chatID, long userID, string currentMessage, Guid sessionID);
        Task<string> AskAssistant(string currentMessage);
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
        Task<ChatEntry[]> GetChats(GetChatsFilter? filter = null);
        Task Update(ChatEntry chat);
        Task<ChatSummaryReport> GetSummaryInfo();
    }

    public record GetChatsFilter(bool IsAdmin = false, bool EnableAdminError = false);

    public class ChatEntry
    {
        public long ID { get; set; }
        public long ChatID { get; set; }
        public long Version { get; set; }
        public long UserID { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime TimeCreate { get; set; }
        public DateTime TimeModified { get; set; }
        public bool IsFirstMessage { get; set; }

        public string? LastActionKey { get; set; }
        public Guid SessionID { get; set; }

        public bool IsAdmin { get; set; }
        public bool EnableAdminError { get; set; }
    }

    public record MessageHistory(long ChatID, DateTime Timestamp, string MessageText, MessageHistoryRole Type, long UserID, Guid SessionID, int ID = -1);

    public enum MessageHistoryRole
    {
        None,
        User,
        Bot,
        Error
    }
    public class ChatSummaryReport
    {
        public int NumberOfChats { get; set; }
        public int NumberOfMessages { get; set; }
        public List<GroupSummary> GroupSummaries { get; set; } = new();
        public List<UserMessageCount> TopUsers { get; set; } = new();
        public List<HourlyAverage> HourlyAverages { get; set; } = new();
    }

    public class GroupSummary
    {
        public string GroupNumber { get; set; } = string.Empty;
        public int UserCount { get; set; }
        public int MessageCount { get; set; }
    }

    public class UserMessageCount
    {
        public string FullName { get; set; } = string.Empty;
        public string GroupNumber { get; set; } = string.Empty;
        public int MessageCount { get; set; }
    }

    public class HourlyAverage
    {
        public string GroupNumber { get; set; } = string.Empty;
        public int Hour { get; set; }
        public int MessageCount { get; set; }
        public double AverageMessages { get; set; }
    }

    public record SummaryInfoChat(long ChatID, string UserName, int NumberOfMessages);

}
