using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PharmaPlus.Models;
using System.Data.Entity;
using BCrypt.Net;

namespace PharmaPlus.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        private PharmaContext db = new PharmaContext();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            
            // Giữ nguyên code khởi tạo viewModel
            var viewModel = new AdminDashboardViewModel
            {
                // Thống kê tổng quan
                TotalOrders = db.Orders.Count(),
                TotalProducts = db.Products.Count(),
                TotalUsers = db.Users.Count(u => u.role == "customer"),
                TotalRevenue = db.Orders
                    .Where(o => o.status != "Đã hủy")
                    .Sum(o => o.total_price),
                
                // Đơn hàng mới nhất
                RecentOrders = db.Orders
                    .OrderByDescending(o => o.created_at)
                    .Take(5)
                    .ToList(),
                
                // Người dùng mới đăng ký
                NewUsers = db.Users
                    .Where(u => u.role == "customer")
                    .OrderByDescending(u => u.created_at)
                    .Take(5)
                    .ToList(),
                
                // Sản phẩm sắp hết hàng
                LowStockProducts = db.Products
                    .Where(p => p.stock <= 10 && p.stock > 0)
                    .OrderBy(p => p.stock)
                    .Take(5)
                    .ToList(),
                
                // Sản phẩm hết hàng
                OutOfStockProducts = db.Products
                    .Where(p => p.stock == 0)
                    .OrderBy(p => p.name)
                    .ToList(),
                
                // Đánh giá mới nhất
                RecentReviews = db.Reviews
                    .Include(r => r.User)  // Đảm bảo có dòng này
                    .Include(r => r.Product)  // Đảm bảo có dòng này
                    .OrderByDescending(r => r.created_at)
                    .Take(5)
                    .ToList(),
                
                // Liên hệ chưa đọc
                UnreadContacts = db.Contacts
                    .Where(c => c.status == "Chưa phản hồi")
                    .OrderByDescending(c => c.created_at)
                    .Take(5)
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Profile
        public ActionResult Profile()
        {
            var userId = Session["UserId"] as int?;
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = db.Users.Find(userId.Value);
            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new AdminProfileViewModel
            {
                Id = user.id,
                Username = user.username,
                FullName = user.full_name,
                Email = user.email,
                Role = user.role,
                CreatedAt = user.created_at.Value
            };

            return View(model);
        }

        // POST: Admin/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(AdminProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = Session["UserId"] as int?;
                if (!userId.HasValue)
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = db.Users.Find(userId.Value);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra email đã tồn tại chưa (nếu có thay đổi)
                if (user.email != model.Email)
                {
                    var emailExists = db.Users.Any(u => u.email == model.Email && u.id != userId);
                    if (emailExists)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                        return View(model);
                    }
                }

                // Cập nhật thông tin
                user.full_name = model.FullName;
                user.email = model.Email;

                db.SaveChanges();

                // Cập nhật Session
                Session["FullName"] = user.full_name;

                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }

        // GET: Admin/ChangePassword
        public ActionResult ChangePassword()
        {
            return View(new AdminChangePasswordViewModel());
        }

        // POST: Admin/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(AdminChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = Session["UserId"] as int?;
                    if (!userId.HasValue)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var user = db.Users.Find(userId.Value);
                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    // Kiểm tra xem mật khẩu hiện tại có phải là hash hay không
                    bool isCurrentPasswordHashed = user.password.StartsWith("$2a$") || user.password.StartsWith("$2b$");
                    bool passwordMatched = false;

                    if (isCurrentPasswordHashed)
                    {
                        // Nếu là hash, sử dụng BCrypt để xác minh
                        passwordMatched = BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.password);
                    }
                    else
                    {
                        // Nếu không phải hash (plaintext), so sánh trực tiếp
                        passwordMatched = (user.password == model.CurrentPassword);
                    }

                    if (!passwordMatched)
                    {
                        ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không chính xác.");
                        return View(model);
                    }

                    // Hash mật khẩu mới trước khi lưu vào database - SỬA Ở ĐÂY
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.NewPassword, workFactor: 12);
                    user.password = hashedPassword;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    // Thêm logging để phát hiện lỗi
                    System.Diagnostics.Debug.WriteLine($"Error in ChangePassword: {ex.Message}");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi đổi mật khẩu: " + ex.Message);
                }
            }

            return View(model);
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