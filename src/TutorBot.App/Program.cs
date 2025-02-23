using System.Text.Json.Serialization;
using TutorBot.Authentication;
using TutorBot.Abstractions;
using TutorBot.Core;
using TutorBot.Frontend; 
using TutorBot.TelegramService;

namespace TutorBot.App
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Console.InputEncoding = System.Text.Encoding.UTF8;

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions()
            {
                Args = args,
                ApplicationName = "TutorBot.App"
            });

             
            var services = builder.Services;

            builder.AddServiceDefaults();

            services.AddFrontendAuthentication();

            services.AddApplicationCore(builder.Configuration);

            services.AddFrontend();

            services.AddTelegramService(builder.Configuration);
             
            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            { 
            }

            app.AddFrontend<TutorBot.App.Components.App>();

            app.UseHttpsRedirection();

            await app.RunAsync();
        }
    }
}