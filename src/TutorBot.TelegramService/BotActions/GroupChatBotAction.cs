using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TutorBot.TelegramService.BotActions
{
    internal class GroupChatBotAction() : IBotAction
    {
        public string Key => "GroupChatBotAction";

        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            if (string.IsNullOrEmpty(message.Text) || message.From == null)
                return;

            string? answer = null;

            if (client.ChatEntry.IsFirstMessage)
            {
                answer = await client.App.ALService.AskAssistant(TextPromts.WelcomeGroup);
                client.ChatEntry.IsFirstMessage = false;
            }
            else
            {
                answer = await client.App.ALService.AskAssistant(client.ChatEntry.ChatID, message.From.Id, message.Text, client.ChatEntry.SessionID);
            }

            if (!string.IsNullOrEmpty(answer))
            {
                var answerEscape = TelegramMarkdownHelper.EscapeMarkdownV2(answer);
                await client.SendMessage(
                    text: answerEscape,
                    parseMode: ParseMode.MarkdownV2
                );
            }
        }
    }
}
