using Telegram.Bot.Types;

namespace TutorBot.TelegramService.BotActions.Admins
{
    internal class AdminBotAction : IBotAction
    {
        public string Key => "/admin";
        public bool EnableProlongated => true;

        private readonly Dictionary<long, int> _attemptsCont = new Dictionary<long, int>();

        public async Task ExecuteAsync(Message message, TutorBotContext client)
        {
            if (!client.ChatEntry.IsAdmin)
            {
                _attemptsCont.TryGetValue(client.ChatEntry.UserID, out int count);
                count++;
                 
                if (count > 10)
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
                        if (client.Opt.AdminKey == message.Text)
                        {
                            await client.SendMessage("Теперь вы администратор", replyMarkup: BotActionHub.GetAdminMenuKeyboard());
                            client.ChatEntry.IsAdmin = true;
                            count = 0;
                        }
                        else
                            await client.SendMessage("Код досутпа введен с ошибкой");
                    }
                }

                _attemptsCont[client.ChatEntry.UserID] = count;
            }
            else
            {
                await client.SendMessage("Выберите опцию:", replyMarkup: BotActionHub.GetAdminMenuKeyboard());
            }
        }
    }
}
