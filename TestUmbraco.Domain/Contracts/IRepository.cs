using TestUmbraco.Domain.Models;

namespace TestUmbraco.Domain.Contracts
{
    public interface IRepository<T> where T : EntityBase
    {
        Task AddAsync(T entity);
    }
}
