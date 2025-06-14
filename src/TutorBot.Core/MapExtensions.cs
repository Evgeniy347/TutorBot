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
                MessagesCount = chat.MessagesCount,
                GroupNumber = chat.GroupNumber,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeLastUpdate = chat.TimeLastUpdate,
                UserID = chat.UserID,
                LastActionKey = chat.LastActionKey,
                SessionID = chat.SessionID,
                IsAdmin = chat.IsAdmin,
                EnableAdminError = chat.EnableAdminError
            };
        }

        public static ChatEntry MapCore(this DBChatEntry chat)
        {
            return new ChatEntry()
            {
                ID = chat.Id,
                ChatID = chat.ChatID,
                MessagesCount = chat.MessagesCount,
                GroupNumber = chat.GroupNumber,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeLastUpdate = chat.TimeLastUpdate,
                UserID = chat.UserID,
                LastActionKey = chat.LastActionKey,
                SessionID = chat.SessionID,
                IsAdmin = chat.IsAdmin,
                EnableAdminError = chat.EnableAdminError,
            };
        }

        public static MessageHistory MapCore(this Abstractions.MessageHistory history)
        {
            return new MessageHistory()
            {
                MessageText = history.MessageText,
                ChatID = history.ChatID,
                UserID = history.UserID,
                OrderID = history.OrderID,
                Type = history.Type.ToString(),
                Timestamp = history.Timestamp,
                SessionID = history.SessionID
            };
        }

        public static Abstractions.MessageHistory MapCore(this MessageHistory history)
        {
            Enum.TryParse(history.Type, out MessageHistoryRole rolel);
            return new Abstractions.MessageHistory(history.ChatID, history.Timestamp, history.MessageText, rolel, history.OrderID, history.UserID, history.SessionID, history.Id);
        }
    }
}
