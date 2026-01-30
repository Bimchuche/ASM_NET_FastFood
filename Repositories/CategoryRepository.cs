using ASM1_NET.Data;
using ASM1_NET.Models;
using Microsoft.EntityFrameworkCore;

namespace ASM1_NET.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Category>> GetActiveAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> GetWithFoodsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Foods)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<int> GetFoodCountAsync(int categoryId)
        {
            return await _context.Foods
                .CountAsync(f => f.CategoryId == categoryId && f.IsAvailable);
        }

        public async Task<Category?> GetWithFoodsExplicitAsync(int id)
        {
            var category = await _dbSet.FindAsync(id);
            
            if (category != null)
            {
                await _context.Entry(category)
                    .Collection(c => c.Foods)
                    .LoadAsync();
            }
            
            return category;
        }
    }
}
