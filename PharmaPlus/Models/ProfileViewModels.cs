using System;
using System.ComponentModel.DataAnnotations;

namespace PharmaPlus.Models
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }
        
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(50, ErrorMessage = "Họ và tên không được vượt quá 50 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Vai trò")]
        public string Role { get; set; }
        
        [Display(Name = "Ngày tạo tài khoản")]
        public DateTime? CreatedAt { get; set; }
        
        // Computed properties
        public string FormattedCreatedAt => CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string RoleDisplay => Role == "admin" ? "Quản trị viên" : "Khách hàng";
    }
    
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }
        
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        public string NewPassword { get; set; }
        
        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
    
    public class UserOrderSummary
    {
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public DateTime? LastOrderDate { get; set; }
        
        public string FormattedTotalSpent => TotalSpent.ToString("N0") + " VNĐ";
        public string FormattedLastOrderDate => LastOrderDate?.ToString("dd/MM/yyyy") ?? "Chưa có đơn hàng";
    }
}