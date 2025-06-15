using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PharmaPlus.Models
{
    public class AdminOrderViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; }
        
        [Display(Name = "Tổng tiền")]
        public decimal TotalPrice { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Người nhận")]
        public string RecipientName { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string RecipientPhone { get; set; }
        
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }
        
        [Display(Name = "Đã thanh toán")]
        public bool IsPaid { get; set; }
        
        [Display(Name = "Ngày đặt")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Số sản phẩm")]
        public int ItemCount { get; set; }
        
        public string StatusClass
        {
            get
            {
                if (Status == null)
                    return "badge bg-secondary";
                    
                string statusLower = Status.ToLower();
                
                if (statusLower == "chờ xử lý")
                    return "badge bg-warning";
                else if (statusLower == "đã xác nhận")
                    return "badge bg-info";
                else if (statusLower == "đang giao hàng")
                    return "badge bg-primary";
                else if (statusLower == "hoàn thành")
                    return "badge bg-success";
                else if (statusLower == "đã hủy")
                    return "badge bg-danger";
                else
                    return "badge bg-secondary";
            }
        }
        
        public string FormattedTotalPrice 
        {
            get { return string.Format("{0:#,##0} VNĐ", TotalPrice); }
        }

        public string FormattedCreatedAt 
        {
            get
            {
                if (CreatedAt.HasValue)
                    return CreatedAt.Value.ToString("dd/MM/yyyy HH:mm");
                return "";
            }
        }
    }

    public class AdminOrderDetailsViewModel
    {
        public int Id { get; set; }
        
        [Display(Name = "Khách hàng")]
        public string CustomerName { get; set; }
        
        public int? CustomerId { get; set; }
        
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }
        
        [Display(Name = "Tổng tiền")]
        public decimal TotalPrice { get; set; }
        
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Người nhận")]
        public string RecipientName { get; set; }
        
        [Display(Name = "Số điện thoại")]
        public string RecipientPhone { get; set; }
        
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }
        
        [Display(Name = "Ngày đặt")]
        public DateTime? CreatedAt { get; set; }
        
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }
        
        [Display(Name = "Đã thanh toán")]
        public bool IsPaid { get; set; }
        
        [Display(Name = "Ngày thanh toán")]
        public DateTime? PaidAt { get; set; }
        
        public List<AdminOrderItemViewModel> OrderItems { get; set; }
        
        public string StatusClass
        {
            get
            {
                if (Status == null)
                    return "badge bg-secondary";
                    
                string statusLower = Status.ToLower();
                
                if (statusLower == "chờ xử lý")
                    return "badge bg-warning";
                else if (statusLower == "đã xác nhận")
                    return "badge bg-info";
                else if (statusLower == "đang giao hàng")
                    return "badge bg-primary";
                else if (statusLower == "hoàn thành")
                    return "badge bg-success";
                else if (statusLower == "đã hủy")
                    return "badge bg-danger";
                else
                    return "badge bg-secondary";
            }
        }
        
        public string FormattedTotalPrice 
        {
            get { return string.Format("{0:#,##0} VNĐ", TotalPrice); }
        }

        public string FormattedCreatedAt 
        {
            get
            {
                if (CreatedAt.HasValue)
                    return CreatedAt.Value.ToString("dd/MM/yyyy HH:mm");
                return "";
            }
        }
        
        public string FormattedPaidAt 
        {
            get
            {
                if (PaidAt.HasValue)
                    return PaidAt.Value.ToString("dd/MM/yyyy HH:mm");
                return "Chưa thanh toán";
            }
        }

        public string PaymentStatusClass 
        {
            get { return IsPaid ? "badge bg-success" : "badge bg-warning"; }
        }

        public string PaymentStatusText 
        {
            get { return IsPaid ? "Đã thanh toán" : "Chưa thanh toán"; }
        }
        
        public AdminOrderDetailsViewModel()
        {
            OrderItems = new List<AdminOrderItemViewModel>();
        }
    }

    public class AdminOrderItemViewModel
    {
        public int Id { get; set; }
        
        public int ProductId { get; set; }
        
        [Display(Name = "Sản phẩm")]
        public string ProductName { get; set; }
        
        [Display(Name = "Ảnh")]
        public string ProductImage { get; set; }
        
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }
        
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }
        
        [Display(Name = "Thành tiền")]
        public decimal SubTotal 
        {
            get { return UnitPrice * Quantity; }
        }
        
        public string FormattedUnitPrice 
        {
            get { return string.Format("{0:#,##0} VNĐ", UnitPrice); }
        }

        public string FormattedSubTotal 
        {
            get { return string.Format("{0:#,##0} VNĐ", SubTotal); }
        }
    }

    public class AdminOrderUpdateViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
        
        [Display(Name = "Ghi chú")]
        public string AdminNote { get; set; }
        
        public static List<string> AvailableStatuses = new List<string>
        {
            "Chờ xử lý",
            "Đã xác nhận",
            "Đang giao hàng",
            "Hoàn thành",
            "Đã hủy"
        };
    }

    public class AdminOrderFilterViewModel
    {
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchTerm { get; set; }
        public string PaymentStatus { get; set; }
    }
}