using Docker.DotNet.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TutorBot.Abstractions;
using TutorBot.App.Components;
using TutorBot.Test.Helpers;
using TutorBot.Test.TestFramework;

namespace TutorBot.Test.Common;

[DatabaseSnapshotGroup]
public class ApplicationCoreTest(CustomAppFactory factory, ITestOutputHelper output) : IntegrationTestsBase
{
    [Fact]
    [TestPriority(1)]
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
    [TestPriority(1)]
    public async Task App_Error()
    {
        using (HttpClient client = await factory.CreateApplication())
        {
            IApplication app = factory.Services.GetRequiredService<IApplication>();

            if (TelegramBotFake.Instance._onError == null)
                throw new NullReferenceException("TelegramBotFake.Instance._onError");

            await EnsureChat(app, UserChatID, "user");
            await EnsureChat(app, AdminChatID, "admin", x => { x.IsAdmin = true; x.EnableAdminError = true; });
            await EnsureChat(app, AlternativeAdminChatID, "admin", x => { x.IsAdmin = true; });

            string message = $"fake exception {Guid.NewGuid()}";

            await TelegramBotFake.Instance._onError.Invoke(new Exception(message), Telegram.Bot.Polling.HandleErrorSource.FatalError);

            MessageHistory[] messages;

            messages = await app.HistoryService.GetMessages(AdminChatID, 0, 10, true);
            if (!messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("not found fake exception");

            messages = await app.HistoryService.GetMessages(AlternativeAdminChatID, 0, 10, true);
            if (messages.Any(x => x.MessageText.Contains(message)))
                Assert.Fail("found fake exception");

            messages = await app.HistoryService.GetMessages(UserChatID, 0, 10, true);
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

    private static long AdminChatID = 10;
    private static long AlternativeAdminChatID = 11;
    private static long UserChatID = 20;
}