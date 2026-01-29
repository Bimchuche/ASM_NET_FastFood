using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models
{
    /// <summary>
    /// Model ghi lại hoạt động trong hệ thống Admin
    /// </summary>
    public class ActivityLog
    {
        public int Id { get; set; }

        /// <summary>
        /// Loại hành động: Login, Logout, Create, Update, Delete, SoftDelete, Restore
        /// </summary>
        [Required]
        [StringLength(50)]
        [Display(Name = "Hành động")]
        public string Action { get; set; } = "";

        /// <summary>
        /// Loại đối tượng: User, Food, Combo, Category, Order
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Loại đối tượng")]
        public string? EntityType { get; set; }

        /// <summary>
        /// ID của đối tượng bị tác động
        /// </summary>
        [Display(Name = "ID đối tượng")]
        public int? EntityId { get; set; }

        /// <summary>
        /// Tên đối tượng (để hiển thị khi đối tượng đã bị xóa)
        /// </summary>
        [StringLength(200)]
        [Display(Name = "Tên đối tượng")]
        public string? EntityName { get; set; }

        /// <summary>
        /// Mô tả chi tiết hành động
        /// </summary>
        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        /// <summary>
        /// ID người thực hiện
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Tên người thực hiện
        /// </summary>
        [StringLength(100)]
        [Display(Name = "Người thực hiện")]
        public string? UserName { get; set; }

        /// <summary>
        /// Vai trò người thực hiện
        /// </summary>
        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string? UserRole { get; set; }

        /// <summary>
        /// Thời gian thực hiện
        /// </summary>
        [Display(Name = "Thời gian")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Địa chỉ IP
        /// </summary>
        [StringLength(50)]
        [Display(Name = "IP")]
        public string? IpAddress { get; set; }

        /// <summary>
        /// Icon hiển thị theo loại action
        /// </summary>
        public string GetActionIcon()
        {
            return Action switch
            {
                "Login" => "🔐",
                "Logout" => "🚪",
                "Create" => "➕",
                "Update" => "✏️",
                "Delete" => "🗑️",
                "SoftDelete" => "📥",
                "Restore" => "♻️",
                "Order" => "📦",
                "Register" => "👤",
                _ => "📋"
            };
        }

        /// <summary>
        /// Màu badge theo loại action
        /// </summary>
        public string GetActionBadgeClass()
        {
            return Action switch
            {
                "Login" => "success",
                "Logout" => "secondary",
                "Create" => "primary",
                "Update" => "info",
                "Delete" => "danger",
                "SoftDelete" => "warning",
                "Restore" => "success",
                "Order" => "primary",
                "Register" => "info",
                _ => "secondary"
            };
        }
    }
}
