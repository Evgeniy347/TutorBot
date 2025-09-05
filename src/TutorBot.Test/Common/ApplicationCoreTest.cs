using Microsoft.Extensions.DependencyInjection;
using TutorBot.Abstractions;
using TutorBot.Test.Helpers;
using TutorBot.Test.TestFramework;

namespace TutorBot.Test.Common;


[DatabaseSnapshotGroup]
public class ApplicationCoreTest(CustomAppFactory factory, ITestOutputHelper output) : IntegrationTestsBase
{
    private readonly TestHelper _helper = new TestHelper(factory);

    [Fact]
    public async Task App_Health()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            using var response = await client.GetAsync("/health", TestContext.Current.CancellationToken);
            output.WriteLine($@"StatusCode {(int)response.StatusCode} {response.StatusCode}
{await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)}");

            if (!response.IsSuccessStatusCode)
            {
                Assert.NotNull(response.Content);

                string html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

                Assert.Fail(html);
            }
        }
    }

    [Fact]
    public async Task App_Error()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            IApplication app = factory.Services.GetRequiredService<IApplication>();

            if (TelegramBotFake.Instance._onError == null)
                throw new NullReferenceException("TelegramBotFake.Instance._onError");

            long adminChatID = UniqueRandomGenerator.Instance.NextUniqueInt64(), alternativeAdminChatID = UniqueRandomGenerator.Instance.NextUniqueInt64(), userChatID = UniqueRandomGenerator.Instance.NextUniqueInt64();

            await EnsureChat(app, userChatID, "user");
            await EnsureChat(app, adminChatID, "admin", x => { x.IsAdmin = true; x.EnableAdminError = true; });
            await EnsureChat(app, alternativeAdminChatID, "admin", x => { x.IsAdmin = true; });

            string message = $"fake exception {Guid.NewGuid()}";

            await TelegramBotFake.Instance._onError.Invoke(new Exception(message), Telegram.Bot.Polling.HandleErrorSource.FatalError);

            MessageHistory[] messages;

            messages = await app.HistoryService.GetMessages(adminChatID, int.MaxValue, 10, true);
            if (!messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("not found fake exception");

            messages = await app.HistoryService.GetMessages(alternativeAdminChatID, int.MaxValue, 10, true);
            if (messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("found fake exception");

            messages = await app.HistoryService.GetMessages(userChatID, int.MaxValue, 10, true);
            if (messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("found fake exception");
        }
    }

    private async Task<ChatEntry> EnsureChat(IApplication app, long chatID, string name, Action<ChatEntry>? update = null)
    {
        ChatEntry? chat = await app.ChatService.Find(chatID);

        if (chat == null)
            chat = await app.ChatService.Create(chatID, name, name, name, chatID);


        if (update != null)
        {
            update(chat);
            await app.ChatService.Update(chat);
        }

        return chat;
    }
}