using ASM1_NET.Models;

namespace ASM1_NET.Repositories
{
    /// <summary>
    /// Food Repository Interface - Specialized queries
    /// </summary>
    public interface IFoodRepository : IRepository<Food>
    {
        /// <summary>
        /// Lấy Food theo Category - Eager Loading
        /// </summary>
        Task<IEnumerable<Food>> GetByCategoryAsync(int categoryId);

        /// <summary>
        /// Lấy tất cả Food đang bán - Query Performance
        /// </summary>
        Task<IEnumerable<Food>> GetAvailableFoodsAsync();

        /// <summary>
        /// Tìm kiếm Food - LINQ to Entities
        /// </summary>
        Task<IEnumerable<Food>> SearchAsync(string keyword);

        /// <summary>
        /// Lấy Food với Category - Explicit Loading
        /// </summary>
        Task<Food?> GetWithCategoryAsync(int id);

        /// <summary>
        /// Lấy danh sách phổ biến - Top N
        /// </summary>
        Task<IEnumerable<Food>> GetTopFoodsAsync(int count);
    }
}
