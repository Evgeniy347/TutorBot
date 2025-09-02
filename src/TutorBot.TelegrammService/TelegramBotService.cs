using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TutorBot.Abstractions;
using TutorBot.TelegramService.BotActions;

namespace TutorBot.TelegramService
{
    internal class TelegramBotService(IApplication app, IOptions<TgBotServiceOptions> opt,
        Func<string, CancellationToken, ITelegramBot> clientFactory) : BackgroundService
    {
        private DialogModelLoader _dialogLoader = new DialogModelLoader(opt.Value.DialogModelPath);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ITelegramBot botClient = clientFactory(opt.Value.Token, stoppingToken);

            try
            {
                User bot = await botClient.GetMe();

                botClient.AddErrorHandler((exception, source) => ErrorHandle(exception, source, bot.Id, botClient));
                botClient.AddMessageHandler((message, type) => MessageHandle(message, type, bot.Id, botClient));

                await app.HistoryService.AddStatusService("Start", $"bot.Id:{bot.Id} FirstName:{bot.FirstName}");

                await Task.Delay(-1, stoppingToken);
            }
            finally
            {
                await botClient.Close(stoppingToken);
                await app.HistoryService.AddStatusService("Stop");
            }
        }

        private async Task MessageHandle(Message message, UpdateType type, long botID, ITelegramBot client)
        {
            TutorBotContext context = new TutorBotContext(client, opt.Value, app, botID);

            if (message.From == null)
            {
                _ = WriteError("From is null", botID);
                return;
            }

            if (message.Chat == null)
            {
                _ = WriteError("Chat is null", botID);
                return;
            }

            context.IsGroupChat = message.Chat.Type is ChatType.Group or ChatType.Supergroup;

            context.ChatEntry = await EnsureChat(message);

            if (message.Type == MessageType.NewChatMembers || context.IsGroupChat)
            {
                GroupChatBotAction resetBotAction = new GroupChatBotAction();
                await resetBotAction.ExecuteAsync(message, context);
            }

            if (message.Type == MessageType.Text && !string.IsNullOrWhiteSpace(message.Text))
            {
                DialogModel model = _dialogLoader.GetModel();
                bool isWelcome = IsWelcome(context, model);

                if (isWelcome)
                {
                    WelcomeBotAction resetBotAction = new WelcomeBotAction(model);
                    await resetBotAction.ExecuteAsync(message, context);
                }

                isWelcome = isWelcome && IsWelcome(context, model);

                if (!isWelcome)
                {
                    IBotAction? action = SelectAction(message, context);

                    if (action != null)
                    {
                        context.ChatEntry.LastActionKey = action.Key;
                        await action.ExecuteAsync(message, context);
                        await context.App.ChatService.Update(context.ChatEntry);
                    }
                    else
                    {
                        IBotAction startAction = BotActionHub.FindHandler(model, model.Start.NextStep, true)!;
                        await startAction.ExecuteAsync(message, context);
                        context.ChatEntry.LastActionKey = startAction.Key;
                        await context.App.ChatService.Update(context.ChatEntry);
                    }
                }
            }

            _ = app.HistoryService.AddHistory(new MessageHistory(message.Chat.Id, DateTime.Now, message.Text ?? string.Empty, MessageHistoryRole.User, message.From.Id, context.ChatEntry.SessionID));
        }

        private static bool IsWelcome(TutorBotContext context, DialogModel model)
        {
            bool isWelcome = string.IsNullOrEmpty(context.ChatEntry.GroupNumber) ||
                string.IsNullOrEmpty(context.ChatEntry.FullName) && !string.IsNullOrEmpty(model.Handlers.Welcome.FullNameQuestion);

            return isWelcome;
        }

        private IBotAction? SelectAction(Message message, TutorBotContext context)
        {
            IBotAction? action = FindAction(message.Text, context);

            if (action == null && !string.IsNullOrEmpty(context.ChatEntry.LastActionKey))
            {
                IBotAction? lastAction = FindAction(context.ChatEntry.LastActionKey, context);
                if (lastAction != null && lastAction.EnableProlongated)
                    action = lastAction;
            }

            return action;
        }

        private IBotAction? FindAction(string? text, TutorBotContext context)
        {
            DialogModel model = _dialogLoader.GetModel();

            IBotAction? action = BotActionHub.FindHandler(model, text);

            if (action == null && context.ChatEntry.IsAdmin)
                action = BotActionHub.AdminHandles.FirstOrDefault(a => a.Key == text);

            return action;
        }

        private async Task WriteError(string message, long botID)
        {
            ChatEntry chat = await GetErrorChat();
            await app.HistoryService.AddHistory(new MessageHistory(chat.UserID, DateTime.Now, message, MessageHistoryRole.Error, botID, new Guid()));
        }

        private async Task<ChatEntry> GetErrorChat()
        {
            ChatEntry? chatEntry = await app.ChatService.Find(-1) ??
                await app.ChatService.Create(-1, "Error Service", string.Empty, string.Empty, -1);

            return chatEntry;
        }

        private async Task<ChatEntry> EnsureChat(Message message)
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

        public async Task ErrorHandle(Exception exception, HandleErrorSource source, long botID, ITelegramBot client)
        {
            Console.WriteLine(exception);
            _ = WriteError(exception.ToString(), botID);

            try
            {
                ChatEntry[] adminChats = await app.ChatService.GetChats(new GetChatsFilter(false, true));

                foreach (ChatEntry adminChat in adminChats)
                {
                    try
                    {
                        TutorBotContext context = new TutorBotContext(client, opt.Value, app, botID);

                        context.ChatEntry = adminChat;
                        await context.SendMessage($"Произошла ошибка:{exception}");
                    }
                    catch { }
                }
            }
            catch { }

            await Task.CompletedTask;
        }
    }
}
