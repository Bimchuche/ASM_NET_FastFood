using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASM1_NET.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [Required]
    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public virtual ChatSession? Session { get; set; }

    [Required]
    public int SenderId { get; set; }

    [ForeignKey("SenderId")]
    public virtual User? Sender { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = "";

    public bool IsFromCustomer { get; set; } = true;

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class ChatSession
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [ForeignKey("CustomerId")]
    public virtual User? Customer { get; set; }

    public int? AdminId { get; set; }

    [ForeignKey("AdminId")]
    public virtual User? Admin { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Open"; // Open, Closed

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? ClosedAt { get; set; }

    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
