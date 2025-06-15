using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PharmaPlus.Models
{
    public class AdminUserViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Vai trò")]
        public string Role { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Số đơn hàng")]
        public int OrderCount { get; set; }
        
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "";
        
        public string RoleDisplay 
        {
            get
            {
                if (Role == "admin") return "Quản trị viên";
                else if (Role == "customer") return "Khách hàng";
                else return Role;
            }
        }
        
        public string RoleClass
        {
            get
            {
                if (Role == "admin") return "badge bg-danger";
                else if (Role == "customer") return "badge bg-primary";
                else return "badge bg-secondary";
            }
        }
    }

    public class AdminUserDetailsViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }
        
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Vai trò")]
        public string Role { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Số đơn hàng")]
        public int OrderCount { get; set; }
        
        [Display(Name = "Số đánh giá")]
        public int ReviewCount { get; set; }
        
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "";
        
        public string RoleDisplay
        {
            get
            {
                if (Role == "admin") return "Quản trị viên";
                else if (Role == "customer") return "Khách hàng";
                else return Role;
            }
        }
        
        public string RoleClass
        {
            get
            {
                if (Role == "admin") return "badge bg-danger";
                else if (Role == "customer") return "badge bg-primary";
                else return "badge bg-secondary";
            }
        }
        
        public List<AdminOrderSummaryViewModel> RecentOrders { get; set; }
        public List<AdminReviewSummaryViewModel> RecentReviews { get; set; }
        
        public AdminUserDetailsViewModel()
        {
            RecentOrders = new List<AdminOrderSummaryViewModel>();
            RecentReviews = new List<AdminReviewSummaryViewModel>();
        }
    }
    
    public class AdminOrderSummaryViewModel
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string FormattedTotalPrice => string.Format("{0:#,##0} VNĐ", TotalPrice);
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy") : "";
        
        public string StatusClass
        {
            get
            {
                if (Status == null) return "badge bg-secondary";
                
                string statusLower = Status.ToLower();
                
                if (statusLower == "chờ xử lý") return "badge bg-warning";
                else if (statusLower == "đã xác nhận") return "badge bg-info";
                else if (statusLower == "đang giao hàng") return "badge bg-primary";
                else if (statusLower == "hoàn thành" || statusLower == "đã hoàn thành") return "badge bg-success";
                else if (statusLower == "đã hủy") return "badge bg-danger";
                else return "badge bg-secondary";
            }
        }
    }
    
    public class AdminReviewSummaryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy") : "";
    }
    
    public class AdminUserEditViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        
        [StringLength(50, ErrorMessage = "Họ tên không được vượt quá 50 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Vai trò")]
        public string Role { get; set; }
        
        [Display(Name = "Đặt lại mật khẩu")]
        public bool ResetPassword { get; set; }
        
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }
        
        public List<SelectListItem> AvailableRoles
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Text = "Quản trị viên", Value = "admin" },
                    new SelectListItem { Text = "Khách hàng", Value = "customer" }
                };
            }
        }
    }
}