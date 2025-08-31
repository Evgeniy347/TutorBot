using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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
                    replyMarkup: new ReplyKeyboardRemove(),
                    parseMode: ParseMode.Html
                );
                client.ChatEntry.IsFirstMessage = false;
                await client.App.ChatService.Update(client.ChatEntry);
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
                        await client.SendMessage(welcomeHandler.FullNameError!,
                            parseMode: ParseMode.Html);
                    }
                    else
                    {
                        client.ChatEntry.FullName = message.Text!;
                        await client.App.ChatService.Update(client.ChatEntry);
                    }

                    return;
                }

                string[] expandNumbers = ExpandNumbers(welcomeHandler.GroupNumbers);

                if (expandNumbers.Contains(message.Text?.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    client.ChatEntry.GroupNumber = message.Text ?? string.Empty;
                    await client.App.ChatService.Update(client.ChatEntry);

                    if (string.IsNullOrEmpty(client.ChatEntry.FullName) && !string.IsNullOrEmpty(welcomeHandler.FullNameQuestion))
                    {
                        await client.SendMessage(
                            text: welcomeHandler.FullNameQuestion,
                            replyMarkup: new ReplyKeyboardRemove(),
                            parseMode: ParseMode.Html
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
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.Html
                    );
                }
            }
        }

        internal static string[] ExpandNumbers(string[] inputList)
        {
            List<string> outputList = new List<string>();

            foreach (string str in inputList)
            {
                string[] parts = str.Split('/');

                outputList.Add(parts[0]);

                if (parts.Length > 1)
                {
                    string source = parts[0];
                    foreach (string numPart in parts.Skip(1))
                    {
                        string group = source.Remove(source.Length - numPart.Length) + numPart;
                        outputList.Add(group);
                    }
                }
            }

            return [.. outputList];
        }
    }
}
