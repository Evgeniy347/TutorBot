using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService;

internal record TutorBotContext(ITelegramBot Client, TgBotServiceOptions Opt, IApplication App, long BotID, CancellationToken stoppingToken)
{
    private ChatEntry? _ChatEntry;
    public ChatEntry ChatEntry
    {
        get => Check.NotNull(_ChatEntry);
        set => _ChatEntry = Check.NotNull(value);
    }

    public CancellationToken Token => stoppingToken;

    public bool IsGroupChat { get; internal set; }

    public async Task<Message> SendMessage(string text, ReplyMarkup? replyMarkup = null, ParseMode parseMode = ParseMode.Html)
    {
        string logText = text;

        if (replyMarkup is null)
        {

        }
        else if (replyMarkup is ReplyKeyboardRemove)
        {
            logText += $@"
{new string('*', 10)} ReplyKeyboardRemove {new string('*', 10)}
";
        }
        else if (replyMarkup is ReplyKeyboardMarkup keyboardMarkup)
        {
            logText += $@"
{new string('*', 10)} ReplyKeyboardMarkup {new string('*', 10)}
{keyboardMarkup.Keyboard.SelectMany(x => x).Select(x => x.Text).JoinString("; ")}
";
        }

        _ = App.HistoryService.AddHistory(new MessageHistory(ChatEntry.ChatID, DateTime.Now, logText, MessageHistoryRole.Bot, ChatEntry.UserID, ChatEntry.SessionID));

        return await Client.SendMessage(ChatEntry.ChatID, text, replyMarkup: replyMarkup, parseMode: parseMode);
    }

    public async Task ErrorHandle(Exception exception, string? title = null)
    {
        Console.WriteLine(exception);
        _ = WriteError($"{title}{Environment.NewLine}{exception}");

        try
        {
            ChatEntry[] adminChats = await App.ChatService.GetChats(new GetChatsFilter(false, true));

            foreach (ChatEntry adminChat in adminChats)
            {
                try
                {
                    TutorBotContext context = new TutorBotContext(Client, Opt, App, BotID, Token);
                    context.ChatEntry = adminChat;
                    await context.SendMessage($"Произошла ошибка:{title}{Environment.NewLine}{exception}");
                }
                catch { }
            }
        }
        catch { }

        await Task.CompletedTask;
    }

    public async Task WriteError(string message)
    {
        Console.WriteLine(message);
        ChatEntry chat = await GetErrorChat();
        await App.HistoryService.AddHistory(new MessageHistory(chat.UserID, DateTime.Now, message, MessageHistoryRole.Error, BotID, new Guid()));
    }

    private async Task<ChatEntry> GetErrorChat()
    {
        ChatEntry? chatEntry = await App.ChatService.Find(-1) ??
            await App.ChatService.Create(-1, "Error Service", string.Empty, string.Empty, -1);

        return chatEntry;
    }
}
