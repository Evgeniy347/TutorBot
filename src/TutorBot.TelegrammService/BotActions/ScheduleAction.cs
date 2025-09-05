using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TutorBot.TelegramService.Helpers;
using static TutorBot.TelegramService.BotActions.DialogModel;

namespace TutorBot.TelegramService.BotActions;

internal class ScheduleAction(DialogModel model) : IBotAction
{
    public string Key => model.Handlers.Schedule.Key;
    public static string BaseUrl { get; set; } = "https://urfu.ru/";

    public bool EnableProlongated => false;

    private readonly ConcurrentDictionary<string, ulong> _groupID = new ConcurrentDictionary<string, ulong>();

    public async Task ExecuteAsync(Message message, TutorBotContext client)
    {
        MenuItem? menu = model.Menus.FirstOrDefault(x => x.Buttons.Contains(Key));

        if (menu == null)
        {
            await client.WriteError($"not found menu '{Key}'");
            return;
        }

        ReplyKeyboardMarkup replyMarkup = menu.Buttons.Select(x => new[] { new KeyboardButton(x) }).ToArray();

        string sourceText = model.Handlers.Schedule.GetText();

        string resultText = StringHelpers.ReplaceUserName(sourceText, client.ChatEntry.FullName);

        if (resultText.Contains("#URL#"))
        {
            if (!_groupID.TryGetValue(client.ChatEntry.GroupNumber, out ulong groupID))
            {
                GroupInfo[] groups = [];
                try
                {
                    groups = await GetGroupsAsync(client.ChatEntry.GroupNumber, client.Token);
                    groupID = groups.Single(x => client.ChatEntry.GroupNumber.Equals(x.Title, StringComparison.OrdinalIgnoreCase)).Id;
                    _groupID[client.ChatEntry.GroupNumber] = groupID;
                }
                catch (Exception ex)
                {
                    string strGroups = groups.Select(x => x.Title).JoinString();
                    await client.ErrorHandle(ex, $@"
Search:'{client.ChatEntry.GroupNumber}'
Groups:'{groups.Select(x => x.Title).JoinString("', '")}'
");
                }
            }

            string url;

            if (groupID != 0)
                url = $"{BaseUrl}ru/students/study/schedule/#/groups/{groupID}";
            else
                url = $"{BaseUrl}ru/students/study/schedule/";

            resultText = resultText.Replace("#URL#", url);
        }

        await client.SendMessage(resultText, replyMarkup: replyMarkup, parseMode: ParseMode.Html);
    }

    public static HttpClient Client { get; internal set; } = new HttpClient();

    public static async Task<GroupInfo[]> GetGroupsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        string encodedSearch = Uri.EscapeDataString(searchTerm);
        string url = $"{BaseUrl}api/v2/schedule/groups?search={encodedSearch}";

        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        HttpResponseMessage response = await Client.GetAsync(url, linkedCts.Token);

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(linkedCts.Token);
        GroupInfo[]? groups = JsonSerializer.Deserialize<GroupInfo[]>(json, DefaultOptions);

        return groups ?? [];
    }

    public static JsonSerializerOptions DefaultOptions => new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public record GroupInfo(ulong Id, ulong DivisionId, ulong Course, string? Title);
}
