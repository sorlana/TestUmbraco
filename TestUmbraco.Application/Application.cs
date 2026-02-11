using TestUmbraco.Application.Contracts;
using TestUmbraco.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace TestUmbraco.Application
{
    public static class Application
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFormSubmissionService, FormSubmissionService>();
            return services;
        }
    }
}
