using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaPlus.Models
{
    [Table("Cart")]
    public class Cart
    {
        public int id { get; set; }
        public int? user_id { get; set; }
        public int? product_id { get; set; }
        public int quantity { get; set; }
        public DateTime? updated_at { get; set; }

        public virtual User User { get; set; }
        public virtual Product Product { get; set; }
    }
}