using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PharmaPlus.Models
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; }
        public List<Category> Categories { get; set; }
        public string CurrentCategory { get; set; }
        public string CurrentSearch { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
        public string SortBy { get; set; }
        public int PageSize { get; set; }
    }

    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(50, ErrorMessage = "Tên sản phẩm không được quá 50 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(0, 1000000000, ErrorMessage = "Giá sản phẩm phải >= 0")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải >= 0")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        [Display(Name = "Yêu cầu đơn thuốc")]
        public bool PrescriptionRequired { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Hình ảnh")]
        public string ImageUrl { get; set; }

        [Display(Name = "Tải hình ảnh mới")]
        public HttpPostedFileBase ImageFile { get; set; }
    }

    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }

    public class InventoryViewModel
    {
        public List<Product> Products { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalProducts { get; set; }
    }

    public class ProductReportViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<TopProductViewModel> TopSellingProducts { get; set; }
        public List<CategorySalesViewModel> CategoryData { get; set; }
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CategorySalesViewModel
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProductDetailViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ImageUrl { get; set; }
        public bool PrescriptionRequired { get; set; }
        public List<Review> Reviews { get; set; }
        public double Rating { get; set; }
        public int ReviewCount { get; set; }
        public List<Product> RelatedProducts { get; set; }

        public ProductDetailViewModel()
        {
            Reviews = new List<Review>();
            RelatedProducts = new List<Product>();
        }

        // Thuộc tính tiện ích để hiển thị trên view
        public string FormattedPrice => Price.ToString("N0") + " VNĐ";
        public string StockStatusText => Stock > 0 ? "Còn hàng" : "Hết hàng";
        public string StockStatusClass => Stock > 0 ? (Stock <= 5 ? "low-stock" : "in-stock") : "out-of-stock";
    }
}