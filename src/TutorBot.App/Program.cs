using TutorBot.Authentication;
using TutorBot.Core;
using TutorBot.Frontend;
using TutorBot.TelegramService;

namespace TutorBot.App;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Console.InputEncoding = System.Text.Encoding.UTF8;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
        {
            Args = args,
            ApplicationName = "TutorBot.App"
        });

        var services = builder.Services;

        if (!AppContext.TryGetSwitch("DisableLoadConfig", out bool isDisableLoadConfig) || !isDisableLoadConfig)
        {
            builder.Configuration.AddJsonFile("appsettings.json");

            if (File.Exists("appsettings.private.json"))
                builder.Configuration.AddJsonFile("appsettings.private.json");
        }

        builder.AddServiceDefaults();

        services.AddFrontendAuthentication();
        services.AddFrontend();

        services.AddApplicationCore(builder.Configuration);

        services.AddTelegramService(builder.Configuration);

        var app = builder.Build();

        app.MapDefaultEndpoints();

        if (app.Environment.IsDevelopment())
        {
        }

        app.AddFrontend<TutorBot.App.Components.App>();

        app.UseHttpsRedirection();

        await app.RunAsync();
    }
}