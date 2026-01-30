using ASM1_NET.Models;

namespace ASM1_NET.Repositories
{
    public interface IFoodRepository : IRepository<Food>
    {
        Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId);

        Task<IEnumerable<Food>> GetAvailableFoodsAsync();

        Task<IEnumerable<Food>> SearchAsync(string keyword);

        Task<Food?> GetWithCategoryAsync(int id);

        Task<IEnumerable<Food>> GetTopFoodsAsync(int count);
    }
}
