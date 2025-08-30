using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService;
using static Telegram.Bot.TelegramBotClient;

namespace TutorBot.Test.Helpers
{
    internal class TelegramBotFake : ITelegramBot
    {
        private string token;
        private CancellationToken cancellationToken;
        public OnErrorHandler? _onError;
        public OnMessageHandler? _onMessage;
        private readonly User _me = new User();

        public List<SendMessageArgs> SendingMessage = [];

        public TelegramBotFake(string token, CancellationToken cancellationToken)
        {
            this.token = token;
            this.cancellationToken = cancellationToken;
            Instance = this;
        }

        public static TelegramBotFake Instance { get; private set; } = new TelegramBotFake("", default);

        public void AddErrorHandler(TelegramBotClient.OnErrorHandler handler) => _onError += handler;

        public void AddMessageHandler(TelegramBotClient.OnMessageHandler handler) => _onMessage += handler;

        public Task Close(CancellationToken stoppingToken) => Task.CompletedTask;

        public Task<User> GetMe() => Task.FromResult(new User());

        public async Task<Message> SendMessage(ChatId chatId, string text, ParseMode parseMode = ParseMode.None, ReplyParameters? replyParameters = null, ReplyMarkup? replyMarkup = null, LinkPreviewOptions? linkPreviewOptions = null, int? messageThreadId = null, IEnumerable<MessageEntity>? entities = null, bool disableNotification = false, bool protectContent = false, string? messageEffectId = null, string? businessConnectionId = null, bool allowPaidBroadcast = false, CancellationToken cancellationToken = default)
        {
            SendingMessage.Add(new SendMessageArgs(
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
            cancellationToken));

            await Task.Yield();

            Message message = new Message();

            return message;
        }
    }

    internal record SendMessageArgs(ChatId chatId, string text, ParseMode parseMode = ParseMode.None, ReplyParameters? replyParameters = null, ReplyMarkup? replyMarkup = null, LinkPreviewOptions? linkPreviewOptions = null, int? messageThreadId = null, IEnumerable<MessageEntity>? entities = null, bool disableNotification = false, bool protectContent = false, string? messageEffectId = null, string? businessConnectionId = null, bool allowPaidBroadcast = false, CancellationToken cancellationToken = default);
}
