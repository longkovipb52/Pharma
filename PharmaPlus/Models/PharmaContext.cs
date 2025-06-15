using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace PharmaPlus.Models
{
    public class PharmaContext : DbContext
    {
        public PharmaContext() : base("name=PharmaContext") // Thay đổi từ DefaultConnection thành PharmaContext
        {
            // Disable lazy loading để tránh lỗi
            this.Configuration.LazyLoadingEnabled = false;
            // Disable proxy creation
            this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; } // Thêm vào class PharmaContext
        public DbSet<Contact> Contacts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // 🔥 FIX: Đơn giản hóa configuration, loại bỏ duplicate mappings
            
            // Product relationships
            modelBuilder.Entity<Product>()
                .HasOptional(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.category_id);
            
            // Cart relationships
            modelBuilder.Entity<Cart>()
                .HasOptional(c => c.Product)
                .WithMany(p => p.Carts)
                .HasForeignKey(c => c.product_id);

            modelBuilder.Entity<Cart>()
                .HasOptional(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.user_id);

            // Review relationships
            modelBuilder.Entity<Review>()
                .HasOptional(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.product_id);

            modelBuilder.Entity<Review>()
                .HasOptional(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.user_id);

            // 🔥 Order relationships - ĐƠN GIẢN HÓA
            modelBuilder.Entity<Order>()
                .HasOptional(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.user_id)
                .WillCascadeOnDelete(false);
            
            // 🔥 OrderItem relationships - ĐƠN GIẢN HÓA, XÓA DUPLICATE
            modelBuilder.Entity<OrderItem>()
                .HasOptional(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.order_id)
                .WillCascadeOnDelete(false);
            
            // 🔥 FIX: CHỈ ĐỊNH NGHĨA MỘT LẦN relationship với Product
            modelBuilder.Entity<OrderItem>()
                .HasOptional(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.product_id)
                .WillCascadeOnDelete(false);
            
            // Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOptional(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.order_id)
                .WillCascadeOnDelete(false);
            
            // Disable cascade delete for safety
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            base.OnModelCreating(modelBuilder);
        }
    }
}