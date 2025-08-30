using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions
{
    internal class WelcomeBotAction(DialogModel model) : IBotAction
    {
        public string Key => "Welcome";
        public bool EnableProlongated => false;

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            WelcomeHandler welcomeHandler = model.Handlers.Welcome;

            if (client.ChatEntry.IsFirstMessage)
            {
                await client.SendMessage(
                    text: welcomeHandler.WelcomeText,
                    replyMarkup: new ReplyKeyboardRemove()
                );
                client.ChatEntry.IsFirstMessage = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(client.ChatEntry.GroupNumber) &&
                    string.IsNullOrEmpty(client.ChatEntry.FullName) &&
                    !string.IsNullOrEmpty(welcomeHandler.FullNameQuestion))
                {
                    bool isValid = Regex.IsMatch(message.Text!, "^[а-яА-Я\\s]+$");

                    if (!isValid)
                    {
                        await client.SendMessage(welcomeHandler.FullNameError!);
                    }
                    else
                    {
                        client.ChatEntry.FullName = message.Text!;
                        await client.App.ChatService.Update(client.ChatEntry);
                    }

                    return;
                }

                if (welcomeHandler.GroupNumbers.Contains(message.Text?.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    client.ChatEntry.GroupNumber = message.Text ?? string.Empty;
                    await client.App.ChatService.Update(client.ChatEntry);

                    if (string.IsNullOrEmpty(client.ChatEntry.FullName) && !string.IsNullOrEmpty(welcomeHandler.FullNameQuestion))
                    {
                        await client.SendMessage(
                            text: welcomeHandler.FullNameQuestion,
                            replyMarkup: new ReplyKeyboardRemove()
                        );
                    }
                    else
                    {
                        IBotAction action = BotActionHub.FindHandler(model, model.Start.NextStep, true)!;
                        await action.ExecuteAsync(message, client);
                    }
                }
                else
                {
                    await client.SendMessage(
                        text: welcomeHandler.ErrorText,
                        replyMarkup: new ReplyKeyboardRemove()
                    );
                }
            }
        }
    }
}
