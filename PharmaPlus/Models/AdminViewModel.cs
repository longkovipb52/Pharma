using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace PharmaPlus.Models
{
    public class AdminDashboardViewModel
    {
        // Thống kê tổng quan
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        
        // Dữ liệu chi tiết
        public List<Order> RecentOrders { get; set; }
        public List<User> NewUsers { get; set; }
        public List<Product> LowStockProducts { get; set; }
        public List<Product> OutOfStockProducts { get; set; }
        public List<Review> RecentReviews { get; set; }
        public List<Contact> UnreadContacts { get; set; }
        
        public AdminDashboardViewModel()
        {
            RecentOrders = new List<Order>();
            NewUsers = new List<User>();
            LowStockProducts = new List<Product>();
            OutOfStockProducts = new List<Product>();
            RecentReviews = new List<Review>();
            UnreadContacts = new List<Contact>();
        }
    }

    public class AdminProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string Username { get; set; }

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Quyền")]
        public string Role { get; set; }
    }

    public class AdminChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
        [Display(Name = "Mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}