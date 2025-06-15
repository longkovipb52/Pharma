using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaPlus.Models
{
    [Table("Payment")]
    public class Payment
    {
        [Key]
        public int id { get; set; }
        
        public int? order_id { get; set; }
        
        [StringLength(20)]
        public string method { get; set; }
        
        [Required]
        [Column(TypeName = "decimal")]
        public decimal amount { get; set; }
        
        public DateTime? paid_at { get; set; }
        
        // Chỉ rõ khóa ngoại và mối quan hệ
        [ForeignKey("order_id")]
        public virtual Order Order { get; set; }
    }
}