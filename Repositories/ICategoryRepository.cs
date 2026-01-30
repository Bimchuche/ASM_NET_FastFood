using ASM1_NET.Models;

namespace ASM1_NET.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveAsync();

        Task<Category?> GetWithFoodsAsync(int id);

        Task<int> GetFoodCountAsync(int categoryId);

        Task<Category?> GetWithFoodsExplicitAsync(int id);
    }
}
