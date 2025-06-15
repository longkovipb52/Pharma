using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PharmaPlus.Models
{
    public class AdminReviewViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Người dùng")]
        public int UserId { get; set; }
        
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; }
        
        [Display(Name = "Email")]
        public string UserEmail { get; set; }
        
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }
        
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }
        
        [Display(Name = "Hình ảnh")]
        public string ProductImage { get; set; }
        
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }
        
        [Display(Name = "Nội dung")]
        public string Comment { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        // Các thuộc tính hiển thị bổ sung
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "";
        
        public string StatusClass
        {
            get
            {
                if (Status == "approved") return "badge bg-success";
                else if (Status == "pending") return "badge bg-warning";
                else if (Status == "rejected") return "badge bg-danger";
                else return "badge bg-secondary";
            }
        }
        
        public string StatusText
        {
            get
            {
                if (Status == "approved") return "Đã duyệt";
                else if (Status == "pending") return "Chờ duyệt";
                else if (Status == "rejected") return "Từ chối";
                else return Status;
            }
        }
        
        public string RatingStars
        {
            get
            {
                string stars = "";
                for (int i = 1; i <= 5; i++)
                {
                    if (i <= Rating)
                        stars += "<i class=\"fas fa-star text-warning\"></i>";
                    else
                        stars += "<i class=\"far fa-star text-secondary\"></i>";
                }
                return stars;
            }
        }
    }
    
    public class AdminReviewFilterViewModel
    {
        [Display(Name = "Từ khóa")]
        public string SearchTerm { get; set; }
        
        [Display(Name = "Sản phẩm")]
        public int? ProductId { get; set; }
        
        [Display(Name = "Đánh giá từ")]
        public int? MinRating { get; set; }
        
        [Display(Name = "Đánh giá đến")]
        public int? MaxRating { get; set; }
        
        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }
        
        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
    }
    
    public class AdminReviewDetailsViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Người dùng")]
        public int UserId { get; set; }
        
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; }
        
        [Display(Name = "Email")]
        public string UserEmail { get; set; }
        
        [Display(Name = "Sản phẩm")]
        public int ProductId { get; set; }
        
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }
        
        [Display(Name = "Hình ảnh")]
        public string ProductImage { get; set; }
        
        [Display(Name = "Giá")]
        public decimal ProductPrice { get; set; }
        
        [Display(Name = "Đánh giá")]
        public int Rating { get; set; }
        
        [Display(Name = "Nội dung")]
        public string Comment { get; set; }
        
        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        // Thuộc tính bổ sung cho trang chi tiết
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "";
        
        public string FormattedProductPrice => string.Format("{0:#,##0} VNĐ", ProductPrice);
        
        public string StatusClass
        {
            get
            {
                if (Status == "approved") return "badge bg-success";
                else if (Status == "pending") return "badge bg-warning";
                else if (Status == "rejected") return "badge bg-danger";
                else return "badge bg-secondary";
            }
        }
        
        public string StatusText
        {
            get
            {
                if (Status == "approved") return "Đã duyệt";
                else if (Status == "pending") return "Chờ duyệt";
                else if (Status == "rejected") return "Từ chối";
                else return Status;
            }
        }
        
        public string RatingStars
        {
            get
            {
                string stars = "";
                for (int i = 1; i <= 5; i++)
                {
                    if (i <= Rating)
                        stars += "<i class=\"fas fa-star text-warning\"></i>";
                    else
                        stars += "<i class=\"far fa-star text-secondary\"></i>";
                }
                return stars;
            }
        }
    }
    
    public class AdminReviewEditViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Trạng thái")]
        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Ghi chú của admin")]
        public string AdminNote { get; set; }
        
        [Display(Name = "Gửi email thông báo đến người dùng")]
        public bool SendNotification { get; set; }
        
        public List<SelectListItem> AvailableStatuses
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem { Text = "Đã duyệt", Value = "approved" },
                    new SelectListItem { Text = "Chờ duyệt", Value = "pending" },
                    new SelectListItem { Text = "Từ chối", Value = "rejected" }
                };
            }
        }
        
        // Thông tin đánh giá để hiển thị (không thể chỉnh sửa)
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}