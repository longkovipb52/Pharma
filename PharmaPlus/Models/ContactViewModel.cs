using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace PharmaPlus.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100, ErrorMessage = "Họ tên không vượt quá 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không vượt quá 100 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200, ErrorMessage = "Tiêu đề không vượt quá 200 ký tự")]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        [Display(Name = "Nội dung")]
        public string Message { get; set; }
        
        // Để hiển thị danh sách dropdown chủ đề
        public List<SelectListItem> SubjectOptions { get; set; }
        
        public ContactViewModel()
        {
            SubjectOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "Thắc mắc về sản phẩm", Text = "Thắc mắc về sản phẩm" },
                new SelectListItem { Value = "Tư vấn sức khỏe", Text = "Tư vấn sức khỏe" },
                new SelectListItem { Value = "Phản hồi dịch vụ", Text = "Phản hồi dịch vụ" },
                new SelectListItem { Value = "Hợp tác kinh doanh", Text = "Hợp tác kinh doanh" },
                new SelectListItem { Value = "Khác", Text = "Khác" }
            };
        }
    }
    
    public class ContactHistoryViewModel
    {
        public List<ContactHistoryItem> Contacts { get; set; } = new List<ContactHistoryItem>();
        public int TotalContacts { get; set; }
        public bool HasAnsweredContacts => Contacts.Any(c => c.Status == "Đã phản hồi");
    }
    
    public class ContactHistoryItem
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Response { get; set; }
        public DateTime? RespondedAt { get; set; }
        
        public string FormattedCreatedAt => CreatedAt.ToString("dd/MM/yyyy HH:mm");
        public string FormattedRespondedAt => RespondedAt?.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string StatusClass => Status == "Đã phản hồi" ? "answered" : "pending";
    }
}