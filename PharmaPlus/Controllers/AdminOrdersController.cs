using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminOrdersController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: AdminOrders
        public ActionResult Index(string status = null, DateTime? fromDate = null, DateTime? toDate = null, string searchTerm = null, string paymentStatus = null, int? page = null)
        {
            var filter = new AdminOrderFilterViewModel
            {
                Status = status,
                FromDate = fromDate,
                ToDate = toDate,
                SearchTerm = searchTerm,
                PaymentStatus = paymentStatus
            };

            ViewBag.Filter = filter;
            ViewBag.CurrentStatus = status;

            var query = db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Include(o => o.Payments) // Thay đổi từ Payment -> Payments
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.status == status);
            }

            // Filter by date range
            if (fromDate.HasValue)
            {
                query = query.Where(o => o.created_at >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                // Include the entire day
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(o => o.created_at < endDate);
            }

            // Filter by search term (recipient name, phone or order ID)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(o => 
                    o.recipient_name.Contains(searchTerm) || 
                    o.recipient_phone.Contains(searchTerm) || 
                    o.id.ToString() == searchTerm
                );
            }

            // Filter by payment status - sửa để truy cập Payments collection
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                bool isPaid = paymentStatus.ToLower() == "paid";
                query = query.Where(o => o.Payments.Any(p => p.paid_at != null) == isPaid);
            }

            // Order by most recent first
            query = query.OrderByDescending(o => o.created_at);

            // Convert to view models
            var orderViewModels = query.ToList().Select(o => new AdminOrderViewModel
            {
                Id = o.id,
                CustomerName = o.User != null ? o.User.full_name : "Khách vãng lai",
                TotalPrice = o.total_price,
                Status = o.status,
                RecipientName = o.recipient_name,
                RecipientPhone = o.recipient_phone,
                // Sử dụng property Navigation Payment mới hoặc truy cập trực tiếp vào Payments
                PaymentMethod = o.Payment != null ? o.Payment.method : "Không xác định",
                IsPaid = o.Payment != null && o.Payment.paid_at != null,
                CreatedAt = o.created_at,
                ItemCount = o.OrderItems != null ? o.OrderItems.Count : 0
            }).ToList();

            return View(orderViewModels);
        }

        // GET: AdminOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Thay đổi cách load dữ liệu để tránh xung đột cột
            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.id == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            // Load OrderItems riêng
            db.Entry(order)
                .Collection(o => o.OrderItems)
                .Load();
                
            // Sau đó load Product cho mỗi OrderItem
            foreach (var item in order.OrderItems)
            {
                if (item.product_id.HasValue)
                {
                    db.Entry(item)
                        .Reference(i => i.Product)
                        .Load();
                }
            }

            // Get product details for each order item
            var orderItems = order.OrderItems
                .Select(i => new AdminOrderItemViewModel
                {
                    Id = i.id,
                    ProductId = i.product_id ?? 0,
                    ProductName = i.Product != null ? i.Product.name : "Sản phẩm không tồn tại",
                    ProductImage = i.Product != null ? i.Product.image_url : null,
                    UnitPrice = i.unit_price,
                    Quantity = i.quantity
                }).ToList();

            // Sử dụng property Payment mới
            string paymentMethod = order.Payment != null ? order.Payment.method : "Không xác định";
            bool isPaid = order.Payment != null && order.Payment.paid_at != null;
            DateTime? paidDate = order.Payment != null ? order.Payment.paid_at : null;

            var viewModel = new AdminOrderDetailsViewModel
            {
                Id = order.id,
                CustomerId = order.user_id,
                CustomerName = order.User != null ? order.User.full_name : "Khách vãng lai",
                CustomerEmail = order.User != null ? order.User.email : null,
                TotalPrice = order.total_price,
                Status = order.status,
                RecipientName = order.recipient_name,
                RecipientPhone = order.recipient_phone,
                ShippingAddress = order.shipping_address,
                CreatedAt = order.created_at,
                PaymentMethod = paymentMethod,
                IsPaid = isPaid,
                PaidAt = paidDate,
                OrderItems = orderItems
            };

            return View(viewModel);
        }

        // GET: AdminOrders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var viewModel = new AdminOrderUpdateViewModel
            {
                Id = order.id,
                Status = order.status,
                AdminNote = "" // You might want to add an admin_note field to your Order table
            };

            ViewBag.AvailableStatuses = new SelectList(AdminOrderUpdateViewModel.AvailableStatuses, order.status);

            return View(viewModel);
        }

        // POST: AdminOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminOrderUpdateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var order = db.Orders.Find(viewModel.Id);
                if (order == null)
                {
                    return HttpNotFound();
                }

                order.status = viewModel.Status;
                // If you have an admin_note field
                // order.admin_note = viewModel.AdminNote;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Cập nhật đơn hàng thành công!";
                return RedirectToAction("Details", new { id = viewModel.Id });
            }

            ViewBag.AvailableStatuses = new SelectList(AdminOrderUpdateViewModel.AvailableStatuses, viewModel.Status);
            return View(viewModel);
        }

        // POST: AdminOrders/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsPaid(int id)
        {
            var payment = db.Payments.FirstOrDefault(p => p.order_id == id);
            if (payment == null)
            {
                return HttpNotFound();
            }

            payment.paid_at = DateTime.Now;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã cập nhật trạng thái thanh toán!";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: AdminOrders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            order.status = "Đã hủy";
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đơn hàng đã được hủy!";
            return RedirectToAction("Details", new { id = id });
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