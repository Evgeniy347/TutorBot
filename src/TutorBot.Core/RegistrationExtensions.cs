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

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                string? connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                    throw new NullReferenceException("connectionString");
                options.UseNpgsql(connectionString);
            });

            services.AddSingleton<IApplication, ApplicationCore>();

            IConfigurationSection section = configuration.GetSection("GigaChat");
            services.Configure<GigaChatOptions>(section);

            return services;
        }
    }
}
