using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Channels;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TutorBot.Abstractions;
using TutorBot.TelegramService.BotActions;

namespace TutorBot.TelegramService
{
    internal class TelegramBotService(IApplication app, IOptions<TgBotServiceOptions> opt,
        IBotFactory clientFactory) : BackgroundService
    {
        private DialogModelLoader _dialogLoader = new DialogModelLoader(opt.Value.DialogModelPath);
        private readonly Channel<bool> _reconnectChannel = Channel.CreateUnbounded<bool>();
        private ITelegramBot? _currentBot;
        private CancellationTokenSource? _botCts;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                { 
                    _botCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                     
                    _currentBot = await clientFactory.CreateBot(_botCts.Token);

                    User bot = await _currentBot.GetMe();
                    await app.HistoryService.AddStatusService("Start", $"bot.Id:{bot.Id}");
                     
                    _currentBot.AddErrorHandler((ex, src) => HandleErrorAsync(ex, src, _botCts));
                    _currentBot.AddMessageHandler((msg, type) => MessageHandle(msg, type, bot.Id, _currentBot, _botCts.Token));
                     
                    await Task.WhenAny(
                        Task.Delay(-1, stoppingToken),
                        _reconnectChannel.Reader.ReadAsync(stoppingToken).AsTask()
                    );
                     
                    if (_currentBot != null)
                    {
                        await _currentBot.Close(_botCts.Token);
                        await app.HistoryService.AddStatusService("Stop", "Connection lost, reconnecting...");
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;  
                }
                catch (Exception ex)
                {
                    await app.HistoryService.AddStatusService("Error", $"Critical: {ex.Message}");
                    await Task.Delay(5000, stoppingToken);  
                }
                finally
                {
                    _botCts?.Dispose();
                }
            }
        }

        private async Task HandleErrorAsync(Exception exception, HandleErrorSource source, CancellationTokenSource cts)
        { 
            if (exception is HttpRequestException or TaskCanceledException or IOException)
            {
                await app.HistoryService.AddStatusService("Error", $"Network error: {exception.Message}. Reconnecting...");
                 
                cts.Cancel();
                 
                await _reconnectChannel.Writer.WriteAsync(true);
            }
            else
            { 
                await app.HistoryService.AddStatusService("Error", $"{source}: {exception.Message}");
            }
        }

        private async Task MessageHandle(Message message, UpdateType type, long botID, ITelegramBot client, CancellationToken stoppingToken)
        {
            TutorBotContext context = new TutorBotContext(client, opt.Value, app, botID, stoppingToken);

            if (message.From == null)
            {
                _ = context.WriteError("From is null");
                return;
            }

            if (message.Chat == null)
            {
                _ = context.WriteError("Chat is null");
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

        public async Task ErrorHandle(Exception exception, HandleErrorSource source, long botID, ITelegramBot client, CancellationToken token)
        {
            TutorBotContext context = new TutorBotContext(client, opt.Value, app, botID, token);
            await context.ErrorHandle(exception, source.ToString());
        }
    }
}
