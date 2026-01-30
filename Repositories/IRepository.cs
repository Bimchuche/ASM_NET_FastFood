using System.Linq.Expressions;

namespace ASM1_NET.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        
        IQueryable<T> Query();
        
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        IQueryable<T> QueryNoTracking();
    }
}
