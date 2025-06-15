using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PharmaPlus.Models;
using System.Data.Entity;

namespace PharmaPlus.Controllers
{
    public class CheckoutController : Controller
    {
        private PharmaContext db = new PharmaContext();
        
        // GET: Checkout
        public ActionResult Index()
        {
            // 🔥 THÊM: Kiểm tra đăng nhập trước khi vào trang thanh toán
            if (Session["UserId"] == null)
            {
                // Lưu URL hiện tại để redirect về sau khi đăng nhập
                var returnUrl = Url.Action("Index", "Checkout");
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var cart = Session["Cart"] as List<CartItem>;
            
            // 🔥 DEBUG: Kiểm tra cart khi vào trang checkout
            System.Diagnostics.Debug.WriteLine($"📦 Checkout Index - Cart count: {cart?.Count ?? 0}");
            
            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Products");
            }

            var model = CreateCheckoutViewModel();
            return View(model);
        }
        
        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Code xử lý lỗi giữ nguyên
                    model = CreateCheckoutViewModel();
                    return View("Index", model);
                }

                var cart = Session["Cart"] as List<CartItem>;
                System.Diagnostics.Debug.WriteLine($"📦 Cart count: {cart?.Count ?? 0}");
                
                if (cart == null || !cart.Any())
                {
                    System.Diagnostics.Debug.WriteLine("❌ Cart is empty!");
                    TempData["ErrorMessage"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index", "Products");
                }

                // Tạo transaction để đảm bảo tính nhất quán
                using (var dbTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tạo Order
                        var order = new Order
                        {
                            user_id = Session["UserId"] as int?,
                            recipient_name = model.ShippingInfo.RecipientName,
                            recipient_phone = model.ShippingInfo.RecipientPhone,
                            shipping_address = model.ShippingInfo.ShippingAddress,
                            created_at = DateTime.Now,
                            status = "Chờ xử lý",
                            total_price = cart.Sum(i => i.Price * i.Quantity)
                        };
                        
                        db.Orders.Add(order);
                        db.SaveChanges();
                        
                        // Thêm OrderItems
                        foreach (var item in cart)
                        {
                            var product = db.Products.Find(item.ProductId);
                            if (product != null)
                            {
                                var orderItem = new OrderItem
                                {
                                    order_id = order.id,
                                    product_id = item.ProductId,
                                    quantity = item.Quantity,
                                    unit_price = item.Price
                                };
                                
                                db.OrderItems.Add(orderItem);
                                
                                // Cập nhật stock
                                if (product.stock >= item.Quantity)
                                {
                                    product.stock -= item.Quantity;
                                }
                            }
                        }
                        db.SaveChanges();
                        
                        // Tạo Payment riêng biệt
                        var payment = new Payment
                        {
                            order_id = order.id,  // Chỉ đặt order_id, không đặt Order
                            method = model.PaymentInfo.PaymentMethod,
                            amount = order.total_price,
                            paid_at = model.PaymentInfo.PaymentMethod == "cod" ? (DateTime?)null : DateTime.Now
                        };
                        
                        db.Payments.Add(payment);
                        db.SaveChanges();
                        
                        // Commit transaction khi mọi thứ thành công
                        dbTransaction.Commit();
                        
                        // Xóa giỏ hàng và chuyển hướng
                        Session["Cart"] = new List<CartItem>();
                        TempData["OrderId"] = order.id;
                        
                        return RedirectToAction("Success");
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction nếu có lỗi
                        dbTransaction.Rollback();
                        throw ex; // Re-throw để xử lý ở outer catch
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ PlaceOrder Exception: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"❌ Inner Inner Exception: {ex.InnerException.InnerException.Message}");
                    }
                }
                
                var errorMessage = ex.InnerException?.InnerException?.Message ?? 
                                  ex.InnerException?.Message ?? 
                                  ex.Message;
                
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đặt hàng: " + errorMessage);
                model = CreateCheckoutViewModel();
                return View("Index", model);
            }
        }
        
        // Thêm action method này
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrderAjax(CheckoutViewModel model)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                        );
                    
                    return Json(new { success = false, errors = errors });
                }
                
                // Kiểm tra giỏ hàng
                var cart = Session["Cart"] as List<CartItem>;
                if (cart == null || !cart.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng của bạn đang trống" });
                }
                
                // Tương tự logic trong PlaceOrder, nhưng trả về JSON
                
                // Giả sử đã xử lý thành công
                TempData["OrderId"] = 123; // Thay bằng order.id thực tế
                
                return Json(new { 
                    success = true, 
                    message = "Đặt hàng thành công", 
                    redirectUrl = Url.Action("Success", "Checkout") 
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = "Đã xảy ra lỗi khi đặt hàng: " + ex.Message 
                });
            }
        }
        
        // GET: Checkout/Success
        public ActionResult Success()
        {
            var orderId = TempData["OrderId"] as int?;
            if (!orderId.HasValue)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng";
                return RedirectToAction("Index", "Products");
            }
            
            // 🔥 FIX: Lấy Order
            var order = db.Orders.FirstOrDefault(o => o.id == orderId.Value);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại";
                return RedirectToAction("Index", "Products");
            }
            
            // 🔥 FIX: Lấy thông tin Payment
            var payment = db.Payments.FirstOrDefault(p => p.order_id == orderId.Value);
            
            // Query OrderItems và Products riêng biệt
            var orderItems = db.OrderItems
                .Where(oi => oi.order_id == orderId.Value)
                .ToList();
            
            var productIds = orderItems.Select(oi => oi.product_id).ToList();
            var products = db.Products
                .Where(p => productIds.Contains(p.id))
                .ToList();
            
            // 🔥 FIX: Tạo ViewModel với đầy đủ thông tin
            var viewModel = new OrderConfirmationViewModel
            {
                OrderId = order.id,
                OrderStatus = order.status,
                RecipientName = order.recipient_name,
                RecipientPhone = order.recipient_phone,
                ShippingAddress = order.shipping_address,
                OrderDate = order.created_at ?? DateTime.Now,
                TotalAmount = order.total_price,
                
                // 🔥 FIX: Thêm thông tin thanh toán
                PaymentMethod = GetPaymentMethodDisplayName(payment?.method ?? ""),
                PaymentStatus = GetPaymentStatusDisplayName(payment?.method ?? "", payment?.paid_at),
                
                OrderItems = orderItems.Select(oi => {
                    var product = products.FirstOrDefault(p => p.id == oi.product_id);
                    return new CartItemViewModel
                    {
                        ProductId = product?.id ?? 0,
                        ProductName = product?.name ?? "Sản phẩm không tồn tại",
                        Price = oi.unit_price,
                        Quantity = oi.quantity,
                        SubTotal = oi.unit_price * oi.quantity,
                        ImageUrl = product?.DisplayImageUrl ?? "/images/products/default-product.jpg" // ✅ Sử dụng DisplayImageUrl
                    };
                }).ToList()
            };
            
            return View(viewModel);
        }
        
        // GET: Checkout/CheckOrderStatus
        [HttpGet]
        public ActionResult CheckOrderStatus(int id)
        {
            var order = db.Orders.FirstOrDefault(o => o.id == id);
            if (order == null)
            {
                return Json(new { exists = false }, JsonRequestBehavior.AllowGet);
            }
            
            return Json(new { 
                exists = true, 
                status = order.status,
                createdAt = order.created_at
            }, JsonRequestBehavior.AllowGet);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // 🔥 Helper method để tạo lại model khi có lỗi
        private CheckoutViewModel CreateCheckoutViewModel()
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            
            // 🔥 FIX: Lấy thông tin product từ DB để có đường dẫn ảnh đúng
            var productIds = cart.Select(x => x.ProductId).ToList();
            var products = db.Products.Where(p => productIds.Contains(p.id)).ToList();
            
            var cartItems = cart.Select(item => {
                // 🔥 FIX: Tìm product từ DB để lấy DisplayImageUrl
                var product = products.FirstOrDefault(p => p.id == item.ProductId);
                
                return new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    SubTotal = item.Price * item.Quantity,
                    ImageUrl = product?.DisplayImageUrl ?? "/images/products/default-product.jpg" // ✅ Sử dụng DisplayImageUrl
                };
            }).ToList();
            
            return new CheckoutViewModel
            {
                CartItems = cartItems,
                ShippingInfo = new ShippingInfoViewModel
                {
                    RecipientName = Session["FullName"]?.ToString() ?? "",
                    RecipientPhone = Session["UserPhone"]?.ToString() ?? ""
                },
                PaymentInfo = new PaymentInfoViewModel()
            };
        }

        // 🔥 FIX: Thêm helper methods để convert payment method và status
        private string GetPaymentMethodDisplayName(string method)
        {
            switch (method?.ToLower())
            {
                case "cod":
                    return "Thanh toán khi nhận hàng (COD)";
                case "banking":
                    return "Chuyển khoản ngân hàng";
                case "momo":
                    return "Ví điện tử MoMo";
                case "zalopay":
                    return "ZaloPay";
                default:
                    return "Chưa xác định";
            }
        }

        private string GetPaymentStatusDisplayName(string method, DateTime? paidAt)
        {
            if (method?.ToLower() == "cod")
            {
                return "Thanh toán khi nhận hàng";
            }
            
            if (paidAt.HasValue)
            {
                return "Đã thanh toán";
            }
            
            return "Chưa thanh toán";
        }
    }
}