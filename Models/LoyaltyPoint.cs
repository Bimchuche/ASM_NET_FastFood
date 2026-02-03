using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models;

public class LoyaltyPoint
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public int? OrderId { get; set; }

    [ForeignKey("OrderId")]
    public virtual Order? Order { get; set; }

    [Required]
    public int Points { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "Earn"; // Earn, Redeem, Bonus, Expired

    [StringLength(200)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired { get; set; } = false;
}
