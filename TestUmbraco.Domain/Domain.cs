using TestUmbraco.Domain.Contracts;
using TestUmbraco.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TestUmbraco.Domain
{
    public static class DomainModule
    {
        public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(x => x.UseSqlServer(config.GetConnectionString("TestUmbracoData")));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IFormSubmissionRepository, FormSubmissionRepository>();
            return services;
        }
    }
}
