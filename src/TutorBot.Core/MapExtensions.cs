namespace TutorBot.Core
{
    internal static class MapExtensions
    {
        public static MessageHistory MapCore(this Abstractions.MessageHistory history)
        {
            return new MessageHistory()
            {
                MessageText = history.MessageText,
                UserId = history.UserID,
                Timestamp = history.Timestamp,
            };
        }
    }
}
