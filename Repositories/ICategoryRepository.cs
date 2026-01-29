using ASM1_NET.Models;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Category Repository Interface
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Lấy danh sách active - Query Performance
        /// </summary>
        Task<IEnumerable<Category>> GetActiveAsync();

        /// <summary>
        /// Lấy Category với Foods - Eager Loading
        /// </summary>
        Task<Category?> GetWithFoodsAsync(int id);

        /// <summary>
        /// Đếm số Foods trong Category
        /// </summary>
        Task<int> GetFoodCountAsync(int categoryId);
    }
}
