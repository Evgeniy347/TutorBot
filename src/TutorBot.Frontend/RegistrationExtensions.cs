using Microsoft.Extensions.FileProviders;
using Radzen;

namespace TutorBot.Frontend;

public static class RegistrationExtensions
{
    public static IServiceCollection AddFrontend(this IServiceCollection services)
    {
        services
            .AddRadzenComponents()
            .AddRazorComponents()
            .AddInteractiveServerComponents();

        return services;
    }

    public static void AddFrontend<TApp>(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new ManifestEmbeddedFileProvider(typeof(RegistrationExtensions).Assembly, "wwwroot")
        });

        app.UseAntiforgery();

        app.MapStaticAssets();

        app.MapRazorComponents<TApp>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(RegistrationExtensions).Assembly);

    }
}
