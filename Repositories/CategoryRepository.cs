using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Category Repository Implementation
    /// </summary>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Query Performance - Filtered + AsNoTracking
        /// </summary>
        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Eager Loading - Include Foods
        /// </summary>
        public async Task<Category?> GetWithFoodsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Foods)  // ✅ Eager Loading
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// LINQ to Entities - Count
        /// </summary>
        public async Task<int> GetFoodCountAsync(int categoryId)
        {
            return await _context.Foods
                .CountAsync(f => f.CategoryId == categoryId && f.IsAvailable);
        }
    }
}
