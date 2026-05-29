using System.Text.Json;

namespace TutorBot.TelegramService.BotActions;

internal class DialogModelLoader(string filePath)
{
    private readonly object _lockObject = new();
    private DateTimeOffset? _lastFileUpdateCheck;
    private DialogModel? _model;

    public DialogModel GetModel()
    {
        lock (_lockObject)
        {
            // Получаем последнюю отметку изменения файла
            DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);

            if (_model == null || !_lastFileUpdateCheck.HasValue || lastWriteTime > _lastFileUpdateCheck.Value)
            {
                string jsonString = File.ReadAllText(filePath);
                _model = JsonSerializer.Deserialize<DialogModel>(
                    jsonString,
                    new JsonSerializerOptions() { PropertyNamingPolicy = null })!;

                CheckStructure(_model);

                _lastFileUpdateCheck = lastWriteTime;
            }

            return _model!;
        }
    }

    private static void CheckStructure(DialogModel model)
    {
        // Проверяем уникальные ключи
        HashSet<string> allKeys = new HashSet<string>();

        foreach (DialogModel.SimpleTextItem item in model.Handlers.SimpleText)
        {
            if (!allKeys.Add(item.Key))
                throw new InvalidOperationException($"duplicate key '{item.Key}'");
        }

        foreach (DialogModel.YandexSearchTextItem item in model.Handlers.YandexSearchText)
        {
            if (!allKeys.Add(item.Key))
                throw new InvalidOperationException($"duplicate key '{item.Key}'");
        }

        foreach (DialogModel.MenuItem menu in model.Menus)
        {
            if (!allKeys.Add(menu.Key))
                throw new InvalidOperationException($"duplicate key '{menu.Key}'");
        }

        HashSet<string> keys = [
            ..model.Handlers.SimpleText.Select(x=>x.Key),
            ..model.Handlers.YandexSearchText.Select(x=>x.Key),
            ..model.Menus.Select(x=>x.Key),
            model.Handlers.Schedule.Key,
            model.Handlers.Welcome.Key
            ];

        // Проверяем наличие ключей для кнопок
        foreach (DialogModel.MenuItem menu in model.Menus)
        {
            foreach (string button in menu.Buttons)
            {
                if (!keys.Contains(button))
                {
                    throw new KeyNotFoundException(button);
                }
            }
        }
    }

}
