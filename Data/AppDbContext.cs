using Microsoft.EntityFrameworkCore;
using ASM1_NET.Models;
namespace ASM1_NET.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<Combo> Combos { get; set; }
        public DbSet<ComboDetail> ComboDetails { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        
        // Activity Log for admin dashboard
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        
        // Reviews
        public DbSet<Review> Reviews { get; set; }
        
        // Coupons
        public DbSet<Coupon> Coupons { get; set; }
        
        // Wishlist
        public DbSet<Wishlist> Wishlists { get; set; }
        
        // User Addresses
        public DbSet<UserAddress> UserAddresses { get; set; }
        
        // Shipping Zones
        public DbSet<ShippingZone> ShippingZones { get; set; }
        
        // Loyalty Points
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        
        // Chat
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ComboDetail>()
                .HasKey(cd => new { cd.ComboId, cd.FoodId });

            // Order - Customer
            _ = modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - Shipper
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shipper)
                .WithMany()
                .HasForeignKey(o => o.ShipperId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chat - Customer
            modelBuilder.Entity<ChatSession>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chat - Admin
            modelBuilder.Entity<ChatSession>()
                .HasOne(c => c.Admin)
                .WithMany()
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatMessage - Sender
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision configurations
            modelBuilder.Entity<Food>()
                .Property(f => f.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Combo>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ShippingZone>()
                .Property(s => s.ShippingFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountPercent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinOrderAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MaxDiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CartItem>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);
        }
    }
}
