using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace PharmaPlus.Models
{
    public class AdminProductFormViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(50, ErrorMessage = "Tên sản phẩm không được vượt quá 50 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }
        
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(1000, 100000000, ErrorMessage = "Giá sản phẩm phải từ 1.000 đến 100.000.000 VNĐ")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Range(0, 10000, ErrorMessage = "Số lượng phải từ 0 đến 10.000")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }
        
        [Display(Name = "Yêu cầu đơn thuốc")]
        public bool PrescriptionRequired { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        
        [Display(Name = "Hình ảnh")]
        public HttpPostedFileBase ImageFile { get; set; }
        
        public string ImageUrl { get; set; }
    }
    
    public class AdminProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; }
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
    }
    
    public class AdminInventoryViewModel
    {
        public List<Product> Products { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalProducts { get; set; }
    }
    
    public class AdminTopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
        public string ImageUrl { get; set; }
    }
    
    public class AdminCategorySalesViewModel
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal Revenue { get; set; }
    }
    
    public class AdminProductReportViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<TopProductViewModel> TopSellingProducts { get; set; }
        public List<CategorySalesViewModel> CategoryData { get; set; }
    }
}