using System.Linq.Expressions;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Generic Repository Interface - Dependency Injection
    /// </summary>
    public interface IRepository<T> where T : class
    {
        // LINQ to Entities - Basic CRUD
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        
        // Query Performance - IQueryable cho flexible queries
        IQueryable<T> Query();
        
        // LINQ to Entities - với điều kiện
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Query Performance - AsNoTracking
        IQueryable<T> QueryNoTracking();
    }
}
