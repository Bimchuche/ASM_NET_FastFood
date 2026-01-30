using System.ComponentModel.DataAnnotations;

namespace ASM1_NET.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Hành động")]
        public string Action { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "Loại đối tượng")]
        public string? EntityType { get; set; }

        [Display(Name = "ID đối tượng")]
        public int? EntityId { get; set; }

        [StringLength(200)]
        [Display(Name = "Tên đối tượng")]
        public string? EntityName { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = "";

        public int? UserId { get; set; }

        [StringLength(100)]
        [Display(Name = "Người thực hiện")]
        public string? UserName { get; set; }

        [StringLength(50)]
        [Display(Name = "Vai trò")]
        public string? UserRole { get; set; }

        [Display(Name = "Thời gian")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [StringLength(50)]
        [Display(Name = "IP")]
        public string? IpAddress { get; set; }

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
