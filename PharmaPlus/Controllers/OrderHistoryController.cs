using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PharmaPlus.Models;
using System.Data.Entity;

namespace PharmaPlus.Controllers
{
    // Vẫn giữ Authorize để đảm bảo bảo mật
    [Authorize]
    public class OrderHistoryController : Controller
    {
        private PharmaContext db = new PharmaContext();
        
        // GET: OrderHistory
        public ActionResult Index(int page = 1, int? Status = null, string Keyword = null, 
                                DateTime? FromDate = null, DateTime? ToDate = null)
        {
            // Kiểm tra đăng nhập
            if (!IsUserAuthenticated())
            {
                // Nếu không xác thực, chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account", 
                    new { returnUrl = Request.Url.PathAndQuery });
            }

            var model = new OrderHistoryViewModel();
            model.Filter = new OrderFilterModel();
            
            // Lấy user id từ session
            int? userId = Session["UserId"] as int?;
            
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "OrderHistory") });
            }
            
            // Truy vấn dữ liệu
            var query = db.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payments)
                .Where(o => o.user_id == userId)
                .AsQueryable();
            
            // Áp dụng bộ lọc
            if (FromDate.HasValue)
            {
                query = query.Where(o => o.created_at >= FromDate.Value);
            }
            
            if (ToDate.HasValue)
            {
                var endDate = ToDate.Value.AddDays(1).AddSeconds(-1); // Đến cuối ngày
                query = query.Where(o => o.created_at <= endDate);
            }
            
            if (Status.HasValue)
            {
                string statusString = GetStatusString(Status.Value);
                query = query.Where(o => o.status == statusString);
            }
            
            if (!string.IsNullOrEmpty(Keyword))
            {
                int orderId;
                if (int.TryParse(Keyword, out orderId))
                {
                    query = query.Where(o => o.id == orderId || 
                                           o.recipient_name.Contains(Keyword) ||
                                           o.recipient_phone.Contains(Keyword));
                }
                else
                {
                    query = query.Where(o => o.recipient_name.Contains(Keyword) ||
                                          o.recipient_phone.Contains(Keyword));
                }
            }
            
            // Phân trang - sửa phần này
            var totalItems = query.Count();
            var pageSize = 10;
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            // Đảm bảo currentPage không bao giờ là 0, tối thiểu phải là 1
            var currentPage = page < 1 ? 1 : (page > totalPages && totalPages > 0 ? totalPages : page);
            
            // Đặt currentPage = 1 ngay cả khi không có đơn hàng
            if (totalPages == 0)
            {
                currentPage = 1; // Thay đổi từ 0 thành 1
            }
            
            model.Pagination = new PaginationInfo
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
            
            // Lấy dữ liệu theo trang - thêm kiểm tra để tránh Skip âm
            if (totalItems > 0)
            {
                var orders = query
                    .OrderByDescending(o => o.created_at)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                    
                // Chuyển đổi dữ liệu
                model.Orders = orders.Select(o => new OrderSummary
                {
                    Id = o.id,
                    OrderCode = $"DH{o.id:D6}",
                    OrderDate = o.created_at ?? DateTime.Now,
                    TotalAmount = o.total_price,
                    Status = o.status,
                    StatusCode = GetStatusCode(o.status), // Phần này không cần sửa vì StatusCode trong OrderSummary là int
                    PaymentMethod = o.Payments.FirstOrDefault()?.method,
                    ItemCount = o.OrderItems.Count,
                    CanCancel = o.status == "Chờ xử lý" || o.status == "Chưa xử lý" || o.status == "Đã xác nhận"
                }).ToList();
            }
            else
            {
                // Đảm bảo Orders không null
                model.Orders = new List<OrderSummary>();
            }
            
            return View(model);
        }
        
        // GET: OrderHistory/Detail/5
        public ActionResult Detail(int id)
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sửa lại để Include OrderItems
            var order = db.Orders
                .Include(o => o.OrderItems)  // Thêm dòng này
                .Include(o => o.Payments)
                .Where(o => o.id == id && o.user_id == userId)
                .FirstOrDefault();
                
            if (order == null)
            {
                return HttpNotFound();
            }
            
            // Thêm kiểm tra null cho OrderItems
            if (order.OrderItems == null)
            {
                order.OrderItems = new List<OrderItem>();
            }
            
            // Còn lại giữ nguyên
            var model = new OrderDetailViewModel
            {
                Id = order.id,
                OrderCode = $"DH{order.id:D6}",
                OrderDate = order.created_at ?? DateTime.Now,
                Status = order.status,
                StatusCode = GetStatusCode(order.status),
                RecipientName = order.recipient_name,
                RecipientPhone = order.recipient_phone,
                ShippingAddress = order.shipping_address,
                PaymentMethod = GetPaymentMethodDisplay(order.Payments.FirstOrDefault()?.method),
                TotalAmount = order.total_price,
                CanCancel = order.status == "Chờ xử lý" || order.status == "Chưa xử lý" || order.status == "Đã xác nhận",
                Items = new List<OrderItemDetail>()
            };
            
            // Gộp các sản phẩm trùng lặp bằng Dictionary
            var productItems = new Dictionary<int, OrderItemDetail>();
            
            foreach (var orderItem in order.OrderItems)
            {
                var product = db.Products.Find(orderItem.product_id);
                if (product != null)
                {
                    // Nếu sản phẩm đã tồn tại trong Dictionary, cộng dồn số lượng
                    if (productItems.ContainsKey(product.id))
                    {
                        productItems[product.id].Quantity += orderItem.quantity;
                        productItems[product.id].Subtotal += orderItem.unit_price * orderItem.quantity;
                    }
                    else
                    {
                        // Nếu sản phẩm chưa tồn tại, thêm mới
                        productItems.Add(product.id, new OrderItemDetail
                        {
                            ProductId = product.id,
                            ProductName = product.name,
                            ProductImage = !string.IsNullOrEmpty(product.image_url) 
                                ? $"/images/products/{product.image_url}" 
                                : "/images/products/default.jpg",
                            UnitPrice = orderItem.unit_price,
                            Quantity = orderItem.quantity,
                            Subtotal = orderItem.unit_price * orderItem.quantity
                        });
                    }
                }
            }
            
            // Gán sản phẩm đã gộp vào model
            model.Items = productItems.Values.ToList();
            
            // Kiểm tra đánh giá cho từng sản phẩm
            foreach (var item in model.Items)
            {
                var review = db.Reviews.FirstOrDefault(r => r.user_id == userId.Value && r.product_id == item.ProductId);
                item.HasReviewed = review != null;
                item.UserRating = review != null ? review.rating : 0;
            }

            // Thêm lịch sử trạng thái đơn hàng
            model.StatusHistory = GetOrderStatusHistory(order);

            return View(model);
        }
        
        // POST: OrderHistory/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            // Lấy user id từ session
            int? userId = Session["UserId"] as int?;
            
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            
            var order = db.Orders
                .Where(o => o.id == id && o.user_id == userId)
                .FirstOrDefault();
                
            if (order == null)
            {
                return HttpNotFound();
            }
            
            // Chỉ cho phép hủy đơn hàng ở trạng thái chưa xử lý hoặc đã xác nhận
            if (order.status == "Chờ xử lý" || order.status == "Chưa xử lý" || order.status == "Đã xác nhận")
            {
                order.status = "Đã hủy";
                
                db.SaveChanges();
                
                TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
            }
            
            return RedirectToAction("Detail", new { id = id });
        }
        
        private int GetStatusCode(string status)
        {
            switch (status)
            {
                case "Chờ xử lý": return 1; // Thêm dòng này
                case "Chưa xử lý": return 1;
                case "Đã xác nhận": return 2;
                case "Đang giao hàng": return 3;
                case "Đã giao hàng": return 4;
                case "Đã hoàn thành": return 5;
                case "Đã hủy": return 6;
                case "Hoàn trả": return 7;
                default: return 1;
            }
        }
        
        private string GetStatusString(int statusCode)
        {
            switch (statusCode)
            {
                case 1: return "Chờ xử lý"; // Thay đổi từ "Chưa xử lý" thành "Chờ xử lý"
                case 2: return "Đã xác nhận";
                case 3: return "Đang giao hàng";
                case 4: return "Đã giao hàng";
                case 5: return "Đã hoàn thành";
                case 6: return "Đã hủy";
                case 7: return "Hoàn trả";
                default: return "Chưa xử lý";
            }
        }
        
        private string GetPaymentMethodDisplay(string method)
        {
            switch (method?.ToLower())
            {
                case "cod": return "Thanh toán khi nhận hàng (COD)";
                case "banking": return "Chuyển khoản ngân hàng";
                case "credit": return "Thẻ tín dụng/Ghi nợ";
                case "momo": return "Ví MoMo";
                case "zalopay": return "ZaloPay";
                default: return "Không xác định";
            }
        }
        
        private List<OrderStatusHistory> GetOrderStatusHistory(Order order)
        {
            var history = new List<OrderStatusHistory>();
            DateTime orderDate = order.created_at ?? DateTime.Now;
            
            // Đơn hàng đã tạo - luôn hiển thị trạng thái này
            history.Add(new OrderStatusHistory
            {
                Status = "Đơn hàng đã tạo",
                Date = orderDate,
                Note = "Đơn hàng của bạn đã được tạo thành công"
            });
            
            // Kiểm tra null cho order.status
            string orderStatus = order.status ?? "Chờ xử lý";
            
            // Sửa phần kiểm tra trạng thái
            if (orderStatus == "Chờ xử lý" || orderStatus == "Chưa xử lý")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Đơn hàng đang chờ xử lý",
                    Date = orderDate.AddMinutes(5),
                    Note = "Đơn hàng của bạn đang được hệ thống xử lý"
                });
            }
            
            // Các trạng thái khác giữ nguyên như code cũ...
            if (order.status == "Đã xác nhận" || order.status == "Đang giao hàng" || 
                order.status == "Đã giao hàng" || order.status == "Đã hoàn thành")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Đã xác nhận",
                    Date = orderDate.AddHours(1),
                    Note = "Đơn hàng của bạn đã được xác nhận"
                });
            }
            
            if (order.status == "Đang giao hàng" || order.status == "Đã giao hàng" || 
                order.status == "Đã hoàn thành")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Đang giao hàng",
                    Date = orderDate.AddHours(4),
                    Note = "Đơn hàng đã được giao cho đơn vị vận chuyển"
                });
            }
            
            if (order.status == "Đã giao hàng" || order.status == "Đã hoàn thành")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Đã giao hàng",
                    Date = orderDate.AddHours(24),
                    Note = "Đơn hàng đã được giao thành công"
                });
            }
            
            if (order.status == "Đã hoàn thành")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Đã hoàn thành",
                    Date = orderDate.AddDays(3),
                    Note = "Cảm ơn bạn đã mua sắm tại PharmaPlus!"
                });
            }
            else if (order.status == "Đã hủy")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Đã hủy",
                    Date = orderDate.AddHours(2),
                    Note = "Đơn hàng đã bị hủy"
                });
            }
            else if (order.status == "Hoàn trả")
            {
                history.Add(new OrderStatusHistory
                {
                    Status = "Hoàn trả",
                    Date = orderDate.AddDays(5),
                    Note = "Đơn hàng đã được hoàn trả"
                });
            }
            
            // Thêm kết quả nhìn thấy trước khi trả về
            System.Diagnostics.Debug.WriteLine($"Order {order.id}, Status: {order.status}, History items: {history.Count}");
            
            return history.OrderByDescending(h => h.Date).ToList();
        }

        private bool CanReviewProduct(int orderId, int productId)
        {
            var order = db.Orders.Find(orderId);
            if (order == null) return false;
            
            // Chỉ đánh giá được khi đơn hàng "Đã giao" hoặc "Đã hủy"
            if (order.status != "Đã giao" && order.status != "Đã hủy" && order.status != "Đã hoàn thành")
                return false;
            
            // Kiểm tra xem đơn hàng có chứa sản phẩm này không
            return order.OrderItems.Any(item => item.product_id == productId);
        }

        private bool IsUserAuthenticated()
        {
            // Kiểm tra cả xác thực forms và session
            bool isFormAuthenticated = User.Identity.IsAuthenticated;
            bool isSessionAuthenticated = Session["UserId"] != null;
            
            return isFormAuthenticated && isSessionAuthenticated;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}