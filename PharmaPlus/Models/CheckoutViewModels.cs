using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PharmaPlus.Models
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public ShippingInfoViewModel ShippingInfo { get; set; } = new ShippingInfoViewModel();
        public PaymentInfoViewModel PaymentInfo { get; set; } = new PaymentInfoViewModel();
        
        public decimal SubTotal => CartItems.Sum(item => item.SubTotal);
        public decimal ShippingFee => CartItems.Any() ? 30000 : 0;
        public decimal TotalAmount => SubTotal + ShippingFee;
        
        public string FormattedSubTotal => SubTotal.ToString("N0") + " VNĐ";
        public string FormattedShippingFee => ShippingFee.ToString("N0") + " VNĐ";
        public string FormattedTotalAmount => TotalAmount.ToString("N0") + " VNĐ";
        
        public bool HasPrescriptionItems => CartItems.Any(item => item.PrescriptionRequired);
        public int TotalItems => CartItems.Sum(item => item.Quantity);
        public bool IsValid => CartItems.Any() && ShippingInfo.IsValid && PaymentInfo.IsValid;
    }
    
    public class ShippingInfoViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ tên người nhận")]
        [StringLength(50, ErrorMessage = "Họ tên không được vượt quá 50 ký tự")]
        public string RecipientName { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string RecipientPhone { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }
        
        [Display(Name = "Ghi chú đơn hàng")]
        public string OrderNotes { get; set; }
        
        public bool IsValid => !string.IsNullOrEmpty(RecipientName) && 
                              !string.IsNullOrEmpty(RecipientPhone) && 
                              !string.IsNullOrEmpty(ShippingAddress);
    }
    
    public class PaymentInfoViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }
        
        public bool IsValid => !string.IsNullOrEmpty(PaymentMethod);
        
        // 🔥 FIX: Sửa từ List<string> thành List<PaymentMethodViewModel>
        public List<PaymentMethodViewModel> AvailablePaymentMethods { get; set; } = new List<PaymentMethodViewModel>
        {
            new PaymentMethodViewModel 
            { 
                Id = "cod", 
                Name = "Thanh toán khi nhận hàng (COD)", 
                Description = "Bạn sẽ thanh toán bằng tiền mặt khi nhận được hàng", 
                Icon = "fa-money-bill",
                Color = "#28a745"
            },
            new PaymentMethodViewModel 
            { 
                Id = "banking", 
                Name = "Chuyển khoản ngân hàng", 
                Description = "Chuyển khoản đến tài khoản ngân hàng của PharmaPlus", 
                Icon = "fa-university",
                Color = "#007bff"
            },
            new PaymentMethodViewModel 
            { 
                Id = "momo", 
                Name = "Ví điện tử MoMo", 
                Description = "Thanh toán qua ví điện tử MoMo", 
                Icon = "fa-wallet",
                Color = "#ae2070"
            },
            new PaymentMethodViewModel 
            { 
                Id = "zalopay", 
                Name = "ZaloPay", 
                Description = "Thanh toán qua ZaloPay", 
                Icon = "fa-qrcode",
                Color = "#0066b3" 
            }
        };
    }
    
    public class PaymentMethodViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; } = "#333";
    }
    
    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CartItemViewModel> OrderItems { get; set; } = new List<CartItemViewModel>();
        
        public string FormattedTotalAmount => TotalAmount.ToString("N0") + " VNĐ";
        public string FormattedOrderDate => OrderDate.ToString("dd/MM/yyyy HH:mm:ss");
        
        // 🔥 FIX: Thêm computed property để kiểm tra payment status
        public string PaymentStatusCssClass 
        { 
            get 
            {
                if (PaymentStatus == "Đã thanh toán") return "paid";
                if (PaymentStatus == "Thanh toán khi nhận hàng") return "cod";
                return "pending";
            } 
        }
        
        public bool ShowSimpleSuccess => !OrderItems.Any();
    }
}