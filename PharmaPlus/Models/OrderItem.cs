using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaPlus.Models
{
    [Table("OrderItem")]
    public class OrderItem
    {
        [Key]
        public int id { get; set; }
        
        // 🔥 FIX: Chỉ dùng foreign key properties, không dùng navigation properties khi tạo
        public int? order_id { get; set; }
        public int? product_id { get; set; }
        
        [Required]
        public int quantity { get; set; }
        
        [Required]
        public decimal unit_price { get; set; }

        // 🔥 FIX: Navigation properties - chỉ dùng để query, không assign khi insert
        [ForeignKey("order_id")]
        public virtual Order Order { get; set; }
        
        [ForeignKey("product_id")]
        public virtual Product Product { get; set; }
    }
}