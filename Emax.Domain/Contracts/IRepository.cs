using Emax.Domain.Models;

namespace Emax.Domain.Contracts
{
    public interface IRepository<T> where T : EntityBase
    {
        Task AddAsync(T entity);
    }
}
