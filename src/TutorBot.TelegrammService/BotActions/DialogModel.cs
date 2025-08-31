
namespace TutorBot.TelegramService.BotActions;
public class DialogModel
{
    public required StartNodeModel Start { get; set; }
    public required HandlersModel Handlers { get; set; }
    public required MenuItem[] Menus { get; set; }


    public class StartNodeModel
    {
        public required string Handler { get; set; }
        public required string NextStep { get; set; }
    }

    public class HandlersModel
    {
        public required WelcomeHandler Welcome { get; set; }
        public required SimpleTextItem[] SimpleText { get; set; }
        public required YandexSearchTextItem[] YandexSearchText { get; set; }
    }

    public class WelcomeHandler
    {
        public required string WelcomeText { get; set; }
        public required string ErrorText { get; set; }

        public string? FullNameQuestion { get; set; }
        public string? FullNameError { get; set; }

        public required string[] GroupNumbers { get; set; }
    }

    public class SimpleTextItem
    {
        public required string Key { get; set; }
        public required string[] Text { get; set; } 
        public string GetText() => Text.JoinString(Environment.NewLine);
    }

    public class YandexSearchTextItem
    {
        public required string Key { get; set; }
        public required string Descriptions { get; set; }
        public string? Pattern { get; set; }
        public string? InvalidPatternMessage { get; set; }
        public required string[] Text { get; set; }
        public string GetText() => Text.JoinString(Environment.NewLine);
    }

    public class MenuItem
    {
        public required string Key { get; set; }
        public required string Text { get; set; }
        public required string[] Buttons { get; set; }
    }
}
