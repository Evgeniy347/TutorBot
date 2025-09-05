using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using TutorBot.TelegramService;
using TutorBot.TelegramService.BotActions;
using TutorBot.TelegramService.Helpers;
using TutorBot.Test.TestFramework;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.Test.Helpers;

internal class TestHelper(CustomAppFactory factory)
{
    public async Task CompleteWelcomeFlow(UserChatHelper chatHelper, DialogModel model, string? groupName = null)
    {
        MenuItem menu = model.Menus.Single(x => x.Key == "↩️ В главное меню");
        string fullName = "иванов иван иванович";
        string menuText = StringHelpers.ReplaceUserName(menu.Text, fullName);

        await chatHelper.SentTextWithCheck("/start", model.Handlers.Welcome.WelcomeText, []);
        await chatHelper.SentTextWithCheck(groupName ?? "РИ-421056", model.Handlers.Welcome.FullNameQuestion!, []);

        await chatHelper.SentTextWithCheck(fullName, menuText, menu.Buttons);
    }

    public UserChatHelper CreateRandomUser(string firstName)
    {
        User from = new User()
        {
            Id = UniqueRandomGenerator.Instance.NextUniqueInt64(),
            FirstName = firstName,
            LastName = "test LastName"
        };

        Chat chat = new Chat() { Id = UniqueRandomGenerator.Instance.NextUniqueInt64() };

        return new UserChatHelper()
        {
            Chat = chat,
            From = from,
        };
    }

    public DialogModel Model
    {
        get
        {
            IOptions<TgBotServiceOptions> opt = factory.Services.GetRequiredService<IOptions<TgBotServiceOptions>>();
            DialogModelLoader dialogLoader = new DialogModelLoader(opt.Value.DialogModelPath);
            return dialogLoader.GetModel();
        }
    }

    public DialogModel MainMenu
    {
        get
        {
            IOptions<TgBotServiceOptions> opt = factory.Services.GetRequiredService<IOptions<TgBotServiceOptions>>();
            DialogModelLoader dialogLoader = new DialogModelLoader(opt.Value.DialogModelPath);
            return dialogLoader.GetModel();
        }
    }
}
