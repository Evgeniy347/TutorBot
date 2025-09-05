using Shouldly;
using System.Runtime.CompilerServices;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace TutorBot.Test.Helpers;

internal class UserChatHelper
{
    public required Chat Chat { get; init; }
    public required User From { get; init; }

    public async Task SentText(string text)
    {
        if (TelegramBotFake.Instance._onMessage == null)
            throw new NullReferenceException("TelegramBotFake.Instance._onMessage");

        Message message = new Message()
        {
            Text = text,
            From = From,
            Chat = Chat,
        };

        await TelegramBotFake.Instance._onMessage.Invoke(message, Telegram.Bot.Types.Enums.UpdateType.Message);
    }

    public async Task SentTextWithCheck(string text, string textResult, string[]? buttons = null, [CallerArgumentExpression(nameof(textResult))] string valueTitle = "")
    {
        await SentText(text);
        SendMessageArgs sendResult = TelegramBotFake.Instance.SendingMessage.First(x => x.chatId == Chat.Id);

        string comment = $@"text:{text} 
{valueTitle}:{textResult}";

        sendResult.text.ShouldBe(textResult, comment);
        sendResult.parseMode.ShouldBe(Telegram.Bot.Types.Enums.ParseMode.Html, comment);

        if (buttons == null)
            sendResult.replyMarkup.ShouldBeNull(comment);
        else if (buttons.Length == 0)
            sendResult.replyMarkup.ShouldBeOfType<ReplyKeyboardRemove>(comment);
        else
        {
            string[] sendButtons = ((ReplyKeyboardMarkup)Check.NotNull(sendResult.replyMarkup, valueTitle)).Keyboard.SelectMany(x => x).Select(x => x.Text).ToArray();
            sendButtons.ShouldBeEquivalentTo(buttons!, comment);
        }
    }
}
