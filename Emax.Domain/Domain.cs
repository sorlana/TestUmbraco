using Emax.Domain.Contracts;
using Emax.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Emax.Domain
{
    public static class DomainModule
    {
        public static IServiceCollection AddDomain(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(x => x.UseSqlServer(config.GetConnectionString("EmaxData")));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }
    }
}
