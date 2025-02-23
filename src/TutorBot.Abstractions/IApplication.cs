namespace TutorBot.Abstractions
{
    public interface IApplication
    {
        IHistoryService HistoryService { get; }
    }

    public interface IHistoryService
    {
        Task AddHistory(MessageHistory history);
        Task AddStatusService(string status, string? message = null);
    }

    public record MessageHistory(long UserID, DateTime Timestamp, string MessageText);
}
