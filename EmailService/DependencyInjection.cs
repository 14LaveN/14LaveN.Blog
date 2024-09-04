using EmailService.Emails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Email.Emails;
using Email.Emails.Settings;

namespace EmailService;

public static class DependencyInjection
{
    public static IServiceCollection AddEmailService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
    
        services.Configure<MailSettings>(configuration.GetSection(MailSettings.SettingsKey));
        
        services.AddOptions<MailSettings>()
            .BindConfiguration(MailSettings.SettingsKey)
            .ValidateOnStart();

        services
            .AddScoped<IEmailService, EmailService.Emails.EmailService>()
            .AddScoped<IEmailNotificationService, EmailNotificationService>();
        
        return services;
    }
}