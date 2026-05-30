using System.Text.Json.Serialization;

namespace TutorBot.TelegramService.BotActions;

[JsonSerializable(typeof(DialogModel))]
internal partial class JsonContext : JsonSerializerContext
{
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ScheduleAction.GroupInfo[]))]
internal partial class JsonCamelCaseContext : JsonSerializerContext
{
}
