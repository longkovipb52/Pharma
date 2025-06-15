using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaPlus.Models
{
    [Table("Review")]
    public class Review
    {
        public int id { get; set; }
        public int? user_id { get; set; }
        public int? product_id { get; set; }
        public int rating { get; set; }
        public string comment { get; set; }
        public DateTime? created_at { get; set; }
        public string status { get; set; } // Thêm trường status
        
        // Đảm bảo các navigation properties được định nghĩa đúng
        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
    }
}