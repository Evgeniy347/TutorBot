using TutorBot.Abstractions;

namespace TutorBot.Core
{
    internal static class MapExtensions
    {
        public static DBChatEntry MapDB(this ChatEntry chat)
        {
            return new DBChatEntry()
            {
                Id = chat.ID,
                ChatID = chat.ChatID,
                CountMessages = chat.CountMessages,
                GroupNumber = chat.GroupNumber,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeLastUpdate = chat.TimeLastUpdate,
                UserID = chat.UserID,
            };
        }

        public static ChatEntry MapCore(this DBChatEntry chat)
        {
            return new ChatEntry()
            {
                ID = chat.Id,
                ChatID = chat.ChatID,
                CountMessages = chat.CountMessages,
                GroupNumber = chat.GroupNumber,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeLastUpdate = chat.TimeLastUpdate,
                UserID = chat.UserID,
            };
        }

        public static MessageHistory MapCore(this Abstractions.MessageHistory history)
        {
            return new MessageHistory()
            {
                MessageText = history.MessageText,
                ChatID = history.ChatID,
                OrderID = history.OrderID,
                Type = history.Type,
                Timestamp = history.Timestamp,
            };
        }
    }
}
