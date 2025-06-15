using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaPlus.Models
{
    [Table("Product")]
    public class Product
    {
        public int id { get; set; }
        
        [StringLength(50)]
        public string name { get; set; }
        
        public string description { get; set; }
        
        public decimal price { get; set; }
        
        public int stock { get; set; }
        
        public bool? prescription_required { get; set; }
        
        public int? category_id { get; set; }
        
        public DateTime? created_at { get; set; }
        
        [StringLength(500)]
        public string image_url { get; set; }

        // Computed properties
        [NotMapped]
        public int stock_quantity 
        { 
            get { return stock; } 
            set { stock = value; } 
        }

        [NotMapped]
        public string DisplayImageUrl 
        { 
            get 
            { 
                if (!string.IsNullOrEmpty(image_url))
                {
                    // Kiểm tra xem là URL đầy đủ hay đường dẫn tương đối
                    if (image_url.StartsWith("http://") || image_url.StartsWith("https://"))
                    {
                        return image_url;
                    }
                    else if (image_url.StartsWith("/"))
                    {
                        return image_url; // Đường dẫn tuyệt đối
                    }
                    else
                    {
                        // Đường dẫn tương đối, thêm vào thư mục images/products/
                        return "/images/products/" + image_url;
                    }
                }
                return "/images/products/default-product.jpg"; 
            } 
        }

        [NotMapped]
        public string ThumbnailUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(image_url))
                {
                    var extension = System.IO.Path.GetExtension(image_url);
                    var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(image_url);
                    return "/images/products/" + nameWithoutExtension + "_thumb" + extension;
                }
                return "/images/products/default-product.jpg";
            }
        }

        [NotMapped]
        public bool IsNew
        {
            get
            {
                return created_at.HasValue && created_at.Value >= DateTime.Now.AddDays(-7);
            }
        }

        [NotMapped]
        public string StockStatus
        {
            get
            {
                if (stock == 0) return "out-of-stock";
                if (stock <= 5) return "low-stock";
                return "in-stock";
            }
        }

        // Thêm các thuộc tính sau vào lớp Product
        [NotMapped]
        public double AverageRating { get; set; }

        [NotMapped]
        public Review UserReview { get; set; }

        // Navigation properties
        public virtual Category Category { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Cart> Carts { get; set; }

        public Product()
        {
            Reviews = new HashSet<Review>();
        }
    }
}