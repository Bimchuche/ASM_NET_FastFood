using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Repositories
{
    public class FoodRepository : Repository<Food>, IFoodRepository
    {
        public FoodRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(f => f.Category)
                .Where(f => f.CategoryId == categoryId && f.IsAvailable)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Food>> GetAvailableFoodsAsync()
        {
            return await _dbSet
                .Where(f => f.IsAvailable)
                .Include(f => f.Category)
                .OrderBy(f => f.Name)
                .AsNoTracking()
                .ToListAsync();
        }

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

        public async Task<Food?> GetWithCategoryAsync(int id)
        {
            var food = await _dbSet.FindAsync(id);
            
            if (food != null)
            {
                await _context.Entry(food)
                    .Reference(f => f.Category)
                    .LoadAsync();
            }
            
            return food;
        }

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
