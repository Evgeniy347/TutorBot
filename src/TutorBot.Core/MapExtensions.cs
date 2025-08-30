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
                Version = chat.Version,
                GroupNumber = chat.GroupNumber,
                FullName = chat.FullName,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeModified = chat.TimeModified,
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
                Version = chat.Version,
                GroupNumber = chat.GroupNumber,
                FullName = chat.FullName,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeModified = chat.TimeModified,
                UserID = chat.UserID,
                LastActionKey = chat.LastActionKey,
                SessionID = chat.SessionID,
                IsAdmin = chat.IsAdmin,
                EnableAdminError = chat.EnableAdminError,
            };
        }

        public static DBChatEntryVersion MapVersion(this DBChatEntry chat)
        {
            return new DBChatEntryVersion()
            {
                Id = chat.Id,
                ChatID = chat.ChatID,
                Version = chat.Version,
                GroupNumber = chat.GroupNumber,
                FullName = chat.FullName,
                IsFirstMessage = chat.IsFirstMessage,
                FirstName = chat.FirstName,
                LastName = chat.LastName,
                UserName = chat.UserName,
                TimeCreate = chat.TimeCreate,
                TimeModified = chat.TimeModified,
                UserID = chat.UserID,
                LastActionKey = chat.LastActionKey,
                SessionID = chat.SessionID,
                IsAdmin = chat.IsAdmin,
                EnableAdminError = chat.EnableAdminError,
            };
        }

        public static DBMessageHistory MapCore(this MessageHistory history)
        {
            return new DBMessageHistory()
            {
                MessageText = history.MessageText,
                ChatID = history.ChatID,
                UserID = history.UserID,
                Type = history.Type.ToString(),
                Timestamp = history.Timestamp,
                SessionID = history.SessionID
            };
        }

        public static MessageHistory MapCore(this DBMessageHistory history)
        {
            Enum.TryParse(history.Type, out MessageHistoryRole rolel);
            return new MessageHistory(history.ChatID, history.Timestamp, history.MessageText, rolel, history.UserID, history.SessionID, history.Id);
        }
    }
}
