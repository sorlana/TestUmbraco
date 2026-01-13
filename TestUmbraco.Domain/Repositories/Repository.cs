using TestUmbraco.Domain.Contracts;
using TestUmbraco.Domain.Models;

namespace TestUmbraco.Domain.Repositories
{
    public class Repository<T> : IRepository<T> where T : EntityBase
    {
        private readonly AppDbContext _dbContext;

        public Repository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(T entity)
        {
            await _dbContext.Set<T>().AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
