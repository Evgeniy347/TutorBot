using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Telegram.Bot.TelegramBotClient;

namespace TutorBot.TelegramService
{
    public class TelegramBot(string token, CancellationToken cancellationToken) : ITelegramBot
    {
        TelegramBotClient botClient = new TelegramBotClient(token, cancellationToken: cancellationToken);

        public void AddErrorHandler(OnErrorHandler handler) => botClient.OnError += handler;

        public void AddMessageHandler(OnMessageHandler handler) => botClient.OnMessage += handler;

        public Task Close(CancellationToken stoppingToken) => botClient.Close(stoppingToken);

        public Task<User> GetMe() => botClient.GetMe();

        public Task<Message> SendMessage(
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
        ) => botClient.SendMessage(
            chatId: chatId,
            text: text,
            parseMode: parseMode,
            replyParameters: replyParameters,
            replyMarkup: replyMarkup,
            linkPreviewOptions: linkPreviewOptions,
            messageThreadId: messageThreadId,
            entities: entities,
            disableNotification: disableNotification,
            protectContent: protectContent,
            messageEffectId: messageEffectId,
            businessConnectionId: businessConnectionId,
            allowPaidBroadcast: allowPaidBroadcast,
            cancellationToken);
    }

}
