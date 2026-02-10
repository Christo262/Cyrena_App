using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ExampleNamespace.Contracts;
using ExampleNamespace.Services;
using ExampleNamespace.Options;

namespace ExampleNamespace.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.Configure<EmailOptions>(options => { }); // placeholder for configuration
            return services;
        }

        public static IServiceCollection AddEmailService(this IServiceCollection services, Action<EmailOptions> configure)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.Configure(configure);
            return services;
        }
    }
}
