using Emax.Application.Contracts;
using Emax.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Emax.Application
{
    public static class Application
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailService, EmailService>();
            return services;
        }
    }
}
