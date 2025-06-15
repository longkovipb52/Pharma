using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq; // Thêm using này để sử dụng FirstOrDefault()

namespace PharmaPlus.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int id { get; set; }
        
        public int? user_id { get; set; }
        
        [Required]
        [Column(TypeName = "decimal")] // Thay vì decimal(12, 2)
        public decimal total_price { get; set; }
        
        [StringLength(20)]
        public string status { get; set; }
        
        [StringLength(50)]
        public string recipient_name { get; set; }
        
        [Required]
        [StringLength(20)]
        public string recipient_phone { get; set; }
        
        public string shipping_address { get; set; }
        
        public DateTime? created_at { get; set; }
        
        [ForeignKey("user_id")]
        public virtual User User { get; set; }
        
        // Một đơn hàng có nhiều OrderItems
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        
        // Thêm Navigation Property cho Payment (một đơn hàng có một payment)
        [NotMapped] // Không ảnh hưởng đến database schema
        public virtual Payment Payment 
        {
            get
            {
                // Lấy payment đầu tiên từ danh sách Payments (nếu có)
                if (Payments != null && Payments.Any())
                {
                    return Payments.FirstOrDefault();
                }
                return null;
            }
        }
        
        // Duy trì Navigation Property Payments hiện có (một đơn hàng có nhiều payments)
        public virtual ICollection<Payment> Payments { get; set; }
        
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
            Payments = new HashSet<Payment>();
        }
    }
}