using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TutorBot.TelegramService;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTelegramService(this IServiceCollection services, IConfigurationManager configuration)
    {
        services.Configure<TgBotServiceOptions>(configuration.GetSection("TelegramService"));
        services.AddHostedService<TelegramBotService>();

        return services;
    }
}
