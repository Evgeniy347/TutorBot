using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.Abstractions;

namespace TutorBot.TelegramService;

internal record TutorBotContext(ITelegramBot Client, TgBotServiceOptions Opt, IApplication App, long BotID) //: IAsyncDisposable
{
    private ChatEntry? _ChatEntry;
    public ChatEntry ChatEntry
    {
        get => Check.NotNull(_ChatEntry);
        set => _ChatEntry = Check.NotNull(value);
    }

    public bool IsGroupChat { get; internal set; }

    public async Task<Message> SendMessage(string text, ReplyMarkup? replyMarkup = null, ParseMode parseMode = ParseMode.None)
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

}
