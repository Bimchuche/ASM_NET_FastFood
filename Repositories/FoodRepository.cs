using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Food Repository Implementation
    /// Demo: Eager Loading, Explicit Loading, LINQ to Entities, Query Performance
    /// </summary>
    public class FoodRepository : Repository<Food>, IFoodRepository
    {
        public FoodRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Eager Loading - Include Category
        /// </summary>
        public async Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(f => f.Category)  // ✅ Eager Loading
                .Where(f => f.CategoryId == categoryId && f.IsAvailable)
                .AsNoTracking()            // ✅ Query Performance
                .ToListAsync();
        }

        /// <summary>
        /// Query Performance - AsNoTracking + Filtered
        /// </summary>
        public async Task<IEnumerable<Food>> GetAvailableFoodsAsync()
        {
            return await _dbSet
                .Where(f => f.IsAvailable)
                .Include(f => f.Category)  // Eager Loading
                .OrderBy(f => f.Name)
                .AsNoTracking()            // Performance: read-only
                .ToListAsync();
        }

        /// <summary>
        /// LINQ to Entities - Search với EF.Functions.Like
        /// </summary>
        public async Task<IEnumerable<Food>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetAvailableFoodsAsync();

            return await _dbSet
                .Where(f => EF.Functions.Like(f.Name, $"%{keyword}%")
                         || EF.Functions.Like(f.Description ?? "", $"%{keyword}%"))
                .Include(f => f.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Explicit Loading - Load navigation property khi cần
        /// </summary>
        public async Task<Food?> GetWithCategoryAsync(int id)
        {
            var food = await _dbSet.FindAsync(id);
            
            if (food != null)
            {
                // ✅ Explicit Loading - chỉ load khi cần
                await _context.Entry(food)
                    .Reference(f => f.Category)
                    .LoadAsync();
            }
            
            return food;
        }

        /// <summary>
        /// Query Performance - Select chỉ cần thiết
        /// </summary>
        public async Task<IEnumerable<Food>> GetTopFoodsAsync(int count)
        {
            return await _dbSet
                .Where(f => f.IsAvailable)
                .OrderByDescending(f => f.Price)
                .Take(count)
                .Include(f => f.Category)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
