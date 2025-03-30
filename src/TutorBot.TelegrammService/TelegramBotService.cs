using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TutorBot.Abstractions;
using TutorBot.TelegramService.BotActions;

namespace TutorBot.TelegramService
{
    internal class TelegramBotService(IApplication app, IOptions<TgBotServiceOptions> opt) : BackgroundService
    {
        private static bool _isRun;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (opt.Value.Enable)
            {
                if (_isRun)
                    throw new Exception("is run");

                _isRun = true;


                TelegramBotClient botClient = new TelegramBotClient(opt.Value.Token, cancellationToken: stoppingToken);

                try
                {
                    User user = await botClient.GetMe();

                    await app.HistoryService.AddStatusService("Start", $"Id:{user.Id} FirstName:{user.FirstName}");

                    botClient.OnError += (exception, source) => ErrorHandle(exception, source, stoppingToken);
                    botClient.OnMessage += (message, type) => MessageHandle(message, type, botClient);

                    await Task.Delay(-1, stoppingToken);
                }
                finally
                {
                    await botClient.Close(stoppingToken);
                    await app.HistoryService.AddStatusService("Stop");
                }
            }
        }


        private async Task MessageHandle(Message message, UpdateType type, ITelegramBotClient client)
        {
            await using (TutorBotContext context = new TutorBotContext(client, opt, app))
            {
                // Проверяем, что сообщение содержит текст
                if (message.Type != MessageType.Text)
                    return;

                if (message.From == null)
                {
                    _ = WriteError("From is null");
                    return;
                }

                if (message.Chat == null)
                {
                    _ = WriteError("Chat is null");
                    return;
                }

                context.ChatEntry = await GetChat(message);

                _ = app.HistoryService.AddHistory(new MessageHistory(message.From.Id, DateTime.Now, message.Text ?? string.Empty, "Client", context.ChatEntry.NextCount()));

                if (string.IsNullOrEmpty(context.ChatEntry.GroupNumber))
                {
                    WelcomeBotAction resetBotAction = new WelcomeBotAction(opt.Value);
                    await resetBotAction.ExecuteAsync(message, context);
                }

                if (!string.IsNullOrEmpty(context.ChatEntry.GroupNumber))
                {
                    var action = BotActionHub.Handles.FirstOrDefault(a => a.Key == message.Text);
                    if (action != null)
                    {
                        await action.ExecuteAsync(message, context);
                    }
                    else
                    {
                        await context.SendMessage("Пожалуйста, выберите опцию из меню.");
                    }
                }
            }
        }

        private async Task WriteError(string message)
        {
            ChatEntry chat = await GetErrorChat();
            await app.HistoryService.AddHistory(new MessageHistory(chat.UserID, DateTime.Now, message, "Error", chat.NextCount()));
        }

        private async Task<ChatEntry> GetErrorChat()
        {
            ChatEntry? chatEntry = await app.ChatService.Find(-1) ??
                await app.ChatService.Create(-1, "Error Service", string.Empty, string.Empty, -1);

            return chatEntry;
        }

        private async Task<ChatEntry> GetChat(Message message)
        {
            Chat chat = Check.NotNull(message.Chat);

            ChatEntry? chatEntry = await app.ChatService.Find(chat.Id);

            if (chatEntry == null)
            {
                User user = Check.NotNull(message.From);
                chatEntry = await app.ChatService.Create(user.Id, user.FirstName, user.LastName ?? string.Empty, user.Username ?? string.Empty, chat.Id);
            }

            return chatEntry!;
        }

        public Task ErrorHandle(Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception);
            _ = WriteError(exception.ToString());
            return Task.CompletedTask;
        }
    }
}
