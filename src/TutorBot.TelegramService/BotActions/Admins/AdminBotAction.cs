using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Collections.Concurrent;
using Telegram.Bot.Types;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class AdminBotAction : IBotAction
    {
        internal static ulong MaxCount => 10; 
        public string Key => "/admin";
        public bool EnableProlongated => true;

        private static readonly ConcurrentDictionary<long, ulong> _attemptsCount = new ConcurrentDictionary<long, ulong>();

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            if (!client.ChatEntry.IsAdmin)
            {
                ulong count = _attemptsCount.AddOrUpdate(client.ChatEntry.UserID, 0, (x, y) => y + 1);

                if (count > MaxCount)
                {
                    await client.SendMessage("Вам запрещено вводить код доступа");
                }
                else
                {
                    if (message.Text == Key)
                    {
                        await client.SendMessage("Введите код доступа");
                    }
                    else
                    { 
                        object resultKey = await CSharpScript.EvaluateAsync(client.Opt.EvaluateKey);

                        if (resultKey?.ToString() == message.Text)
                        {
                            await client.SendMessage("Теперь вы администратор", replyMarkup: BotActionHub.GetAdminMenuKeyboard());
                            client.ChatEntry.IsAdmin = true;
                            count = 0;
                        }
                        else
                            await client.SendMessage("Код доступа введен с ошибкой");
                    }
                }
            }
            else
            {
                await client.SendMessage("Выберите опцию:", replyMarkup: BotActionHub.GetAdminMenuKeyboard());
            }
        }
    }
}
