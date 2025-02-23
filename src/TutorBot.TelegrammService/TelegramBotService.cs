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
        private static bool _isRun;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_opt.Enable)
            {
                if (_isRun)
                    throw new Exception("is run");

                _isRun = true;


                TelegramBotClient botClient = new TelegramBotClient(_opt.Token, cancellationToken: stoppingToken);

                try
                {
                    User user = await botClient.GetMe();
                    await app.HistoryService.AddStatusService("Start", $"Id:{user.Id} FirstName:{user.FirstName}");

                    botClient.OnError += (exception, source) => ErrorHandle(exception, source, stoppingToken);
                    botClient.OnMessage += (message, type) => MessageHandle(message, type, botClient);
                    botClient.OnUpdate += (update) => UpdateHandle(botClient, update, stoppingToken);

                    await Task.Delay(-1, stoppingToken);
                }
                finally
                {
                    await botClient.Close(stoppingToken);
                    await app.HistoryService.AddStatusService("Stop");
                }
            }
        }

        private async Task MessageHandle(Message message, UpdateType type, TelegramBotClient botClient)
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

        public Task ErrorHandle(Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
            _ = app.HistoryService.AddHistory(new MessageHistory(-1, DateTime.Now, exception.ToString()));
            return Task.CompletedTask;
        }

        public async Task UpdateHandle(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            await Task.Yield();

            if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery?.Message?.From == null)
                    return;

                CallbackQuery callbackQuery = update.CallbackQuery;
                Message message = callbackQuery.Message;

                _ = app.HistoryService.AddHistory(new MessageHistory(message.From.Id, DateTime.Now, message.Text ?? string.Empty));

                // Отвечаем на нажатие кнопки
                await botClient.AnswerCallbackQuery(
                    callbackQueryId: callbackQuery.Id,
                    text: $"Вы нажали: {callbackQuery.Data}"
                );

                //Редактируем сообщение, чтобы показать результат
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
