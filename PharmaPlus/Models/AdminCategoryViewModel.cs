using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace PharmaPlus.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }
        
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        
        [Display(Name = "Số lượng sản phẩm")]
        public int ProductCount { get; set; }
    }

    public class CategoryDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
        public List<ProductSummaryViewModel> Products { get; set; }
    }

    public class ProductSummaryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string ImageUrl { get; set; }
    }
}