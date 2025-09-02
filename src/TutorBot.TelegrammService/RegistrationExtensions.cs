using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TutorBot.TelegramService;

public static class RegistrationExtensions
{
    public static IServiceCollection AddTelegramService(this IServiceCollection services, IConfigurationManager configuration)
    {
        IConfigurationSection section = configuration.GetSection("TelegramService");
        services.Configure<TgBotServiceOptions>(section);
        TgBotServiceOptions? tgBotConfig = section.Get<TgBotServiceOptions>();

        services.AddTransient<Func<string, CancellationToken, ITelegramBot>>(provider =>
            (token, cancellationToken) => new TelegramBot(token, cancellationToken: cancellationToken));

        if (tgBotConfig != null && tgBotConfig.Enable)
        {
            services.AddHostedService<TelegramBotService>();
        }

        return services;
    }
}
