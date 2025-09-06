using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MGA.Mailing;

/// <summary>
/// Extension methods for registering <see cref="IEmailSender"/> in DI.
/// </summary>

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IEmailSender"/> with <see cref="SmtpEmailSender"/> implementation,
    /// binding settings from the specified configuration section.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="sectionName">The configuration section name (default: "YourSectionName").</param>
    public static IServiceCollection AddMailing(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "YourSectionName")
    {
        services.Configure<SmtpSettings>(configuration.GetSection(sectionName));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
