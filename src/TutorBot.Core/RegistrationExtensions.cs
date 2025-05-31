using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorBot.Abstractions;

namespace TutorBot.Core
{
    public static class RegistrationExtensions
    {
        public static IServiceCollection AddApplicationCore(this IServiceCollection services, IConfigurationManager configuration)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

            string? connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));  
            services.AddSingleton<IApplication, ApplicationCore>();

            IConfigurationSection sectionGigaChat = configuration.GetSection("GigaChat");
            services.Configure<GigaChatOptions>(sectionGigaChat);

            IConfigurationSection sectionYandexSearch = configuration.GetSection("YandexSearch");
            services.Configure<YandexSearchOptions>(sectionYandexSearch);

            return services;
        }
    }
}
