using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace PharmaPlus.Models
{
    public class AdminContactViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Người gửi")]
        public string Name { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; }

        [Display(Name = "Nội dung")]
        public string Message { get; set; }

        [Display(Name = "Ngày gửi")]
        public DateTime? CreatedAt { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "ID người dùng")]
        public int? UserId { get; set; }

        [Display(Name = "Tên người dùng")]
        public string UserFullName { get; set; }

        // Các thuộc tính bổ sung
        public string FormattedCreatedAt => CreatedAt.HasValue ? CreatedAt.Value.ToString("dd/MM/yyyy HH:mm") : "";

        public string StatusClass
        {
            get
            {
                switch (Status?.ToLower())
                {
                    case "đã phản hồi":
                    case "da phan hoi":
                    case "hoàn thành":
                    case "hoan thanh":
                        return "badge bg-success";
                    case "đang xử lý":
                    case "dang xu ly":
                        return "badge bg-primary";
                    case "chưa phản hồi":
                    case "chua phan hoi":
                        return "badge bg-warning";
                    default:
                        return "badge bg-secondary";
                }
            }
        }
    }

    public class AdminContactDetailsViewModel : AdminContactViewModel
    {
        [Display(Name = "Phản hồi của admin")]
        public string AdminReply { get; set; }

        [Display(Name = "Ngày phản hồi")]
        public DateTime? RepliedAt { get; set; }

        public string FormattedRepliedAt => RepliedAt.HasValue ? RepliedAt.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa phản hồi";
    }

    public class AdminContactEditViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Trạng thái")]
        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Phản hồi")]
        [DataType(DataType.MultilineText)]
        public string AdminReply { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime? CreatedAt { get; set; }

        public List<SelectListItem> StatusOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "Chưa phản hồi", Value = "Chưa phản hồi" },
            new SelectListItem { Text = "Đang xử lý", Value = "Đang xử lý" },
            new SelectListItem { Text = "Đã phản hồi", Value = "Đã phản hồi" },
            new SelectListItem { Text = "Hoàn thành", Value = "Hoàn thành" }
        };
    }

    public class AdminContactFilterViewModel
    {
        [Display(Name = "Tìm kiếm")]
        public string SearchTerm { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; }

        [Display(Name = "Từ ngày")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "Đến ngày")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public List<SelectListItem> StatusOptions => new List<SelectListItem>
        {
            new SelectListItem { Text = "Tất cả", Value = "" },
            new SelectListItem { Text = "Chưa phản hồi", Value = "Chưa phản hồi" },
            new SelectListItem { Text = "Đang xử lý", Value = "Đang xử lý" },
            new SelectListItem { Text = "Đã phản hồi", Value = "Đã phản hồi" },
            new SelectListItem { Text = "Hoàn thành", Value = "Hoàn thành" }
        };
    }
}