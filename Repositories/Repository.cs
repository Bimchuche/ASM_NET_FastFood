using System.Linq.Expressions;
using ASM1_NET.Data;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Generic Repository Implementation - Dependency Injection
    /// Implements common CRUD operations với EF Core
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// Lấy tất cả records - LINQ to Entities
        /// </summary>
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// Lấy theo ID
        /// </summary>
        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// Thêm mới entity
        /// </summary>
        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// Cập nhật entity
        /// </summary>
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Xóa theo ID
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// IQueryable cho flexible queries - Eager/Explicit Loading
        /// </summary>
        public IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        /// <summary>
        /// Query Performance - AsNoTracking cho read-only
        /// </summary>
        public IQueryable<T> QueryNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        /// <summary>
        /// LINQ to Entities - Find với Expression
        /// </summary>
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }
    }
}
