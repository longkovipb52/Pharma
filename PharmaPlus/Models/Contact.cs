using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaPlus.Models
{
    [Table("Contact")]
    public class Contact
    {
        [Key]
        public int id { get; set; }
        
        public int? user_id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string name { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string email { get; set; }
        
        [Required]
        [StringLength(200)]
        public string subject { get; set; }
        
        [Required]
        public string message { get; set; }
        
        [StringLength(20)]
        public string status { get; set; }
        
        public DateTime? created_at { get; set; }        
        public string admin_reply { get; set; }
        public DateTime? replied_at { get; set; }
        
        [ForeignKey("user_id")]
        public virtual User User { get; set; }
    }
}