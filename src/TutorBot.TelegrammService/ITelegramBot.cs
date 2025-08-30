using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;

namespace TutorBot.TelegramService
{
    public interface ITelegramBot
    {
        Task<User> GetMe();
        void AddErrorHandler(OnErrorHandler handler);
        void AddMessageHandler(OnMessageHandler handler);
        Task<Message> SendMessage(
            ChatId chatId,
            string text,
            ParseMode parseMode = default,
            ReplyParameters? replyParameters = default,
            ReplyMarkup? replyMarkup = default,
            LinkPreviewOptions? linkPreviewOptions = default,
            int? messageThreadId = default,
            IEnumerable<MessageEntity>? entities = default,
            bool disableNotification = default,
            bool protectContent = default,
            string? messageEffectId = default,
            string? businessConnectionId = default,
            bool allowPaidBroadcast = default,
            CancellationToken cancellationToken = default
        );
        Task Close(CancellationToken stoppingToken); 
    } 
}
