using Shouldly;
using TutorBot.TelegramService.BotActions;

namespace TutorBot.Test.BotActions;

[Trait("Category", "Unit")]
public class DialogModelLoaderTests
{
    private static string CreateTempFile(string jsonContent)
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        File.WriteAllText(path, jsonContent);
        return path;
    }

    private static string ValidJson => /*lang=json*/ """
    {
        "Start": { "Handler": "start_handler", "NextStep": "main_menu" },
        "Handlers": {
            "Welcome": { "Key": "/start", "WelcomeText": "Welcome!", "ErrorText": "Error!", "GroupNumbers": ["101"] },
            "Schedule": { "Key": "Schedule", "Text": ["schedule text"] },
            "SimpleText": [
                { "Key": "Help", "Text": ["help text line 1", "help text line 2"] }
            ],
            "YandexSearchText": []
        },
        "Menus": [
            { "Key": "MainMenu", "Text": "Choose option:", "Buttons": ["Help", "Schedule"] }
        ]
    }
    """;

    [Fact]
    public void LoadModel_ValidJson_ReturnsModel()
    {
        string path = CreateTempFile(ValidJson);
        try
        {
            DialogModelLoader loader = new DialogModelLoader(path);
            DialogModel model = loader.GetModel();

            model.ShouldNotBeNull();
            model.Start.Handler.ShouldBe("start_handler");
            model.Start.NextStep.ShouldBe("main_menu");
            model.Handlers.Welcome.WelcomeText.ShouldBe("Welcome!");
            model.Handlers.Schedule.Key.ShouldBe("Schedule");
            model.Handlers.SimpleText.ShouldHaveSingleItem();
            model.Handlers.SimpleText[0].Key.ShouldBe("Help");
            model.Menus.ShouldHaveSingleItem();
            model.Menus[0].Key.ShouldBe("MainMenu");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LoadModel_CachesModel_OnSecondCall()
    {
        string path = CreateTempFile(ValidJson);
        try
        {
            DialogModelLoader loader = new DialogModelLoader(path);

            DialogModel model1 = loader.GetModel();
            DialogModel model2 = loader.GetModel();

            model1.ShouldBeSameAs(model2);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LoadModel_Reloads_WhenFileChanges()
    {
        string path = CreateTempFile(ValidJson);
        try
        {
            DialogModelLoader loader = new DialogModelLoader(path);
            DialogModel model1 = loader.GetModel();

            string updatedJson = /*lang=json*/ """
            {
                "Start": { "Handler": "updated_handler", "NextStep": "updated_menu" },
                "Handlers": {
                    "Welcome": { "Key": "/start", "WelcomeText": "Updated!", "ErrorText": "Error!", "GroupNumbers": ["101"] },
                    "Schedule": { "Key": "Schedule", "Text": ["schedule text"] },
                    "SimpleText": [],
                    "YandexSearchText": []
                },
                "Menus": [
                    { "Key": "MainMenu", "Text": "Choose option:", "Buttons": ["Schedule"] }
                ]
            }
            """;

            File.WriteAllText(path, updatedJson);
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(1));

            DialogModel model2 = loader.GetModel();

            model2.ShouldNotBeSameAs(model1);
            model2.Start.Handler.ShouldBe("updated_handler");
            model2.Handlers.Welcome.WelcomeText.ShouldBe("Updated!");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LoadModel_DuplicateSimpleTextKey_Throws()
    {
        string json = /*lang=json*/ """
        {
            "Start": { "Handler": "s", "NextStep": "n" },
            "Handlers": {
                "Welcome": { "Key": "/start", "WelcomeText": "W", "ErrorText": "E", "GroupNumbers": ["101"] },
                "Schedule": { "Key": "Sched", "Text": ["t"] },
                "SimpleText": [
                    { "Key": "dup", "Text": ["first"] },
                    { "Key": "dup", "Text": ["second"] }
                ],
                "YandexSearchText": []
            },
            "Menus": [
                { "Key": "M", "Text": "T", "Buttons": ["Sched"] }
            ]
        }
        """;
        string path = CreateTempFile(json);
        try
        {
            DialogModelLoader loader = new DialogModelLoader(path);

            Should.Throw<InvalidOperationException>(() => loader.GetModel())
                .Message.ShouldContain("dup");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LoadModel_DuplicateMenuKey_Throws()
    {
        string json = /*lang=json*/ """
        {
            "Start": { "Handler": "s", "NextStep": "n" },
            "Handlers": {
                "Welcome": { "Key": "/start", "WelcomeText": "W", "ErrorText": "E", "GroupNumbers": ["101"] },
                "Schedule": { "Key": "Sched", "Text": ["t"] },
                "SimpleText": [],
                "YandexSearchText": []
            },
            "Menus": [
                { "Key": "dup", "Text": "First", "Buttons": [] },
                { "Key": "dup", "Text": "Second", "Buttons": [] }
            ]
        }
        """;
        string path = CreateTempFile(json);
        try
        {
            DialogModelLoader loader = new DialogModelLoader(path);

            Should.Throw<InvalidOperationException>(() => loader.GetModel())
                .Message.ShouldContain("dup");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void LoadModel_MissingButtonKey_Throws()
    {
        string json = /*lang=json*/ """
        {
            "Start": { "Handler": "s", "NextStep": "n" },
            "Handlers": {
                "Welcome": { "Key": "/start", "WelcomeText": "W", "ErrorText": "E", "GroupNumbers": ["101"] },
                "Schedule": { "Key": "Sched", "Text": ["t"] },
                "SimpleText": [],
                "YandexSearchText": []
            },
            "Menus": [
                { "Key": "M", "Text": "T", "Buttons": ["NonExistent"] }
            ]
        }
        """;
        string path = CreateTempFile(json);
        try
        {
            DialogModelLoader loader = new DialogModelLoader(path);

            Should.Throw<KeyNotFoundException>(() => loader.GetModel())
                .Message.ShouldBe("NonExistent");
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
