using TutorBot.Abstractions;
using TutorBot.Authentication;
using TutorBot.Core;
using TutorBot.Frontend;
using TutorBot.TelegramService;

Console.OutputEncoding = Console.InputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ApplicationName = "TutorBot.App"
});

var services = builder.Services;

builder.Configuration.AddJsonFile("appsettings.json");
builder.Configuration.AddJsonFile("appsettings.private.json", true);

builder.AddServiceDefaults();

services.AddFrontendAuthentication();
services.AddFrontend();

services.AddApplicationCore(builder.Configuration);

services.AddTelegramService(builder.Configuration);

var app = builder.Build();

var alServise = app.Services.GetRequiredService<IApplication>();

await alServise.ALService.TransferQuestionAL(123, "C# Lang", new Guid());

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.AddFrontend<TutorBot.App.Components.App>();

app.UseHttpsRedirection();

await app.RunAsync();
