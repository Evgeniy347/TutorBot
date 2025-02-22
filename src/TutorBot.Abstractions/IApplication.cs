namespace TutorBot.Abstractions
{
    public interface IApplication
    {
        IHistoryService HistoryService { get; }
    }

    public interface IHistoryService
    {
        public Task AddHistory(MessageHistory history);
    }

    public record MessageHistory(int UserID, DateTime Timestamp, string MessageText);
}
