using LikhodedDynamics.Sber.GigaChatSDK;
using LikhodedDynamics.Sber.GigaChatSDK.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TutorBot.Abstractions;

namespace TutorBot.Core
{
    internal class ALServiceService(ServiceLocator locator) : IALServiceService
    {
        private readonly GigaChatOptions _options = locator.Services.GetRequiredService<IOptions<GigaChatOptions>>().Value;

        private GigaChat? _gigaChat;

        public bool Enable => _options.Enable;

        public Task<string> AskAssistant(string currentMessage)
        {
            MessageQuery messageQuery = CreateMessage();
            messageQuery.messages.Add(new MessageContent("user", currentMessage));

            return CompletionsAsync(messageQuery);
        }

        public async Task<string> AskAssistant(long chatID, long userID, string currentMessage, Guid sessionID)
        {
            await using (var scope = locator.CreateAsyncScope())
            {
                DBChatEntry? chatDB = await scope.DBContext.Chats.Where(x => x.ChatID == chatID).FirstOrDefaultAsync();

                if (chatDB == null)
                    throw new ArgumentException(nameof(chatID));

                DBMessageHistory[] histories = await scope.DBContext.MessageHistories
                    .Where(x => x.ChatID == chatID && x.SessionID == sessionID && !string.IsNullOrWhiteSpace(x.Type))
                    .OrderBy(x => x.Id)
                    .Take(1000)
                    .ToArrayAsync();

                string systemPromt = BuildPromt(_options.SystemPromtGroupBot, chatDB);

                MessageQuery messageQuery = BuildMessage(currentMessage, chatDB, userID, systemPromt, histories);

                string message = await CompletionsAsync(messageQuery);

                if (message.StartsWith("принято", StringComparison.OrdinalIgnoreCase))
                {
                    await locator.Application.HistoryService.AddHistory(new Abstractions.MessageHistory(chatID, DateTime.Now, message, MessageHistoryRole.Bot, 0, sessionID));
                    return string.Empty;
                }

                if (message.StartsWith("не знаю", StringComparison.OrdinalIgnoreCase))
                {
                    await locator.Application.HistoryService.AddHistory(new Abstractions.MessageHistory(chatID, DateTime.Now, message, MessageHistoryRole.Bot, 0, sessionID));
                    return string.Empty;
                }

                if (message.StartsWith("я знаю ответ", StringComparison.OrdinalIgnoreCase))
                {
                    message = message.Remove(0, "я знаю ответ".Length).TrimStart(':', ' ', '\r', '\n', '\t');
                    message = char.ToUpper(message[0]) + message.Remove(0, 1);

                    return message;
                }

                throw new Exception(message);
            }
        }

        public async Task<string> TransferQuestionAL(long chatID, string currentMessage, Guid sessionID)
        {
            await using (var scope = locator.CreateAsyncScope())
            {
                DBChatEntry? chatDB = await scope.DBContext.Chats.Where(x => x.ChatID == chatID).FirstOrDefaultAsync();

                if (chatDB == null)
                    throw new ArgumentException(nameof(chatID));

                DBMessageHistory[] histories = await scope.DBContext.MessageHistories
                    .Where(x => x.ChatID == chatID && x.SessionID == sessionID && !string.IsNullOrWhiteSpace(x.Type))
                    .OrderBy(x => x.Id)
                    .Take(1000)
                    .ToArrayAsync();

                string systemPromt = BuildPromt(_options.SystemPromtBot, chatDB);

                MessageQuery messageQuery = BuildMessage(currentMessage, chatDB, systemPromt, histories);

                string message = await CompletionsAsync(messageQuery);

                return message;
            }
        }

        private static MessageQuery BuildMessage(string currentMessage, DBChatEntry chatDB, long userID, string systemPromt, DBMessageHistory[] histories)
        {
            MessageQuery messageQuery = CreateMessage();

            messageQuery.messages.Add(new MessageContent("system", systemPromt));

            foreach (DBMessageHistory history in histories)
            {
                string role = string.Empty;

                switch (history.Type)
                {
                    case "Client":
                    case "User":
                        role = "user";
                        messageQuery.messages.Add(new MessageContent(role, $"[Timestamp:{history.Timestamp}] [UserID_{history.UserID}] {history.MessageText}"));
                        break;
                    case "Bot":
                        role = "assistant";
                        messageQuery.messages.Add(new MessageContent(role, "Я знаю ответ: " + history.MessageText));
                        break;
                    case "Error":
                        break;
                    default: throw new InvalidOperationException(role);
                }
            }

            messageQuery.messages.Add(new MessageContent("user", @$"
ОТВЕТ ДОЛЖЕН НАЧИНАТЬСЯ с фраз ""Принято"", ""Не знаю"" или ""Я знаю ответ""
Текущее время: [{DateTime.Now}]
[UserID_{userID}]  ""{currentMessage}"""));
            return messageQuery;
        }

        private static MessageQuery BuildMessage(string currentMessage, DBChatEntry chatDB, string systemPromt, DBMessageHistory[] histories)
        {
            MessageQuery messageQuery = CreateMessage();

            messageQuery.messages.Add(new MessageContent("system", systemPromt));

            foreach (DBMessageHistory history in histories)
            {
                string role = string.Empty;

                switch (history.Type)
                {
                    case "Client":
                    case "User":
                        role = "user";
                        messageQuery.messages.Add(new MessageContent(role, $"[{history.Timestamp}]{history.MessageText}"));
                        break;
                    case "Bot":
                        role = "assistant";
                        messageQuery.messages.Add(new MessageContent(role, $"[{history.Timestamp}]{history.MessageText}"));
                        break;
                    case "Error":
                        break;
                    default: throw new InvalidOperationException(role);
                }

            }

            messageQuery.messages.Add(new MessageContent("user", currentMessage));
            return messageQuery;
        }

        private string BuildPromt(string systemPromt, DBChatEntry chat)
        {
            if (systemPromt.StartsWith("FilePath:"))
            {
                systemPromt = systemPromt.Remove(0, "FilePath:".Length);
                systemPromt = File.ReadAllText(systemPromt);
            }

            return systemPromt
                .Replace("#FirstName#", chat.FirstName)
                .Replace("#LastName#", chat.LastName)
                .Replace("#UserName#", chat.UserName);
        }

        private static MessageQuery CreateMessage()
        {
            return new MessageQuery(null, "GigaChat:latest", 0.87f, 0.47f, 1L, stream: false, 512L);
        }

        private async Task<string> CompletionsAsync(MessageQuery messageQuery)
        {
            if (_gigaChat == null)
                _gigaChat = new GigaChat(_options.SecretKey, _options.IsCommercial, _options.IgnoreTLS, true);

            if (_gigaChat.Token == null)
                if (await _gigaChat.CreateTokenAsync() == null)
                    throw new InvalidOperationException("CreateTokenAsync");

            var result = await _gigaChat.CompletionsAsync(messageQuery) ??
                throw new InvalidOperationException("CompletionsAsync");

            string? message = result?.choices?[0]?.message?.content;

            if (string.IsNullOrEmpty(message))
                throw new InvalidOperationException("message");

            return message;
        }
    }

    internal class GigaChatOptions
    {
        public required bool Enable { get; init; }
        public required string SecretKey { get; init; }
        public required bool IsCommercial { get; init; }
        public required bool IgnoreTLS { get; init; }
        public required string SystemPromtBot { get; set; }
        public required string SystemPromtGroupBot { get; set; }
    }
}
