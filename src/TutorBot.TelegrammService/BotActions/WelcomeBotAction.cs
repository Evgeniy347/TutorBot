using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TutorBot.TelegramService.BotActions
{
    internal class WelcomeBotAction(TgBotServiceOptions opt) : IBotAction
    {
        public string Key => "Welcome";
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            if (client.ChatEntry.IsFirstMessage)
            {
                await client.SendMessage(
                    text: TextMessages.WelcomeMessage,
                    replyMarkup: new ReplyKeyboardRemove()
                );
                client.ChatEntry.IsFirstMessage = false;
            }
            else
            {
                if (opt.GroupNumbers.Contains(message.Text, StringComparer.OrdinalIgnoreCase))
                {
                    client.ChatEntry.GroupNumber = message.Text ?? string.Empty;

                    // Отправляем сообщение с кнопками 
                    await client.SendMessage(TextMessages.AskInterest, replyMarkup: BotActionHub.GetMainMenuKeyboard());
                }
                else
                {
                    await client.SendMessage(
                        text: TextMessages.ErrorGroupNumber,
                        replyMarkup: new ReplyKeyboardRemove()
                    );
                }
            }
        }
    }
}
