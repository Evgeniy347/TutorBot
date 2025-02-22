using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService
{
    internal class TelegramBotService(IApplication app, IOptions<TgBotServiceOptions> opt) : BackgroundService
    {
        private readonly TgBotServiceOptions _opt = opt.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_opt.Enable)
            {
                TelegramBotClient botClient = new TelegramBotClient(_opt.Token, cancellationToken: stoppingToken);

                // Запуск получения обновлений
                botClient.OnMessage += (m, t) => BotClient_OnMessage(m, t, botClient);

                TgUpdateHandler handler = new TgUpdateHandler(app, _opt);

                botClient.StartReceiving(handler, cancellationToken: stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.MaxValue, stoppingToken);
            }
        }

        private async Task BotClient_OnMessage(Message message, UpdateType type, TelegramBotClient botClient)
        {
            // Проверяем, что сообщение содержит текст
            if (message.Type == MessageType.Text)
            {
                // Создаем Inline Keyboard (кнопки под сообщением)
                var inlineKeyboard = new InlineKeyboardMarkup(new[]
                {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Кнопка 1", "button1"),
                    InlineKeyboardButton.WithCallbackData("Кнопка 2", "button2")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Кнопка 3", "button3")
                }
            });

                // Отправляем сообщение с кнопками
                await botClient.SendMessage(
                      chatId: message.Chat.Id,
                      text: "Выберите действие:",
                      replyMarkup: inlineKeyboard
                  );
            }
        }
    }

    internal class TgUpdateHandler(IApplication app, TgBotServiceOptions opt) : IUpdateHandler
    {
        private readonly IApplication _app = app;
        private readonly TgBotServiceOptions _opt = opt;

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery?.Message == null)
                    return;

                var callbackQuery = update.CallbackQuery;

                // Отвечаем на нажатие кнопки
                await botClient.AnswerCallbackQuery(
                    callbackQueryId: callbackQuery.Id,
                    text: $"Вы нажали: {callbackQuery.Data}"
                );

                // Редактируем сообщение, чтобы показать результат
                await botClient.EditMessageText(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: $"Вы выбрали: {callbackQuery.Data}",
                    replyMarkup: null // Убираем кнопки после выбора
                );
            }
        }
    }
}
